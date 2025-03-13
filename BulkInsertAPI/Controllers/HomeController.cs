using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Domain.Entities;
using EFCore.BulkExtensions;
using Infrastructure.DBContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace BulkInsertAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BulkInsertController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public BulkInsertController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadCsvFile(IFormFile file)
        {

            if (file == null || file.Length == 0)
            {
                return BadRequest("File is empty or not provided.");
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only CSV files are allowed.");
            }

            var allowedMimeTypes = new List<string> { "text/csv", "application/vnd.ms-excel" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLower()))
            {
                return BadRequest("Invalid file format. Only CSV files are supported.");
            }

            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                var errors = new List<string>();
                var validAssignments = new List<WorkerZoneAssignment>();
                var uniqueAssignments = new List<WorkerZoneAssignment>();
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true
                });

                var records = csv.GetRecords<WorkerZoneAssignmentRequest>().ToList();
                var recordsToBeRemoved = new List<WorkerZoneAssignmentRequest>();
                if (records.Count == 0)
                {
                    return BadRequest("No records found in the CSV file.");
                }

                if (records.Count > 70000)
                {
                    return BadRequest(" records exceeds the limit of 50000.");
                }

                foreach (var record in records)
                {
                    // Validate Worker Code Length
                    if (record.WorkerCode?.Length > 10)
                    {
                        errors.Add($"Worker Code '{record.WorkerCode}' exceeds 10 characters.");
                        recordsToBeRemoved.Add(record);
                        continue;
                    }

                    // Validate Zone Code Length
                    if (record.ZoneCode?.Length > 10)
                    {
                        errors.Add($"Zone Code '{record.ZoneCode}' exceeds 10 characters.");
                        recordsToBeRemoved.Add(record);
                        continue;
                    }

                    // Validate Date Format
                    if (!DateOnly.TryParseExact(record.AssignmentDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                    {
                        errors.Add($"Invalid date format for Worker '{record.WorkerCode}' on '{record.AssignmentDate}'.");
                        recordsToBeRemoved.Add(record);
                        continue;
                    }
                }

                var workers = await _dbContext.Workers.Where(w => records.Select(x => x.WorkerCode).Contains(w.Code)).ToListAsync();
                var zones = await _dbContext.Zones.Where(w => records.Select(x => x.ZoneCode).Contains(w.Code)).ToListAsync();

                foreach (var record in records)
                {
                    var worker = workers.FirstOrDefault(w => w.Code == record.WorkerCode);
                    if (worker == null)
                    {
                        errors.Add($"Worker Code '{record.WorkerCode}' does not exist.");
                        recordsToBeRemoved.Add(record);
                        continue;
                    }

                    var zone = zones.FirstOrDefault(z => z.Code == record.ZoneCode);
                    if (zone == null)
                    {
                        errors.Add($"Zone Code '{record.ZoneCode}' does not exist.");
                        recordsToBeRemoved.Add(record);
                        continue;
                    }

                    DateOnly.TryParseExact(record.AssignmentDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate);
                    // Check Duplicate Assignment in File
                    if (validAssignments.Any(a => a.WorkerId == worker.Id && a.ZoneId == zone.Id && a.AssignmentDate == parsedDate))
                    {
                        errors.Add($"Duplicate assignment in file for Worker '{record.WorkerCode}' on '{record.AssignmentDate}'.");
                        continue;
                    }

                    validAssignments.Add(new WorkerZoneAssignment
                    {
                        WorkerId = worker.Id,
                        ZoneId = zone.Id,
                        AssignmentDate = parsedDate,
                        Worker = worker,
                        Zone = zone
                    });
                }

                var existingAssignments = await _dbContext.WorkerZoneAssignments
                       .Where(a => validAssignments.Select(x => x.WorkerId).Contains(a.WorkerId) && validAssignments.Select(x => x.AssignmentDate).Contains(a.AssignmentDate)).ToListAsync();
                foreach (var assignment in validAssignments)
                {
                    var existingAssignment = existingAssignments
                       .Any(a => a.WorkerId == assignment.WorkerId && a.AssignmentDate == assignment.AssignmentDate);
                    if (existingAssignment)
                    {
                        errors.Add($"Worker '{assignment?.Worker?.Code}' already assigned on '{assignment?.AssignmentDate}'.");
                        continue;
                    }
                    uniqueAssignments.Add(assignment);
                }
                await _dbContext.BulkInsertAsync(uniqueAssignments);

                if (errors.Count != 0)
                    return BadRequest(new { Message = "Validation failed", Errors = errors });

                return Ok(new { Message = "File uploaded and processed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    public class WorkerZoneAssignmentRequest
    {
        [Name("worker_code")]
        public string? WorkerCode { get; set; }

        [Name("zone_code")]
        public string? ZoneCode { get; set; }

        [Name("assignment_date")]
        public string? AssignmentDate { get; set; }
    }
}

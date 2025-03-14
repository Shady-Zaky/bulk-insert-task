﻿// <auto-generated />
using System;
using Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.14")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Domain.Entities.Worker", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Code")
                        .HasColumnType("text")
                        .HasColumnName("code");

                    b.Property<string>("Name")
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.HasKey("Id");

                    b.ToTable("worker", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.WorkerZoneAssignment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateOnly>("AssignmentDate")
                        .HasColumnType("date")
                        .HasColumnName("effective_date");

                    b.Property<int>("WorkerId")
                        .HasColumnType("integer")
                        .HasColumnName("worker_id");

                    b.Property<int>("ZoneId")
                        .HasColumnType("integer")
                        .HasColumnName("zone_id");

                    b.HasKey("Id");

                    b.HasIndex("WorkerId");

                    b.HasIndex("ZoneId");

                    b.ToTable("worker_zone_assignment", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.Zone", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Code")
                        .HasColumnType("text")
                        .HasColumnName("code");

                    b.Property<string>("Name")
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.HasKey("Id");

                    b.ToTable("zone", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.WorkerZoneAssignment", b =>
                {
                    b.HasOne("Domain.Entities.Worker", "Worker")
                        .WithMany("WorkerZoneAssignments")
                        .HasForeignKey("WorkerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Zone", "Zone")
                        .WithMany("WorkerZoneAssignments")
                        .HasForeignKey("ZoneId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Worker");

                    b.Navigation("Zone");
                });

            modelBuilder.Entity("Domain.Entities.Worker", b =>
                {
                    b.Navigation("WorkerZoneAssignments");
                });

            modelBuilder.Entity("Domain.Entities.Zone", b =>
                {
                    b.Navigation("WorkerZoneAssignments");
                });
#pragma warning restore 612, 618
        }
    }
}

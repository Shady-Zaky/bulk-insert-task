drop table if exists worker_zone_assignment;
drop table if exists worker;
drop table if exists "zone";

-- Create tables
CREATE TABLE worker (
    id SERIAL PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    code VARCHAR(200) NOT NULL
);

CREATE TABLE zone (
    id SERIAL PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    code VARCHAR(200) NOT NULL
);

CREATE TABLE worker_zone_assignment (
    id SERIAL PRIMARY KEY,
    worker_id INT NOT NULL REFERENCES worker(id) ON DELETE CASCADE,
    zone_id INT NOT NULL REFERENCES zone(id) ON DELETE CASCADE,
    effective_date DATE NOT NULL
);

-- Insert 50K workers
INSERT INTO worker (name, code)
SELECT 'W' || g, 'W' || g
FROM generate_series(1, 50000) g;

-- Insert 1K zones
INSERT INTO zone (name, code)
SELECT 'Z' || g, 'Z' || g
FROM generate_series(1, 1000) g;

-- Insert 300K worker assignments ensuring unique worker-date combination
INSERT INTO worker_zone_assignment (worker_id, zone_id, effective_date)
SELECT DISTINCT ON (worker_id, effective_date)
    worker_id,
    zone_id,
    effective_date
FROM (
    SELECT 
        FLOOR(RANDOM() * 50000) + 1 AS worker_id,  -- Random worker ID
        FLOOR(RANDOM() * 1000) + 1 AS zone_id,  -- Random zone ID
        (CURRENT_DATE - (FLOOR(RANDOM() * 365)::int)) AS effective_date  -- Ensure integer subtraction
    FROM generate_series(1, 600000)  -- Generate more rows to ensure uniqueness
) subquery
LIMIT 300000;








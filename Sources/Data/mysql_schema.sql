-- ===========================================================================
--  LifeEngine schema (MySQL)
--
--  The MySQL build of schema.sql. Same tables, same columns, same views as the
--  SQLite original, so the program's Data layer can talk to either store.
--
--  What changed for MySQL:
--      * INTEGER PRIMARY KEY AUTOINCREMENT   -> BIGINT ... AUTO_INCREMENT
--      * TEXT / MEMO                          -> TEXT / VARCHAR, with lengths
--                                                where MySQL needs them for keys
--      * REAL                                 -> DOUBLE
--      * the INTEGER 0/1 flags                -> TINYINT(1)
--      * PRAGMA foreign_keys = ON;            -> not needed: InnoDB enforces
--                                                foreign keys already
--      * "" for a string literal in DDL       -> no: MySQL wants single quotes
--        when a DEFAULT is given for TEXT
--      * partial indexes on a prefix          -> a key length is always given so
--                                                the index fits InnoDB's limit
--      * rowid, without a declared key        -> cell and population already
--                                                had composite keys
--
--  The datetime columns are kept as VARCHAR rather than DATETIME, because the
--  program writes them itself in "yyyy-MM-dd HH:mm:ss" form and reads them back
--  as text; making them DATETIME would only add a conversion with no benefit.
--
--  Run it with:
--      mysql -u root -p < Sources/Data/mysql_schema.sql
--  or from a client, source it after choosing a database:
--      CREATE DATABASE lifeengine CHARACTER SET utf8mb4;
--      USE lifeengine;
--      SOURCE Sources/Data/mysql_schema.sql;
--
--  Tables
--      run            one session of the program
--      generation     one generation of the cellular grid, with its statistics
--      cell           a live cell within a generation
--      world          one generated planet
--      tile           one square of a world
--      species        one species registered in an ecosystem
--      population     the headcount of one species on one tile at one tick
--      wolf_pack      one pack
--      wolf           one wolf, alive or dead
--      hunt           one hunt, its prey and its outcome
--      schedule       one cron-style job and when it last and next runs
--      release        one build offered by the update channel
--      setting        one configuration entry and its value
--      script_run     one execution of a script, and what it printed
--      event          one line of the program's event log
-- ===========================================================================

-- InnoDB is the default storage engine and the only one that enforces the
-- foreign keys declared below; stating it avoids depending on the server's
-- default. No PRAGMA is needed to switch foreign key checking on.
SET NAMES utf8mb4;
SET sql_mode = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION';

-- ---------------------------------------------------------------------------
-- Runs and generations
-- ---------------------------------------------------------------------------

-- One session of the program. Everything else hangs off this, so a database can
-- hold many runs side by side and they can be compared.
CREATE TABLE IF NOT EXISTS run (
    run_id          BIGINT       NOT NULL AUTO_INCREMENT,
    started_at      VARCHAR(32)  NOT NULL,                -- ISO 8601, local time
    finished_at     VARCHAR(32)  NULL,                    -- NULL while running
    build           VARCHAR(32)  NOT NULL,                -- e.g. 2026.10.07-r3
    version         VARCHAR(32)  NOT NULL,                -- e.g. 1.1.0.0
    product         VARCHAR(64)  NOT NULL DEFAULT 'LifeEngine',
    developer       VARCHAR(64)  NOT NULL DEFAULT 'ArkansasIo',
    seed            BIGINT       NOT NULL,                -- the run's random seed
    rules           VARCHAR(32)  NOT NULL,                -- survive/birth
    grid_size       INT          NOT NULL,                -- cells per side
    speed           INT          NOT NULL,                -- 1 to 10
    note            TEXT         NULL,
    PRIMARY KEY (run_id),
    KEY ix_run_started (started_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- One generation of the grid, with the statistics that describe it. The cells
-- themselves are in the cell table, keyed on this row.
CREATE TABLE IF NOT EXISTS generation (
    generation_id   BIGINT       NOT NULL AUTO_INCREMENT,
    run_id          BIGINT       NOT NULL,
    number          INT          NOT NULL,                -- 0, 1, 2, ...
    population      INT          NOT NULL,                -- live cells
    density         DOUBLE       NOT NULL,                -- population / cells
    born            INT          NOT NULL DEFAULT 0,      -- cells that came alive
    died            INT          NOT NULL DEFAULT 0,      -- cells that died
    elapsed_ms      DOUBLE       NOT NULL DEFAULT 0,      -- time this step took
    recorded_at     VARCHAR(32)  NOT NULL,
    PRIMARY KEY (generation_id),
    UNIQUE KEY uq_generation_run (run_id, number),
    KEY ix_generation_run (run_id, number),
    CONSTRAINT fk_generation_run FOREIGN KEY (run_id)
        REFERENCES run (run_id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- A live cell. Only live cells are stored: an empty board is the absence of
-- rows, not a row per dead cell, which would be 62,500 rows per generation at
-- the largest grid size.
CREATE TABLE IF NOT EXISTS cell (
    generation_id   BIGINT       NOT NULL,
    x               INT          NOT NULL,
    y               INT          NOT NULL,
    alive           TINYINT(1)   NOT NULL DEFAULT 1,      -- 0 or 1
    ever_alive      TINYINT(1)   NOT NULL DEFAULT 1,      -- the history flag
    PRIMARY KEY (generation_id, x, y),
    CONSTRAINT fk_cell_generation FOREIGN KEY (generation_id)
        REFERENCES generation (generation_id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ---------------------------------------------------------------------------
-- Worlds and ecosystems
-- ---------------------------------------------------------------------------

-- One generated planet. The generation parameters are stored alongside it, so a
-- world can be reproduced from this row alone.
CREATE TABLE IF NOT EXISTS world (
    world_id        BIGINT       NOT NULL AUTO_INCREMENT,
    run_id          BIGINT       NULL,                    -- NULL if standalone
    name            VARCHAR(128) NOT NULL,
    seed            BIGINT       NOT NULL,
    columns         INT          NOT NULL,
    rows            INT          NOT NULL,
    sea_level       DOUBLE       NOT NULL DEFAULT 0.0,
    axial_tilt      DOUBLE       NOT NULL DEFAULT 23.44,
    day_length      DOUBLE       NOT NULL DEFAULT 24.0,
    gravity         DOUBLE       NOT NULL DEFAULT 9.81,
    land_fraction   DOUBLE       NOT NULL DEFAULT 0.0,
    mean_temp       DOUBLE       NOT NULL DEFAULT 0.0,
    mean_elevation  DOUBLE       NOT NULL DEFAULT 0.0,
    river_count     INT          NOT NULL DEFAULT 0,
    created_at      VARCHAR(32)  NOT NULL,
    PRIMARY KEY (world_id),
    KEY ix_world_seed (seed),
    CONSTRAINT fk_world_run FOREIGN KEY (run_id)
        REFERENCES run (run_id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- One square of a world. This is the bulk of the data: a 100x100 world is
-- 10,000 rows, and a 250x250 one is 62,500.
CREATE TABLE IF NOT EXISTS tile (
    world_id        BIGINT       NOT NULL,
    x               INT          NOT NULL,
    y               INT          NOT NULL,
    latitude        DOUBLE       NOT NULL,
    longitude       DOUBLE       NOT NULL,
    biome           VARCHAR(48)  NOT NULL,                -- BiomeType name
    climate_zone    VARCHAR(32)  NOT NULL,                -- ClimateZone name
    terrain         VARCHAR(32)  NOT NULL,                -- HabitatTerrain name
    zone            VARCHAR(32)  NOT NULL,                -- ZoneType name
    temperature     DOUBLE       NOT NULL,                -- Celsius
    rainfall        DOUBLE       NOT NULL,                -- millimetres a year
    elevation       DOUBLE       NOT NULL,                -- metres above sea level
    has_river       TINYINT(1)   NOT NULL DEFAULT 0,
    is_coastal      TINYINT(1)   NOT NULL DEFAULT 0,
    is_water        TINYINT(1)   NOT NULL DEFAULT 0,
    PRIMARY KEY (world_id, x, y),
    KEY ix_tile_biome (world_id, biome),
    CONSTRAINT fk_tile_world FOREIGN KEY (world_id)
        REFERENCES world (world_id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- One species registered in an ecosystem, with the habitat envelope that
-- decides where it can live. These are the columns the Species class exposes.
CREATE TABLE IF NOT EXISTS species (
    species_id      BIGINT       NOT NULL AUTO_INCREMENT,
    world_id        BIGINT       NOT NULL,
    name            VARCHAR(128) NOT NULL,
    kingdom         VARCHAR(32)  NOT NULL,                -- Kingdom name
    diet            VARCHAR(32)  NOT NULL,                -- DietType name
    locomotion      VARCHAR(32)  NOT NULL,                -- LocomotionType name
    trophic_level   VARCHAR(32)  NOT NULL,                -- TrophicLevel name
    symbol          VARCHAR(4)   NOT NULL,                -- one character
    colour_key      VARCHAR(32)  NOT NULL,
    min_temp        DOUBLE       NOT NULL,
    max_temp        DOUBLE       NOT NULL,
    min_rainfall    DOUBLE       NOT NULL,
    max_rainfall    DOUBLE       NOT NULL,
    max_elevation   DOUBLE       NOT NULL,
    needs_water     TINYINT(1)   NOT NULL DEFAULT 0,
    needs_land      TINYINT(1)   NOT NULL DEFAULT 0,
    needs_coast     TINYINT(1)   NOT NULL DEFAULT 0,
    avoids_desert   TINYINT(1)   NOT NULL DEFAULT 0,
    avoids_ice      TINYINT(1)   NOT NULL DEFAULT 0,
    growth_rate     DOUBLE       NOT NULL,
    death_rate      DOUBLE       NOT NULL,
    max_population  INT          NOT NULL,
    seed_population INT          NOT NULL,
    PRIMARY KEY (species_id),
    UNIQUE KEY uq_species_world_name (world_id, name),
    CONSTRAINT fk_species_world FOREIGN KEY (world_id)
        REFERENCES world (world_id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- The headcount of one species on one tile at one tick. Only rows with a
-- population above zero are written.
CREATE TABLE IF NOT EXISTS population (
    world_id        BIGINT       NOT NULL,
    tick            INT          NOT NULL,
    species_id      BIGINT       NOT NULL,
    x               INT          NOT NULL,
    y               INT          NOT NULL,
    head            INT          NOT NULL,
    PRIMARY KEY (world_id, tick, species_id, x, y),
    KEY ix_population_species (species_id, tick),
    CONSTRAINT fk_population_world FOREIGN KEY (world_id)
        REFERENCES world (world_id) ON DELETE CASCADE,
    CONSTRAINT fk_population_species FOREIGN KEY (species_id)
        REFERENCES species (species_id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ---------------------------------------------------------------------------
-- Wolves
-- ---------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS wolf_pack (
    pack_id         BIGINT       NOT NULL AUTO_INCREMENT,
    run_id          BIGINT       NULL,
    name            VARCHAR(128) NOT NULL,
    territory       VARCHAR(128) NOT NULL,
    founded_at      VARCHAR(32)  NOT NULL,
    alpha_name      VARCHAR(64)  NULL,
    cohesion        DOUBLE       NOT NULL DEFAULT 0.5,
    alpha_changes   INT          NOT NULL DEFAULT 0,
    PRIMARY KEY (pack_id),
    CONSTRAINT fk_wolf_pack_run FOREIGN KEY (run_id)
        REFERENCES run (run_id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- One wolf. Wolves are never deleted when they die: is_alive goes to 0 and the
-- row stays, because a pack's history is the point of recording it.
CREATE TABLE IF NOT EXISTS wolf (
    wolf_id         BIGINT       NOT NULL AUTO_INCREMENT,
    pack_id         BIGINT       NOT NULL,
    name            VARCHAR(64)  NOT NULL,
    rank            VARCHAR(16)  NOT NULL,                -- PackRank name
    age_years       DOUBLE       NOT NULL DEFAULT 0.0,
    mass_kg         DOUBLE       NOT NULL DEFAULT 40.0,
    health          DOUBLE       NOT NULL DEFAULT 1.0,
    energy          DOUBLE       NOT NULL DEFAULT 1.0,
    is_alive        TINYINT(1)   NOT NULL DEFAULT 1,
    is_cub          TINYINT(1)   NOT NULL DEFAULT 1,
    is_elder        TINYINT(1)   NOT NULL DEFAULT 0,
    strength        DOUBLE       NOT NULL,
    speed           DOUBLE       NOT NULL,
    aggression      DOUBLE       NOT NULL,
    hearing         DOUBLE       NOT NULL,
    scent           DOUBLE       NOT NULL,
    night_sight     DOUBLE       NOT NULL,
    healing         DOUBLE       NOT NULL,
    human_retention DOUBLE       NOT NULL,
    is_cursed       TINYINT(1)   NOT NULL DEFAULT 1,
    is_shapeshifter TINYINT(1)   NOT NULL DEFAULT 1,
    scar_count      INT          NOT NULL DEFAULT 0,
    kills           INT          NOT NULL DEFAULT 0,
    dominance_wins  INT          NOT NULL DEFAULT 0,
    dominance_losses INT         NOT NULL DEFAULT 0,
    mother_id       BIGINT       NULL,
    father_id       BIGINT       NULL,
    PRIMARY KEY (wolf_id),
    KEY ix_wolf_pack (pack_id, is_alive),
    CONSTRAINT fk_wolf_pack FOREIGN KEY (pack_id)
        REFERENCES wolf_pack (pack_id) ON DELETE CASCADE,
    CONSTRAINT fk_wolf_mother FOREIGN KEY (mother_id)
        REFERENCES wolf (wolf_id) ON DELETE SET NULL,
    CONSTRAINT fk_wolf_father FOREIGN KEY (father_id)
        REFERENCES wolf (wolf_id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- One hunt: what was chased, what it cost, and who ate.
CREATE TABLE IF NOT EXISTS hunt (
    hunt_id         BIGINT       NOT NULL AUTO_INCREMENT,
    pack_id         BIGINT       NOT NULL,
    occurred_at     VARCHAR(32)  NOT NULL,
    is_night        TINYINT(1)   NOT NULL DEFAULT 1,
    is_full_moon    TINYINT(1)   NOT NULL DEFAULT 0,
    prey_name       VARCHAR(128) NOT NULL,
    succeeded       TINYINT(1)   NOT NULL,
    taken           INT          NOT NULL DEFAULT 0,
    meat_kg         DOUBLE       NOT NULL DEFAULT 0.0,
    hunter_count    INT          NOT NULL DEFAULT 0,
    wounded_count   INT          NOT NULL DEFAULT 0,
    narrative       TEXT         NULL,
    PRIMARY KEY (hunt_id),
    KEY ix_hunt_pack (pack_id, occurred_at),
    CONSTRAINT fk_hunt_pack FOREIGN KEY (pack_id)
        REFERENCES wolf_pack (pack_id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ---------------------------------------------------------------------------
-- Configuration, scheduling, updates and scripts
-- ---------------------------------------------------------------------------

-- The settings as they stood at the end of a run. Written as key and value
-- pairs rather than as columns, so a setting added later needs no migration.
-- "key" is a reserved word in MySQL, so it is quoted with backticks throughout;
-- the column name itself is unchanged so the program's SQL still matches.
CREATE TABLE IF NOT EXISTS setting (
    setting_id      BIGINT       NOT NULL AUTO_INCREMENT,
    run_id          BIGINT       NULL,
    `key`           VARCHAR(128) NOT NULL,
    value           VARCHAR(255) NOT NULL,
    default_value   VARCHAR(255) NOT NULL,
    type_name       VARCHAR(32)  NOT NULL,
    category        VARCHAR(64)  NOT NULL,
    description     TEXT         NULL,
    is_modified     TINYINT(1)   NOT NULL DEFAULT 0,
    PRIMARY KEY (setting_id),
    UNIQUE KEY uq_setting_run_key (run_id, `key`),
    KEY ix_setting_key (`key`),
    CONSTRAINT fk_setting_run FOREIGN KEY (run_id)
        REFERENCES run (run_id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS schedule (
    schedule_id     BIGINT       NOT NULL AUTO_INCREMENT,
    name            VARCHAR(128) NOT NULL,
    action          VARCHAR(32)  NOT NULL,                -- ScheduleAction name
    repeat_mode     VARCHAR(32)  NOT NULL,                -- ScheduleRepeat name
    interval_text   VARCHAR(32)  NULL,
    enabled         TINYINT(1)   NOT NULL DEFAULT 1,
    runs            INT          NOT NULL DEFAULT 0,
    max_runs        INT          NOT NULL DEFAULT 0,
    last_run        VARCHAR(32)  NULL,
    next_run        VARCHAR(32)  NULL,
    description     TEXT         NULL,
    PRIMARY KEY (schedule_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS release (
    release_id      BIGINT       NOT NULL AUTO_INCREMENT,
    build           VARCHAR(32)  NOT NULL,
    version         VARCHAR(32)  NOT NULL,
    name            VARCHAR(128) NULL,
    developer       VARCHAR(64)  NULL,
    released_on     VARCHAR(32)  NULL,
    kind            VARCHAR(16)  NOT NULL,                -- ReleaseKind name
    minimum_version VARCHAR(32)  NULL,
    path            VARCHAR(255) NULL,
    url             VARCHAR(255) NULL,
    sha256          VARCHAR(80)  NULL,
    size_bytes      BIGINT       NOT NULL DEFAULT 0,
    notes           TEXT         NULL,
    PRIMARY KEY (release_id),
    UNIQUE KEY uq_release_build (build)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS script_run (
    script_run_id   BIGINT       NOT NULL AUTO_INCREMENT,
    run_id          BIGINT       NULL,
    name            VARCHAR(128) NOT NULL,
    succeeded       TINYINT(1)   NOT NULL,
    error           TEXT         NULL,
    output          TEXT         NULL,
    steps           INT          NOT NULL DEFAULT 0,
    elapsed_ms      DOUBLE       NOT NULL DEFAULT 0.0,
    ran_at          VARCHAR(32)  NOT NULL,
    PRIMARY KEY (script_run_id),
    CONSTRAINT fk_script_run_run FOREIGN KEY (run_id)
        REFERENCES run (run_id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS event (
    event_id        BIGINT       NOT NULL AUTO_INCREMENT,
    run_id          BIGINT       NULL,
    logged_at       VARCHAR(32)  NOT NULL,
    message         TEXT         NOT NULL,
    PRIMARY KEY (event_id),
    KEY ix_event_run (run_id, logged_at),
    CONSTRAINT fk_event_run FOREIGN KEY (run_id)
        REFERENCES run (run_id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ---------------------------------------------------------------------------
-- Views, for the reports and the Excel exports
-- ---------------------------------------------------------------------------

-- The shape of a run, generation by generation, with the change from the
-- previous generation worked out. This is what a population graph plots.
CREATE OR REPLACE VIEW v_generation_growth AS
SELECT
    g.run_id,
    g.number,
    g.population,
    g.density,
    g.born,
    g.died,
    g.elapsed_ms,
    g.population - COALESCE(
        (SELECT p.population FROM generation p
          WHERE p.run_id = g.run_id AND p.number = g.number - 1), 0) AS `change`
FROM generation g;

-- How much of each biome a world has, and how many species live on it.
CREATE OR REPLACE VIEW v_biome_census AS
SELECT
    w.world_id,
    w.name                AS world_name,
    t.biome,
    COUNT(*)              AS tiles,
    ROUND(AVG(t.temperature), 2) AS mean_temp,
    ROUND(AVG(t.rainfall), 1)    AS mean_rainfall,
    ROUND(AVG(t.elevation), 1)   AS mean_elevation
FROM tile t
JOIN world w ON w.world_id = t.world_id
GROUP BY w.world_id, w.name, t.biome;

-- The final headcount of every species in every world. MySQL's only group-by
-- mode now is full grouping, so every selected column is in the GROUP BY, which
-- is what the SQLite original relied on being permitted anyway.
CREATE OR REPLACE VIEW v_species_totals AS
SELECT
    s.species_id,
    s.world_id,
    s.name,
    s.trophic_level,
    COALESCE(SUM(p.head), 0) AS head,
    COUNT(DISTINCT CONCAT(p.x, ',', p.y)) AS occupied_tiles,
    MAX(p.tick) AS last_tick
FROM species s
LEFT JOIN population p ON p.species_id = s.species_id
GROUP BY s.species_id, s.world_id, s.name, s.trophic_level;

-- The pack order, strongest first, which is the order the program uses.
CREATE OR REPLACE VIEW v_pack_order AS
SELECT
    w.wolf_id,
    w.pack_id,
    p.name AS pack_name,
    w.name,
    w.rank,
    w.age_years,
    w.health,
    w.energy,
    w.kills,
    w.dominance_wins,
    w.dominance_losses,
    ROUND(w.strength * 0.5 + w.aggression * 0.25 + w.speed * 0.15 +
          w.scar_count * 0.01, 4) AS dominance,
    w.is_alive
FROM wolf w
JOIN wolf_pack p ON p.pack_id = w.pack_id
ORDER BY w.pack_id, dominance DESC;

-- How the hunts have gone.
CREATE OR REPLACE VIEW v_hunt_summary AS
SELECT
    h.pack_id,
    p.name                AS pack_name,
    COUNT(*)              AS hunts,
    SUM(h.succeeded)      AS successes,
    ROUND(100.0 * SUM(h.succeeded) / COUNT(*), 1) AS success_percent,
    SUM(h.taken)          AS animals_taken,
    ROUND(SUM(h.meat_kg), 1) AS meat_kg,
    SUM(h.wounded_count)  AS wounded
FROM hunt h
JOIN wolf_pack p ON p.pack_id = h.pack_id
GROUP BY h.pack_id, p.name;

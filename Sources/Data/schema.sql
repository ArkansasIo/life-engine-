-- ===========================================================================
--  LifeEngine schema
--
--  Creates the database the program writes its runs into, and that the Excel
--  exports are read back from.
--
--  Written for SQLite: the whole point is a single file the program can carry
--  around and that the reporting layer can read without a server. Everything
--  here is plain SQL-92 except the two pragmas and AUTOINCREMENT, which are
--  noted where they appear and are the only lines that need changing for
--  SQL Server or PostgreSQL.
--
--  Run it with:
--      sqlite3 LifeEngine.db < Sources/Data/schema.sql
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

PRAGMA foreign_keys = ON;

-- ---------------------------------------------------------------------------
-- Runs and generations
-- ---------------------------------------------------------------------------

-- One session of the program. Everything else hangs off this, so a database can
-- hold many runs side by side and they can be compared.
CREATE TABLE IF NOT EXISTS run (
    run_id          INTEGER PRIMARY KEY AUTOINCREMENT,   -- SQLite: AUTOINCREMENT
    started_at      TEXT    NOT NULL,                    -- ISO 8601, local time
    finished_at     TEXT,                                -- NULL while running
    build           TEXT    NOT NULL,                    -- e.g. 2026.10.07-r3
    version         TEXT    NOT NULL,                    -- e.g. 1.1.0.0
    product         TEXT    NOT NULL DEFAULT 'LifeEngine',
    developer       TEXT    NOT NULL DEFAULT 'ArkansasIo',
    seed            INTEGER NOT NULL,                    -- the run's random seed
    rules           TEXT    NOT NULL,                    -- survive/birth
    grid_size       INTEGER NOT NULL,                    -- cells per side
    speed           INTEGER NOT NULL,                    -- 1 to 10
    note            TEXT
);

CREATE INDEX IF NOT EXISTS ix_run_started ON run (started_at);

-- One generation of the grid, with the statistics that describe it. The cells
-- themselves are in the cell table, keyed on this row.
CREATE TABLE IF NOT EXISTS generation (
    generation_id   INTEGER PRIMARY KEY AUTOINCREMENT,
    run_id          INTEGER NOT NULL,
    number          INTEGER NOT NULL,                    -- 0, 1, 2, ...
    population      INTEGER NOT NULL,                    -- live cells
    density         REAL    NOT NULL,                    -- population / cells
    born            INTEGER NOT NULL DEFAULT 0,          -- cells that came alive
    died            INTEGER NOT NULL DEFAULT 0,          -- cells that died
    elapsed_ms      REAL    NOT NULL DEFAULT 0,          -- time this step took
    recorded_at     TEXT    NOT NULL,

    FOREIGN KEY (run_id) REFERENCES run (run_id) ON DELETE CASCADE,
    UNIQUE (run_id, number)
);

CREATE INDEX IF NOT EXISTS ix_generation_run ON generation (run_id, number);

-- A live cell. Only live cells are stored: an empty board is the absence of
-- rows, not a row per dead cell, which would be 62,500 rows per generation at
-- the largest grid size.
CREATE TABLE IF NOT EXISTS cell (
    generation_id   INTEGER NOT NULL,
    x               INTEGER NOT NULL,
    y               INTEGER NOT NULL,
    alive           INTEGER NOT NULL DEFAULT 1,          -- 0 or 1
    ever_alive      INTEGER NOT NULL DEFAULT 1,          -- the history flag

    PRIMARY KEY (generation_id, x, y),
    FOREIGN KEY (generation_id) REFERENCES generation (generation_id) ON DELETE CASCADE
);

-- ---------------------------------------------------------------------------
-- Worlds and ecosystems
-- ---------------------------------------------------------------------------

-- One generated planet. The generation parameters are stored alongside it, so a
-- world can be reproduced from this row alone.
CREATE TABLE IF NOT EXISTS world (
    world_id        INTEGER PRIMARY KEY AUTOINCREMENT,
    run_id          INTEGER,                             -- NULL if standalone
    name            TEXT    NOT NULL,
    seed            INTEGER NOT NULL,
    columns         INTEGER NOT NULL,
    rows            INTEGER NOT NULL,
    sea_level       REAL    NOT NULL DEFAULT 0.0,
    axial_tilt      REAL    NOT NULL DEFAULT 23.44,
    day_length      REAL    NOT NULL DEFAULT 24.0,
    gravity         REAL    NOT NULL DEFAULT 9.81,
    land_fraction   REAL    NOT NULL DEFAULT 0.0,
    mean_temp       REAL    NOT NULL DEFAULT 0.0,
    mean_elevation  REAL    NOT NULL DEFAULT 0.0,
    river_count     INTEGER NOT NULL DEFAULT 0,
    created_at      TEXT    NOT NULL,

    FOREIGN KEY (run_id) REFERENCES run (run_id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS ix_world_seed ON world (seed);

-- One square of a world. This is the bulk of the data: a 100x100 world is
-- 10,000 rows, and a 250x250 one is 62,500.
CREATE TABLE IF NOT EXISTS tile (
    world_id        INTEGER NOT NULL,
    x               INTEGER NOT NULL,
    y               INTEGER NOT NULL,
    latitude        REAL    NOT NULL,
    longitude       REAL    NOT NULL,
    biome           TEXT    NOT NULL,                    -- BiomeType name
    climate_zone    TEXT    NOT NULL,                    -- ClimateZone name
    terrain         TEXT    NOT NULL,                    -- HabitatTerrain name
    zone            TEXT    NOT NULL,                    -- ZoneType name
    temperature     REAL    NOT NULL,                    -- Celsius
    rainfall        REAL    NOT NULL,                    -- millimetres a year
    elevation       REAL    NOT NULL,                    -- metres above sea level
    has_river       INTEGER NOT NULL DEFAULT 0,
    is_coastal      INTEGER NOT NULL DEFAULT 0,
    is_water        INTEGER NOT NULL DEFAULT 0,

    PRIMARY KEY (world_id, x, y),
    FOREIGN KEY (world_id) REFERENCES world (world_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_tile_biome ON tile (world_id, biome);

-- One species registered in an ecosystem, with the habitat envelope that
-- decides where it can live. These are the columns the Species class exposes.
CREATE TABLE IF NOT EXISTS species (
    species_id      INTEGER PRIMARY KEY AUTOINCREMENT,
    world_id        INTEGER NOT NULL,
    name            TEXT    NOT NULL,
    kingdom         TEXT    NOT NULL,                    -- Kingdom name
    diet            TEXT    NOT NULL,                    -- DietType name
    locomotion      TEXT    NOT NULL,                    -- LocomotionType name
    trophic_level   TEXT    NOT NULL,                    -- TrophicLevel name
    symbol          TEXT    NOT NULL,                    -- one character
    colour_key      TEXT    NOT NULL,
    min_temp        REAL    NOT NULL,
    max_temp        REAL    NOT NULL,
    min_rainfall    REAL    NOT NULL,
    max_rainfall    REAL    NOT NULL,
    max_elevation   REAL    NOT NULL,
    needs_water     INTEGER NOT NULL DEFAULT 0,
    needs_land      INTEGER NOT NULL DEFAULT 0,
    needs_coast     INTEGER NOT NULL DEFAULT 0,
    avoids_desert   INTEGER NOT NULL DEFAULT 0,
    avoids_ice      INTEGER NOT NULL DEFAULT 0,
    growth_rate     REAL    NOT NULL,
    death_rate      REAL    NOT NULL,
    max_population  INTEGER NOT NULL,
    seed_population INTEGER NOT NULL,

    FOREIGN KEY (world_id) REFERENCES world (world_id) ON DELETE CASCADE,
    UNIQUE (world_id, name)
);

-- The headcount of one species on one tile at one tick. Only rows with a
-- population above zero are written.
CREATE TABLE IF NOT EXISTS population (
    world_id        INTEGER NOT NULL,
    tick            INTEGER NOT NULL,
    species_id      INTEGER NOT NULL,
    x               INTEGER NOT NULL,
    y               INTEGER NOT NULL,
    head            INTEGER NOT NULL,

    PRIMARY KEY (world_id, tick, species_id, x, y),
    FOREIGN KEY (world_id) REFERENCES world (world_id) ON DELETE CASCADE,
    FOREIGN KEY (species_id) REFERENCES species (species_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_population_species ON population (species_id, tick);

-- ---------------------------------------------------------------------------
-- Wolves
-- ---------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS wolf_pack (
    pack_id         INTEGER PRIMARY KEY AUTOINCREMENT,
    run_id          INTEGER,
    name            TEXT    NOT NULL,
    territory       TEXT    NOT NULL,
    founded_at      TEXT    NOT NULL,
    alpha_name      TEXT,
    cohesion        REAL    NOT NULL DEFAULT 0.5,
    alpha_changes   INTEGER NOT NULL DEFAULT 0,

    FOREIGN KEY (run_id) REFERENCES run (run_id) ON DELETE SET NULL
);

-- One wolf. Wolves are never deleted when they die: is_alive goes to 0 and the
-- row stays, because a pack's history is the point of recording it.
CREATE TABLE IF NOT EXISTS wolf (
    wolf_id         INTEGER PRIMARY KEY AUTOINCREMENT,
    pack_id         INTEGER NOT NULL,
    name            TEXT    NOT NULL,
    rank            TEXT    NOT NULL,                    -- PackRank name
    age_years       REAL    NOT NULL DEFAULT 0.0,
    mass_kg         REAL    NOT NULL DEFAULT 40.0,
    health          REAL    NOT NULL DEFAULT 1.0,
    energy          REAL    NOT NULL DEFAULT 1.0,
    is_alive        INTEGER NOT NULL DEFAULT 1,
    is_cub          INTEGER NOT NULL DEFAULT 1,
    is_elder        INTEGER NOT NULL DEFAULT 0,
    strength        REAL    NOT NULL,
    speed           REAL    NOT NULL,
    aggression      REAL    NOT NULL,
    hearing         REAL    NOT NULL,
    scent           REAL    NOT NULL,
    night_sight     REAL    NOT NULL,
    healing         REAL    NOT NULL,
    human_retention REAL    NOT NULL,
    is_cursed       INTEGER NOT NULL DEFAULT 1,
    is_shapeshifter INTEGER NOT NULL DEFAULT 1,
    scar_count      INTEGER NOT NULL DEFAULT 0,
    kills           INTEGER NOT NULL DEFAULT 0,
    dominance_wins  INTEGER NOT NULL DEFAULT 0,
    dominance_losses INTEGER NOT NULL DEFAULT 0,
    mother_id       INTEGER,
    father_id       INTEGER,

    FOREIGN KEY (pack_id) REFERENCES wolf_pack (pack_id) ON DELETE CASCADE,
    FOREIGN KEY (mother_id) REFERENCES wolf (wolf_id) ON DELETE SET NULL,
    FOREIGN KEY (father_id) REFERENCES wolf (wolf_id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS ix_wolf_pack ON wolf (pack_id, is_alive);

-- One hunt: what was chased, what it cost, and who ate.
CREATE TABLE IF NOT EXISTS hunt (
    hunt_id         INTEGER PRIMARY KEY AUTOINCREMENT,
    pack_id         INTEGER NOT NULL,
    occurred_at     TEXT    NOT NULL,
    is_night        INTEGER NOT NULL DEFAULT 1,
    is_full_moon    INTEGER NOT NULL DEFAULT 0,
    prey_name       TEXT    NOT NULL,
    succeeded       INTEGER NOT NULL,
    taken           INTEGER NOT NULL DEFAULT 0,
    meat_kg         REAL    NOT NULL DEFAULT 0.0,
    hunter_count    INTEGER NOT NULL DEFAULT 0,
    wounded_count   INTEGER NOT NULL DEFAULT 0,
    narrative       TEXT,

    FOREIGN KEY (pack_id) REFERENCES wolf_pack (pack_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_hunt_pack ON hunt (pack_id, occurred_at);

-- ---------------------------------------------------------------------------
-- Configuration, scheduling, updates and scripts
-- ---------------------------------------------------------------------------

-- The settings as they stood at the end of a run. Written as key and value
-- pairs rather than as columns, so a setting added later needs no migration.
CREATE TABLE IF NOT EXISTS setting (
    setting_id      INTEGER PRIMARY KEY AUTOINCREMENT,
    run_id          INTEGER,
    key             TEXT    NOT NULL,
    value           TEXT    NOT NULL,
    default_value   TEXT    NOT NULL,
    type_name       TEXT    NOT NULL,
    category        TEXT    NOT NULL,
    description     TEXT,
    is_modified     INTEGER NOT NULL DEFAULT 0,

    FOREIGN KEY (run_id) REFERENCES run (run_id) ON DELETE CASCADE,
    UNIQUE (run_id, key)
);

CREATE INDEX IF NOT EXISTS ix_setting_key ON setting (key);

CREATE TABLE IF NOT EXISTS schedule (
    schedule_id     INTEGER PRIMARY KEY AUTOINCREMENT,
    name            TEXT    NOT NULL,
    action          TEXT    NOT NULL,                    -- ScheduleAction name
    repeat_mode     TEXT    NOT NULL,                    -- ScheduleRepeat name
    interval_text   TEXT,
    enabled         INTEGER NOT NULL DEFAULT 1,
    runs            INTEGER NOT NULL DEFAULT 0,
    max_runs        INTEGER NOT NULL DEFAULT 0,
    last_run        TEXT,
    next_run        TEXT,
    description     TEXT
);

CREATE TABLE IF NOT EXISTS release (
    release_id      INTEGER PRIMARY KEY AUTOINCREMENT,
    build           TEXT    NOT NULL UNIQUE,
    version         TEXT    NOT NULL,
    name            TEXT,
    developer       TEXT,
    released_on     TEXT,
    kind            TEXT    NOT NULL,                    -- ReleaseKind name
    minimum_version TEXT,
    path            TEXT,
    url             TEXT,
    sha256          TEXT,
    size_bytes      INTEGER NOT NULL DEFAULT 0,
    notes           TEXT
);

CREATE TABLE IF NOT EXISTS script_run (
    script_run_id   INTEGER PRIMARY KEY AUTOINCREMENT,
    run_id          INTEGER,
    name            TEXT    NOT NULL,
    succeeded       INTEGER NOT NULL,
    error           TEXT,
    output          TEXT,
    steps           INTEGER NOT NULL DEFAULT 0,
    elapsed_ms      REAL    NOT NULL DEFAULT 0.0,
    ran_at          TEXT    NOT NULL,

    FOREIGN KEY (run_id) REFERENCES run (run_id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS event (
    event_id        INTEGER PRIMARY KEY AUTOINCREMENT,
    run_id          INTEGER,
    logged_at       TEXT    NOT NULL,
    message         TEXT    NOT NULL,

    FOREIGN KEY (run_id) REFERENCES run (run_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_event_run ON event (run_id, logged_at);

-- ---------------------------------------------------------------------------
-- Views, for the reports and the Excel exports
-- ---------------------------------------------------------------------------

-- The shape of a run, generation by generation, with the change from the
-- previous generation worked out. This is what a population graph plots.
DROP VIEW IF EXISTS v_generation_growth;

CREATE VIEW v_generation_growth AS
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
          WHERE p.run_id = g.run_id AND p.number = g.number - 1), 0) AS change
FROM generation g;

-- How much of each biome a world has, and how many species live on it.
DROP VIEW IF EXISTS v_biome_census;

CREATE VIEW v_biome_census AS
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

-- The final headcount of every species in every world.
DROP VIEW IF EXISTS v_species_totals;

CREATE VIEW v_species_totals AS
SELECT
    s.species_id,
    s.world_id,
    s.name,
    s.trophic_level,
    COALESCE(SUM(p.head), 0) AS head,
    COUNT(DISTINCT p.x || ',' || p.y) AS occupied_tiles,
    MAX(p.tick) AS last_tick
FROM species s
LEFT JOIN population p ON p.species_id = s.species_id
GROUP BY s.species_id, s.world_id, s.name, s.trophic_level;

-- The pack order, strongest first, which is the order the program uses.
DROP VIEW IF EXISTS v_pack_order;

CREATE VIEW v_pack_order AS
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
DROP VIEW IF EXISTS v_hunt_summary;

CREATE VIEW v_hunt_summary AS
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

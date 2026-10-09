-- ===========================================================================
--  LifeEngine seed data (MySQL)
--
--  The MySQL build of seed.sql. Same reference rows and the same worked example
--  as the SQLite original.
--
--  What changed for MySQL:
--      * INSERT OR IGNORE          -> INSERT IGNORE
--      * "||" string concatenation -> CONCAT()
--      * char(10)                  -> CHAR(10), unchanged
--      * CREATE VIEW ... AS SELECT
--        without OR REPLACE        -> CREATE OR REPLACE VIEW
--      * a view's "rule" column    -> quoted with backticks, as it is reserved
--
--  Run it after the schema:
--      mysql -u root -p LifeEngine < Sources/Data/mysql_seed.sql
--
--  Every insert is INSERT IGNORE, so running it twice is harmless and it will
--  not overwrite a real run.
-- ===========================================================================

SET NAMES utf8mb4;

-- ---------------------------------------------------------------------------
-- The rulesets the program ships with
-- ---------------------------------------------------------------------------

-- There is no ruleset table: the list lives in FormLife.AddRulesToComboBox.
-- This view exposes it for reporting, so a query can name a rule by its code
-- without the caller having to remember the description.
CREATE OR REPLACE VIEW v_ruleset AS
SELECT '23/3'          AS `rule`, 'Conway''s Life'   AS name UNION ALL
SELECT '125/36',        '2x2'                      UNION ALL
SELECT '34/34',         '34 Life'                  UNION ALL
SELECT '1358/357',      'Amoeba'                   UNION ALL
SELECT '35678/4678',    'Anneal'                   UNION ALL
SELECT '4567/345',      'Assimilation'             UNION ALL
SELECT '235678/378',    'Coagulations'             UNION ALL
SELECT '45678/3',       'Coral'                    UNION ALL
SELECT '34678/3678',    'Day & Night'              UNION ALL
SELECT '5678/35678',    'Diamoeba'                 UNION ALL
SELECT '012345678/3',   'Flakes'                   UNION ALL
SELECT '1/1',           'Gnarl'                    UNION ALL
SELECT '23/36',         'HighLife'                 UNION ALL
SELECT '34678/0123478', 'Inverse Life'             UNION ALL
SELECT '5/345',         'Long Life'                UNION ALL
SELECT '12345/3',       'Maze'                     UNION ALL
SELECT '1234/3',        'Mazectric'                UNION ALL
SELECT '245/368',       'Move'                     UNION ALL
SELECT '238/357',       'Pseudo Life'              UNION ALL
SELECT '1357/1357',     'Replicator'               UNION ALL
SELECT '/2',            'Seeds'                    UNION ALL
SELECT '/234',          'Serviettes'               UNION ALL
SELECT '235678/3678',   'Stains'                   UNION ALL
SELECT '2345/45678',    'Walled cities';

-- ---------------------------------------------------------------------------
-- The releases the updater offers
-- ---------------------------------------------------------------------------

INSERT IGNORE INTO release
    (build, version, name, developer, released_on, kind, minimum_version,
     path, url, sha256, size_bytes, notes)
VALUES
    ('2026.10.07-r3', '1.1.0.0', 'Rivers and Rain', 'ArkansasIo', '2026-10-07',
     'Patch', '1.0.0.0',
     'updates/LifeEngine-1.1.0.0-patch.zip',
     'https://example.invalid/lifeengine/2026.10.07-r3/LifeEngine-patch.zip',
     '0000000000000000000000000000000000000000000000000000000000000000',
     1482752,
     'Adds the world map, the ecosystems that live on it and the animal catalogue.');

INSERT IGNORE INTO release
    (build, version, name, developer, released_on, kind, minimum_version,
     path, sha256, size_bytes, notes)
VALUES
    ('2026.09.21-r1', '1.0.1.0', 'Interface Rebuild', 'ArkansasIo', '2026-09-21',
     'Patch', '1.0.0.0',
     'updates/LifeEngine-1.0.1.0-patch.zip',
     '0000000000000000000000000000000000000000000000000000000000000000',
     524288,
     'Rebuilds the interface around a ribbon menu, a sidebar and a status bar.');

INSERT IGNORE INTO release
    (build, version, name, developer, released_on, kind,
     path, sha256, size_bytes, notes)
VALUES
    ('2026.09.01-r1', '1.0.0.0', 'Project Restart', 'ArkansasIo', '2026-09-01',
     'Full',
     'updates/LifeEngine-1.0.0.0-full.zip',
     '0000000000000000000000000000000000000000000000000000000000000000',
     8388608,
     'First ArkansasIo build: the Conway engine with the biological model underneath.');

-- ---------------------------------------------------------------------------
-- The default scheduled jobs
-- ---------------------------------------------------------------------------

INSERT IGNORE INTO schedule
    (name, action, repeat_mode, interval_text, enabled, max_runs, description)
VALUES
    ('hourly snapshot', 'Snapshot', 'Interval', '1h', 1, 0,
     'Write the state of the world and the pack to the log.');

INSERT IGNORE INTO schedule
    (name, action, repeat_mode, interval_text, enabled, max_runs, description)
VALUES
    ('flush log', 'FlushLog', 'Interval', '15m', 1, 0,
     'Trim the event log so it does not grow without limit.');

INSERT IGNORE INTO schedule
    (name, action, repeat_mode, interval_text, enabled, max_runs, description)
VALUES
    ('check for updates', 'CheckForUpdates', 'Daily', NULL, 1, 0,
     'Look for a newer build on the update channel.');

-- ---------------------------------------------------------------------------
-- The settings, at their defaults
-- ---------------------------------------------------------------------------

INSERT IGNORE INTO setting
    (run_id, `key`, value, default_value, type_name, category, description, is_modified)
VALUES
    (NULL, 'Application.Name',            'LifeEngine',      'LifeEngine',      'string', 'Application', 'Name shown in the title bar.', 0),
    (NULL, 'Application.Developer',       'ArkansasIo',      'ArkansasIo',      'string', 'Application', 'Who builds this program.', 0),
    (NULL, 'Application.UpdateChannel',   'stable',          'stable',          'string', 'Application', 'Channel updates come from.', 0),
    (NULL, 'Application.CheckForUpdates', 'true',            'true',            'bool',   'Application', 'Look for a newer build on start-up.', 0),
    (NULL, 'Application.SaveSettingsOnExit', 'true',         'true',            'bool',   'Application', 'Write settings when the program closes.', 0),
    (NULL, 'Application.SettingsFile',    '',                '',                'string', 'Application', 'An alternative settings file.', 0),
    (NULL, 'Window.Width',                '604',             '604',             'int',    'Window',      'Starting width in pixels.', 0),
    (NULL, 'Window.Height',               '688',             '688',             'int',    'Window',      'Starting height in pixels.', 0),
    (NULL, 'Window.CenterOnStart',        'true',            'true',            'bool',   'Window',      'Open centred on the screen.', 0),
    (NULL, 'Window.SidebarVisible',       'true',            'true',            'bool',   'Window',      'Show the control sidebar.', 0),
    (NULL, 'Window.SidebarWidth',         '220',             '220',             'int',    'Window',      'Width of the sidebar in pixels.', 0),
    (NULL, 'Simulation.GridSize',         '100',             '100',             'int',    'Simulation',  'Cells per side.', 0),
    (NULL, 'Simulation.Speed',            '10',              '10',              'int',    'Simulation',  'Speed preset, 1 to 10.', 0),
    (NULL, 'Simulation.Rules',            '23/3',            '23/3',            'string', 'Simulation',  'The ruleset, survive/birth.', 0),
    (NULL, 'Simulation.RandomDensity',    '0.15',            '0.15',            'double', 'Simulation',  'Density used when randomising.', 0),
    (NULL, 'Simulation.AutoStart',        'false',           'false',           'bool',   'Simulation',  'Start running when the program opens.', 0),
    (NULL, 'Appearance.ShowGrid',         'true',            'true',            'bool',   'Appearance',  'Draw the cell border lines.', 0),
    (NULL, 'Appearance.GridLineSize',     '1',               '1',               'double', 'Appearance',  'Thickness of those lines.', 0),
    (NULL, 'Appearance.LiveColor',        '#00bfff',         '#00bfff',         'color',  'Appearance',  'Colour of a live cell.', 0),
    (NULL, 'Appearance.DeadColor',        '#ffffff',         '#ffffff',         'color',  'Appearance',  'Colour of an empty cell.', 0),
    (NULL, 'Appearance.HistoryColor',     '#90ee90',         '#90ee90',         'color',  'Appearance',  'Colour of a recently live cell.', 0),
    (NULL, 'Appearance.BackgroundColor',  '#ffffff',         '#ffffff',         'color',  'Appearance',  'Colour behind the cells.', 0),
    (NULL, 'Appearance.Zoom',             '1',               '1',               'int',    'Appearance',  'Zoom at start-up.', 0),
    (NULL, 'World.Seed',                  '20261007',        '20261007',        'int',    'World',       'Seed for world generation.', 0),
    (NULL, 'World.SeaLevel',              '0',               '0',               'double', 'World',       'Height at or below which everything is water.', 0),
    (NULL, 'World.MountainLine',          '2000',            '2000',            'double', 'World',       'Height at which mountains become impassable.', 0),
    (NULL, 'World.CutRivers',             'true',            'true',            'bool',   'World',       'Cut rivers from the high ground to the sea.', 0),
    (NULL, 'World.TerrainDetail',         '1',               '1',               'double', 'World',       'Scale factor for elevation detail.', 0),
    (NULL, 'Ecosystem.TicksPerRun',       '100',             '100',             'int',    'Ecosystem',   'Ticks run by the Run command.', 0),
    (NULL, 'Ecosystem.FollowSimulation',  'false',           'false',           'bool',   'Ecosystem',   'Advance the ecosystem on every simulation step.', 0),
    (NULL, 'Ecosystem.GrowthMultiplier',  '1',               '1',               'double', 'Ecosystem',   'Multiplier on every growth rate.', 0),
    (NULL, 'Ecosystem.DeathMultiplier',   '1',               '1',               'double', 'Ecosystem',   'Multiplier on every death rate.', 0),
    (NULL, 'Ecosystem.AllowExtinction',   'true',            'true',            'bool',   'Ecosystem',   'Drop a species with nobody left.', 0),
    (NULL, 'Ecosystem.Migration',         'true',            'true',            'bool',   'Ecosystem',   'Species spread into empty tiles.', 0),
    (NULL, 'Updates.ManifestPath',        'Updates\\updates.xml', 'Updates\\updates.xml', 'string', 'Updates', 'Path of the update manifest.', 0),
    (NULL, 'Updates.AllowPrerelease',     'false',           'false',           'bool',   'Updates',     'Offer prerelease builds.', 0),
    (NULL, 'Updates.VerifyChecksums',     'true',            'true',            'bool',   'Updates',     'Check the SHA-256 before applying.', 0),
    (NULL, 'Updates.RestartAfterUpdate',  'true',            'true',            'bool',   'Updates',     'Restart after applying an update.', 0),
    (NULL, 'Updates.ApplyPatchesIncrementally', 'true',      'true',            'bool',   'Updates',     'Write only the changed files.', 0),
    (NULL, 'Diagnostics.LogLevel',        'info',            'info',            'string', 'Diagnostics', 'silent, error, warning, info or debug.', 0),
    (NULL, 'Diagnostics.LogFile',         '',                '',                'string', 'Diagnostics', 'Where the log is written.', 0),
    (NULL, 'Diagnostics.LogUnhandledExceptions', 'true',     'true',            'bool',   'Diagnostics', 'Log a crash before closing.', 0),
    (NULL, 'Diagnostics.ProfileSimulation', 'false',         'false',           'bool',   'Diagnostics', 'Time each generation.', 0);

-- ---------------------------------------------------------------------------
-- A worked example
-- ---------------------------------------------------------------------------

-- A short run on a 100x100 grid, so the growth view and the Excel export have
-- something to read before the program has recorded a real session. Three
-- generations of a blinker, which is the smallest thing that changes.

INSERT IGNORE INTO run
    (run_id, started_at, finished_at, build, version, product, developer,
     seed, rules, grid_size, speed, note)
VALUES
    (1, '2026-10-07T09:00:00', '2026-10-07T09:02:00',
     '2026.10.07-r3', '1.1.0.0', 'LifeEngine', 'ArkansasIo',
     20261007, '23/3', 100, 10, 'Example run: a blinker on an empty board.');

INSERT IGNORE INTO generation
    (generation_id, run_id, number, population, density, born, died, elapsed_ms, recorded_at)
VALUES
    (1, 1, 0, 3, 0.0003, 3, 0, 0.4, '2026-10-07T09:00:00'),
    (2, 1, 1, 3, 0.0003, 2, 2, 0.5, '2026-10-07T09:00:01'),
    (3, 1, 2, 3, 0.0003, 2, 2, 0.4, '2026-10-07T09:00:02');

-- Generation 0: three in a row, horizontal.
INSERT IGNORE INTO cell (generation_id, x, y, alive, ever_alive) VALUES
    (1, 49, 50, 1, 1), (1, 50, 50, 1, 1), (1, 51, 50, 1, 1);

-- Generation 1: three in a column. The two ends died, the two sides were born.
INSERT IGNORE INTO cell (generation_id, x, y, alive, ever_alive) VALUES
    (2, 50, 49, 1, 1), (2, 50, 50, 1, 1), (2, 50, 51, 1, 1);

-- Generation 2: back to a row, one cell to the right of where it began.
INSERT IGNORE INTO cell (generation_id, x, y, alive, ever_alive) VALUES
    (3, 49, 50, 1, 1), (3, 50, 50, 1, 1), (3, 51, 50, 1, 1);

-- A small world with two species on it.
INSERT IGNORE INTO world
    (world_id, run_id, name, seed, columns, rows, sea_level, axial_tilt,
     day_length, gravity, land_fraction, mean_temp, mean_elevation,
     river_count, created_at)
VALUES
    (1, 1, 'Terra Nova', 20261007, 100, 100, 0.0, 23.44, 24.0, 9.81,
     0.42, 11.3, 180.0, 6, '2026-10-07T09:00:30');

INSERT IGNORE INTO species
    (species_id, world_id, name, kingdom, diet, locomotion, trophic_level,
     symbol, colour_key, min_temp, max_temp, min_rainfall, max_rainfall,
     max_elevation, needs_water, needs_land, needs_coast, avoids_desert,
     avoids_ice, growth_rate, death_rate, max_population, seed_population)
VALUES
    (1, 1, 'Grassland Grass', 'Plantae', 'Photosynthetic', 'Sessile', 'Producer',
     'g', 'LawnGreen', -5.0, 38.0, 150.0, 1200.0,
     9000.0, 0, 1, 0, 1, 1, 0.10, 0.02, 60, 25);

INSERT IGNORE INTO species
    (species_id, world_id, name, kingdom, diet, locomotion, trophic_level,
     symbol, colour_key, min_temp, max_temp, min_rainfall, max_rainfall,
     max_elevation, needs_water, needs_land, needs_coast, avoids_desert,
     avoids_ice, growth_rate, death_rate, max_population, seed_population)
VALUES
    (2, 1, 'Grey Wolf', 'Animalia', 'Carnivore', 'Running', 'SecondaryConsumer',
     'W', 'DimGray', -35.0, 30.0, 0.0, 10000.0,
     3000.0, 0, 1, 0, 1, 0, 0.04, 0.08, 18, 6);

INSERT IGNORE INTO population (world_id, tick, species_id, x, y, head) VALUES
    (1, 0, 1, 10, 20, 42), (1, 0, 1, 11, 20, 38), (1, 0, 1, 12, 21, 51),
    (1, 0, 2, 10, 20, 4),  (1, 0, 2, 11, 20, 3);

INSERT IGNORE INTO population (world_id, tick, species_id, x, y, head) VALUES
    (1, 1, 1, 10, 20, 45), (1, 1, 1, 11, 20, 41), (1, 1, 1, 12, 21, 49),
    (1, 1, 2, 10, 20, 5),  (1, 1, 2, 11, 20, 3);

-- A pack of three, so the order view has something to sort.
INSERT IGNORE INTO wolf_pack
    (pack_id, run_id, name, territory, founded_at, alpha_name, cohesion, alpha_changes)
VALUES
    (1, 1, 'The Grey Company', 'Terra Nova', '2026-10-07T09:01:00', 'Fenrir', 0.6, 0);

INSERT IGNORE INTO wolf
    (wolf_id, pack_id, name, rank, age_years, mass_kg, health, energy, is_alive,
     is_cub, is_elder, strength, speed, aggression, hearing, scent, night_sight,
     healing, human_retention, is_cursed, is_shapeshifter, scar_count, kills,
     dominance_wins, dominance_losses)
VALUES
    (1, 1, 'Fenrir', 'Alpha', 4.5, 48.0, 0.92, 0.80, 1, 0, 0,
     0.78, 0.66, 0.72, 0.70, 0.85, 0.82, 0.55, 0.62, 1, 1, 3, 11, 5, 1),
    (2, 1, 'Skoll', 'Second', 3.2, 44.0, 0.88, 0.74, 1, 0, 0,
     0.70, 0.74, 0.66, 0.68, 0.80, 0.78, 0.50, 0.58, 1, 1, 1, 7, 3, 2),
    (3, 1, 'Varg', 'Omega', 2.1, 39.0, 0.61, 0.45, 1, 0, 0,
     0.48, 0.60, 0.35, 0.62, 0.72, 0.70, 0.44, 0.41, 1, 1, 4, 2, 0, 4);

INSERT IGNORE INTO hunt
    (hunt_id, pack_id, occurred_at, is_night, is_full_moon, prey_name,
     succeeded, taken, meat_kg, hunter_count, wounded_count, narrative)
VALUES
    (1, 1, '2026-10-07T09:01:30', 1, 0, 'Red Deer',
     1, 2, 240.0, 3, 0, 'The pack brought down 2 Red Deer (240 kg).'),
    (2, 1, '2026-10-07T09:02:30', 1, 0, 'Red Deer',
     0, 0, 0.0, 3, 1, 'The pack chased a Red Deer and lost it.');

INSERT IGNORE INTO script_run
    (script_run_id, run_id, name, succeeded, error, output, steps, elapsed_ms, ran_at)
VALUES
    (1, 1, 'example.lua', 1, NULL,
     CONCAT('LifeEngine scripting', CHAR(10), 'grid is 100 cells a side'),
     412, 8.4, '2026-10-07T09:01:45');

INSERT IGNORE INTO event (event_id, run_id, logged_at, message) VALUES
    (1, 1, '2026-10-07T09:00:00', '09:00:00  LifeEngine 1.1.0 ready'),
    (2, 1, '2026-10-07T09:00:30', '09:00:30  world built: 100x100 tiles, seed 20261007, 12 species'),
    (3, 1, '2026-10-07T09:01:00', '09:01:00  pack founded: The Grey Company (3 wolves, alpha Fenrir)'),
    (4, 1, '2026-10-07T09:01:30', '09:01:30  The pack brought down 2 Red Deer (240 kg).');

-- ---------------------------------------------------------------------------
-- A few queries worth having to hand
-- ---------------------------------------------------------------------------

-- Population over time for a run:
--
--   SELECT number, population, `change` FROM v_generation_growth
--    WHERE run_id = 1 ORDER BY number;
--
-- The most numerous species in a world:
--
--   SELECT name, head FROM v_species_totals
--    WHERE world_id = 1 ORDER BY head DESC;
--
-- How much of each biome a world has:
--
--   SELECT biome, tiles FROM v_biome_census
--    WHERE world_id = 1 ORDER BY tiles DESC;
--
-- The pack, strongest first:
--
--   SELECT rank, name, dominance, kills FROM v_pack_order WHERE pack_id = 1;
--
-- Hunting success:
--
--   SELECT * FROM v_hunt_summary;
--
-- Which tiles a species occupies at the last tick:
--
--   SELECT x, y, head FROM population
--    WHERE species_id = 1 AND tick = (SELECT MAX(tick) FROM population WHERE species_id = 1)
--    ORDER BY head DESC;

/**
 * LifeEngine is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * LifeEngine is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 *
 * Copyright © 2026 ArkansasIo
 */

using System;
using System.Collections.Generic;
using System.Text;
using LifeEngine.Biology;

namespace LifeEngine.Ecosystem
{
    /// <summary>
    /// Builds a ready-to-run world: a <see cref="WorldMap"/>, an
    /// <see cref="Ecosystem"/> on it, and the species that live there.
    ///
    /// Everything is driven from a seed, so the same call always produces the
    /// same planet and the same starting populations.
    /// </summary>
    public static class WorldFactory
    {
        /// <summary>The seed used when a caller does not care which world it gets.</summary>
        public const int DefaultSeed = 20261007;

        /// <summary>Builds a world with the default species on it, already seeded.</summary>
        public static Ecosystem CreateDefaultWorld(int columns, int rows, int seed)
        {
            WorldMap world = new WorldMap("Terra Nova", columns, rows, seed);

            Ecosystem ecosystem = new Ecosystem(world.Name, world, seed);

            RegisterDefaultSpecies(ecosystem);

            ecosystem.SeedPopulations();

            return ecosystem;
        }

        /// <summary>Builds a world with the default species on it, but not yet seeded.</summary>
        public static Ecosystem CreateEmptyWorld(int columns, int rows, int seed)
        {
            WorldMap world = new WorldMap("Terra Nova", columns, rows, seed);

            Ecosystem ecosystem = new Ecosystem(world.Name, world, seed);

            RegisterDefaultSpecies(ecosystem);

            return ecosystem;
        }

        /// <summary>
        /// The species that live on a default world: producers first, then the
        /// herbivores that eat them, then the predators that eat the herbivores.
        /// </summary>
        public static void RegisterDefaultSpecies(Ecosystem ecosystem)
        {
            if (ecosystem == null)
                return;

            // ---- Producers -------------------------------------------------

            Species grass = new Species("Grassland Grass");
            grass.Kingdom = Kingdom.Plantae;
            grass.Diet = DietType.Photosynthetic;
            grass.TrophicLevel = TrophicLevel.Producer;
            grass.Locomotion = LocomotionType.Sessile;
            grass.MinTemperatureCelsius = -5.0;
            grass.MaxTemperatureCelsius = 38.0;
            grass.MinRainfallMm = 150.0;
            grass.MaxRainfallMm = 1200.0;
            grass.NeedsLandTile = true;
            grass.AvoidsDesert = true;
            grass.AvoidsIce = true;
            grass.GrowthRate = 0.10;
            grass.DeathRate = 0.02;
            grass.MaxPopulation = 60;
            grass.SeedPopulation = 25;
            grass.Symbol = 'g';
            grass.ColourKey = "LawnGreen";
            ecosystem.Register(grass);

            Species forest = new Species("Temperate Forest");
            forest.Kingdom = Kingdom.Plantae;
            forest.Diet = DietType.Photosynthetic;
            forest.TrophicLevel = TrophicLevel.Producer;
            forest.Locomotion = LocomotionType.Sessile;
            forest.MinTemperatureCelsius = 0.0;
            forest.MaxTemperatureCelsius = 30.0;
            forest.MinRainfallMm = 500.0;
            forest.MaxRainfallMm = 2200.0;
            forest.MaxElevationMetres = 1500.0;
            forest.NeedsLandTile = true;
            forest.AvoidsDesert = true;
            forest.AvoidsIce = true;
            forest.GrowthRate = 0.05;
            forest.DeathRate = 0.01;
            forest.MaxPopulation = 45;
            forest.SeedPopulation = 20;
            forest.Symbol = 'T';
            forest.ColourKey = "ForestGreen";
            ecosystem.Register(forest);

            Species jungle = new Species("Jungle Canopy");
            jungle.Kingdom = Kingdom.Plantae;
            jungle.Diet = DietType.Photosynthetic;
            jungle.TrophicLevel = TrophicLevel.Producer;
            jungle.Locomotion = LocomotionType.Sessile;
            jungle.MinTemperatureCelsius = 16.0;
            jungle.MaxTemperatureCelsius = 45.0;
            jungle.MinRainfallMm = 1500.0;
            jungle.MaxRainfallMm = 6000.0;
            jungle.NeedsLandTile = true;
            jungle.AvoidsIce = true;
            jungle.GrowthRate = 0.07;
            jungle.DeathRate = 0.01;
            jungle.MaxPopulation = 50;
            jungle.SeedPopulation = 22;
            jungle.Symbol = 'J';
            jungle.ColourKey = "LimeGreen";
            ecosystem.Register(jungle);

            Species algae = new Species("Algal Bloom");
            algae.Kingdom = Kingdom.Protista;
            algae.Diet = DietType.Photosynthetic;
            algae.TrophicLevel = TrophicLevel.Producer;
            algae.Locomotion = LocomotionType.None;
            algae.MinTemperatureCelsius = -2.0;
            algae.MaxTemperatureCelsius = 35.0;
            algae.NeedsWaterTile = true;
            algae.GrowthRate = 0.12;
            algae.DeathRate = 0.03;
            algae.MaxPopulation = 70;
            algae.SeedPopulation = 30;
            algae.Symbol = 'A';
            algae.ColourKey = "Teal";
            ecosystem.Register(algae);

            // ---- Primary consumers -----------------------------------------

            Species bee = new Species("Honey Bee");
            bee.Kingdom = Kingdom.Animalia;
            bee.Diet = DietType.Herbivore;
            bee.TrophicLevel = TrophicLevel.PrimaryConsumer;
            bee.Locomotion = LocomotionType.Flying;
            bee.MinTemperatureCelsius = 5.0;
            bee.MaxTemperatureCelsius = 40.0;
            bee.MinRainfallMm = 200.0;
            bee.MaxRainfallMm = 2500.0;
            bee.NeedsLandTile = true;
            bee.AvoidsDesert = true;
            bee.AvoidsIce = true;
            bee.GrowthRate = 0.06;
            bee.DeathRate = 0.04;
            bee.MaxPopulation = 30;
            bee.SeedPopulation = 12;
            bee.Symbol = 'B';
            bee.ColourKey = "Gold";
            ecosystem.Register(bee);

            Species frog = new Species("Common Frog");
            frog.Kingdom = Kingdom.Animalia;
            frog.Diet = DietType.Carnivore;
            frog.TrophicLevel = TrophicLevel.PrimaryConsumer;
            frog.Locomotion = LocomotionType.Swimming;
            frog.MinTemperatureCelsius = 0.0;
            frog.MaxTemperatureCelsius = 32.0;
            frog.MinRainfallMm = 400.0;
            frog.NeedsCoast = true;
            frog.NeedsLandTile = true;
            frog.AvoidsIce = true;
            frog.GrowthRate = 0.05;
            frog.DeathRate = 0.05;
            frog.MaxPopulation = 25;
            frog.SeedPopulation = 10;
            frog.Symbol = 'f';
            frog.ColourKey = "Olive";
            ecosystem.Register(frog);

            Species salmon = new Species("Atlantic Salmon");
            salmon.Kingdom = Kingdom.Animalia;
            salmon.Diet = DietType.Carnivore;
            salmon.TrophicLevel = TrophicLevel.PrimaryConsumer;
            salmon.Locomotion = LocomotionType.Swimming;
            salmon.MinTemperatureCelsius = -2.0;
            salmon.MaxTemperatureCelsius = 22.0;
            salmon.NeedsWaterTile = true;
            salmon.AvoidsIce = true;
            salmon.GrowthRate = 0.05;
            salmon.DeathRate = 0.05;
            salmon.MaxPopulation = 35;
            salmon.SeedPopulation = 14;
            salmon.Symbol = 's';
            salmon.ColourKey = "DeepSkyBlue";
            ecosystem.Register(salmon);

            Species spider = new Species("Garden Spider");
            spider.Kingdom = Kingdom.Animalia;
            spider.Diet = DietType.Carnivore;
            spider.TrophicLevel = TrophicLevel.PrimaryConsumer;
            spider.Locomotion = LocomotionType.Crawling;
            spider.MinTemperatureCelsius = -5.0;
            spider.MaxTemperatureCelsius = 40.0;
            spider.MinRainfallMm = 100.0;
            spider.NeedsLandTile = true;
            spider.AvoidsIce = true;
            spider.GrowthRate = 0.07;
            spider.DeathRate = 0.06;
            spider.MaxPopulation = 28;
            spider.SeedPopulation = 11;
            spider.Symbol = 'x';
            spider.ColourKey = "SaddleBrown";
            ecosystem.Register(spider);

            // ---- Secondary consumers ---------------------------------------

            Species wolf = new Species("Grey Wolf");
            wolf.Kingdom = Kingdom.Animalia;
            wolf.Diet = DietType.Carnivore;
            wolf.TrophicLevel = TrophicLevel.SecondaryConsumer;
            wolf.Locomotion = LocomotionType.Running;
            wolf.MinTemperatureCelsius = -35.0;
            wolf.MaxTemperatureCelsius = 30.0;
            wolf.MaxElevationMetres = 3000.0;
            wolf.NeedsLandTile = true;
            wolf.AvoidsDesert = true;
            wolf.GrowthRate = 0.04;
            wolf.DeathRate = 0.08;
            wolf.MaxPopulation = 18;
            wolf.SeedPopulation = 6;
            wolf.Symbol = 'W';
            wolf.ColourKey = "DimGray";
            ecosystem.Register(wolf);

            Species eagle = new Species("Golden Eagle");
            eagle.Kingdom = Kingdom.Animalia;
            eagle.Diet = DietType.Carnivore;
            eagle.TrophicLevel = TrophicLevel.SecondaryConsumer;
            eagle.Locomotion = LocomotionType.Flying;
            eagle.MinTemperatureCelsius = -30.0;
            eagle.MaxTemperatureCelsius = 35.0;
            eagle.MaxElevationMetres = 4500.0;
            eagle.NeedsLandTile = true;
            eagle.AvoidsIce = true;
            eagle.GrowthRate = 0.03;
            eagle.DeathRate = 0.09;
            eagle.MaxPopulation = 12;
            eagle.SeedPopulation = 4;
            eagle.Symbol = 'E';
            eagle.ColourKey = "Goldenrod";
            ecosystem.Register(eagle);

            Species octopus = new Species("Common Octopus");
            octopus.Kingdom = Kingdom.Animalia;
            octopus.Diet = DietType.Carnivore;
            octopus.TrophicLevel = TrophicLevel.SecondaryConsumer;
            octopus.Locomotion = LocomotionType.Swimming;
            octopus.MinTemperatureCelsius = 2.0;
            octopus.MaxTemperatureCelsius = 30.0;
            octopus.NeedsWaterTile = true;
            octopus.AvoidsIce = true;
            octopus.GrowthRate = 0.03;
            octopus.DeathRate = 0.09;
            octopus.MaxPopulation = 14;
            octopus.SeedPopulation = 5;
            octopus.Symbol = 'O';
            octopus.ColourKey = "Violet";
            ecosystem.Register(octopus);

            // ---- Apex ------------------------------------------------------

            Species orca = new Species("Orca");
            orca.Kingdom = Kingdom.Animalia;
            orca.Diet = DietType.Carnivore;
            orca.TrophicLevel = TrophicLevel.ApexPredator;
            orca.Locomotion = LocomotionType.Swimming;
            orca.MinTemperatureCelsius = -5.0;
            orca.MaxTemperatureCelsius = 25.0;
            orca.NeedsWaterTile = true;
            orca.AvoidsIce = true;
            orca.GrowthRate = 0.02;
            orca.DeathRate = 0.12;
            orca.MaxPopulation = 8;
            orca.SeedPopulation = 2;
            orca.Symbol = 'K';
            orca.ColourKey = "Black";
            ecosystem.Register(orca);
        }

        /// <summary>
        /// Draws the world as text: one character per tile, for a console or a
        /// stats pane. Land biomes get a character, water gets its own, and any
        /// tile with a population on it shows that species' symbol instead.
        /// </summary>
        public static string RenderMap(WorldMap world, bool showLife)
        {
            if (world == null)
                return string.Empty;

            StringBuilder builder = new StringBuilder();

            for (int y = 0; y < world.Rows; y++)
            {
                for (int x = 0; x < world.Columns; x++)
                {
                    Tile tile = world.GetTile(x, y);

                    if (tile == null)
                    {
                        builder.Append(' ');
                        continue;
                    }

                    if (showLife && tile.TotalPopulation() > 0)
                    {
                        builder.Append(DominantSymbol(tile));
                        continue;
                    }

                    builder.Append(BiomeSymbol(tile));
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        /// <summary>The symbol of whatever species has the most tiles' worth here.</summary>
        private static char DominantSymbol(Tile tile)
        {
            int best = -1;
            char symbol = '?';

            for (int i = 0; i < tile.Organisms.Count; i++)
            {
                int population = tile.Populations[i];

                if (population > best)
                {
                    best = population;

                    Species species = tile.Organisms[i] as Species;

                    if (species != null)
                        symbol = species.Symbol;
                }
            }

            return symbol;
        }

        /// <summary>One character standing for a biome.</summary>
        public static char BiomeSymbol(Tile tile)
        {
            if (tile == null)
                return ' ';

            if (tile.HasRiver)
                return '~';

            switch (tile.Biome)
            {
                case BiomeType.Ocean:
                    return '~';

                case BiomeType.IceSheet:
                    return '#';

                case BiomeType.Tundra:
                    return '-';

                case BiomeType.BorealForest:
                    return 'p';

                case BiomeType.TemperateDeciduousForest:
                case BiomeType.TemperateRainforest:
                    return 't';

                case BiomeType.TemperateGrassland:
                    return '.';

                case BiomeType.Mediterranean:
                    return ',';

                case BiomeType.HotDesert:
                    return ':';

                case BiomeType.ColdDesert:
                    return ';';

                case BiomeType.TropicalSavanna:
                    return '\'';

                case BiomeType.TropicalSeasonalForest:
                case BiomeType.TropicalRainforest:
                    return 'j';

                case BiomeType.Mountain:
                    return '^';

                case BiomeType.Wetland:
                case BiomeType.Freshwater:
                    return '~';

                default:
                    return '?';
            }
        }
    }
}

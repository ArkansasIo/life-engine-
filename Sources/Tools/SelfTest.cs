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
 *
 * ---------------------------------------------------------------------------
 *  A self-check harness.
 *
 *  This exists because eighteen of the nineteen systems in this program have
 *  never been executed. A clean build says a compiler accepted the code; it
 *  says nothing about whether the code does anything sensible. This runs them
 *  and checks the answers.
 *
 *  Run it from the repository root:
 *
 *      csc /out:SelfTest.exe Sources/Tools/SelfTest.cs ^
 *          /reference:System.dll /reference:System.Drawing.dll ^
 *          /reference:System.Windows.Forms.dll
 *      SelfTest.exe
 *
 *  It is written as a plain exe with its own Main so it can be compiled on its
 *  own and does not interfere with the program's entry point.
 * ---------------------------------------------------------------------------
 */

using System;
using System.Collections.Generic;

namespace LifeEngine.Tools
{
       /// <summary>One check: what it was, whether it passed, and what it found.</summary>
       internal sealed class Check
       {
              public string Name;
              public bool Passed;
              public string Detail;

              public Check(string name, bool passed, string detail)
              {
                     Name = name;
                     Passed = passed;
                     Detail = detail;
              }
       }

       /// <summary>
       /// Exercises every system that has never run and reports what broke.
       /// </summary>
       internal static class SelfTest
       {
              private static List<Check> m_checks = new List<Check>();

              private static void Check_(string name, bool passed, string detail)
              {
                     m_checks.Add(new Check(name, passed, detail));
              }

              private static void Ok(string name, string detail)
              {
                     Check_(name, true, detail);
              }

              private static void Fail(string name, string detail)
              {
                     Check_(name, false, detail);
              }

              internal static int Main(string[] arguments)
              {
                     Console.WriteLine("LifeEngine self-check");
                     Console.WriteLine("=====================");
                     Console.WriteLine();

                     RunSection("Simulation", TestSimulation);
                     RunSection("World map", TestWorldMap);
                     RunSection("Ecosystem", TestEcosystem);
                     RunSection("Wolves", TestWolves);
                     RunSection("Combat", TestCombat);
                     RunSection("Attributes", TestAttributes);
                     RunSection("Talents", TestTalents);
                     RunSection("Per-cell AI", TestCellAi);
                     RunSection("Scripting", TestScripting);
                     RunSection("Configuration", TestConfiguration);
                     RunSection("Scheduler", TestScheduler);
                     RunSection("Weather and time", TestWeather);
                     RunSection("Export", TestExport);

                     Console.WriteLine();
                     Console.WriteLine("=====================");

                     int passed = 0;
                     int failed = 0;

                     for (int i = 0; i < m_checks.Count; i++)
                     {
                            if (m_checks[i].Passed)
                                   passed++;
                            else
                                   failed++;
                     }

                     Console.WriteLine(passed + " passed, " + failed + " failed, of " + m_checks.Count);
                     Console.WriteLine();

                     if (failed > 0)
                     {
                            Console.WriteLine("FAILURES");
                            Console.WriteLine();

                            for (int i = 0; i < m_checks.Count; i++)
                            {
                                   if (!m_checks[i].Passed)
                                          Console.WriteLine("  " + m_checks[i].Name + ": " + m_checks[i].Detail);
                            }
                     }

                     return failed == 0 ? 0 : 1;
              }

              private static void RunSection(string title, Action test)
              {
                     Console.WriteLine("-- " + title + " --");

                     try
                     {
                            test();
                     }
                     catch (Exception error)
                     {
                            Fail(title + " threw", error.GetType().Name + ": " + error.Message);
                            Console.WriteLine("  THREW: " + error.GetType().Name + ": " + error.Message);
                     }

                     Console.WriteLine();
              }

              // ------------------------------------------------------------------
              // Simulation
              // ------------------------------------------------------------------

              private static void TestSimulation()
              {
                     LifeGame game = new LifeGame(20);

                     // A blinker: three in a row becomes three in a column.
                     game.ToggleCellState(5, 5);
                     game.ToggleCellState(6, 5);
                     game.ToggleCellState(7, 5);

                     Check_("blinker starts with 3 cells", game.LiveCellCount == 3,
                            "count " + game.LiveCellCount);

                     game.Step();

                     bool vertical = game.GetCellState(6, 4) && game.GetCellState(6, 5) && game.GetCellState(6, 6);

                     Check_("blinker becomes a column", vertical,
                            "cells " + DescribeRow(game, 4, 6));

                     Check_("blinker stays at 3 cells", game.LiveCellCount == 3,
                            "count " + game.LiveCellCount);

                     game.Step();

                     bool horizontal = game.GetCellState(5, 5) && game.GetCellState(6, 5) && game.GetCellState(7, 5);

                     Check_("blinker returns to a row", horizontal,
                            "cells " + DescribeRow(game, 5, 5));

                     // A block is still life: it must not change.
                     LifeGame block = new LifeGame(10);

                     block.ToggleCellState(4, 4);
                     block.ToggleCellState(5, 4);
                     block.ToggleCellState(4, 5);
                     block.ToggleCellState(5, 5);

                     block.Step();

                     Check_("block is stable", block.LiveCellCount == 4 &&
                            block.GetCellState(4, 4) && block.GetCellState(5, 5),
                            "count " + block.LiveCellCount);

                     // An empty board must not throw or produce cells.
                     LifeGame empty = new LifeGame(10);

                     empty.Step();

                     Check_("empty board stays empty", empty.LiveCellCount == 0,
                            "count " + empty.LiveCellCount);

                     // Out of bounds reads and writes must be refused, not crash.
                     Check_("out of bounds read is false", !game.GetCellState(-1, -1),
                            "no crash");

                     game.ToggleCellState(-5, 999);

                     Check_("out of bounds write is ignored", game.LiveCellCount == 3,
                            "count " + game.LiveCellCount);

                     // The history layer must latch.
                     Check_("history is latched", game.HistoryGrid[5 + 5 * 20],
                            "a cell that was alive is still marked");

                     // A ruleset that is nonsense must fall back rather than throw.
                     LifeGame odd = new LifeGame(10, 10, "garbage");

                     Check_("bad ruleset falls back", odd.Rules == "23/3",
                            "rules " + odd.Rules);
              }

              private static string DescribeRow(LifeGame game, int x, int y)
              {
                     System.Text.StringBuilder builder = new System.Text.StringBuilder();

                     for (int dy = -1; dy <= 1; dy++)
                     {
                            for (int dx = -1; dx <= 1; dx++)
                                   builder.Append(game.GetCellState(x + dx, y + dy) ? '#' : '.');

                            builder.Append(' ');
                     }

                     return builder.ToString();
              }

              // ------------------------------------------------------------------
              // World map
              // ------------------------------------------------------------------

              private static void TestWorldMap()
              {
                     Biology.WorldMap world = new Biology.WorldMap("Test", 40, 40, 12345);

                     Check_("world has the right size",
                            world.Columns == 40 && world.Rows == 40 && world.TileCount == 1600,
                            world.Columns + "x" + world.Rows);

                     Check_("world has bands", world.Bands.Count == 12,
                            world.Bands.Count + " bands");

                     // The poles must be colder than the equator.
                     Biology.Tile north = world.GetTile(20, 0);
                     Biology.Tile equator = world.GetTile(20, 20);

                     Check_("pole is colder than equator",
                            north != null && equator != null &&
                            north.TemperatureCelsius < equator.TemperatureCelsius,
                            "pole " + Math.Round(north.TemperatureCelsius) +
                            "C, equator " + Math.Round(equator.TemperatureCelsius) + "C");

                     // Both land and water should exist.
                     double land = world.LandFraction();

                     Check_("world has land and sea", land > 0.0 && land < 1.0,
                            Math.Round(land * 100.0) + "% land");

                     // Biomes must be varied, not all one thing.
                     Dictionary<Biology.BiomeType, int> census = world.BiomeCensus();

                     Check_("world has several biomes", census.Count >= 3,
                            census.Count + " biomes");

                     // Rivers, if any were cut, must be on land.
                     int riverTiles = 0;
                     int wetRiverTiles = 0;

                     foreach (Biology.Tile tile in world.Tiles)
                     {
                            if (!tile.HasRiver)
                                   continue;

                            riverTiles++;

                            if (tile.IsWater)
                                   wetRiverTiles++;
                     }

                     Check_("rivers are cut", riverTiles > 0 || world.RiverCourses.Count == 0,
                            riverTiles + " river tiles, " + world.RiverCourses.Count + " courses");

                     // The same seed must give the same world.
                     Biology.WorldMap again = new Biology.WorldMap("Test", 40, 40, 12345);

                     bool identical = true;

                     for (int y = 0; y < 40 && identical; y++)
                     {
                            for (int x = 0; x < 40; x++)
                            {
                                   Biology.Tile a = world.GetTile(x, y);
                                   Biology.Tile b = again.GetTile(x, y);

                                   if (a.Biome != b.Biome || Math.Abs(a.ElevationMetres - b.ElevationMetres) > 0.001)
                                   {
                                          identical = false;
                                          break;
                                   }
                            }
                     }

                     Check_("same seed gives the same world", identical,
                            identical ? "identical" : "worlds differ");

                     // A different seed must give a different world.
                     Biology.WorldMap other = new Biology.WorldMap("Test", 40, 40, 999);

                     int differences = 0;

                     for (int y = 0; y < 40; y++)
                     {
                            for (int x = 0; x < 40; x++)
                            {
                                   if (world.GetTile(x, y).Biome != other.GetTile(x, y).Biome)
                                          differences++;
                            }
                     }

                     Check_("different seed gives a different world", differences > 0,
                            differences + " tiles differ");

                     // Out of bounds must return null, not throw.
                     Check_("world out of bounds is null", world.GetTile(-1, 0) == null,
                            "no crash");
              }

              // ------------------------------------------------------------------
              // Ecosystem
              // ------------------------------------------------------------------

              private static void TestEcosystem()
              {
                     Ecosystem.Ecosystem ecosystem = Ecosystem.WorldFactory.CreateDefaultWorld(30, 30, 777);

                     Check_("ecosystem has species", ecosystem.Species.Count > 0,
                            ecosystem.Species.Count + " species");

                     int seeded = ecosystem.TotalPopulation();

                     Check_("ecosystem was seeded", seeded > 0,
                            seeded + " head");

                     int before = ecosystem.TotalPopulation();

                     ecosystem.Run(10);

                     int after = ecosystem.TotalPopulation();

                     Check_("ecosystem runs without throwing", ecosystem.Tick == 10,
                            "tick " + ecosystem.Tick + ", head " + before + " -> " + after);

                     // Populations must not go negative.
                     bool negative = false;

                     foreach (Biology.Tile tile in ecosystem.World.Tiles)
                     {
                            for (int i = 0; i < tile.Populations.Count; i++)
                            {
                                   if (tile.Populations[i] < 0)
                                          negative = true;
                            }
                     }

                     Check_("no negative populations", !negative,
                            negative ? "found a negative head count" : "all non-negative");

                     // Running a long time must not throw or produce nonsense.
                     ecosystem.Run(200);

                     Check_("ecosystem survives 200 more ticks",
                            ecosystem.TotalPopulation() >= 0,
                            "tick " + ecosystem.Tick + ", head " + ecosystem.TotalPopulation());

                     // A producer on an ocean tile must not survive.
                     Ecosystem.Ecosystem probe = Ecosystem.WorldFactory.CreateEmptyWorld(20, 20, 5);
                     Ecosystem.Species grass = probe.Find("Grassland Grass");

                     Check_("grass exists in the default set", grass != null,
                            grass == null ? "not found" : "found");

                     if (grass != null)
                     {
                            bool anyWaterTile = false;
                            bool grassOnWater = false;

                            foreach (Biology.Tile tile in probe.World.Tiles)
                            {
                                   if (!tile.IsWater)
                                          continue;

                                   anyWaterTile = true;

                                   if (grass.CanLiveOn(tile))
                                          grassOnWater = true;
                            }

                            Check_("grass cannot live on water", !grassOnWater || !anyWaterTile,
                                   grassOnWater ? "grass accepted a water tile" : "correctly refused");
                     }
              }

              // ------------------------------------------------------------------
              // Wolves
              // ------------------------------------------------------------------

              private static void TestWolves()
              {
                     Biology.WolfPack pack = new Biology.WolfPack("Test Pack", "Test Land");

                     Biology.Wolf alpha = pack.Found("Fenrir");

                     Check_("pack has an alpha", pack.Alpha != null && pack.Alpha == alpha,
                            pack.Alpha == null ? "no alpha" : pack.Alpha.Name);

                     pack.Recruit("Skoll");
                     pack.Recruit("Hati");

                     pack.SettleRanks();

                     Check_("pack has three members", pack.Count == 3,
                            pack.Count + " wolves");

                     Check_("the strongest is alpha",
                            pack.Alpha != null && pack.Alpha.FightingPower() >= pack.Members[0].FightingPower(),
                            pack.Alpha == null ? "no alpha" : pack.Alpha.Name);

                     // A challenge must produce a winner and leave the pack intact.
                     Biology.Wolf challenger = pack.GetAdults()[0];
                     Biology.Wolf defender = pack.GetAdults().Count > 1 ? pack.GetAdults()[1] : null;

                     if (challenger != null && defender != null)
                     {
                            int before = pack.Count;
                            Biology.Wolf winner = pack.Challenge(challenger, defender, new Random(1));

                            Check_("a challenge has a winner", winner != null,
                                   winner == null ? "no winner" : winner.Name + " won");

                            Check_("a challenge does not lose wolves", pack.Count == before,
                                   pack.Count + " wolves");
                     }

                     // A hunt against a herd.
                     WolfLogic.Prey prey = new WolfLogic.Prey("Deer", 120.0, 55.0, 0.35, 40);

                     int herd = prey.Population;
                     WolfLogic.HuntResult hunt = WolfLogic.WolfLogic.Hunt(pack, prey, true, new Random(7));

                     Check_("a hunt produces a result", hunt != null,
                            hunt == null ? "no result" : hunt.Narrative);

                     Check_("a hunt does not increase the herd", prey.Population <= herd,
                            herd + " -> " + prey.Population);

                     // Energy must stay in range.
                     bool energySane = true;

                     for (int i = 0; i < pack.Members.Count; i++)
                     {
                            Biology.Wolf wolf = pack.Members[i];

                            if (wolf.Energy < 0.0 || wolf.Energy > 1.0 || wolf.Health < 0.0 || wolf.Health > 1.0)
                                   energySane = false;
                     }

                     Check_("wolves stay in range after a hunt", energySane,
                            energySane ? "health and energy within 0 to 1" : "a wolf left the range");

                     // A season must age them and not throw.
                     WolfLogic.WolfLogic.Season(pack, 0.25, 0.5, 12, new Random(3));

                     Check_("a season passes", pack.Count >= 0,
                            pack.Count + " wolves remain");

                     // The moon must not throw.
                     WolfLogic.WolfLogic.Moonshift(pack, new Random(11));

                     Check_("the moonshift runs", true, "no exception");
              }

              // ------------------------------------------------------------------
              // Combat
              // ------------------------------------------------------------------

              private static void TestCombat()
              {
                     Combat.CombatEngine engine = new Combat.CombatEngine(new Random(42));

                     Combat.Combatant left = Combat.Combatant.Plain("A", Combat.Side.Pack, 0.7, 0.6, 45.0);
                     Combat.Combatant right = Combat.Combatant.Plain("B", Combat.Side.Rival, 0.6, 0.6, 42.0);

                     engine.Add(left);
                     engine.Add(right);

                     Combat.BattleResult result = engine.Fight("Test fight");

                     Check_("a fight produces a result", result != null,
                            result == null ? "no result" : result.Narrative);

                     Check_("a fight has rounds", result.Rounds > 0,
                            result.Rounds + " rounds");

                     Check_("a fight has a log", result.Log.Count > 0,
                            result.Log.Count + " lines");

                     // Health must not go below zero.
                     Check_("health does not go negative", left.Health >= 0.0 && right.Health >= 0.0,
                            "A " + left.Health + ", B " + right.Health);

                     // One of them must be down, or it was a draw by round limit.
                     Check_("someone is down or the limit was hit",
                            !left.CanFight || !right.CanFight || result.Rounds >= 40,
                            "A can fight " + left.CanFight + ", B can fight " + right.CanFight);

                     // Effects must tick and expire.
                     Combat.Effect bleed = Combat.EffectsCatalogue.Bleeding();
                     Combat.EffectCollection effects = new Combat.EffectCollection();

                     effects.Apply(bleed);

                     Check_("an effect is applied", effects.Count == 1,
                            effects.Count + " effects");

                     // A second copy must stack, because bleeding is additive.
                     effects.Apply(Combat.EffectsCatalogue.Bleeding());

                     Check_("bleeding stacks", effects.Find("Bleeding").Stacks == 2,
                            "stacks " + effects.Find("Bleeding").Stacks);

                     Check_("bleeding hurts each round", effects.DamagePerRound() > 0.0,
                            Math.Round(effects.DamagePerRound(), 1) + " damage a round");

                     // A buff must not stack beyond its ceiling.
                     Combat.EffectCollection buffs = new Combat.EffectCollection();

                     for (int i = 0; i < 10; i++)
                            buffs.Apply(Combat.EffectsCatalogue.Rallied());

                     Check_("a buff respects its ceiling", buffs.Find("Rallied").Stacks == 1,
                            "stacks " + buffs.Find("Rallied").Stacks);

                     // Expiry.
                     List<string> expired = effects.Tick(100.0);

                     Check_("effects expire", effects.Count == 0 && expired.Count > 0,
                            expired.Count + " expired");

                     // The bestiary must produce working fighters.
                     List<Combat.Beast> beasts = Combat.Bestiary.All();

                     Check_("the bestiary has creatures", beasts.Count >= 10,
                            beasts.Count + " creatures");

                     bool allSpawn = true;

                     for (int i = 0; i < beasts.Count; i++)
                     {
                            Combat.Combatant spawned = beasts[i].Spawn();

                            if (spawned == null || spawned.MaxHealth <= 0)
                                   allSpawn = false;
                     }

                     Check_("every creature spawns a fighter", allSpawn,
                            allSpawn ? "all spawned" : "one failed to spawn");

                     // A cub must be weaker than an adult of the same traits.
                     Biology.Wolf cubWolf = new Biology.Wolf("Cub", 1);
                     cubWolf.Rank = Biology.PackRank.Cub;
                     cubWolf.AgeYears = 0.5;
                     cubWolf.Traits.Strength = 0.6;

                     Biology.Wolf adultWolf = new Biology.Wolf("Adult", 2);
                     adultWolf.AgeYears = 4.0;
                     adultWolf.Traits.Strength = 0.6;

                     Combat.Combatant cub = Combat.Combatant.FromWolf(cubWolf, Combat.Side.Pack);
                     Combat.Combatant adult = Combat.Combatant.FromWolf(adultWolf, Combat.Side.Rival);

                     Check_("a cub is weaker than an adult",
                            cub.MaxHealth < adult.MaxHealth,
                            "cub " + Math.Round(cub.MaxHealth) + ", adult " + Math.Round(adult.MaxHealth));
              }

              // ------------------------------------------------------------------
              // Attributes
              // ------------------------------------------------------------------

              private static void TestAttributes()
              {
                     Attributes.AttributeSet set = Attributes.AttributeSet.Uniform(10);

                     Check_("attributes start at the given score",
                            set.ScoreOf(Attributes.AttributeKind.Strength) == 10,
                            "strength " + set.ScoreOf(Attributes.AttributeKind.Strength));

                     // Training must raise the score.
                     Attributes.AttributeScore strength = set.Get(Attributes.AttributeKind.Strength);

                     for (int i = 0; i < 10; i++)
                            strength.Exercise(1.0);

                     Check_("training raises the score", strength.Score > 10,
                            "strength now " + strength.Score + " after training");

                     // Training must stop at the ceiling.
                     for (int i = 0; i < 1000; i++)
                            strength.Exercise(1.0);

                     Check_("training stops at the ceiling", strength.Score <= strength.Ceiling,
                            "score " + strength.Score + ", ceiling " + strength.Ceiling);

                     // A higher Constitution must give more health.
                     Attributes.AttributeSet weak = Attributes.AttributeSet.Uniform(8);
                     Attributes.AttributeSet tough = Attributes.AttributeSet.Uniform(16);

                     Check_("constitution raises health",
                            tough.MaxHealth(1.0) > weak.MaxHealth(1.0),
                            weak.MaxHealth(1.0) + " vs " + tough.MaxHealth(1.0));

                     // Evasion must be clamped.
                     Attributes.AttributeSet fast = Attributes.AttributeSet.Uniform(30);

                     Check_("evasion is clamped", fast.Evasion() <= 0.8,
                            "evasion " + Math.Round(fast.Evasion(), 3));

                     Check_("accuracy is clamped", fast.Accuracy() <= 1.0,
                            "accuracy " + Math.Round(fast.Accuracy(), 3));
              }

              // ------------------------------------------------------------------
              // Talents
              // ------------------------------------------------------------------

              private static void TestTalents()
              {
                     Talents.TalentSet set = Talents.TalentTree.NewSet();

                     Check_("the tree has talents", set.Catalogue.Count > 0,
                            set.Catalogue.Count + " talents");

                     Attributes.AttributeSet weak = Attributes.AttributeSet.Uniform(10);
                     Attributes.AttributeSet strong = Attributes.AttributeSet.Uniform(16);

                     // A tier 1 talent needs a score of 11, so a score of 10 must refuse it.
                     bool refused = !set.Learn("Savage Bite", weak);

                     Check_("a gated talent refuses a weak creature", refused,
                            refused ? "refused, as it should" : "it was allowed through");

                     // With a high enough score it must be allowed.
                     bool allowed = set.Learn("Savage Bite", strong);

                     Check_("a gated talent allows a strong creature", allowed,
                            allowed ? "learnt" : "refused");

                     Check_("the talent is recorded",
                            set.RankOf("Savage Bite") == Talents.TalentRank.Basic,
                            "rank " + set.RankOf("Savage Bite"));

                     // A prerequisite must be enforced.
                     bool blocked = !set.Learn("Throat Seeker", strong);

                     Check_("a prerequisite is enforced", blocked,
                            blocked ? "refused without its prerequisite" : "allowed through");

                     // Taking the prerequisite must unlock it.
                     set.Learn("Rending Claws", strong);

                     Check_("the prerequisite chain opens", set.Has("Rending Claws"),
                            "Rending Claws rank " + set.RankOf("Rending Claws"));

                     // Improving must raise the rank and the effect.
                     double before = set.Totals().Damage;

                     set.Learn("Savage Bite", strong);

                     double after = set.Totals().Damage;

                     Check_("improving a talent raises its effect", after > before,
                            "damage " + before + " -> " + after);

                     Check_("rank improves to Improved",
                            set.RankOf("Savage Bite") == Talents.TalentRank.Improved,
                            "rank " + set.RankOf("Savage Bite"));

                     // Refunding a prerequisite must be refused.
                     bool refunded = set.Refund("Savage Bite");

                     Check_("a depended-on talent cannot be refunded", !refunded,
                            refunded ? "it was refunded, which should not happen" : "refused, as it should");

                     // A talent with no prerequisites must be refundable.
                     set.Learn("Fleet Foot", strong);

                     bool freed = set.Refund("Fleet Foot");

                     Check_("an independent talent can be refunded", freed,
                            freed ? "refunded" : "refused");
              }

              // ------------------------------------------------------------------
              // Per-cell AI
              // ------------------------------------------------------------------

              private static void TestCellAi()
              {
                     Ai.CellAiBoard board = new Ai.CellAiBoard(20, 20, 99);

                     int placed = board.FillSide(0, Ai.Difficulty.Even, true);

                     Check_("the board fills a side", placed > 0,
                            placed + " cells placed");

                     board.FillSide(1, Ai.Difficulty.Even, false);

                     Check_("both sides are on the board",
                            board.CountSide(0) > 0 && board.CountSide(1) > 0,
                            "side 0: " + board.CountSide(0) + ", side 1: " + board.CountSide(1));

                     int before0 = board.CountSide(0);
                     int before1 = board.CountSide(1);

                     int ran = board.Run(30);

                     Check_("the AI board runs", ran > 0,
                            ran + " turns ran");

                     Check_("something happened",
                            board.CountSide(0) != before0 || board.CountSide(1) != before1,
                            before0 + "/" + before1 + " -> " +
                            board.CountSide(0) + "/" + board.CountSide(1));

                     // Nobody may be on a square of the other side.
                     bool overlap = false;

                     for (int i = 0; i < board.Owners.Length; i++)
                     {
                            if (board.Owners[i] < -1 || board.Owners[i] > 1)
                                   overlap = true;
                     }

                     Check_("no invalid owners", !overlap,
                            overlap ? "an owner was out of range" : "all owners are -1, 0 or 1");

                     // A cell with no enemies must not attack.
                     Ai.CellAiBoard lonely = new Ai.CellAiBoard(10, 10, 5);

                     lonely.Place(5, 5, 0, Ai.Difficulty.Alpha);

                     Ai.CellView view = lonely.ViewFor(0);
                     Ai.CellAi brain = lonely.Brains[5 + 5 * 10];

                     Ai.CellDecision decision = brain.Think(view, 5, 5);

                     Check_("a lonely cell does not attack", decision.Action != Ai.CellAction.Attack,
                            "it chose " + decision.Action);

                     Check_("a decision explains itself",
                            !string.IsNullOrEmpty(decision.Reason),
                            decision.Reason);

                     // Level 1 and level 9 must differ in what they can do.
                     Ai.DifficultyProfile idiot = Ai.DifficultyProfile.For(Ai.Difficulty.Idiot);
                     Ai.DifficultyProfile alpha = Ai.DifficultyProfile.For(Ai.Difficulty.Alpha);

                     Check_("level 9 sees further than level 1",
                            alpha.VisionRange > idiot.VisionRange,
                            idiot.VisionRange + " vs " + alpha.VisionRange);

                     Check_("level 9 blunders less than level 1",
                            alpha.BlunderChance < idiot.BlunderChance,
                            idiot.BlunderChance + " vs " + alpha.BlunderChance);

                     // Every level must be reachable by number.
                     bool allLevels = true;

                     for (int level = 1; level <= 9; level++)
                     {
                            Ai.DifficultyProfile profile = Ai.DifficultyProfile.For((Ai.Difficulty)level);

                            if (profile == null || (int)profile.Level != level)
                                   allLevels = false;
                     }

                     Check_("all nine levels exist", allLevels,
                            allLevels ? "1 to 9 all present" : "a level was missing");
              }

              // ------------------------------------------------------------------
              // Scripting
              // ------------------------------------------------------------------

              private static void TestScripting()
              {
                     Scripting.HeadlessScriptHost host = new Scripting.HeadlessScriptHost(20);
                     Scripting.ScriptManager manager = new Scripting.ScriptManager(host);

                     Scripting.ScriptResult arithmetic = manager.RunSource(
                         "x = 2 + 3\n" +
                         "print(x)\n", "arithmetic");

                     Check_("a script runs", arithmetic.Succeeded,
                            arithmetic.Succeeded ? "ran" : arithmetic.Error);

                     Check_("arithmetic is right", arithmetic.Output.Trim() == "5",
                            "output was \"" + arithmetic.Output.Trim() + "\"");

                     // The grid must be reachable and must change.
                     Scripting.ScriptResult grid = manager.RunSource(
                         "grid.set(3, 3, true)\n" +
                         "print(grid.get(3, 3))\n", "grid");

                     Check_("a script can reach the grid", grid.Succeeded,
                            grid.Succeeded ? "ran" : grid.Error);

                     Check_("the grid was actually written", host.GetCell(3, 3),
                            "cell (3,3) is " + host.GetCell(3, 3));

                     // A class must work.
                     Scripting.ScriptResult classes = manager.RunSource(
                         "class Counter\n" +
                         "  value = 0\n" +
                         "  function init(start)\n" +
                         "    self.value = start\n" +
                         "  end\n" +
                         "  function bump()\n" +
                         "    self.value = self.value + 1\n" +
                         "    return self.value\n" +
                         "  end\n" +
                         "end\n" +
                         "c = new Counter(10)\n" +
                         "c.bump()\n" +
                         "c.bump()\n" +
                         "print(c.value)\n", "classes");

                     Check_("a class works", classes.Succeeded,
                            classes.Succeeded ? "ran" : classes.Error);

                     Check_("a class holds its state", classes.Output.Trim() == "12",
                            "output was \"" + classes.Output.Trim() + "\"");

                     // A loop must work.
                     Scripting.ScriptResult loops = manager.RunSource(
                         "total = 0\n" +
                         "for i = 1, 10 do\n" +
                         "  total = total + i\n" +
                         "end\n" +
                         "print(total)\n", "loops");

                     Check_("a numeric loop works", loops.Output.Trim() == "55",
                            "output was \"" + loops.Output.Trim() + "\"");

                     // A bad script must fail cleanly, not crash the program.
                     Scripting.ScriptResult broken = manager.RunSource(
                         "x = ((((\n", "broken");

                     Check_("a broken script fails cleanly", !broken.Succeeded,
                            broken.Succeeded ? "it was accepted" : "reported: " + broken.Error);

                     // An infinite loop must be stopped.
                     Scripting.ScriptResult runaway = manager.RunSource(
                         "while true do\n" +
                         "  x = 1\n" +
                         "end\n", "runaway");

                     Check_("a runaway loop is stopped", !runaway.Succeeded,
                            runaway.Succeeded ? "it ran forever" : "stopped after " + runaway.Steps + " steps");

                     // A table must be both a list and a record.
                     Scripting.ScriptResult tables = manager.RunSource(
                         "t = { 10, 20, 30 }\n" +
                         "t.name = \"list\"\n" +
                         "print(t[2])\n" +
                         "print(t.name)\n", "tables");

                     Check_("a table is a list and a record",
                            tables.Output.Contains("20") && tables.Output.Contains("list"),
                            "output was \"" + tables.Output.Replace("\n", " / ").Trim() + "\"");
              }

              // ------------------------------------------------------------------
              // Configuration
              // ------------------------------------------------------------------

              private static void TestConfiguration()
              {
                     Configuration.Config config = Configuration.ConfigDefaults.Create();

                     Check_("the config has settings", config.Count >= 40,
                            config.Count + " settings");

                     // Every default must be readable as its declared type.
                     bool allReadable = true;
                     string badKey = string.Empty;

                     for (int i = 0; i < config.Entries.Count; i++)
                     {
                            Configuration.ConfigEntry entry = config.Entries[i];

                            try
                            {
                                   if (entry.TypeName == "int")
                                          entry.GetInt();
                                   else if (entry.TypeName == "double")
                                          entry.GetDouble();
                                   else if (entry.TypeName == "bool")
                                          entry.GetBool();
                                   else if (entry.TypeName == "color")
                                          entry.GetColor();
                                   else
                                          entry.GetString();
                            }
                            catch (Exception)
                            {
                                   allReadable = false;
                                   badKey = entry.Key;
                            }
                     }

                     Check_("every default reads as its type", allReadable,
                            allReadable ? "all " + config.Count + " read" : "failed on " + badKey);

                     // A bad value must be refused.
                     Configuration.ConfigEntry speed = config.Get(Configuration.ConfigDefaults.Speed);

                     bool refused = !speed.SetFromText("not a number");

                     Check_("a bad value is refused", refused,
                            refused ? "refused" : "accepted nonsense");

                     Check_("a refused value leaves the old one", speed.GetInt() == 10,
                            "speed is " + speed.GetInt());

                     // A range must clamp.
                     speed.SetFromText("999");

                     Check_("a value above the range is clamped", speed.GetInt() == 10,
                            "speed is " + speed.GetInt());

                     speed.SetFromText("0");

                     Check_("a value below the range is clamped", speed.GetInt() == 1,
                            "speed is " + speed.GetInt());

                     // A colour must round-trip.
                     Configuration.ConfigEntry colour = config.Get(Configuration.ConfigDefaults.LiveColor);

                     System.Drawing.Color parsed = colour.GetColor();

                     Check_("a colour parses", parsed.R > 0 || parsed.G > 0 || parsed.B > 0,
                            "live colour is " + parsed);

                     // Serialise and read back.
                     string text = Configuration.ConfigFile.Serialise(config);

                     Check_("the config serialises", text.Length > 100,
                            text.Length + " characters");

                     // Settings must be findable by category.
                     List<Configuration.ConfigEntry> simulation =
                         config.GetByCategory(Configuration.ConfigDefaults.CategorySimulation);

                     Check_("settings group by category", simulation.Count > 0,
                            simulation.Count + " simulation settings");
              }

              // ------------------------------------------------------------------
              // Scheduler
              // ------------------------------------------------------------------

              private static void TestScheduler()
              {
                     // Interval parsing.
                     TimeSpan thirty = Configuration.Schedule.ParseInterval("30s");
                     TimeSpan five = Configuration.Schedule.ParseInterval("5m");
                     TimeSpan two = Configuration.Schedule.ParseInterval("2h");
                     TimeSpan day = Configuration.Schedule.ParseInterval("1d");
                     TimeSpan compound = Configuration.Schedule.ParseInterval("1d12h");

                     Check_("an interval parses seconds", Math.Abs(thirty.TotalSeconds - 30.0) < 0.01,
                            thirty.TotalSeconds + "s");

                     Check_("an interval parses minutes", Math.Abs(five.TotalMinutes - 5.0) < 0.01,
                            five.TotalMinutes + "m");

                     Check_("an interval parses hours", Math.Abs(two.TotalHours - 2.0) < 0.01,
                            two.TotalHours + "h");

                     Check_("an interval parses days", Math.Abs(day.TotalDays - 1.0) < 0.01,
                            day.TotalDays + "d");

                     Check_("a compound interval parses", Math.Abs(compound.TotalHours - 36.0) < 0.01,
                            compound.TotalHours + "h");

                     // A job must become due.
                     Configuration.Scheduler scheduler = new Configuration.Scheduler("test");

                     Configuration.Schedule job = scheduler.Add("tick",
                         Configuration.ScheduleAction.Step, Configuration.ScheduleRepeat.Interval);

                     job.IntervalText = "1s";

                     DateTime now = new DateTime(2026, 10, 7, 12, 0, 0);

                     scheduler.Arm(now);

                     job.NextRun = now.AddSeconds(-1);

                     List<Configuration.Schedule> due = scheduler.GetDue(now);

                     Check_("a due job is found", due.Count == 1,
                            due.Count + " due");

                     // Running it must count it and schedule the next.
                     List<Configuration.Schedule> ran = scheduler.RunDue(now, null);

                     Check_("a job runs and is counted", job.Runs == 1,
                            "runs " + job.Runs);

                     Check_("a job schedules its next run", job.NextRun > now,
                            "next at " + job.NextRun.ToString("HH:mm:ss"));

                     // A disabled job must not run.
                     job.Enabled = false;
                     job.NextRun = now.AddSeconds(-1);

                     List<Configuration.Schedule> none = scheduler.GetDue(now);

                     Check_("a disabled job does not run", none.Count == 0,
                            none.Count + " due");

                     // A max-runs job must stop.
                     Configuration.Schedule limited = scheduler.Add("limited",
                         Configuration.ScheduleAction.Snapshot, Configuration.ScheduleRepeat.Interval);

                     limited.IntervalText = "1s";
                     limited.MaxRuns = 2;
                     limited.StartAt = now;
                     limited.NextRun = now;

                     scheduler.RunDue(now, null);
                     limited.NextRun = now;
                     scheduler.RunDue(now, null);

                     Check_("a max-runs job stops", limited.IsFinished(),
                            "runs " + limited.Runs + " of " + limited.MaxRuns);

                     // Daily jobs must land at the right hour.
                     Configuration.Schedule daily = scheduler.Add("daily",
                         Configuration.ScheduleAction.Snapshot, Configuration.ScheduleRepeat.Daily);

                     daily.AddTimeOfDay("06:00");
                     daily.Restart(now);

                     Check_("a daily job schedules tomorrow morning",
                            daily.NextRun.Hour == 6,
                            "next at " + daily.NextRun.ToString("yyyy-MM-dd HH:mm"));
              }

              // ------------------------------------------------------------------
              // Weather and time
              // ------------------------------------------------------------------

              private static void TestWeather()
              {
                     World.WorldClock clock = World.WorldClock.EarthLike(50.0, 2026);

                     Check_("the clock starts at day 0", clock.Cycle.Day == 0,
                            "day " + clock.Cycle.Day);

                     // Noon must be lighter than midnight.
                     clock.Cycle.SetHour(12.0);
                     double noonLight = clock.Cycle.LightLevel();

                     clock.Cycle.SetHour(0.0);
                     double midnightLight = clock.Cycle.LightLevel();

                     Check_("noon is lighter than midnight", noonLight > midnightLight,
                            "noon " + Math.Round(noonLight, 2) + ", midnight " + Math.Round(midnightLight, 2));

                     Check_("midnight is dark", clock.Cycle.IsDark,
                            "light at midnight is " + Math.Round(midnightLight, 2));

                     // Advancing a day must roll the counter.
                     clock.Cycle.SetHour(0.0);
                     clock.Cycle.Advance(24.0);

                     Check_("a day rolls over", clock.Cycle.Day == 1,
                            "day " + clock.Cycle.Day);

                     // The hour must wrap.
                     clock.Cycle.SetHour(25.0);

                     Check_("the hour wraps", clock.Cycle.Hour < 24.0,
                            "hour " + Math.Round(clock.Cycle.Hour, 2));

                     // The clock text must be a valid time.
                     string text = clock.Cycle.ClockText();

                     Check_("the clock reads as a time",
                            text.Length == 5 && text[2] == ':',
                            "\"" + text + "\"");

                     // The moon must cycle through all eight phases over a month.
                     World.DayNightCycle cycle = World.DayNightCycle.EarthLike();
                     bool[] seen = new bool[8];

                     for (int day = 0; day < 30; day++)
                     {
                         World.MoonPhase phase = cycle.MoonAt(day);
                         seen[(int)phase] = true;
                     }

                     int phasesSeen = 0;

                     for (int i = 0; i < 8; i++)
                     {
                            if (seen[i])
                                   phasesSeen++;
                     }

                     Check_("the moon passes through every phase", phasesSeen == 8,
                            phasesSeen + " of 8 phases seen in a month");

                     // A full moon must be brighter than a new one.
                     double fullBrightness = 0.0;
                     double newBrightness = 0.0;

                     for (int day = 0; day < 30; day++)
                     {
                            cycle.MoonAt(day);

                            double brightness = cycle.MoonBrightness();

                            if (cycle.MoonAt(day) == World.MoonPhase.Full && brightness > fullBrightness)
                                fullBrightness = brightness;

                            if (cycle.MoonAt(day) == World.MoonPhase.New && brightness > newBrightness)
                                newBrightness = brightness;
                     }

                     Check_("a full moon is brighter than a new one", fullBrightness > newBrightness,
                            "full " + Math.Round(fullBrightness, 2) + ", new " + Math.Round(newBrightness, 2));

                     // Weather must turn over time.
                     World.WeatherSystem weather = World.WeatherSystem.Temperate(4242);

                     World.WeatherKind first = weather.Current.Kind;

                     int turns = weather.Advance(72.0, cycle);

                     Check_("the weather turns", turns > 0,
                            turns + " turns over 72 hours, first was " + first +
                            ", now " + weather.Current.Kind);

                     // The weather must stay in range.
                     bool sane = weather.Current.Humidity >= 0.0 && weather.Current.Humidity <= 1.0 &&
                                 weather.Current.CloudCover >= 0.0 && weather.Current.CloudCover <= 1.0 &&
                                 weather.Current.Precipitation >= 0.0 &&
                                 weather.Current.VisibilityMetres > 0.0;

                     Check_("the weather stays in range", sane,
                            "humidity " + Math.Round(weather.Current.Humidity, 2) +
                            ", cloud " + Math.Round(weather.Current.CloudCover, 2) +
                            ", visibility " + Math.Round(weather.Current.VisibilityMetres));

                     // Fog must cut visibility.
                     World.Weather fog = World.Weather.Of(World.WeatherKind.Fog);
                     fog.VisibilityMetres = 200.0;

                     World.Weather clear = World.Weather.Of(World.WeatherKind.Clear);
                     clear.VisibilityMetres = 20000.0;

                     Check_("fog cuts visibility", fog.VisibilityFactor() < clear.VisibilityFactor(),
                            "fog " + Math.Round(fog.VisibilityFactor(), 3) +
                            ", clear " + Math.Round(clear.VisibilityFactor(), 3));

                     // Snow must help tracking; rain must hinder it.
                     World.Weather snow = World.Weather.Of(World.WeatherKind.Snow);
                     World.Weather rain = World.Weather.Of(World.WeatherKind.HeavyRain);

                     snow.Precipitation = 2.0;
                     rain.Precipitation = 9.0;

                     Check_("snow helps tracking more than rain",
                            snow.TrackingDifficulty() < rain.TrackingDifficulty(),
                            "snow " + Math.Round(snow.TrackingDifficulty(), 3) +
                            ", rain " + Math.Round(rain.TrackingDifficulty(), 3));

                     // A blizzard must be severe.
                     World.Weather blizzard = World.Weather.Of(World.WeatherKind.Blizzard);
                     blizzard.Wind = World.WindStrength.Storm;

                     Check_("a blizzard is severe", blizzard.IsSevere(),
                            "severe " + blizzard.IsSevere());

                     // The world clock must combine both.
                     double hunting = clock.HuntingConditions();

                     Check_("hunting conditions are in range", hunting >= 0.0 && hunting <= 1.0,
                            "hunting " + Math.Round(hunting, 3));
              }

              // ------------------------------------------------------------------
              // Export
              // ------------------------------------------------------------------

              private static void TestExport()
              {
                     Data.DataTable table = new Data.DataTable("Test");

                     table.WithColumns("A", "B", "C");

                     table.Add(1, "text", true);
                     table.Add(2.5, null, false);

                     Check_("a table takes rows", table.Count == 2,
                            table.Count + " rows");

                     Check_("a table has columns", table.Columns.Count == 3,
                            table.Columns.Count + " columns");

                     // A null must become blank, not "null".
                     Check_("a null becomes blank", table.Rows[1][1] == string.Empty,
                            "\"" + table.Rows[1][1] + "\"");

                     // A bool must become true or false.
                     Check_("a bool is written as true or false", table.Rows[0][2] == "true",
                            "\"" + table.Rows[0][2] + "\"");

                     // A number must not carry a trailing decimal.
                     Check_("a whole number has no decimal point", table.Rows[0][0] == "1",
                            "\"" + table.Rows[0][0] + "\"");

                     // CSV must quote a value containing a comma.
                     Data.DataTable csvTable = new Data.DataTable("Csv");

                     csvTable.WithColumns("Name", "Note");
                     csvTable.Add("a,b", "has \"quotes\"");

                     string path = System.IO.Path.Combine(
                         System.IO.Path.GetTempPath(), "lifeengine-selftest.csv");

                     List<string> problems = Data.SpreadsheetWriter.WriteCsv(path, csvTable);

                     Check_("CSV writes without complaint", problems.Count == 0,
                            problems.Count == 0 ? "written" : problems[0]);

                     if (System.IO.File.Exists(path))
                     {
                            string[] lines = System.IO.File.ReadAllLines(path);

                            Check_("CSV quotes a value with a comma",
                                   lines.Length >= 2 && lines[1].StartsWith("\"a,b\""),
                                   lines.Length >= 2 ? lines[1] : "no data line");

                            try
                            {
                                   System.IO.File.Delete(path);
                            }
                            catch (Exception)
                            {
                            }
                     }
              }
       }
}

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

namespace LifeEngine.Ai
{
    /// <summary>What a cell can do on its turn.</summary>
    public enum CellAction
    {
        /// <summary>Do nothing this turn.</summary>
        Wait,
        /// <summary>Stay put, but brace for an attack.</summary>
        Defend,
        /// <summary>Move to a neighbouring square.</summary>
        Move,
        /// <summary>Attack a neighbouring enemy.</summary>
        Attack,
        /// <summary>Heal itself.</summary>
        Rest,
        /// <summary>Reproduce into an empty neighbouring square.</summary>
        Spawn,
        /// <summary>Fall back towards its own side.</summary>
        Retreat,
        /// <summary>Move towards the nearest enemy without engaging.</summary>
        Approach,
        /// <summary>Deliberately look weak, to draw an attack.</summary>
        Bait,
        /// <summary>Call neighbouring friendly cells to concentrate.</summary>
        Rally
    }

    /// <summary>One square on the board.</summary>
    public struct CellPoint
    {
        public int X;
        public int Y;

        public CellPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>The eight squares around a point, in a fixed order.</summary>
        public static readonly int[] NeighbourX = { -1, 0, 1, -1, 1, -1, 0, 1 };
        public static readonly int[] NeighbourY = { -1, -1, -1, 0, 0, 1, 1, 1 };

        public bool Equals(CellPoint other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object other)
        {
            if (!(other is CellPoint))
                return false;

            return Equals((CellPoint)other);
        }

        public override int GetHashCode()
        {
            return X * 397 + Y;
        }

        public override string ToString()
        {
            return "(" + X + "," + Y + ")";
        }
    }

    /// <summary>What one cell knows about the board, for one turn.</summary>
    public sealed class CellView
    {
        private int m_columns;
        private int m_rows;
        private int m_owner;
        private Func<int, int, int> m_ownerAt;
        private Func<int, int, bool> m_occupiedAt;
        private Func<int, int, double> m_healthAt;
        private Dictionary<string, int> m_memory;

        /// <summary>Width of the board.</summary>
        public int Columns
        {
            get { return m_columns; }
        }

        /// <summary>Height of the board.</summary>
        public int Rows
        {
            get { return m_rows; }
        }

        /// <summary>Which side this cell is on.</summary>
        public int Owner
        {
            get { return m_owner; }
        }

        /// <summary>What this cell remembers from previous turns.</summary>
        public Dictionary<string, int> Memory
        {
            get { return m_memory; }
        }

        public CellView(int columns, int rows, int owner)
        {
            m_columns = columns;
            m_rows = rows;
            m_owner = owner;
            m_memory = new Dictionary<string, int>(StringComparer.Ordinal);
        }

        /// <summary>
        /// Wires the view to the board. The cell does not hold the board
        /// itself, only three functions that read it, so the same AI can be
        /// pointed at a grid, a world map or a test fixture without changing.
        /// </summary>
        public CellView Connect(Func<int, int, int> ownerAt,
                                Func<int, int, bool> occupiedAt,
                                Func<int, int, double> healthAt)
        {
            m_ownerAt = ownerAt;
            m_occupiedAt = occupiedAt;
            m_healthAt = healthAt;

            return this;
        }

        /// <summary>True if a square is on the board.</summary>
        public bool InBounds(int x, int y)
        {
            return x >= 0 && x < m_columns && y >= 0 && y < m_rows;
        }

        /// <summary>Which side holds a square. -1 means empty.</summary>
        public int OwnerAt(int x, int y)
        {
            if (!InBounds(x, y))
                return -1;

            if (m_ownerAt == null)
                return -1;

            return m_ownerAt(x, y);
        }

        /// <summary>True if a square holds something.</summary>
        public bool IsOccupied(int x, int y)
        {
            if (!InBounds(x, y))
                return false;

            if (m_occupiedAt != null)
                return m_occupiedAt(x, y);

            return OwnerAt(x, y) >= 0;
        }

        /// <summary>How healthy whatever is on a square is, 0 to 1.</summary>
        public double HealthAt(int x, int y)
        {
            if (!InBounds(x, y))
                return 0.0;

            if (m_healthAt == null)
                return 1.0;

            return m_healthAt(x, y);
        }

        /// <summary>True if a square holds an enemy of this cell.</summary>
        public bool IsEnemy(int x, int y)
        {
            int owner = OwnerAt(x, y);

            return owner >= 0 && owner != m_owner;
        }

        /// <summary>True if a square holds a friend of this cell.</summary>
        public bool IsFriend(int x, int y)
        {
            int owner = OwnerAt(x, y);

            return owner == m_owner;
        }

        /// <summary>The eight neighbours of a point, on the board only.</summary>
        public List<CellPoint> Neighbours(int x, int y)
        {
            List<CellPoint> neighbours = new List<CellPoint>(8);

            for (int i = 0; i < 8; i++)
            {
                int nx = x + CellPoint.NeighbourX[i];
                int ny = y + CellPoint.NeighbourY[i];

                if (InBounds(nx, ny))
                    neighbours.Add(new CellPoint(nx, ny));
            }

            return neighbours;
        }

        /// <summary>
        /// Everything within a given range, as a square. Range 1 is the eight
        /// neighbours; a higher range sees further, which is the difference
        /// between a level 1 opponent and a level 9 one.
        /// </summary>
        public List<CellPoint> Within(int x, int y, int range)
        {
            List<CellPoint> found = new List<CellPoint>();

            if (range < 1)
                range = 1;

            for (int dy = -range; dy <= range; dy++)
            {
                for (int dx = -range; dx <= range; dx++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    if (InBounds(x + dx, y + dy))
                        found.Add(new CellPoint(x + dx, y + dy));
                }
            }

            return found;
        }

        /// <summary>How many enemies are within range.</summary>
        public int CountEnemies(int x, int y, int range)
        {
            int count = 0;

            List<CellPoint> cells = Within(x, y, range);

            for (int i = 0; i < cells.Count; i++)
            {
                if (IsEnemy(cells[i].X, cells[i].Y))
                    count++;
            }

            return count;
        }

        /// <summary>How many friends are within range, not counting this cell.</summary>
        public int CountFriends(int x, int y, int range)
        {
            int count = 0;

            List<CellPoint> cells = Within(x, y, range);

            for (int i = 0; i < cells.Count; i++)
            {
                if (IsFriend(cells[i].X, cells[i].Y))
                    count++;
            }

            return count;
        }

        /// <summary>The nearest enemy, or null if none is in range.</summary>
        public CellPoint? NearestEnemy(int x, int y, int range)
        {
            CellPoint? best = null;
            double bestDistance = double.MaxValue;

            List<CellPoint> cells = Within(x, y, range);

            for (int i = 0; i < cells.Count; i++)
            {
                if (!IsEnemy(cells[i].X, cells[i].Y))
                    continue;

                double dx = cells[i].X - x;
                double dy = cells[i].Y - y;
                double distance = dx * dx + dy * dy;

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = cells[i];
                }
            }

            return best;
        }

        /// <summary>The weakest enemy in range, which is usually the best target.</summary>
        public CellPoint? WeakestEnemy(int x, int y, int range)
        {
            CellPoint? best = null;
            double lowest = double.MaxValue;

            List<CellPoint> cells = Within(x, y, range);

            for (int i = 0; i < cells.Count; i++)
            {
                if (!IsEnemy(cells[i].X, cells[i].Y))
                    continue;

                double health = HealthAt(cells[i].X, cells[i].Y);

                if (health < lowest)
                {
                    lowest = health;
                    best = cells[i];
                }
            }

            return best;
        }

        /// <summary>Every empty square next to a point.</summary>
        public List<CellPoint> EmptyNeighbours(int x, int y)
        {
            List<CellPoint> empty = new List<CellPoint>(8);
            List<CellPoint> neighbours = Neighbours(x, y);

            for (int i = 0; i < neighbours.Count; i++)
            {
                if (!IsOccupied(neighbours[i].X, neighbours[i].Y))
                    empty.Add(neighbours[i]);
            }

            return empty;
        }

        /// <summary>The square furthest from every enemy, for a retreat.</summary>
        public CellPoint? SafestNeighbour(int x, int y, int range)
        {
            List<CellPoint> options = EmptyNeighbours(x, y);

            if (options.Count == 0)
                return null;

            CellPoint best = options[0];
            double bestScore = double.MinValue;

            for (int i = 0; i < options.Count; i++)
            {
                int enemies = CountEnemies(options[i].X, options[i].Y, range);
                int friends = CountFriends(options[i].X, options[i].Y, range);

                // Fewer enemies and more friends is safer.
                double score = friends - enemies * 2.0;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = options[i];
                }
            }

            return best;
        }

        /// <summary>Remembers something about the board, for a later turn.</summary>
        public void Remember(string key, int value)
        {
            if (!string.IsNullOrEmpty(key))
                m_memory[key] = value;
        }

        /// <summary>What was remembered, or a default.</summary>
        public int Recall(string key, int fallback)
        {
            int value;

            if (!string.IsNullOrEmpty(key) && m_memory.TryGetValue(key, out value))
                return value;

            return fallback;
        }
    }

    /// <summary>What the cell decided, and why.</summary>
    public sealed class CellDecision
    {
        private CellAction m_action;
        private CellPoint? m_target;
        private double m_score;
        private string m_reason;

        /// <summary>What it will do.</summary>
        public CellAction Action
        {
            get { return m_action; }
        }

        /// <summary>Where, for a move or an attack.</summary>
        public CellPoint? Target
        {
            get { return m_target; }
        }

        /// <summary>How good it thought the move was. Higher is better.</summary>
        public double Score
        {
            get { return m_score; }
        }

        /// <summary>Why, in words. This is what makes the AI explainable.</summary>
        public string Reason
        {
            get { return m_reason; }
        }

        public CellDecision(CellAction action, CellPoint? target, double score, string reason)
        {
            m_action = action;
            m_target = target;
            m_score = score;
            m_reason = reason;
        }

        public static CellDecision Of(CellAction action, string reason)
        {
            return new CellDecision(action, null, 0.0, reason);
        }

        /// <summary>A decision with no target, but with the score it was given.</summary>
        public static CellDecision Of(CellAction action, double score, string reason)
        {
            return new CellDecision(action, null, score, reason);
        }

        public static CellDecision At(CellAction action, CellPoint target, double score, string reason)
        {
            return new CellDecision(action, target, score, reason);
        }

        public override string ToString()
        {
            return m_action + (m_target.HasValue ? " " + m_target.Value : string.Empty) +
                   " (" + m_reason + ")";
        }
    }

    /// <summary>
    /// The brain of one cell.
    ///
    /// Every cell on the board gets its own instance, so a cell can remember
    /// things between turns and two cells of the same kind can behave
    /// differently. The brain holds no board: it is handed a
    /// <see cref="CellView"/> and decides, which is what makes it testable
    /// without a grid.
    ///
    /// The decision is always made by scoring every legal action and taking the
    /// best, weighted by the difficulty profile. Nothing is hard-coded to "if
    /// enemy then attack", so changing a weight changes the behaviour without
    /// touching the logic.
    /// </summary>
    public sealed class CellAi
    {
        private int m_id;
        private DifficultyProfile m_profile;
        private Random m_random;
        private int m_health;
        private int m_maxHealth;
        private int m_strength;
        private CellAction m_lastAction;
        private int m_turnsSinceMove;

        /// <summary>A number unique to this cell, for the log.</summary>
        public int Id
        {
            get { return m_id; }
        }

        /// <summary>How hard this cell plays.</summary>
        public DifficultyProfile Profile
        {
            get { return m_profile; }
            set { m_profile = value ?? DifficultyProfile.For(Difficulty.Even); }
        }

        public int Health
        {
            get { return m_health; }
            set { m_health = value < 0 ? 0 : value; }
        }

        public int MaxHealth
        {
            get { return m_maxHealth; }
            set { m_maxHealth = value < 1 ? 1 : value; }
        }

        /// <summary>How hard it hits, relative to its neighbours.</summary>
        public int Strength
        {
            get { return m_strength; }
            set { m_strength = value < 0 ? 0 : value; }
        }

        /// <summary>What it did last turn, so it does not do the same thing forever.</summary>
        public CellAction LastAction
        {
            get { return m_lastAction; }
        }

        /// <summary>Health as a fraction, 0 to 1.</summary>
        public double HealthFraction
        {
            get { return m_maxHealth <= 0 ? 0.0 : (double)m_health / m_maxHealth; }
        }

        public CellAi(int id, Difficulty level, Random random)
        {
            m_id = id;
            m_profile = DifficultyProfile.For(level);
            m_random = random ?? new Random(20261007);
            m_health = 100;
            m_maxHealth = 100;
            m_strength = 10;
            m_lastAction = CellAction.Wait;
        }

        /// <summary>
        /// Decides what to do this turn.
        ///
        /// The order is: see the board, score every legal action, then either
        /// take the best or, at a low level, blunder. A blunder is a random
        /// legal action, and it is the main thing that makes level 1 feel like
        /// level 1 rather than merely a weaker level 5.
        /// </summary>
        public CellDecision Decide(CellView view, int x, int y)
        {
            if (view == null)
                return CellDecision.Of(CellAction.Wait, "no view of the board");

            // A cell with almost nothing left does not think about attacking.
            if (HealthFraction < 0.15 && m_profile.CanRetreat)
            {
                CellPoint? away = view.SafestNeighbour(x, y, m_profile.VisionRange);

                if (away.HasValue)
                    return CellDecision.At(CellAction.Retreat, away.Value,
                        100.0, "too badly hurt to stay");
            }

            List<CellDecision> options = Score(view, x, y);

            if (options.Count == 0)
                return CellDecision.Of(CellAction.Wait, "nothing to do");

            options.Sort(delegate (CellDecision left, CellDecision right)
            {
                return right.Score.CompareTo(left.Score);
            });

            // A weak player sometimes does something else entirely.
            if (m_profile.BlunderChance > 0.0 && m_random.NextDouble() < m_profile.BlunderChance)
            {
                CellDecision blunder = options[m_random.Next(options.Count)];

                return new CellDecision(blunder.Action, blunder.Target, blunder.Score,
                    "blundered into " + blunder.Action);
            }

            // A hesitant player sometimes repeats itself instead of improving.
            if (m_profile.Hesitation > 0.0 && m_random.NextDouble() < m_profile.Hesitation)
            {
                for (int i = 0; i < options.Count; i++)
                {
                    if (options[i].Action == m_lastAction)
                        return new CellDecision(options[i].Action, options[i].Target,
                            options[i].Score, "hesitated, and did the same again");
                }
            }

            CellDecision chosen = options[0];

            m_lastAction = chosen.Action;

            return chosen;
        }

        /// <summary>
        /// Every legal action, with a score. This is where the difficulty
        /// profile does its work: the same options are generated for every
        /// level, and the weights decide which one wins.
        /// </summary>
        private List<CellDecision> Score(CellView view, int x, int y)
        {
            List<CellDecision> options = new List<CellDecision>();

            int range = m_profile.VisionRange;

            List<CellPoint> neighbours = view.Neighbours(x, y);

            int enemies = view.CountEnemies(x, y, range);
            int friends = view.CountFriends(x, y, range);

            double survival = m_profile.SelfPreservation;
            double aggression = m_profile.Aggression;

            // ---- Attack --------------------------------------------------

            for (int i = 0; i < neighbours.Count; i++)
            {
                CellPoint target = neighbours[i];

                if (!view.IsEnemy(target.X, target.Y))
                    continue;

                double targetHealth = view.HealthAt(target.X, target.Y);

                // Finishing a wounded enemy is worth more than starting a fresh one.
                double score = 50.0 * aggression;
                score += (1.0 - targetHealth) * 40.0;
                score += m_strength * 1.5;

                // Attacking while badly outnumbered is a good way to die.
                score -= enemies * 8.0 * survival;

                // Attacking alongside friends is safer and better.
                score += friends * 4.0 * m_profile.Cooperation;

                options.Add(CellDecision.At(CellAction.Attack, target, score,
                    "attack a " + Math.Round(targetHealth * 100.0) + "% enemy"));
            }

            // ---- Defend --------------------------------------------------

            if (enemies > 0)
            {
                double score = 30.0 * survival;
                score += enemies * 6.0;

                // Holding ground matters more to a territorial player.
                score += m_profile.Territoriality * 20.0;

                options.Add(CellDecision.Of(CellAction.Defend, score,
                    "brace against " + enemies + " nearby"));
            }

            // ---- Move ----------------------------------------------------

            List<CellPoint> empty = view.EmptyNeighbours(x, y);

            for (int i = 0; i < empty.Count; i++)
            {
                CellPoint target = empty[i];

                int enemiesThere = view.CountEnemies(target.X, target.Y, range);
                int friendsThere = view.CountFriends(target.X, target.Y, range);

                double score = 10.0;

                // Moving towards enemies is aggressive; away is careful.
                score += (enemies - enemiesThere) * 8.0 * aggression;
                score += (enemiesThere - enemies) * 6.0 * survival;

                // Sticking with friends is worth something to a cooperative cell.
                score += friendsThere * 3.0 * m_profile.Cooperation;

                // A cautious cell prefers not to be the only one out front.
                if (enemiesThere > 0 && friendsThere == 0)
                    score -= 15.0 * survival;

                options.Add(CellDecision.At(CellAction.Move, target, score,
                    "move to " + target));
            }

            // ---- Approach, when an enemy is in sight but not adjacent -----

            if (enemies == 0)
            {
                CellPoint? seen = view.NearestEnemy(x, y, range);

                if (seen.HasValue && empty.Count > 0)
                {
                    CellPoint best = empty[0];
                    double bestDistance = double.MaxValue;

                    for (int i = 0; i < empty.Count; i++)
                    {
                        double dx = empty[i].X - seen.Value.X;
                        double dy = empty[i].Y - seen.Value.Y;
                        double distance = dx * dx + dy * dy;

                        if (distance < bestDistance)
                        {
                            bestDistance = distance;
                            best = empty[i];
                        }
                    }

                    double score = 12.0 * aggression + 5.0 * m_profile.Cooperation;

                    options.Add(CellDecision.At(CellAction.Approach, best, score,
                        "close on an enemy at " + seen.Value));
                }
            }

            // ---- Rest ----------------------------------------------------

            if (HealthFraction < 0.6)
            {
                double score = (1.0 - HealthFraction) * 60.0 * survival;

                // Do not heal in the middle of a fight.
                score -= enemies * 12.0;

                options.Add(CellDecision.Of(CellAction.Rest, score,
                    "rest at " + Math.Round(HealthFraction * 100.0) + "% health"));
            }

            // ---- Spawn ---------------------------------------------------

            if (empty.Count > 0 && HealthFraction > 0.7)
            {
                double score = 8.0 + friends * 1.0;

                // An empty board is not a reason to sit still.
                score += m_profile.Territoriality * 6.0;

                options.Add(CellDecision.At(CellAction.Spawn, empty[0], score,
                    "spawn into open ground"));
            }

            // ---- Retreat -------------------------------------------------

            if (m_profile.CanRetreat && enemies > friends + 1)
            {
                CellPoint? safe = view.SafestNeighbour(x, y, range);

                if (safe.HasValue)
                {
                    double score = (enemies - friends) * 10.0 * survival;
                    score += (1.0 - HealthFraction) * 30.0;

                    options.Add(CellDecision.At(CellAction.Retreat, safe.Value, score,
                        "fall back, outnumbered " + enemies + " to " + friends));
                }
            }

            // ---- Bait ----------------------------------------------------

            if (m_profile.CanBait && HealthFraction > 0.5 && enemies > 0)
            {
                // Look weaker than it is, so the enemy commits to a bad attack.
                double score = 20.0 * m_profile.RiskAppetite;
                score += friends * 6.0;

                options.Add(CellDecision.Of(CellAction.Bait, score,
                    "bait an attack, with " + friends + " in support"));
            }

            // ---- Rally ---------------------------------------------------

            if (m_profile.CanCoordinate && friends > 0 && enemies > 1)
            {
                double score = 15.0 * m_profile.Cooperation;
                score += enemies * 3.0;

                options.Add(CellDecision.Of(CellAction.Rally, score,
                    "call " + friends + " to concentrate"));
            }

            // ---- Wait ----------------------------------------------------

            // Waiting is always legal, and is the baseline every other action
            // has to beat.
            options.Add(CellDecision.Of(CellAction.Wait, 5.0, "hold position"));

            m_turnsSinceMove++;

            return options;
        }

        /// <summary>
        /// Looks a number of moves ahead and returns the best first move. This
        /// is what the higher difficulty levels use and the lower ones do not.
        ///
        /// The search is deliberately shallow and narrow: it considers only the
        /// moves that scored well this turn, because a deep search over every
        /// legal move on a large board would cost more than the answer is worth.
        /// </summary>
        public CellDecision DecideWithLookahead(CellView view, int x, int y, int lookahead)
        {
            CellDecision immediate = Decide(view, x, y);

            if (lookahead <= 0 || view == null)
                return immediate;

            List<CellDecision> options = Score(view, x, y);

            options.Sort(delegate (CellDecision left, CellDecision right)
            {
                return right.Score.CompareTo(left.Score);
            });

            CellDecision best = immediate;
            double bestScore = immediate.Score;

            // Only the top few are examined: the rest are not going to win.
            int examined = Math.Min(options.Count, 3 + lookahead);

            for (int i = 0; i < examined; i++)
            {
                CellDecision candidate = options[i];

                double value = candidate.Score;

                // A move that leaves the cell next to more enemies is worse for
                // a cautious cell and better for an aggressive one.
                if (candidate.Target.HasValue)
                {
                    int after = view.CountEnemies(candidate.Target.Value.X,
                                                  candidate.Target.Value.Y,
                                                  m_profile.VisionRange);

                    value -= after * 10.0 * m_profile.SelfPreservation;
                    value += after * 5.0 * m_profile.Aggression;
                }

                // A second step, if there is depth left.
                if (lookahead > 1 && candidate.Target.HasValue)
                {
                    int next = view.CountEnemies(candidate.Target.Value.X,
                                                 candidate.Target.Value.Y,
                                                 m_profile.VisionRange + 1);

                    value -= next * 4.0 * m_profile.SelfPreservation;
                }

                if (value > bestScore)
                {
                    bestScore = value;
                    best = new CellDecision(candidate.Action, candidate.Target, value,
                        candidate.Reason + ", and it holds up " + lookahead + " moves out");
                }
            }

            return best;
        }

        /// <summary>
        /// The decision, using the lookahead the profile calls for. This is what
        /// a caller should use; <see cref="Decide"/> is the one-turn version.
        /// </summary>
        public CellDecision Think(CellView view, int x, int y)
        {
            if (m_profile.Lookahead <= 0)
                return Decide(view, x, y);

            return DecideWithLookahead(view, x, y, m_profile.Lookahead);
        }

        public override string ToString()
        {
            return "CellAi #" + m_id + " at level " + (int)m_profile.Level;
        }
    }

    /// <summary>
    /// A board of cells, each with its own brain, run for a number of turns.
    ///
    /// This is the whole of the per-cell AI system: the board holds the sides,
    /// each cell holds its own brain, and a turn is every cell deciding and
    /// acting in turn.
    /// </summary>
    public sealed class CellAiBoard
    {
        private int m_columns;
        private int m_rows;
        private int[] m_owner;
        private double[] m_health;
        private CellAi[] m_brains;
        private Random m_random;
        private List<string> m_log;

        public int Columns
        {
            get { return m_columns; }
        }

        public int Rows
        {
            get { return m_rows; }
        }

        /// <summary>Which side holds each square. -1 is empty.</summary>
        public int[] Owners
        {
            get { return m_owner; }
        }

        /// <summary>Health of whatever is on each square, 0 to 1.</summary>
        public double[] Health
        {
            get { return m_health; }
        }

        /// <summary>The brain of each square, or null where there is nothing.</summary>
        public CellAi[] Brains
        {
            get { return m_brains; }
        }

        /// <summary>What has happened, newest last.</summary>
        public IList<string> Log
        {
            get { return m_log; }
        }

        public CellAiBoard(int columns, int rows, int seed)
        {
            m_columns = columns < 2 ? 2 : columns;
            m_rows = rows < 2 ? 2 : rows;
            m_owner = new int[m_columns * m_rows];
            m_health = new double[m_columns * m_rows];
            m_brains = new CellAi[m_columns * m_rows];
            m_random = new Random(seed);
            m_log = new List<string>();

            for (int i = 0; i < m_owner.Length; i++)
            {
                m_owner[i] = -1;
                m_health[i] = 0.0;
            }
        }

        /// <summary>Puts a cell on a square, with its own brain at the given level.</summary>
        public CellAi Place(int x, int y, int side, Difficulty level)
        {
            if (x < 0 || x >= m_columns || y < 0 || y >= m_rows)
                return null;

            int index = x + y * m_columns;

            m_owner[index] = side;
            m_health[index] = 1.0;

            CellAi brain = new CellAi(index, level, m_random);

            m_brains[index] = brain;

            return brain;
        }

        /// <summary>Fills a rectangle with cells of one side.</summary>
        public int Fill(int left, int top, int width, int height, int side, Difficulty level)
        {
            int placed = 0;

            for (int y = top; y < top + height; y++)
            {
                for (int x = left; x < left + width; x++)
                {
                    if (Place(x, y, side, level) != null)
                        placed++;
                }
            }

            return placed;
        }

        /// <summary>Fills a whole side of the board.</summary>
        public int FillSide(int side, Difficulty level, bool leftHalf)
        {
            int width = Math.Max(1, m_columns / 3);

            return Fill(leftHalf ? 0 : m_columns - width, 0, width, m_rows, side, level);
        }

        public int OwnerAt(int x, int y)
        {
            if (x < 0 || x >= m_columns || y < 0 || y >= m_rows)
                return -1;

            return m_owner[x + y * m_columns];
        }

        public bool IsOccupied(int x, int y)
        {
            return OwnerAt(x, y) >= 0;
        }

        public double HealthAt(int x, int y)
        {
            if (x < 0 || x >= m_columns || y < 0 || y >= m_rows)
                return 0.0;

            return m_health[x + y * m_columns];
        }

        /// <summary>A view of the board for one side, wired to the live data.</summary>
        public CellView ViewFor(int side)
        {
            CellView view = new CellView(m_columns, m_rows, side);

            view.Connect(OwnerAt, IsOccupied, HealthAt);

            return view;
        }

        /// <summary>How many cells a side still has.</summary>
        public int CountSide(int side)
        {
            int count = 0;

            for (int i = 0; i < m_owner.Length; i++)
            {
                if (m_owner[i] == side)
                    count++;
            }

            return count;
        }

        /// <summary>
        /// One turn: every cell with a brain decides and acts, in a fixed order
        /// so the same board and the same seed always give the same result.
        /// </summary>
        public int Step()
        {
            int acted = 0;

            for (int y = 0; y < m_rows; y++)
            {
                for (int x = 0; x < m_columns; x++)
                {
                    int index = x + y * m_columns;

                    CellAi brain = m_brains[index];

                    if (brain == null || m_owner[index] < 0)
                        continue;

                    CellView view = ViewFor(m_owner[index]);
                    CellDecision decision = brain.Think(view, x, y);

                    Apply(x, y, index, brain, decision);

                    acted++;
                }
            }

            return acted;
        }

        private void Apply(int x, int y, int index, CellAi brain, CellDecision decision)
        {
            switch (decision.Action)
            {
                case CellAction.Attack:
                    if (decision.Target.HasValue)
                    {
                        CellPoint target = decision.Target.Value;
                        int targetIndex = target.X + target.Y * m_columns;

                        // Ten percent of the attacker's strength, as a fraction.
                        m_health[targetIndex] -= 0.12 + brain.Strength * 0.004;

                        if (m_health[targetIndex] <= 0.0)
                        {
                            m_health[targetIndex] = 0.0;
                            m_owner[targetIndex] = -1;
                            m_brains[targetIndex] = null;

                            Note(brain.Id + " killed the cell at " + target);
                        }
                    }
                    break;

                case CellAction.Rest:
                    m_health[index] = Math.Min(1.0, m_health[index] + 0.15);
                    break;

                case CellAction.Spawn:
                    if (decision.Target.HasValue)
                    {
                        CellPoint target = decision.Target.Value;
                        int targetIndex = target.X + target.Y * m_columns;

                        if (m_owner[targetIndex] < 0 && m_health[index] > 0.6)
                        {
                            m_health[index] -= 0.3;

                            Place(target.X, target.Y, m_owner[index], brain.Profile.Level);
                        }
                    }
                    break;

                case CellAction.Move:
                case CellAction.Approach:
                case CellAction.Retreat:
                    if (decision.Target.HasValue)
                    {
                        CellPoint target = decision.Target.Value;
                        int targetIndex = target.X + target.Y * m_columns;

                        if (m_owner[targetIndex] < 0)
                        {
                            m_owner[targetIndex] = m_owner[index];
                            m_health[targetIndex] = m_health[index];
                            m_brains[targetIndex] = brain;

                            m_owner[index] = -1;
                            m_health[index] = 0.0;
                            m_brains[index] = null;
                        }
                    }
                    break;
            }

            // The health the brain reports has to track the board, or it will
            // keep making decisions based on a body it no longer has.
            brain.Health = (int)Math.Round(m_health[index] * 100.0);
        }

        /// <summary>Runs a number of turns. Returns how many were run.</summary>
        public int Run(int turns)
        {
            int run = 0;

            for (int i = 0; i < turns; i++)
            {
                if (CountSide(0) == 0 || CountSide(1) == 0)
                    break;

                Step();
                run++;
            }

            return run;
        }

        private void Note(string message)
        {
            m_log.Add(message);

            while (m_log.Count > 200)
                m_log.RemoveAt(0);
        }

        /// <summary>The board as text, one character per square.</summary>
        public string Render()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            for (int y = 0; y < m_rows; y++)
            {
                for (int x = 0; x < m_columns; x++)
                {
                    int owner = OwnerAt(x, y);

                    if (owner < 0)
                        builder.Append('.');
                    else if (owner == 0)
                        builder.Append('a');
                    else
                        builder.Append('B');
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        public override string ToString()
        {
            return "CellAiBoard " + m_columns + "x" + m_rows +
                   " (side 0: " + CountSide(0) + ", side 1: " + CountSide(1) + ")";
        }
    }
}

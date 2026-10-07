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

namespace LifeEngine.Combat
{
    /// <summary>What one line of the log is about.</summary>
    public enum LogKind
    {
        Start,
        Round,
        Blow,
        Miss,
        Effect,
        Wound,
        Death,
        Flee,
        End,
        Note
    }

    /// <summary>One line of a battle: what happened, and in which round.</summary>
    public sealed class LogEntry
    {
        private int m_round;
        private LogKind m_kind;
        private string m_actor;
        private string m_target;
        private string m_text;
        private int m_amount;

        public int Round
        {
            get { return m_round; }
        }

        public LogKind Kind
        {
            get { return m_kind; }
        }

        /// <summary>Who did it.</summary>
        public string Actor
        {
            get { return m_actor; }
        }

        /// <summary>Who it was done to.</summary>
        public string Target
        {
            get { return m_target; }
        }

        /// <summary>The sentence, already written out.</summary>
        public string Text
        {
            get { return m_text; }
        }

        /// <summary>How much damage or healing, where that applies.</summary>
        public int Amount
        {
            get { return m_amount; }
        }

        public LogEntry(int round, LogKind kind, string text)
        {
            m_round = round;
            m_kind = kind;
            m_text = text;
            m_actor = string.Empty;
            m_target = string.Empty;
        }

        public LogEntry(int round, LogKind kind, string actor, string target, string text, int amount)
        {
            m_round = round;
            m_kind = kind;
            m_actor = actor ?? string.Empty;
            m_target = target ?? string.Empty;
            m_text = text;
            m_amount = amount;
        }

        /// <summary>"3.  Fenrir strikes Skoll for 12."</summary>
        public string Formatted()
        {
            return m_round.ToString().PadLeft(3) + ".  " + m_text;
        }

        public override string ToString()
        {
            return Formatted();
        }
    }

    /// <summary>
    /// The record of a battle: every blow, every effect, and how it ended.
    ///
    /// The log is written as the fight happens rather than reconstructed
    /// afterwards, because the interesting parts of a fight are the order things
    /// happened in and a reconstruction would lose that.
    /// </summary>
    public sealed class BattleLog
    {
        private List<LogEntry> m_entries;
        private int m_round;

        /// <summary>Every line, in order.</summary>
        public IList<LogEntry> Entries
        {
            get { return m_entries; }
        }

        /// <summary>The round the log has reached.</summary>
        public int Round
        {
            get { return m_round; }
        }

        public BattleLog()
        {
            m_entries = new List<LogEntry>();
            m_round = 0;
        }

        /// <summary>Starts a new round.</summary>
        public void BeginRound(int round)
        {
            m_round = round;

            Add(new LogEntry(round, LogKind.Round, "-- round " + round + " --"));
        }

        public void Add(LogEntry entry)
        {
            if (entry != null)
                m_entries.Add(entry);
        }

        public void Note(string text)
        {
            Add(new LogEntry(m_round, LogKind.Note, text));
        }

        public void Start(string text)
        {
            Add(new LogEntry(m_round, LogKind.Start, text));
        }

        public void End(string text)
        {
            Add(new LogEntry(m_round, LogKind.End, text));
        }

        public void Blow(string actor, string target, int damage)
        {
            Add(new LogEntry(m_round, LogKind.Blow, actor, target,
                actor + " strikes " + target + " for " + damage + ".", damage));
        }
        public void Miss(string actor, string target)
        {
            Add(new LogEntry(m_round, LogKind.Miss, actor, target,
                actor + " lunges at " + target + " and misses.", 0));
        }

        public void Dodge(string actor, string target)
        {
            Add(new LogEntry(m_round, LogKind.Miss, actor, target,
                actor + " strikes at " + target + ", who slips aside.", 0));
        }

        public void Effect(string actor, string effectName, string detail)
        {
            Add(new LogEntry(m_round, LogKind.Effect, actor, string.Empty,
                actor + " is " + effectName + (string.IsNullOrEmpty(detail) ? "." : ": " + detail + "."), 0));
        }

        public void Expired(string actor, string effectName)
        {
            Add(new LogEntry(m_round, LogKind.Effect, actor, string.Empty,
                actor + " shrugs off " + effectName + ".", 0));
        }

        public void Wound(string actor, string wound)
        {
            Add(new LogEntry(m_round, LogKind.Wound, actor, string.Empty,
                actor + " takes " + wound + ".", 0));
        }

        public void Death(string actor, string cause)
        {
            Add(new LogEntry(m_round, LogKind.Death, actor, string.Empty,
                actor + " " + cause + ".", 0));
        }

        public void Flee(string actor)
        {
            Add(new LogEntry(m_round, LogKind.Flee, actor, string.Empty,
                actor + " breaks away and runs.", 0));
        }

        /// <summary>Everything of one kind, for a filtered view.</summary>
        public List<LogEntry> OfKind(LogKind kind)
        {
            List<LogEntry> result = new List<LogEntry>();

            for (int i = 0; i < m_entries.Count; i++)
            {
                if (m_entries[i].Kind == kind)
                    result.Add(m_entries[i]);
            }

            return result;
        }

        /// <summary>The whole log as text, one line each.</summary>
        public string AsText()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            for (int i = 0; i < m_entries.Count; i++)
                builder.AppendLine(m_entries[i].Formatted());

            return builder.ToString();
        }

        /// <summary>Just the blows, for a compact view.</summary>
        public string BlowsAsText()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            for (int i = 0; i < m_entries.Count; i++)
            {
                LogKind kind = m_entries[i].Kind;

                if (kind == LogKind.Blow || kind == LogKind.Death || kind == LogKind.Flee)
                    builder.AppendLine(m_entries[i].Formatted());
            }

            return builder.ToString();
        }

        public int Count
        {
            get { return m_entries.Count; }
        }

        public override string ToString()
        {
            return "BattleLog (" + m_entries.Count + " lines, " + m_round + " rounds)";
        }
    }

    /// <summary>
    /// The outcome of a battle: who won, who died, and what it cost.
    /// </summary>
    public sealed class BattleResult
    {
        private string m_name;
        private Side m_winner;
        private int m_rounds;
        private List<Combatant> m_combatants;
        private List<Combatant> m_dead;
        private List<Combatant> m_fled;
        private BattleLog m_log;
        private string m_narrative;

        /// <summary>What the fight was called.</summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>Which side won, or Neutral for a draw.</summary>
        public Side Winner
        {
            get { return m_winner; }
            set { m_winner = value; }
        }

        public int Rounds
        {
            get { return m_rounds; }
            set { m_rounds = value; }
        }

        /// <summary>Everyone who took part.</summary>
        public IList<Combatant> Combatants
        {
            get { return m_combatants; }
        }

        public IList<Combatant> Dead
        {
            get { return m_dead; }
        }

        public IList<Combatant> Fled
        {
            get { return m_fled; }
        }

        /// <summary>Every line of the fight.</summary>
        public BattleLog Log
        {
            get { return m_log; }
            set { m_log = value; }
        }

        /// <summary>A sentence or two describing how it went.</summary>
        public string Narrative
        {
            get { return m_narrative; }
            set { m_narrative = value; }
        }

        public BattleResult(string name)
        {
            m_name = name;
            m_winner = Side.Neutral;
            m_combatants = new List<Combatant>();
            m_dead = new List<Combatant>();
            m_fled = new List<Combatant>();
            m_log = new BattleLog();
            m_narrative = string.Empty;
        }

        /// <summary>Whether a given fighter died.</summary>
        public bool Died(Combatant fighter)
        {
            return fighter != null && m_dead.Contains(fighter);
        }

        /// <summary>How many of one side are still standing.</summary>
        public int Standing(Side side)
        {
            int count = 0;

            for (int i = 0; i < m_combatants.Count; i++)
            {
                if (m_combatants[i].Side == side && m_combatants[i].CanFight)
                    count++;
            }

            return count;
        }

        /// <summary>How many of one side took part.</summary>
        public int Started(Side side)
        {
            int count = 0;

            for (int i = 0; i < m_combatants.Count; i++)
            {
                if (m_combatants[i].Side == side)
                    count++;
            }

            return count;
        }

        /// <summary>The whole fight, for a report window.</summary>
        public string Report()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            builder.AppendLine(m_name);
            builder.AppendLine(new string('=', m_name.Length));
            builder.AppendLine();
            builder.AppendLine("Rounds:    " + m_rounds);
            builder.AppendLine("Winner:    " + m_winner);
            builder.AppendLine("Fighters:  " + m_combatants.Count);
            builder.AppendLine("Dead:      " + m_dead.Count);
            builder.AppendLine("Fled:      " + m_fled.Count);
            builder.AppendLine();

            if (!string.IsNullOrEmpty(m_narrative))
            {
                builder.AppendLine(m_narrative);
                builder.AppendLine();
            }

            builder.AppendLine("The fighters");
            builder.AppendLine();

            for (int i = 0; i < m_combatants.Count; i++)
            {
                Combatant fighter = m_combatants[i];

                builder.AppendLine("  " + Pad(fighter.Name, 16) +
                                   Pad(fighter.Side.ToString(), 9) +
                                   Pad(Math.Round(fighter.HealthFraction * 100.0) + "%", 7) +
                                   Pad(fighter.State.ToString(), 10) +
                                   "struck " + fighter.BlowsStruck +
                                   ", took " + fighter.BlowsTaken +
                                   ", dealt " + fighter.DamageDealt +
                                   ", suffered " + fighter.DamageTaken);

                if (fighter.Effects.Count > 0)
                    builder.AppendLine("      effects: " + fighter.Effects.Describe());

                if (fighter.Wounds.Count > 0)
                    builder.AppendLine("      wounds:  " + string.Join(", ", ToArray(fighter.Wounds)));
            }

            builder.AppendLine();
            builder.AppendLine("The fight");
            builder.AppendLine();
            builder.Append(m_log.AsText());

            return builder.ToString();
        }

        private static string[] ToArray(IList<string> list)
        {
            string[] array = new string[list.Count];

            for (int i = 0; i < list.Count; i++)
                array[i] = list[i];

            return array;
        }

        private static string Pad(string text, int width)
        {
            if (text == null)
                text = string.Empty;

            if (text.Length >= width)
                return text + " ";

            return text + new string(' ', width - text.Length);
        }

        public override string ToString()
        {
            return m_name + " (" + m_rounds + " rounds, " + m_winner + " won)";
        }
    }
}

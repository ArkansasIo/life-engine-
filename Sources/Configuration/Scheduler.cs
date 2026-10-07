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
using System.Globalization;

namespace LifeEngine.Configuration
{
    /// <summary>
    /// What a schedule does when it fires. The names are the ones used in the
    /// schedule file, lower case, and the same strings the scheduler registers.
    /// </summary>
    public enum ScheduleAction
    {
        Nothing,
        /// <summary>Advance the cellular simulation by one generation.</summary>
        Step,
        /// <summary>Start the simulation running.</summary>
        Start,
        /// <summary>Pause the simulation.</summary>
        Stop,
        /// <summary>Empty the grid.</summary>
        Clear,
        /// <summary>Fill the grid at the configured density.</summary>
        Randomize,
        /// <summary>Go back to the state captured at the last Start.</summary>
        Reset,
        /// <summary>Advance the ecosystem by the configured number of ticks.</summary>
        RunEcosystem,
        /// <summary>Build a fresh world map and ecosystem.</summary>
        NewWorld,
        /// <summary>Look for a newer build on the update channel.</summary>
        CheckForUpdates,
        /// <summary>Write the log out, if the log is buffered.</summary>
        FlushLog,
        /// <summary>Write the current state of the program to the log.</summary>
        Snapshot
    }

    /// <summary>
    /// How often something repeats.
    /// </summary>
    public enum ScheduleRepeat
    {
        /// <summary>Once, and then the schedule retires.</summary>
        Once,
        /// <summary>Every N seconds, counted from when it last ran.</summary>
        Interval,
        /// <summary>Every day at a fixed time of day.</summary>
        Daily,
        /// <summary>Every week on a fixed day and time.</summary>
        Weekly,
        /// <summary>On the first day of every month, at a fixed time.</summary>
        Monthly,
        /// <summary>At a fixed set of times of day, every day.</summary>
        TimesOfDay
    }

    /// <summary>
    /// One scheduled job: what to do, when to do it and how often.
    ///
    /// A schedule knows nothing about timers, threads or the form. It answers
    /// one question: given the time now, is it due, and what is the next time it
    /// will be due?
    /// </summary>
    public sealed class Schedule
    {
        private string m_name;
        private string m_description;
        private ScheduleAction m_action;
        private ScheduleRepeat m_repeat;
        private bool m_enabled;
        private string m_intervalText;
        private List<TimeSpan> m_timesOfDay;
        private int m_dayOfWeek;
        private int m_dayOfMonth;
        private DateTime m_startAt;
        private DateTime m_endAfter;
        private int m_maxRuns;
        private int m_runs;
        private DateTime m_lastRun;
        private DateTime m_nextRun;
        private string m_tag;

        /// <summary>The name the schedule is known by, unique within a set.</summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>A sentence about what the job is for.</summary>
        public string Description
        {
            get { return m_description; }
            set { m_description = value; }
        }

        /// <summary>What to do when it fires.</summary>
        public ScheduleAction Action
        {
            get { return m_action; }
            set { m_action = value; }
        }

        /// <summary>How often it repeats.</summary>
        public ScheduleRepeat Repeat
        {
            get { return m_repeat; }
            set { m_repeat = value; }
        }

        /// <summary>Whether the scheduler should consider it at all.</summary>
        public bool Enabled
        {
            get { return m_enabled; }
            set { m_enabled = value; }
        }

        /// <summary>
        /// The gap between runs, written the way a person writes it: "30s",
        /// "5m", "2h", "1d", or "1.02:30:00" for days, hours, minutes and
        /// seconds. Only used when <see cref="Repeat"/> is Interval.
        /// </summary>
        public string IntervalText
        {
            get { return m_intervalText; }
            set { m_intervalText = value; }
        }

        /// <summary>The span the interval text stands for.</summary>
        public TimeSpan Interval
        {
            get { return ParseInterval(m_intervalText); }
        }

        /// <summary>
        /// The times of day a daily schedule runs at. One entry is a daily job;
        /// several make it a list of times, and they are run in order.
        /// </summary>
        public IList<TimeSpan> TimesOfDay
        {
            get { return m_timesOfDay; }
        }

        /// <summary>Day of the week for a weekly schedule, 0 is Sunday.</summary>
        public int DayOfWeek
        {
            get { return m_dayOfWeek; }
            set { m_dayOfWeek = value; }
        }

        /// <summary>Day of the month for a monthly schedule, 1 to 28.</summary>
        public int DayOfMonth
        {
            get { return m_dayOfMonth; }
            set { m_dayOfMonth = value; }
        }

        /// <summary>Do not run before this time. Default is no lower bound.</summary>
        public DateTime StartAt
        {
            get { return m_startAt; }
            set { m_startAt = value; }
        }

        /// <summary>Do not run after this time. Default is no upper bound.</summary>
        public DateTime EndAfter
        {
            get { return m_endAfter; }
            set { m_endAfter = value; }
        }

        /// <summary>Stop after this many runs. Zero means never stop.</summary>
        public int MaxRuns
        {
            get { return m_maxRuns; }
            set { m_maxRuns = value; }
        }

        /// <summary>How many times it has run.</summary>
        public int Runs
        {
            get { return m_runs; }
        }

        /// <summary>When it last ran, or DateTime.MinValue.</summary>
        public DateTime LastRun
        {
            get { return m_lastRun; }
        }

        /// <summary>
        /// When it is next due, or DateTime.MaxValue for never. The scheduler
        /// sets this when it arms a job; a caller may also set it to delay or
        /// bring forward a single job without changing its repeat rule.
        /// </summary>
        public DateTime NextRun
        {
            get { return m_nextRun; }
            set { m_nextRun = value; }
        }

        /// <summary>A free label, for grouping jobs in a list.</summary>
        public string Tag
        {
            get { return m_tag; }
            set { m_tag = value; }
        }

        public Schedule(string name, ScheduleAction action, ScheduleRepeat repeat)
        {
            m_name = name;
            m_action = action;
            m_repeat = repeat;
            m_enabled = true;
            m_intervalText = "5m";
            m_timesOfDay = new List<TimeSpan>();
            m_dayOfWeek = 1;
            m_dayOfMonth = 1;
            m_startAt = DateTime.MinValue;
            m_endAfter = DateTime.MaxValue;
            m_maxRuns = 0;
            m_lastRun = DateTime.MinValue;
            m_nextRun = DateTime.MaxValue;
            m_tag = string.Empty;
            m_description = string.Empty;
        }

        /// <summary>Adds a time of day, e.g. "06:00" or "18:30".</summary>
        public Schedule AddTimeOfDay(string time)
        {
            TimeSpan parsed;

            if (TryParseTimeOfDay(time, out parsed) && !m_timesOfDay.Contains(parsed))
                m_timesOfDay.Add(parsed);

            return this;
        }

        /// <summary>True if the job has run as many times as it is allowed to.</summary>
        public bool IsFinished()
        {
            if (m_maxRuns > 0 && m_runs >= m_maxRuns)
                return true;

            if (m_repeat == ScheduleRepeat.Once && m_runs > 0)
                return true;

            return false;
        }

        /// <summary>True if the job is switched on, not finished and inside its window.</summary>
        public bool IsActive(DateTime now)
        {
            if (!m_enabled)
                return false;

            if (IsFinished())
                return false;

            if (now < m_startAt)
                return false;

            if (now > m_endAfter)
                return false;

            return true;
        }

        /// <summary>True if the job is due now.</summary>
        public bool IsDue(DateTime now)
        {
            return IsActive(now) && now >= m_nextRun;
        }

        /// <summary>
        /// The next time this job should run, given a reference time. Returns
        /// DateTime.MaxValue if it will never run again.
        /// </summary>
        public DateTime ComputeNextRun(DateTime from)
        {
            if (!m_enabled)
                return DateTime.MaxValue;

            if (m_maxRuns > 0 && m_runs >= m_maxRuns)
                return DateTime.MaxValue;

            if (m_repeat == ScheduleRepeat.Once && m_runs > 0)
                return DateTime.MaxValue;

            DateTime floor = from;

            if (floor < m_startAt)
                floor = m_startAt;

            DateTime candidate;

            switch (m_repeat)
            {
                case ScheduleRepeat.Once:
                    candidate = floor;
                    break;

                case ScheduleRepeat.Interval:
                    candidate = floor + Interval;

                    // Step forward until it lands after the reference time, so a
                    // job whose interval is shorter than the check period does
                    // not fire a burst to catch up.
                    while (candidate < floor)
                        candidate = candidate + Interval;

                    break;

                case ScheduleRepeat.Daily:
                    candidate = NextTimeOfDay(floor, 0);
                    break;

                case ScheduleRepeat.Weekly:
                    candidate = NextWeekday(floor);
                    break;

                case ScheduleRepeat.Monthly:
                    candidate = NextDayOfMonth(floor);
                    break;

                case ScheduleRepeat.TimesOfDay:
                    candidate = NextTimeOfDay(floor, 0);

                    if (m_timesOfDay.Count == 0)
                        candidate = DateTime.MaxValue;

                    break;

                default:
                    candidate = DateTime.MaxValue;
                    break;
            }

            if (candidate > m_endAfter)
                return DateTime.MaxValue;

            return candidate;
        }

        /// <summary>
        /// Records that the job has run, and works out when it is next due.
        /// </summary>
        public void MarkRun(DateTime when)
        {
            m_runs++;
            m_lastRun = when;

            if (m_repeat == ScheduleRepeat.Interval)
                m_nextRun = when + Interval;
            else
                m_nextRun = ComputeNextRun(when);

            if (IsFinished())
                m_nextRun = DateTime.MaxValue;
        }

        /// <summary>Resets the counters, for a manual re-run.</summary>
        public void Restart(DateTime now)
        {
            m_runs = 0;
            m_lastRun = DateTime.MinValue;
            m_nextRun = ComputeNextRun(now);
        }

        // ------------------------------------------------------------------
        // Timing helpers
        // ------------------------------------------------------------------

        private DateTime NextTimeOfDay(DateTime from, int index)
        {
            if (m_timesOfDay.Count == 0)
                return DateTime.MaxValue;

            List<TimeSpan> sorted = new List<TimeSpan>(m_timesOfDay);
            sorted.Sort();

            // The next listed time today, otherwise the first one tomorrow.
            for (int i = 0; i < sorted.Count; i++)
            {
                DateTime candidate = from.Date + sorted[i];

                if (candidate > from)
                    return candidate;
            }

            return from.Date.AddDays(1) + sorted[0];
        }

        private DateTime NextWeekday(DateTime from)
        {
            int current = (int)from.DayOfWeek;
            int target = m_dayOfWeek;

            if (target < 0)
                target = 0;

            if (target > 6)
                target = 6;

            int daysAhead = target - current;

            if (daysAhead <= 0)
                daysAhead += 7;

            DateTime day = from.Date.AddDays(daysAhead);

            if (m_timesOfDay.Count > 0)
                return day + Smallest(m_timesOfDay);

            return day;
        }

        private DateTime NextDayOfMonth(DateTime from)
        {
            int day = m_dayOfMonth;

            if (day < 1)
                day = 1;

            if (day > 28)
                day = 28;

            DateTime candidate = new DateTime(from.Year, from.Month, day);

            if (candidate <= from)
                candidate = new DateTime(from.Year, from.Month, day).AddMonths(1);

            if (m_timesOfDay.Count > 0)
                candidate = candidate.Date + Smallest(m_timesOfDay);

            if (candidate <= from)
                candidate = candidate.AddMonths(1);

            return candidate;
        }

        private static TimeSpan Smallest(List<TimeSpan> spans)
        {
            TimeSpan smallest = spans[0];

            for (int i = 1; i < spans.Count; i++)
            {
                if (spans[i] < smallest)
                    smallest = spans[i];
            }

            return smallest;
        }

        /// <summary>
        /// Reads an interval the way a person writes one: "30s", "5m", "2h",
        /// "1d", "1d12h", or "0.02:30:00". A bare number means seconds.
        /// </summary>
        public static TimeSpan ParseInterval(string text)
        {
            if (string.IsNullOrEmpty(text))
                return TimeSpan.FromMinutes(5);

            string trimmed = text.Trim();

            TimeSpan span;

            if (TimeSpan.TryParse(trimmed, CultureInfo.InvariantCulture, out span))
                return span;

            // Long form: a series of number-unit pairs, e.g. 1d12h30m.
            double totalSeconds = 0.0;
            double number = 0.0;
            bool hasNumber = false;
            bool hasUnit = false;

            for (int i = 0; i < trimmed.Length; i++)
            {
                char letter = trimmed[i];

                if (char.IsDigit(letter) || letter == '.')
                {
                    number = (number * 10.0) + (letter == '.' ? 0.0 : (letter - '0'));
                    hasNumber = true;
                    continue;
                }

                if (!hasNumber)
                    continue;

                char unit = char.ToLowerInvariant(letter);

                if (unit == 's')
                    totalSeconds += number;
                else if (unit == 'm')
                    totalSeconds += number * 60.0;
                else if (unit == 'h')
                    totalSeconds += number * 3600.0;
                else if (unit == 'd')
                    totalSeconds += number * 86400.0;
                else
                {
                    // An unknown unit: treat the number as seconds and stop.
                    totalSeconds += number;
                    number = 0.0;
                    hasNumber = false;
                    continue;
                }

                hasUnit = true;
                number = 0.0;
                hasNumber = false;
            }

            if (hasNumber)
                totalSeconds += number;

            if (!hasUnit && totalSeconds <= 0.0)
                return TimeSpan.FromMinutes(5);

            if (totalSeconds <= 0.0)
                return TimeSpan.FromMinutes(5);

            return TimeSpan.FromSeconds(totalSeconds);
        }

        /// <summary>Reads "06:30" or "6:30 PM" into a time of day.</summary>
        public static bool TryParseTimeOfDay(string text, out TimeSpan time)
        {
            time = TimeSpan.Zero;

            if (string.IsNullOrEmpty(text))
                return false;

            if (TimeSpan.TryParse(text.Trim(), CultureInfo.InvariantCulture, out time))
            {
                if (time < TimeSpan.Zero)
                    time = TimeSpan.Zero;

                return true;
            }

            DateTime parsed;

            if (DateTime.TryParse(text.Trim(), CultureInfo.InvariantCulture,
                                  DateTimeStyles.NoCurrentDateDefault, out parsed))
            {
                time = parsed.TimeOfDay;
                return true;
            }

            return false;
        }

        /// <summary>"2.00:30:00" reads back as "2d30m", which is how a person says it.</summary>
        public string DescribeInterval()
        {
            return DescribeSpanText(Interval);
        }

        public static string DescribeSpanText(TimeSpan span)
        {
            if (span.TotalDays >= 1.0 && span.TotalDays == Math.Floor(span.TotalDays))
                return ((int)span.TotalDays) + "d";

            if (span.TotalHours >= 1.0 && span.TotalHours == Math.Floor(span.TotalHours))
                return ((int)span.TotalHours) + "h";

            if (span.TotalMinutes >= 1.0 && span.TotalMinutes == Math.Floor(span.TotalMinutes))
                return ((int)span.TotalMinutes) + "m";

            return ((int)span.TotalSeconds) + "s";
        }

        public override string ToString()
        {
            string state = m_enabled ? "on" : "off";

            return m_name + " [" + m_action + ", " + m_repeat + ", " + state +
                   ", runs " + m_runs + "]";
        }
    }

    /// <summary>
    /// A set of schedules. It holds them, decides which are due, and issues the
    /// due ones oldest first so the order between two jobs that fired together
    /// is the order they ran last time.
    /// </summary>
    public sealed class Scheduler
    {
        private string m_name;
        private List<Schedule> m_schedules;
        private bool m_enabled;
        private DateTime m_lastCheck;
        private int m_totalRuns;

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>The schedules, in the order they were added.</summary>
        public IList<Schedule> Schedules
        {
            get { return m_schedules; }
        }

        /// <summary>Whether the whole set runs. Individual jobs have their own switch.</summary>
        public bool Enabled
        {
            get { return m_enabled; }
            set { m_enabled = value; }
        }

        /// <summary>When the due list was last computed.</summary>
        public DateTime LastCheck
        {
            get { return m_lastCheck; }
        }

        /// <summary>How many jobs have fired since this schedder was built.</summary>
        public int TotalRuns
        {
            get { return m_totalRuns; }
        }

        public Scheduler(string name)
        {
            m_name = name;
            m_schedules = new List<Schedule>();
            m_enabled = true;
            m_lastCheck = DateTime.MinValue;
        }

        public Schedule Add(Schedule schedule)
        {
            if (schedule == null)
                return null;

            if (!m_schedules.Contains(schedule))
                m_schedules.Add(schedule);

            return schedule;
        }

        public Schedule Add(string name, ScheduleAction action, ScheduleRepeat repeat)
        {
            return Add(new Schedule(name, action, repeat));
        }

        public Schedule Find(string name)
        {
            for (int i = 0; i < m_schedules.Count; i++)
            {
                if (string.Equals(m_schedules[i].Name, name, StringComparison.OrdinalIgnoreCase))
                    return m_schedules[i];
            }

            return null;
        }

        public bool Remove(string name)
        {
            Schedule schedule = Find(name);

            if (schedule == null)
                return false;

            return m_schedules.Remove(schedule);
        }

        /// <summary>
        /// Arms every schedule. Call it once, when the program starts.
        /// </summary>
        public void Arm(DateTime now)
        {
            for (int i = 0; i < m_schedules.Count; i++)
            {
                Schedule schedule = m_schedules[i];

                schedule.StartAt = schedule.StartAt == DateTime.MinValue ? now : schedule.StartAt;
                schedule.NextRun = schedule.ComputeNextRun(now);
            }

            m_lastCheck = now;
        }

        /// <summary>
        /// Returns the schedules that are due at this moment, oldest first. It
        /// does not run them and does not mark them run: the caller does that
        /// after the work succeeds, so a failed job is not counted.
        ///
        /// It also catches up after a pause: any job whose next run has already
        /// passed since the last check is included, each once.
        /// </summary>
        public List<Schedule> GetDue(DateTime now)
        {
            List<Schedule> due = new List<Schedule>();

            m_lastCheck = now;

            if (!m_enabled)
                return due;

            for (int i = 0; i < m_schedules.Count; i++)
            {
                Schedule schedule = m_schedules[i];

                if (schedule.IsDue(now))
                    due.Add(schedule);
            }

            due.Sort(delegate (Schedule left, Schedule right)
            {
                return left.NextRun.CompareTo(right.NextRun);
            });

            return due;
        }

        /// <summary>Runs everything that is due, through a supplied handler.</summary>
        public List<Schedule> RunDue(DateTime now, Action<Schedule> handler)
        {
            List<Schedule> due = GetDue(now);

            for (int i = 0; i < due.Count; i++)
            {
                Schedule schedule = due[i];

                try
                {
                    if (handler != null)
                        handler(schedule);

                    schedule.MarkRun(now);
                    m_totalRuns++;
                }
                catch (Exception)
                {
                    // A job that throws is left un-marked, so it will be tried
                    // again on the next check rather than silently skipped.
                }
            }

            return due;
        }

        /// <summary>Runs one named job right now, whatever its schedule says.</summary>
        public bool RunNow(string name, DateTime now, Action<Schedule> handler)
        {
            Schedule schedule = Find(name);

            if (schedule == null)
                return false;

            if (handler != null)
                handler(schedule);

            schedule.MarkRun(now);
            m_totalRuns++;

            return true;
        }

        /// <summary>Turns every job on or off at once.</summary>
        public void SetAllEnabled(bool enabled)
        {
            for (int i = 0; i < m_schedules.Count; i++)
                m_schedules[i].Enabled = enabled;
        }

        /// <summary>The soonest thing that is going to happen.</summary>
        public Schedule GetNext()
        {
            Schedule next = null;

            for (int i = 0; i < m_schedules.Count; i++)
            {
                Schedule schedule = m_schedules[i];

                if (!schedule.IsActive(DateTime.Now))
                    continue;

                if (next == null || schedule.NextRun < next.NextRun)
                    next = schedule;
            }

            return next;
        }

        /// <summary>A table of every job and when it next runs, for a panel or a log.</summary>
        public string Describe()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            builder.AppendLine("Scheduler: " + m_name + " (" + m_schedules.Count + " jobs, " +
                               (m_enabled ? "enabled" : "disabled") + ")");
            builder.AppendLine();

            for (int i = 0; i < m_schedules.Count; i++)
            {
                Schedule schedule = m_schedules[i];

                string next = schedule.NextRun == DateTime.MaxValue
                    ? "never"
                    : schedule.NextRun.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                builder.AppendLine("  " + Pad(schedule.Name, 22) +
                                   Pad(schedule.Action.ToString(), 18) +
                                   Pad(schedule.Enabled ? "on" : "off", 6) +
                                   Pad("runs " + schedule.Runs, 10) + "next " + next);
            }

            return builder.ToString();
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
            return m_name + " (" + m_schedules.Count + " schedules)";
        }
    }
}

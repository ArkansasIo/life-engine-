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

namespace LifeEngine.World
{
    /// <summary>
    /// Where the sun is in its daily round. Eight named parts rather than a
    /// continuous number, because the things that care — hunting, spawning,
    /// the moon — care about periods, not about minutes.
    /// </summary>
    public enum DayPhase
    {
        /// <summary>Before first light. The coldest hour.</summary>
        Night,
        /// <summary>The first grey light before the sun shows.</summary>
        Dawn,
        /// <summary>The sun is up. Warmth returns.</summary>
        Morning,
        /// <summary>The middle of the day. Brightest and hottest.</summary>
        Noon,
        /// <summary>Afternoon, light beginning to go long.</summary>
        Afternoon,
        /// <summary>The sun is going down. The second hunt of the day.</summary>
        Dusk,
        /// <summary>Dark, but not yet the deep of night.</summary>
        Evening,
        /// <summary>The middle of the night. The moon is highest.</summary>
        Midnight
    }

    /// <summary>The moon's state, which decides whether the cursed change.</summary>
    public enum MoonPhase
    {
        New,
        WaxingCrescent,
        FirstQuarter,
        WaxingGibbous,
        Full,
        WaningGibbous,
        LastQuarter,
        WaningCrescent
    }

    /// <summary>
    /// One day on one world: how long it is, how it is divided, and what the
    /// moon is doing.
    ///
    /// The cycle is a clock, not a calendar. It knows the hour, the phase of the
    /// day and the phase of the moon, and it knows how light and warm it is at
    /// that hour given the world's latitude. It does not know what day it is.
    /// </summary>
    public sealed class DayNightCycle
    {
        private string m_name;
        private double m_dayLengthHours;
        private double m_hour;
        private int m_day;
        private double m_axialTiltDegrees;
        private double m_lunarMonthDays;
        private double m_moonOffsetDays;
        private double m_baseTemperatureCelsius;
        private double m_temperatureSwingCelsius;
        private double m_dawnFraction;
        private double m_duskFraction;

        /// <summary>A name for the cycle, e.g. the world it belongs to.</summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>Hours in one day. Twenty-four on Earth.</summary>
        public double DayLengthHours
        {
            get { return m_dayLengthHours; }
            set { m_dayLengthHours = value < 1.0 ? 1.0 : value; }
        }

        /// <summary>The hour it is now, 0 to DayLengthHours.</summary>
        public double Hour
        {
            get { return m_hour; }
            set { m_hour = Wrap(value, m_dayLengthHours); }
        }

        /// <summary>How many days have passed since the cycle started.</summary>
        public int Day
        {
            get { return m_day; }
        }

        /// <summary>The planet's tilt, which sets how much the day length varies.</summary>
        public double AxialTiltDegrees
        {
            get { return m_axialTiltDegrees; }
            set { m_axialTiltDegrees = value; }
        }

        /// <summary>Days in one lunar month. About 29.5 on Earth.</summary>
        public double LunarMonthDays
        {
            get { return m_lunarMonthDays; }
            set { m_lunarMonthDays = value < 1.0 ? 1.0 : value; }
        }

        /// <summary>Where in the lunar month the cycle started.</summary>
        public double MoonOffsetDays
        {
            get { return m_moonOffsetDays; }
            set { m_moonOffsetDays = value; }
        }

        /// <summary>The temperature at the coldest hour, in Celsius.</summary>
        public double BaseTemperatureCelsius
        {
            get { return m_baseTemperatureCelsius; }
            set { m_baseTemperatureCelsius = value; }
        }

        /// <summary>How much warmer it gets at the hottest hour.</summary>
        public double TemperatureSwingCelsius
        {
            get { return m_temperatureSwingCelsius; }
            set { m_temperatureSwingCelsius = value; }
        }

        /// <summary>What fraction of the day is dawn, either side of sunrise.</summary>
        public double DawnFraction
        {
            get { return m_dawnFraction; }
            set { m_dawnFraction = Limit(value, 0.01, 0.25); }
        }

        /// <summary>What fraction of the day is dusk.</summary>
        public double DuskFraction
        {
            get { return m_duskFraction; }
            set { m_duskFraction = Limit(value, 0.01, 0.25); }
        }

        /// <summary>True while it is dark enough for a wolf to have the advantage.</summary>
        public bool IsDark
        {
            get
            {
                DayPhase phase = Phase;
                return phase == DayPhase.Night || phase == DayPhase.Midnight ||
                       phase == DayPhase.Evening;
            }
        }

        /// <summary>True at dawn and dusk, when the light is changing.</summary>
        public bool IsTwilight
        {
            get
            {
                DayPhase phase = Phase;
                return phase == DayPhase.Dawn || phase == DayPhase.Dusk;
            }
        }

        /// <summary>True on the night of a full moon.</summary>
        public bool IsFullMoon
        {
            get { return Moon == MoonPhase.Full; }
        }

        public DayNightCycle(string name, double dayLengthHours)
        {
            m_name = name;
            m_dayLengthHours = dayLengthHours < 1.0 ? 24.0 : dayLengthHours;
            m_hour = 0.0;
            m_day = 0;
            m_axialTiltDegrees = 23.44;
            m_lunarMonthDays = 29.53;
            m_moonOffsetDays = 0.0;
            m_baseTemperatureCelsius = 5.0;
            m_temperatureSwingCelsius = 12.0;
            m_dawnFraction = 0.08;
            m_duskFraction = 0.08;
        }

        /// <summary>An Earth-like cycle: 24 hours, a 29.5-day month.</summary>
        public static DayNightCycle EarthLike()
        {
            return new DayNightCycle("Earth", 24.0);
        }

        // ------------------------------------------------------------------
        // The clock
        // ------------------------------------------------------------------

        /// <summary>Advances the clock by some hours, rolling the day over.</summary>
        public int Advance(double hours)
        {
            if (hours <= 0.0)
                return 0;

            m_hour += hours;

            int rolled = 0;

            while (m_hour >= m_dayLengthHours)
            {
                m_hour -= m_dayLengthHours;
                m_day++;
                rolled++;
            }

            return rolled;
        }

        /// <summary>Advances by a fraction of a day.</summary>
        public int AdvanceDays(double days)
        {
            return Advance(days * m_dayLengthHours);
        }

        /// <summary>Puts the clock at a given hour, without moving the day on.</summary>
        public void SetHour(double hour)
        {
            m_hour = Wrap(hour, m_dayLengthHours);
        }

        /// <summary>The hour as a clock reading, e.g. "06:30".</summary>
        public string ClockText()
        {
            // Hours are shown on a 24-hour scale whatever the day length, so a
            // 30-hour day still reads as a familiar time.
            double fraction = m_hour / m_dayLengthHours;

            int totalMinutes = (int)Math.Round(fraction * 24.0 * 60.0);

            int hours = (totalMinutes / 60) % 24;
            int minutes = totalMinutes % 60;

            return hours.ToString("00") + ":" + minutes.ToString("00");
        }

        // ------------------------------------------------------------------
        // The phase of the day
        // ------------------------------------------------------------------

        /// <summary>Which part of the day it is now.</summary>
        public DayPhase Phase
        {
            get { return PhaseAt(m_hour); }
        }

        /// <summary>Which part of the day a given hour falls in.</summary>
        public DayPhase PhaseAt(double hour)
        {
            double fraction = Wrap(hour, m_dayLengthHours) / m_dayLengthHours;

            // Sixths of the day, with dawn and dusk straddling sunrise and
            // sunset. Night is the remainder.
            double dawnStart = 0.22 - m_dawnFraction;
            double dawnEnd = 0.22 + m_dawnFraction;
            double duskStart = 0.78 - m_duskFraction;
            double duskEnd = 0.78 + m_duskFraction;

            if (fraction < dawnStart)
                return DayPhase.Midnight;

            if (fraction < dawnEnd)
                return DayPhase.Dawn;

            if (fraction < 0.38)
                return DayPhase.Morning;

            if (fraction < 0.62)
                return DayPhase.Noon;

            if (fraction < duskStart)
                return DayPhase.Afternoon;

            if (fraction < duskEnd)
                return DayPhase.Dusk;

            if (fraction < 0.92)
                return DayPhase.Evening;

            return DayPhase.Night;
        }

        /// <summary>
        /// How light it is, 0 (full dark) to 1 (full day). Smooth through dawn
        /// and dusk, so a caller can fade something in rather than snapping it.
        /// </summary>
        public double LightLevel()
        {
            return LightLevelAt(m_hour);
        }

        public double LightLevelAt(double hour)
        {
            double fraction = Wrap(hour, m_dayLengthHours) / m_dayLengthHours;

            // A sine over the daylight half, and nothing over the night half.
            double sunrise = 0.22;
            double sunset = 0.78;

            if (fraction <= sunrise || fraction >= sunset)
            {
                // Twilight: a little light leaks past the horizon.
                double distance;

                if (fraction <= sunrise)
                    distance = sunrise - fraction;
                else
                    distance = fraction - sunset;

                double twilight = m_dawnFraction;

                if (twilight <= 0.0)
                    return 0.0;

                double fade = 1.0 - (distance / twilight);

                if (fade < 0.0)
                    fade = 0.0;

                return fade * 0.25;
            }

            double through = (fraction - sunrise) / (sunset - sunrise);

            return 0.25 + Math.Sin(through * Math.PI) * 0.75;
        }

        /// <summary>
        /// The temperature now: coldest before dawn, warmest in the early
        /// afternoon. The swing is scaled by latitude, because the poles have
        /// little day-to-day variation and the tropics have less than temperate
        /// regions.
        /// </summary>
        public double TemperatureAt(double latitude)
        {
            double light = LightLevel();

            // The lag is why the hottest hour is not noon: the ground takes time
            // to give its heat back.
            double lagged = LightLevelAt(m_hour - m_dayLengthHours * 0.06);

            double warmth = m_baseTemperatureCelsius + lagged * m_temperatureSwingCelsius;

            // Latitude damps the swing towards the poles.
            double damping = Math.Cos(latitude * Math.PI / 180.0);

            if (damping < 0.2)
                damping = 0.2;

            return m_baseTemperatureCelsius + (warmth - m_baseTemperatureCelsius) * damping;
        }

        // ------------------------------------------------------------------
        // The moon
        // ------------------------------------------------------------------

        /// <summary>The moon's phase now.</summary>
        public MoonPhase Moon
        {
            get { return MoonAt(m_day + m_hour / m_dayLengthHours); }
        }

        /// <summary>The moon's phase on a given day.</summary>
        public MoonPhase MoonAt(double dayNumber)
        {
            double age = (dayNumber + m_moonOffsetDays) % m_lunarMonthDays;

            if (age < 0.0)
                age += m_lunarMonthDays;

            double fraction = age / m_lunarMonthDays;

            // Eight phases, each an eighth of the month, centred on the
            // quarters so that "full" means full and not "nearly".
            if (fraction < 0.0625 || fraction >= 0.9375)
                return MoonPhase.New;

            if (fraction < 0.1875)
                return MoonPhase.WaxingCrescent;

            if (fraction < 0.3125)
                return MoonPhase.FirstQuarter;

            if (fraction < 0.4375)
                return MoonPhase.WaxingGibbous;

            if (fraction < 0.5625)
                return MoonPhase.Full;

            if (fraction < 0.6875)
                return MoonPhase.WaningGibbous;

            if (fraction < 0.8125)
                return MoonPhase.LastQuarter;

            return MoonPhase.WaningCrescent;
        }

        /// <summary>How bright the moon is, 0 at new to 1 at full.</summary>
        public double MoonBrightness()
        {
            double age = (m_day + m_hour / m_dayLengthHours + m_moonOffsetDays) % m_lunarMonthDays;

            if (age < 0.0)
                age += m_lunarMonthDays;

            double fraction = age / m_lunarMonthDays;

            // A cosine over the month: new at 0, full at the half.
            return (1.0 - Math.Cos(fraction * Math.PI * 2.0)) / 2.0;
        }

        /// <summary>
        /// How dark the night is, combining the sun and the moon. This is what
        /// decides whether a hunt is a moonlit one or a blind one.
        /// </summary>
        public double Darkness()
        {
            double light = LightLevel();

            if (light > 0.3)
                return 0.0;

            double moon = MoonBrightness();

            // A full moon gives back most of the darkness.
            return (1.0 - light) * (1.0 - moon * 0.7);
        }

        /// <summary>How many days until the next full moon.</summary>
        public double DaysUntilFullMoon()
        {
            double age = (m_day + m_hour / m_dayLengthHours + m_moonOffsetDays) % m_lunarMonthDays;

            if (age < 0.0)
                age += m_lunarMonthDays;

            double toFull = m_lunarMonthDays * 0.5 - age;

            if (toFull < 0.0)
                toFull += m_lunarMonthDays;

            return toFull;
        }

        // ------------------------------------------------------------------
        // Reporting
        // ------------------------------------------------------------------

        /// <summary>Everything a panel would show about the time.</summary>
        public string Describe()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            builder.AppendLine("Day and night");
            builder.AppendLine("  Day          " + m_day);
            builder.AppendLine("  Clock        " + ClockText());
            builder.AppendLine("  Phase        " + Phase);
            builder.AppendLine("  Light        " + Math.Round(LightLevel(), 2));
            builder.AppendLine("  Dark         " + IsDark);
            builder.AppendLine("  Darkness     " + Math.Round(Darkness(), 2));
            builder.AppendLine("  Moon         " + Moon + " (" + Math.Round(MoonBrightness() * 100.0) + "%)");
            builder.AppendLine("  Full in      " + Math.Round(DaysUntilFullMoon(), 1) + " days");
            builder.AppendLine("  Day length   " + m_dayLengthHours + " hours");
            builder.AppendLine("  Tilt         " + m_axialTiltDegrees + " degrees");

            return builder.ToString();
        }

        /// <summary>A one-line summary, for a status bar.</summary>
        public string Summary()
        {
            return "Day " + m_day + " " + ClockText() + " " + Phase +
                   (IsFullMoon ? " (full moon)" : string.Empty);
        }

        private static double Wrap(double value, double period)
        {
            if (period <= 0.0)
                return 0.0;

            double wrapped = value % period;

            if (wrapped < 0.0)
                wrapped += period;

            return wrapped;
        }

        private static double Limit(double value, double minimum, double maximum)
        {
            if (value < minimum)
                return minimum;

            if (value > maximum)
                return maximum;

            return value;
        }

        public override string ToString()
        {
            return m_name + " " + Summary();
        }
    }
}

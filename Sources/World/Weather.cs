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
    /// <summary>What the sky is doing.</summary>
    public enum WeatherKind
    {
        Clear,
        PartlyCloudy,
        Overcast,
        Mist,
        Fog,
        Drizzle,
        Rain,
        HeavyRain,
        Thunderstorm,
        Sleet,
        Snow,
        Blizzard,
        Hail,
        Sandstorm
    }

    /// <summary>How hard the wind is blowing.</summary>
    public enum WindStrength
    {
        Calm,
        Light,
        Moderate,
        Fresh,
        Strong,
        Gale,
        Storm
    }

    /// <summary>
    /// The weather at one place and time: what the sky is doing, how hard the
    /// wind is blowing, and what that does to everything that has to live in it.
    ///
    /// Weather is a set of numbers, not a story. It answers questions —
    /// `visibility()`, `coldStress()`, `trackingDifficulty()` — and the systems
    /// that care ask them.
    /// </summary>
    public sealed class Weather
    {
        private WeatherKind m_kind;
        private WindStrength m_wind;
        private double m_temperatureCelsius;
        private double m_humidity;
        private double m_precipitation;
        private double m_cloudCover;
        private double m_windSpeedKph;
        private double m_windDirectionDegrees;
        private double m_visibilityMetres;
        private double m_pressureHpa;
        private int m_durationMinutes;
        private string m_description;

        public WeatherKind Kind
        {
            get { return m_kind; }
            set { m_kind = value; }
        }

        public WindStrength Wind
        {
            get { return m_wind; }
            set { m_wind = value; }
        }

        public double TemperatureCelsius
        {
            get { return m_temperatureCelsius; }
            set { m_temperatureCelsius = value; }
        }

        /// <summary>Relative humidity, 0 to 1.</summary>
        public double Humidity
        {
            get { return m_humidity; }
            set { m_humidity = Limit(value, 0.0, 1.0); }
        }

        /// <summary>Rain, snow or hail falling now, in millimetres an hour.</summary>
        public double Precipitation
        {
            get { return m_precipitation; }
            set { m_precipitation = value < 0.0 ? 0.0 : value; }
        }

        /// <summary>How much of the sky is covered, 0 to 1.</summary>
        public double CloudCover
        {
            get { return m_cloudCover; }
            set { m_cloudCover = Limit(value, 0.0, 1.0); }
        }

        /// <summary>Wind speed in kilometres an hour.</summary>
        public double WindSpeedKph
        {
            get { return m_windSpeedKph; }
            set { m_windSpeedKph = value < 0.0 ? 0.0 : value; }
        }

        /// <summary>Where the wind is coming from, in degrees.</summary>
        public double WindDirectionDegrees
        {
            get { return m_windDirectionDegrees; }
            set { m_windDirectionDegrees = value; }
        }

        /// <summary>How far it is possible to see, in metres.</summary>
        public double VisibilityMetres
        {
            get { return m_visibilityMetres; }
            set { m_visibilityMetres = value < 1.0 ? 1.0 : value; }
        }

        /// <summary>Air pressure, in hectopascals.</summary>
        public double PressureHpa
        {
            get { return m_pressureHpa; }
            set { m_pressureHpa = value; }
        }

        /// <summary>How long this weather is expected to hold.</summary>
        public int DurationMinutes
        {
            get { return m_durationMinutes; }
            set { m_durationMinutes = value < 0 ? 0 : value; }
        }

        /// <summary>A sentence describing it, for a panel.</summary>
        public string Description
        {
            get { return m_description; }
            set { m_description = value; }
        }

        public Weather()
        {
            m_kind = WeatherKind.Clear;
            m_wind = WindStrength.Calm;
            m_temperatureCelsius = 10.0;
            m_humidity = 0.5;
            m_precipitation = 0.0;
            m_cloudCover = 0.0;
            m_windSpeedKph = 0.0;
            m_pressureHpa = 1013.0;
            m_visibilityMetres = 10000.0;
            m_description = string.Empty;
        }

        public static Weather Of(WeatherKind kind)
        {
            Weather weather = new Weather();

            weather.Kind = kind;

            return weather;
        }

        /// <summary>A copy, so a caller can adjust the forecast without writing
        /// back into the live weather.</summary>
        public Weather Clone()
        {
            Weather copy = new Weather();

            copy.m_kind = m_kind;
            copy.m_wind = m_wind;
            copy.m_temperatureCelsius = m_temperatureCelsius;
            copy.m_humidity = m_humidity;
            copy.m_precipitation = m_precipitation;
            copy.m_cloudCover = m_cloudCover;
            copy.m_windSpeedKph = m_windSpeedKph;
            copy.m_windDirectionDegrees = m_windDirectionDegrees;
            copy.m_visibilityMetres = m_visibilityMetres;
            copy.m_pressureHpa = m_pressureHpa;
            copy.m_durationMinutes = m_durationMinutes;
            copy.m_description = m_description;

            return copy;
        }

        // ------------------------------------------------------------------
        // What the weather does
        // ------------------------------------------------------------------

        /// <summary>
        /// How far a creature can see, as a fraction of a clear day. Fog and
        /// blizzards cut this badly; a clear night does not.
        /// </summary>
        public double VisibilityFactor()
        {
            double factor = VisibilityMetres / 10000.0;

            // Heavy cloud hides what a clear sky would show.
            factor *= 1.0 - m_cloudCover * 0.1;

            return Limit(factor, 0.02, 1.0);
        }

        /// <summary>
        /// How much harder the weather makes tracking. Rain washes a scent away,
        /// wind scatters it, snow holds it.
        /// </summary>
        public double TrackingDifficulty()
        {
            double difficulty = 0.0;

            // Rain washes a trail out.
            difficulty += m_precipitation * 0.08;

            // Wind scatters it, up to a point.
            difficulty += m_windSpeedKph / 100.0;

            // Snow holds a scent, so it helps.
            if (m_kind == WeatherKind.Snow)
                difficulty -= 0.2;

            if (m_kind == WeatherKind.Fog || m_kind == WeatherKind.Mist)
                difficulty -= 0.1;

            return Limit(difficulty, -0.3, 0.9);
        }

        /// <summary>
        /// How much the cold is hurting a creature that is not adapted to it.
        /// Positive is stress; zero or below is comfort.
        /// </summary>
        public double ColdStress(double adaptedToCelsius)
        {
            double excess = adaptedToCelsius - m_temperatureCelsius;

            if (excess <= 0.0)
                return 0.0;

            double stress = excess / 20.0;

            // Wind makes cold worse, which is the whole reason wind chill exists.
            stress *= 1.0 + m_windSpeedKph / 60.0;

            // Wet fur is worse than dry fur.
            if (m_precipitation > 0.0)
                stress *= 1.4;

            return stress;
        }

        /// <summary>How much the heat is hurting a creature.</summary>
        public double HeatStress(double adaptedToCelsius)
        {
            double excess = m_temperatureCelsius - adaptedToCelsius;

            if (excess <= 0.0)
                return 0.0;

            double stress = excess / 20.0;

            // Humidity makes heat worse, because sweating stops working.
            stress *= 1.0 + m_humidity * 0.6;

            return stress;
        }

        /// <summary>How much this weather favours a hunter that works in the dark.</summary>
        public double Concealment()
        {
            double concealment = 1.0 - VisibilityFactor();

            // Rain and wind cover sound and scent as well as sight.
            concealment += m_precipitation * 0.05;
            concealment += m_windSpeedKph / 200.0;

            return Limit(concealment, 0.0, 1.0);
        }

        /// <summary>Whether it is dangerous to be out in this.</summary>
        public bool IsSevere()
        {
            return m_kind == WeatherKind.Blizzard ||
                   m_kind == WeatherKind.Thunderstorm ||
                   m_kind == WeatherKind.Sandstorm ||
                   m_kind == WeatherKind.Hail ||
                   m_wind == WindStrength.Storm ||
                   m_wind == WindStrength.Gale;
        }

        /// <summary>Whether anything is falling.</summary>
        public bool IsWet()
        {
            return m_precipitation > 0.0 &&
                   m_kind != WeatherKind.Snow &&
                   m_kind != WeatherKind.Sleet;
        }

        public bool IsFrozen()
        {
            return m_kind == WeatherKind.Snow ||
                   m_kind == WeatherKind.Sleet ||
                   m_kind == WeatherKind.Blizzard ||
                   m_kind == WeatherKind.Hail;
        }

        /// <summary>One line, for a status bar.</summary>
        public string Summary()
        {
            return m_kind + ", " + Math.Round(m_temperatureCelsius) + "C, wind " +
                   Math.Round(m_windSpeedKph) + " kph (" + m_wind + ")";
        }

        public override string ToString()
        {
            return Summary();
        }

        private static double Limit(double value, double minimum, double maximum)
        {
            if (value < minimum)
                return minimum;

            if (value > maximum)
                return maximum;

            return value;
        }
    }

    /// <summary>
    /// Generates and advances the weather for one world.
    ///
    /// The model is deliberately simple and readable rather than
    /// meteorologically correct: temperature comes from the day/night cycle and
    /// the latitude, humidity drifts, and the weather kind follows from
    /// temperature, humidity and how the pressure is moving. That is enough to
    /// make weather that varies, that lasts a while, and that a player can learn
    /// to read.
    /// </summary>
    public sealed class WeatherSystem
    {
        private string m_name;
        private Weather m_current;
        private double m_latitude;
        private double m_humidity;
        private double m_pressure;
        private double m_pressureTrend;
        private double m_windDirection;
        private int m_minutesUntilChange;
        private Random m_random;
        private List<string> m_log;
        private double m_baseTemperatureCelsius;
        private double m_seasonalOffsetCelsius;
        private int m_changes;

        /// <summary>The weather right now.</summary>
        public Weather Current
        {
            get { return m_current; }
        }

        /// <summary>How far north or south this place is.</summary>
        public double Latitude
        {
            get { return m_latitude; }
            set { m_latitude = value; }
        }

        /// <summary>How many times the weather has turned since it started.</summary>
        public int Changes
        {
            get { return m_changes; }
        }

        /// <summary>A running commentary, newest last.</summary>
        public IList<string> Log
        {
            get { return m_log; }
        }

        /// <summary>A seasonal offset, added to the temperature.</summary>
        public double SeasonalOffsetCelsius
        {
            get { return m_seasonalOffsetCelsius; }
            set { m_seasonalOffsetCelsius = value; }
        }

        public WeatherSystem(string name, double latitude, int seed)
        {
            m_name = name;
            m_latitude = latitude;
            m_random = new Random(seed);
            m_log = new List<string>();
            m_humidity = 0.5;
            m_pressure = 1013.0;
            m_pressureTrend = 0.0;
            m_windDirection = 0.0;
            m_minutesUntilChange = 0;
            m_baseTemperatureCelsius = 10.0;
            m_seasonalOffsetCelsius = 0.0;
            m_changes = 0;

            m_current = new Weather();

            // Settle to a starting state, so the first reading is not a default.
            m_current.Kind = WeatherKind.PartlyCloudy;
            m_current.TemperatureCelsius = m_baseTemperatureCelsius;
            m_current.Humidity = m_humidity;
            m_current.PressureHpa = m_pressure;
            m_current.VisibilityMetres = 10000.0;

            m_minutesUntilChange = 60 + m_random.Next(120);

            Note("weather started: " + m_current.Summary());
        }

        /// <summary>A system for a temperate place at 50 degrees north.</summary>
        public static WeatherSystem Temperate(int seed)
        {
            WeatherSystem system = new WeatherSystem("Temperate", 50.0, seed);

            system.m_baseTemperatureCelsius = 10.0;

            return system;
        }

        /// <summary>A system for the tropics, hot and wet.</summary>
        public static WeatherSystem Tropical(int seed)
        {
            WeatherSystem system = new WeatherSystem("Tropical", 5.0, seed);

            system.m_baseTemperatureCelsius = 28.0;
            system.m_humidity = 0.8;

            return system;
        }

        /// <summary>A system for the poles, cold and dry.</summary>
        public static WeatherSystem Polar(int seed)
        {
            WeatherSystem system = new WeatherSystem("Polar", 78.0, seed);

            system.m_baseTemperatureCelsius = -18.0;
            system.m_humidity = 0.3;

            return system;
        }

        /// <summary>
        /// Advances the weather by some hours, given the cycle so the
        /// temperature follows the sun.
        ///
        /// Returns how many times the weather turned during that time.
        /// </summary>
        public int Advance(double hours, DayNightCycle cycle)
        {
            if (hours <= 0.0)
                return 0;

            int minutes = (int)Math.Round(hours * 60.0);
            int turned = 0;

            // Step in slices, so a long advance does not skip a change.
            while (minutes > 0)
            {
                int slice = Math.Min(minutes, 15);

                m_minutesUntilChange -= slice;
                minutes -= slice;

                Drift(slice, cycle);

                if (m_minutesUntilChange <= 0)
                {
                    Turn(cycle);
                    turned++;
                }
            }

            return turned;
        }

        /// <summary>
        /// The slow background drift: temperature follows the day, humidity
        /// creeps toward what the temperature allows, pressure moves along its
        /// trend.
        /// </summary>
        private void Drift(int minutes, DayNightCycle cycle)
        {
            double hours = minutes / 60.0;

            if (cycle != null)
            {
                double dayTemperature = cycle.TemperatureAt(m_latitude);

                // The air takes time to catch up with the ground, so the weather
                // temperature moves a fraction of the way toward the day's.
                double target = dayTemperature + m_seasonalOffsetCelsius;

                m_current.TemperatureCelsius += (target - m_current.TemperatureCelsius) * 0.25 * hours;
            }

            // Humidity falls in the warm part of the day and rises in the cold.
            double warmth = m_current.TemperatureCelsius / 30.0;

            m_humidity -= warmth * 0.02 * hours;

            // Rain puts moisture back into the air.
            if (m_current.Precipitation > 0.0)
                m_humidity += 0.05 * hours;

            m_humidity = Limit(m_humidity, 0.05, 0.98);

            // Pressure follows its trend and is pulled back toward normal.
            m_pressure += m_pressureTrend * hours;
            m_pressure += (1013.0 - m_pressure) * 0.02 * hours;

            m_current.Humidity = m_humidity;
            m_current.PressureHpa = m_pressure;

            // Wind wanders.
            m_windDirection += (m_random.NextDouble() - 0.5) * 20.0 * hours;
            m_current.WindDirectionDegrees = m_windDirection;
        }

        /// <summary>
        /// Decides what the weather does next, from the temperature, the
        /// humidity and how the pressure is moving.
        ///
        /// Falling pressure means worse weather; rising pressure means it is
        /// clearing. That single rule is what makes the weather feel as though it
        /// has causes rather than being drawn at random.
        /// </summary>
        private void Turn(DayNightCycle cycle)
        {
            double roll = m_random.NextDouble();

            // A new pressure trend: this is what the weather is reacting to.
            m_pressureTrend = (m_random.NextDouble() - 0.5) * 1.6;

            double freezing = 0.0;
            double warmth = m_current.TemperatureCelsius;

            // Where in the world we are decides what is plausible at all.
            double tropicalBias = 1.0 - Math.Abs(m_latitude) / 90.0;

            WeatherKind kind;

            if (warmth <= freezing)
            {
                // Freezing: precipitation falls as snow, or nothing falls.
                if (roll < 0.30 + m_humidity * 0.4)
                    kind = m_pressureTrend < 0.0 ? WeatherKind.Snow : WeatherKind.Overcast;
                else if (roll < 0.55)
                    kind = WeatherKind.Clear;
                else if (roll < 0.75)
                    kind = WeatherKind.PartlyCloudy;
                else if (roll < 0.88)
                    kind = WeatherKind.Mist;
                else
                    kind = WeatherKind.Blizzard;
            }
            else if (warmth <= 4.0)
            {
                // Around freezing: sleet is possible, and so is rain.
                if (roll < 0.25)
                    kind = WeatherKind.Sleet;
                else if (roll < 0.45)
                    kind = WeatherKind.Overcast;
                else if (roll < 0.65)
                    kind = WeatherKind.Clear;
                else if (roll < 0.85)
                    kind = WeatherKind.PartlyCloudy;
                else
                    kind = WeatherKind.Fog;
            }
            else if (warmth >= 30.0 && tropicalBias > 0.6)
            {
                // Hot: heat haze, storms, or a dry sandstorm.
                if (roll < 0.20)
                    kind = WeatherKind.Clear;
                else if (roll < 0.40)
                    kind = WeatherKind.Thunderstorm;
                else if (roll < 0.60)
                    kind = WeatherKind.HeavyRain;
                else if (roll < 0.80)
                    kind = WeatherKind.PartlyCloudy;
                else
                    kind = WeatherKind.Sandstorm;
            }
            else
            {
                // Temperate: the full range.
                if (roll < 0.18)
                    kind = WeatherKind.Clear;
                else if (roll < 0.38)
                    kind = WeatherKind.PartlyCloudy;
                else if (roll < 0.54)
                    kind = WeatherKind.Overcast;
                else if (roll < 0.64)
                    kind = WeatherKind.Mist;
                else if (roll < 0.72)
                    kind = WeatherKind.Fog;
                else if (roll < 0.80)
                    kind = WeatherKind.Drizzle;
                else if (roll < 0.88)
                    kind = WeatherKind.Rain;
                else if (roll < 0.94)
                    kind = WeatherKind.HeavyRain;
                else
                    kind = WeatherKind.Thunderstorm;
            }

            Apply(kind, cycle);

            m_changes++;

            Note("weather turned: " + m_current.Summary());
        }

        /// <summary>Sets every number the weather kind implies.</summary>
        private void Apply(WeatherKind kind, DayNightCycle cycle)
        {
            m_current.Kind = kind;

            // Rain adds to humidity, so a wet spell feeds itself for a while.
            switch (kind)
            {
                case WeatherKind.Clear:
                    m_current.CloudCover = 0.0;
                    m_current.Precipitation = 0.0;
                    m_current.VisibilityMetres = 20000.0;
                    m_current.WindSpeedKph = 5.0;
                    m_current.Wind = WindStrength.Light;
                    m_humidity -= 0.05;
                    break;

                case WeatherKind.PartlyCloudy:
                    m_current.CloudCover = 0.4;
                    m_current.Precipitation = 0.0;
                    m_current.VisibilityMetres = 15000.0;
                    m_current.WindSpeedKph = 12.0;
                    m_current.Wind = WindStrength.Moderate;
                    break;

                case WeatherKind.Overcast:
                    m_current.CloudCover = 0.9;
                    m_current.Precipitation = 0.0;
                    m_current.VisibilityMetres = 10000.0;
                    m_current.WindSpeedKph = 18.0;
                    m_current.Wind = WindStrength.Moderate;
                    break;

                case WeatherKind.Mist:
                    m_current.CloudCover = 0.6;
                    m_current.Precipitation = 0.0;
                    m_current.VisibilityMetres = 2000.0;
                    m_current.WindSpeedKph = 3.0;
                    m_current.Wind = WindStrength.Calm;
                    m_humidity += 0.1;
                    break;

                case WeatherKind.Fog:
                    m_current.CloudCover = 0.8;
                    m_current.Precipitation = 0.0;
                    m_current.VisibilityMetres = 200.0;
                    m_current.WindSpeedKph = 2.0;
                    m_current.Wind = WindStrength.Calm;
                    m_humidity += 0.15;
                    break;

                case WeatherKind.Drizzle:
                    m_current.CloudCover = 0.9;
                    m_current.Precipitation = 0.4;
                    m_current.VisibilityMetres = 8000.0;
                    m_current.WindSpeedKph = 10.0;
                    m_current.Wind = WindStrength.Light;
                    m_humidity += 0.1;
                    break;

                case WeatherKind.Rain:
                    m_current.CloudCover = 1.0;
                    m_current.Precipitation = 2.5;
                    m_current.VisibilityMetres = 5000.0;
                    m_current.WindSpeedKph = 22.0;
                    m_current.Wind = WindStrength.Fresh;
                    m_humidity += 0.15;
                    break;

                case WeatherKind.HeavyRain:
                    m_current.CloudCover = 1.0;
                    m_current.Precipitation = 9.0;
                    m_current.VisibilityMetres = 2000.0;
                    m_current.WindSpeedKph = 35.0;
                    m_current.Wind = WindStrength.Strong;
                    m_humidity += 0.2;
                    break;

                case WeatherKind.Thunderstorm:
                    m_current.CloudCover = 1.0;
                    m_current.Precipitation = 16.0;
                    m_current.VisibilityMetres = 1200.0;
                    m_current.WindSpeedKph = 55.0;
                    m_current.Wind = WindStrength.Gale;
                    m_humidity += 0.2;
                    break;

                case WeatherKind.Sleet:
                    m_current.CloudCover = 1.0;
                    m_current.Precipitation = 3.0;
                    m_current.VisibilityMetres = 3000.0;
                    m_current.WindSpeedKph = 25.0;
                    m_current.Wind = WindStrength.Fresh;
                    break;

                case WeatherKind.Snow:
                    m_current.CloudCover = 1.0;
                    m_current.Precipitation = 2.0;
                    m_current.VisibilityMetres = 4000.0;
                    m_current.WindSpeedKph = 15.0;
                    m_current.Wind = WindStrength.Moderate;
                    break;

                case WeatherKind.Blizzard:
                    m_current.CloudCover = 1.0;
                    m_current.Precipitation = 8.0;
                    m_current.VisibilityMetres = 150.0;
                    m_current.WindSpeedKph = 70.0;
                    m_current.Wind = WindStrength.Storm;
                    break;

                case WeatherKind.Hail:
                    m_current.CloudCover = 1.0;
                    m_current.Precipitation = 6.0;
                    m_current.VisibilityMetres = 2500.0;
                    m_current.WindSpeedKph = 40.0;
                    m_current.Wind = WindStrength.Strong;
                    break;

                case WeatherKind.Sandstorm:
                    m_current.CloudCover = 0.5;
                    m_current.Precipitation = 0.0;
                    m_current.VisibilityMetres = 400.0;
                    m_current.WindSpeedKph = 65.0;
                    m_current.Wind = WindStrength.Storm;
                    m_humidity -= 0.15;
                    break;
            }

            m_humidity = Limit(m_humidity, 0.05, 0.98);
            m_current.Humidity = m_humidity;

            // Sandstorms and fog last longer than a shower.
            switch (kind)
            {
                case WeatherKind.Clear:
                case WeatherKind.PartlyCloudy:
                    m_minutesUntilChange = 90 + m_random.Next(180);
                    break;

                case WeatherKind.Fog:
                case WeatherKind.Sandstorm:
                case WeatherKind.Blizzard:
                    m_minutesUntilChange = 120 + m_random.Next(240);
                    break;

                default:
                    m_minutesUntilChange = 45 + m_random.Next(120);
                    break;
            }

            m_current.DurationMinutes = m_minutesUntilChange;
            m_current.Description = Describe(kind);
        }

        private static string Describe(WeatherKind kind)
        {
            switch (kind)
            {
                case WeatherKind.Clear: return "Open sky, and a long view.";
                case WeatherKind.PartlyCloudy: return "Clouds moving through, and plenty of light.";
                case WeatherKind.Overcast: return "A flat grey sky with no break in it.";
                case WeatherKind.Mist: return "A damp haze hangs over the ground.";
                case WeatherKind.Fog: return "Fog thick enough to lose the pack in.";
                case WeatherKind.Drizzle: return "A fine rain that soaks through slowly.";
                case WeatherKind.Rain: return "Steady rain, and the ground is turning to mud.";
                case WeatherKind.HeavyRain: return "Rain coming down hard enough to flatten grass.";
                case WeatherKind.Thunderstorm: return "Thunder, and rain in sheets.";
                case WeatherKind.Sleet: return "Half rain, half ice, and it stings.";
                case WeatherKind.Snow: return "Snow falling steadily, and it holds a scent.";
                case WeatherKind.Blizzard: return "Snow driven sideways. Nothing can see anything.";
                case WeatherKind.Hail: return "Ice falling hard enough to bruise.";
                case WeatherKind.Sandstorm: return "Grit in the air and no horizon.";
                default: return string.Empty;
            }
        }

        /// <summary>Forces the weather to a particular kind, for a test or a scene.</summary>
        public void Force(WeatherKind kind, DayNightCycle cycle)
        {
            Apply(kind, cycle);
            m_changes++;
            Note("weather forced: " + m_current.Summary());
        }

        private void Note(string message)
        {
            m_log.Add(message);

            while (m_log.Count > 200)
                m_log.RemoveAt(0);
        }

        /// <summary>Everything a panel would show about the weather.</summary>
        public string Describe()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            builder.AppendLine("Weather");
            builder.AppendLine("  Kind         " + m_current.Kind);
            builder.AppendLine("  " + m_current.Description);
            builder.AppendLine("  Temperature  " + Math.Round(m_current.TemperatureCelsius, 1) + " C");
            builder.AppendLine("  Wind         " + Math.Round(m_current.WindSpeedKph) + " kph " +
                               m_current.Wind + ", from " +
                               Math.Round(m_current.WindDirectionDegrees) + " degrees");
            builder.AppendLine("  Humidity     " + Math.Round(m_current.Humidity * 100.0) + "%");
            builder.AppendLine("  Cloud        " + Math.Round(m_current.CloudCover * 100.0) + "%");
            builder.AppendLine("  Rain         " + Math.Round(m_current.Precipitation, 1) + " mm/h");
            builder.AppendLine("  Visibility   " + Math.Round(m_current.VisibilityMetres) + " m");
            builder.AppendLine("  Pressure     " + Math.Round(m_current.PressureHpa, 1) + " hPa" +
                               (m_pressureTrend < 0.0 ? " (falling)" : m_pressureTrend > 0.0 ? " (rising)" : string.Empty));
            builder.AppendLine("  Holds for    " + m_current.DurationMinutes + " minutes");
            builder.AppendLine();
            builder.AppendLine("What it does");
            builder.AppendLine("  Visibility   " + Math.Round(m_current.VisibilityFactor(), 3) + " of clear");
            builder.AppendLine("  Tracking     " + Math.Round(m_current.TrackingDifficulty(), 3) + " harder");
            builder.AppendLine("  Concealment  " + Math.Round(m_current.Concealment(), 3));
            builder.AppendLine("  Cold stress  " + Math.Round(m_current.ColdStress(20.0), 3) + " (at 20 C comfort)");
            builder.AppendLine("  Severe       " + m_current.IsSevere());
            builder.AppendLine("  Wet          " + m_current.IsWet());
            builder.AppendLine("  Frozen       " + m_current.IsFrozen());

            return builder.ToString();
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
            return m_name + " weather: " + m_current.Summary();
        }
    }

    /// <summary>
    /// Weather and time together: one clock, one weather system, advanced as a
    /// unit.
    ///
    /// This exists because the two are not independent — the temperature comes
    /// from the sun, the moon decides how dark the night is, and the weather
    /// decides whether that matters. Splitting them would mean every caller
    /// having to remember to advance both in the right order.
    /// </summary>
    public sealed class WorldClock
    {
        private DayNightCycle m_cycle;
        private WeatherSystem m_weather;
        private List<string> m_log;

        /// <summary>The day and night cycle.</summary>
        public DayNightCycle Cycle
        {
            get { return m_cycle; }
        }

        /// <summary>The weather.</summary>
        public WeatherSystem Weather
        {
            get { return m_weather; }
        }

        /// <summary>What has happened, newest last.</summary>
        public IList<string> Log
        {
            get { return m_log; }
        }

        /// <summary>True when it is dark enough for a night hunt.</summary>
        public bool IsNight
        {
            get { return m_cycle.IsDark; }
        }

        /// <summary>True on the night of a full moon.</summary>
        public bool IsFullMoon
        {
            get { return m_cycle.IsFullMoon; }
        }

        /// <summary>
        /// How good the conditions are for hunting, 0 to 1. Darkness helps, bad
        /// weather helps, and a full moon takes some of it back.
        /// </summary>
        public double HuntingConditions()
        {
            double conditions = 0.3;

            conditions += m_cycle.Darkness() * 0.4;
            conditions += m_weather.Current.Concealment() * 0.2;

            // A full moon is bright enough to be a disadvantage.
            if (m_cycle.IsFullMoon)
                conditions -= 0.15;

            // Severe weather is dangerous rather than merely useful.
            if (m_weather.Current.IsSevere())
                conditions -= 0.2;

            return Limit(conditions, 0.0, 1.0);
        }

        public WorldClock(string name, double latitude, int seed)
        {
            m_cycle = new DayNightCycle(name, 24.0);
            m_weather = new WeatherSystem(name, latitude, seed);
            m_log = new List<string>();
        }

        /// <summary>An Earth-like world clock at a given latitude.</summary>
        public static WorldClock EarthLike(double latitude, int seed)
        {
            return new WorldClock("Terra Nova", latitude, seed);
        }

        /// <summary>Advances both the clock and the weather.</summary>
        public void Advance(double hours)
        {
            if (hours <= 0.0)
                return;

            int days = m_cycle.Advance(hours);
            int turns = m_weather.Advance(hours, m_cycle);

            if (days > 0)
                Note("day " + m_cycle.Day + " begins");

            if (turns > 0)
                Note("weather turned: " + m_weather.Current.Kind);
        }

        /// <summary>Runs forward by whole days.</summary>
        public void AdvanceDays(double days)
        {
            Advance(days * m_cycle.DayLengthHours);
        }

        /// <summary>How dark it is, with the weather taken into account.</summary>
        public double EffectiveDarkness()
        {
            double darkness = m_cycle.Darkness();

            // Heavy cloud hides the moon, so a cloudy night is darker.
            darkness += m_weather.Current.CloudCover * 0.15;

            return Limit(darkness, 0.0, 1.0);
        }

        /// <summary>Everything a panel would show, time and weather together.</summary>
        public string Describe()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            builder.Append(m_cycle.Describe());
            builder.AppendLine();
            builder.Append(m_weather.Describe());
            builder.AppendLine();
            builder.AppendLine("Together");
            builder.AppendLine("  Night            " + IsNight);
            builder.AppendLine("  Effective dark   " + Math.Round(EffectiveDarkness(), 3));
            builder.AppendLine("  Hunting          " + Math.Round(HuntingConditions(), 3));

            return builder.ToString();
        }

        /// <summary>One line, for a status bar.</summary>
        public string Summary()
        {
            return m_cycle.Summary() + " | " + m_weather.Current.Summary();
        }

        private void Note(string message)
        {
            m_log.Add(message);

            while (m_log.Count > 200)
                m_log.RemoveAt(0);
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
            return Summary();
        }
    }
}

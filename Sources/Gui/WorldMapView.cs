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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using LifeEngine.Biology;
using LifeEngine.Ecosystem;

namespace LifeEngine.Gui
{
    /// <summary>
    /// Draws a <see cref="WorldMap"/>: one tile per pixel block, coloured by
    /// biome, with the living population drawn on top as a dot per species.
    ///
    /// It reads the map every paint rather than keeping a copy, because the
    /// ecosystem mutates the tiles' populations on every tick and a cached copy
    /// would go stale silently.
    /// </summary>
    public sealed class WorldMapView : Control
    {
        private AppContext m_context;
        private bool m_showLife = true;
        private bool m_showRivers = true;
        private int m_selectedTileX = -1;
        private int m_selectedTileY = -1;

        /// <summary>Raised when the user clicks a tile.</summary>
        public event EventHandler TileSelected;

        public WorldMapView(AppContext context)
        {
            m_context = context;

            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = Color.FromArgb(12, 18, 28);
            TabStop = false;
        }

        /// <summary>Whether the living population is drawn over the terrain.</summary>
        public bool ShowLife
        {
            get { return m_showLife; }
            set { m_showLife = value; Invalidate(); }
        }

        /// <summary>Whether rivers are drawn.</summary>
        public bool ShowRivers
        {
            get { return m_showRivers; }
            set { m_showRivers = value; Invalidate(); }
        }

        /// <summary>The tile the user last clicked, or -1.</summary>
        public int SelectedTileX
        {
            get { return m_selectedTileX; }
        }

        public int SelectedTileY
        {
            get { return m_selectedTileY; }
        }

        /// <summary>A description of the selected tile, for a details bar.</summary>
        public string DescribeSelection()
        {
            WorldMap world = GetWorld();

            if (world == null)
                return "No world.";

            Tile tile = world.GetTile(m_selectedTileX, m_selectedTileY);

            if (tile == null)
                return "Click the map for a tile.";

            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            builder.Append("(" + tile.X + "," + tile.Y + ")  " + tile.Biome);
            builder.Append("  " + Math.Round(tile.TemperatureCelsius, 1) + "C");
            builder.Append("  " + Math.Round(tile.RainfallMm) + "mm");
            builder.Append("  " + Math.Round(tile.ElevationMetres) + "m");

            if (tile.HasRiver)
                builder.Append("  river");

            if (tile.IsCoastal)
                builder.Append("  coastal");

            if (m_showLife && tile.Organisms.Count > 0)
            {
                builder.Append("  |  ");

                for (int i = 0; i < tile.Organisms.Count; i++)
                {
                    Ecosystem.Species species = tile.Organisms[i] as Ecosystem.Species;

                    if (species == null)
                        continue;

                    if (i > 0)
                        builder.Append(", ");

                    builder.Append(species.Name + " " + tile.Populations[i]);
                }
            }

            return builder.ToString();
        }

        private WorldMap GetWorld()
        {
            if (m_context == null || m_context.Ecosystem == null)
                return null;

            return m_context.Ecosystem.World;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            WorldMap world = GetWorld();

            if (world != null)
            {
                Rectangle area = MapArea(world);

                if (area.Contains(e.Location))
                {
                    m_selectedTileX = (e.X - area.Left) * world.Columns / Math.Max(1, area.Width);
                    m_selectedTileY = (e.Y - area.Top) * world.Rows / Math.Max(1, area.Height);

                    Invalidate();

                    EventHandler handler = TileSelected;

                    if (handler != null)
                        handler(this, EventArgs.Empty);
                }
            }

            base.OnMouseDown(e);
        }

        /// <summary>
        /// The rectangle the map is drawn into: the control's client area with
        /// the aspect ratio of the world kept, so tiles stay square.
        /// </summary>
        private static Rectangle MapArea(WorldMap world)
        {
            return new Rectangle(0, 0, world.Columns, world.Rows);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics painter = e.Graphics;

            painter.Clear(BackColor);

            WorldMap world = GetWorld();

            if (world == null)
            {
                DrawCentredText(painter, "No world yet.  World menu, New World.");
                return;
            }

            // Work out the tile size that fits the control, keeping tiles square.
            float cellWidth = (float)ClientSize.Width / world.Columns;
            float cellHeight = (float)ClientSize.Height / world.Rows;

            float cell = Math.Min(cellWidth, cellHeight);

            if (cell < 1.0f)
                cell = 1.0f;

            float offsetX = (ClientSize.Width - cell * world.Columns) / 2.0f;
            float offsetY = (ClientSize.Height - cell * world.Rows) / 2.0f;

            // Terrain.
            for (int y = 0; y < world.Rows; y++)
            {
                for (int x = 0; x < world.Columns; x++)
                {
                    Tile tile = world.GetTile(x, y);

                    if (tile == null)
                        continue;

                    using (SolidBrush brush = new SolidBrush(TileColour(tile)))
                    {
                        painter.FillRectangle(brush,
                            offsetX + x * cell, offsetY + y * cell,
                            cell + 0.5f, cell + 0.5f);
                    }
                }
            }

            // Rivers over the terrain, as thin blue lines down the tile.
            if (m_showRivers && world.RiverCourses.Count > 0)
            {
                using (Pen pen = new Pen(Color.FromArgb(120, 90, 170, 255), Math.Max(1.0f, cell * 0.2f)))
                {
                    for (int y = 0; y < world.Rows; y++)
                    {
                        for (int x = 0; x < world.Columns; x++)
                        {
                            Tile tile = world.GetTile(x, y);

                            if (tile == null || !tile.HasRiver)
                                continue;

                            float centreX = offsetX + x * cell + cell / 2.0f;
                            float centreY = offsetY + y * cell + cell / 2.0f;

                            painter.DrawLine(pen, centreX, centreY - cell / 2.0f,
                                                  centreX, centreY + cell / 2.0f);
                        }
                    }
                }
            }

            // The living population, as a dot per species per tile.
            if (m_showLife)
            {
                for (int y = 0; y < world.Rows; y++)
                {
                    for (int x = 0; x < world.Columns; x++)
                    {
                        Tile tile = world.GetTile(x, y);

                        if (tile == null || tile.Organisms.Count == 0)
                            continue;

                        DrawLife(painter, tile, offsetX + x * cell, offsetY + y * cell, cell);
                    }
                }
            }

            // The selection.
            if (m_selectedTileX >= 0 && m_selectedTileY >= 0)
            {
                using (Pen pen = new Pen(Color.White, 1.5f))
                {
                    painter.DrawRectangle(pen,
                        offsetX + m_selectedTileX * cell,
                        offsetY + m_selectedTileY * cell,
                        cell, cell);
                }
            }

            DrawLegend(painter, offsetX + cell * world.Columns + 4.0f, offsetY);
        }

        /// <summary>One dot per species on the tile, sized by how many there are.</summary>
        private static void DrawLife(Graphics painter, Tile tile, float left, float top, float cell)
        {
            int count = tile.Organisms.Count;

            if (count == 0)
                return;

            float step = cell / (count + 1);

            for (int i = 0; i < count; i++)
            {
                Ecosystem.Species species = tile.Organisms[i] as Ecosystem.Species;

                if (species == null)
                    continue;

                int population = tile.Populations[i];

                if (population <= 0)
                    continue;

                float size = Math.Max(1.0f, Math.Min(cell * 0.7f, cell * population / 60.0f));
                float dotX = left + step * (i + 1);
                float dotY = top + cell / 2.0f;

                using (SolidBrush brush = new SolidBrush(SpeciesColour(species)))
                    painter.FillEllipse(brush, dotX - size / 2.0f, dotY - size / 2.0f, size, size);
            }
        }

        private static void DrawLegend(Graphics painter, float right, float top)
        {
            string[] names = { "Grass", "Forest", "Jungle", "Algae", "Bee", "Wolf", "Eagle" };
            Color[] colours = { LawnGreen, ForestGreen, LimeGreen, Teal, Gold, DimGray, Goldenrod };

            float y = top + 4.0f;
            float x = right + 6.0f;

            if (x > 1000.0f)
                return;

            using (Font font = new Font("Segoe UI", 7.0f))
            {
                for (int i = 0; i < names.Length; i++)
                {
                    using (SolidBrush brush = new SolidBrush(colours[i]))
                        painter.FillEllipse(brush, x, y + 2.0f, 7.0f, 7.0f);

                    using (SolidBrush text = new SolidBrush(Color.Gainsboro))
                        painter.DrawString(names[i], font, text, x + 11.0f, y);

                    y += 14.0f;
                }
            }
        }

        private void DrawCentredText(Graphics painter, string text)
        {
            using (Font font = new Font("Segoe UI", 10.0f))
            using (SolidBrush brush = new SolidBrush(Color.Silver))
            {
                SizeF size = painter.MeasureString(text, font);

                painter.DrawString(text, font, brush,
                    (ClientSize.Width - size.Width) / 2.0f,
                    (ClientSize.Height - size.Height) / 2.0f);
            }
        }

        // ------------------------------------------------------------------
        // Colours
        // ------------------------------------------------------------------

        private static readonly Color LawnGreen = Color.FromArgb(124, 252, 0);
        private static readonly Color ForestGreen = Color.FromArgb(34, 139, 34);
        private static readonly Color LimeGreen = Color.FromArgb(50, 205, 50);
        private static readonly Color Teal = Color.FromArgb(0, 128, 128);
        private static readonly Color Gold = Color.FromArgb(255, 215, 0);
        private static readonly Color DimGray = Color.FromArgb(105, 105, 105);
        private static readonly Color Goldenrod = Color.FromArgb(218, 165, 32);

        /// <summary>The colour of a biome, and of the water and ice within it.</summary>
        public static Color TileColour(Tile tile)
        {
            if (tile == null)
                return Color.Black;

            if (tile.HasRiver)
                return Color.FromArgb(70, 120, 200);

            if (tile.IsWater)
            {
                // Deeper water is darker, which is the only bathymetry on the map.
                if (tile.ElevationMetres < -2000.0)
                    return Color.FromArgb(8, 24, 60);

                if (tile.ElevationMetres < -500.0)
                    return Color.FromArgb(14, 40, 90);

                return Color.FromArgb(24, 64, 120);
            }

            switch (tile.Biome)
            {
                case BiomeType.IceSheet: return Color.FromArgb(235, 240, 248);
                case BiomeType.Tundra: return Color.FromArgb(140, 150, 130);
                case BiomeType.BorealForest: return Color.FromArgb(38, 84, 52);
                case BiomeType.TemperateDeciduousForest: return Color.FromArgb(58, 118, 60);
                case BiomeType.TemperateRainforest: return Color.FromArgb(28, 96, 66);
                case BiomeType.TemperateGrassland: return Color.FromArgb(138, 168, 82);
                case BiomeType.Mediterranean: return Color.FromArgb(168, 160, 84);
                case BiomeType.ColdDesert: return Color.FromArgb(178, 170, 140);
                case BiomeType.HotDesert: return Color.FromArgb(214, 184, 108);
                case BiomeType.TropicalSavanna: return Color.FromArgb(186, 170, 76);
                case BiomeType.TropicalSeasonalForest: return Color.FromArgb(52, 128, 56);
                case BiomeType.TropicalRainforest: return Color.FromArgb(22, 96, 40);
                case BiomeType.Mountain: return Color.FromArgb(120, 120, 120);
                case BiomeType.Wetland: return Color.FromArgb(70, 110, 96);
                case BiomeType.Freshwater: return Color.FromArgb(70, 130, 180);
                default: return Color.FromArgb(90, 90, 90);
            }
        }

        /// <summary>The colour a species is drawn in, from its colour key.</summary>
        public static Color SpeciesColour(Ecosystem.Species species)
        {
            if (species == null)
                return Color.White;

            Color known = Color.FromName(species.ColourKey);

            if (known.A == 0 && known.R == 0 && known.G == 0 && known.B == 0)
            {
                // FromName yields a zero colour for a name it does not know.
                if (!string.Equals(species.ColourKey, "Black", StringComparison.OrdinalIgnoreCase))
                    return Color.DeepSkyBlue;
            }

            return known;
        }

        /// <summary>The colour for a species' symbol, for a text legend.</summary>
        public static Brush SpeciesBrush(Ecosystem.Species species)
        {
            return new SolidBrush(SpeciesColour(species));
        }
    }
}

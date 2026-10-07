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
 *  Generates the menu and toolbar icons.
 *
 *  Run it from the repository root:
 *
 *      csc /out:IconGen.exe Sources/Tools/IconGenerator.cs ^
 *          /reference:System.Drawing.dll /reference:System.Windows.Forms.dll
 *      IconGen.exe
 *
 *  Or build and run it through the project, which compiles it with everything
 *  else but does not call it unless the icon files are missing.
 *
 *  The style is copied from the three icons the project already had: a 32 by 32
 *  canvas, pure black on a transparent background, flat rectangles, no
 *  anti-aliasing, no gradients, no colour. Every icon is a composition of a few
 *  filled rectangles and polygons, so they read at 16 by 16 in a menu and at 32
 *  by 32 in a toolbar.
 *
 *  Nothing here runs at start-up. It is a build-time tool, kept in the tree so
 *  the icons can be regenerated or added to rather than being unreadable binary
 *  files of unknown provenance.
 * ---------------------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace LifeEngine.Tools
{
    /// <summary>
    /// Draws every icon the program uses and writes them to Sources/Resources.
    /// </summary>
    public static class IconGenerator
    {
        private const int Size = 32;

        /// <summary>Every icon, by the file name it is written to.</summary>
        public static List<string> GenerateAll(string directory)
        {
            List<string> written = new List<string>();

            if (string.IsNullOrEmpty(directory))
                directory = "Resources";

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // Existing icons, regenerated to the same style.
            written.Add(Write(directory, "IconGrid", DrawGrid));
            written.Add(Write(directory, "IconColor", DrawColor));
            written.Add(Write(directory, "IconHelp", DrawHelp));

            // Simulation.
            written.Add(Write(directory, "IconStart", DrawStart));
            written.Add(Write(directory, "IconStop", DrawStop));
            written.Add(Write(directory, "IconStep", DrawStep));
            written.Add(Write(directory, "IconReset", DrawReset));
            written.Add(Write(directory, "IconClear", DrawClear));
            written.Add(Write(directory, "IconRandom", DrawRandom));

            // View.
            written.Add(Write(directory, "IconZoomIn", DrawZoomIn));
            written.Add(Write(directory, "IconZoomOut", DrawZoomOut));
            written.Add(Write(directory, "IconSidebar", DrawSidebar));
            written.Add(Write(directory, "IconHistory", DrawHistory));

            // The new systems.
            written.Add(Write(directory, "IconWorld", DrawWorld));
            written.Add(Write(directory, "IconEcosystem", DrawEcosystem));
            written.Add(Write(directory, "IconSpecies", DrawSpecies));
            written.Add(Write(directory, "IconTile", DrawTile));
            written.Add(Write(directory, "IconWolf", DrawWolf));
            written.Add(Write(directory, "IconPack", DrawPack));
            written.Add(Write(directory, "IconHunt", DrawHunt));
            written.Add(Write(directory, "IconMoon", DrawMoon));

            written.Add(Write(directory, "IconScript", DrawScript));
            written.Add(Write(directory, "IconConsole", DrawConsole));
            written.Add(Write(directory, "IconSettings", DrawSettings));
            written.Add(Write(directory, "IconSchedule", DrawSchedule));
            written.Add(Write(directory, "IconUpdate", DrawUpdate));
            written.Add(Write(directory, "IconDatabase", DrawDatabase));
            written.Add(Write(directory, "IconExcel", DrawExcel));
            written.Add(Write(directory, "IconExport", DrawExport));
            written.Add(Write(directory, "IconLog", DrawLog));

            // The cell states, for the legend and the colour swatches.
            written.Add(Write(directory, "IconCellAlive", DrawCellAlive));
            written.Add(Write(directory, "IconCellDead", DrawCellDead));
            written.Add(Write(directory, "IconCellHistory", DrawCellHistory));

            return written;
        }

        /// <summary>Writes one icon, drawing it through the supplied action.</summary>
        private static string Write(string directory, string name, Action<Graphics> draw)
        {
            string path = Path.Combine(directory, name + ".png");

            using (Bitmap bitmap = new Bitmap(Size, Size, PixelFormat.Format32bppArgb))
            {
                using (Graphics painter = Graphics.FromImage(bitmap))
                {
                    // Flat, aliased drawing: the existing icons have hard edges,
                    // and smoothing them would make them look like a different
                    // set.
                    painter.SmoothingMode = SmoothingMode.None;
                    painter.InterpolationMode = InterpolationMode.NearestNeighbor;
                    painter.PixelOffsetMode = PixelOffsetMode.Half;
                    painter.Clear(Color.Transparent);

                    draw(painter);
                }

                bitmap.Save(path, ImageFormat.Png);
            }

            return path;
        }

        // ------------------------------------------------------------------
        // Drawing helpers
        // ------------------------------------------------------------------

        private static readonly SolidBrush Black = new SolidBrush(Color.Black);
        private static readonly SolidBrush White = new SolidBrush(Color.White);

        /// <summary>A filled black rectangle.</summary>
        private static void Rect(Graphics g, int x, int y, int width, int height)
        {
            g.FillRectangle(Black, x, y, width, height);
        }

        /// <summary>
        /// A filled WHITE rectangle. This is how detail is cut out of a black
        /// silhouette: the icons are two-tone, so anything that has to read as a
        /// feature rather than part of the shape is drawn white.
        ///
        /// The first attempt used black for these, which is why the wolf's eye
        /// and muzzle and the world's continents were invisible: black on black.
        /// </summary>
        private static void Cut(Graphics g, int x, int y, int width, int height)
        {
            g.FillRectangle(White, x, y, width, height);
        }

        /// <summary>A filled black rectangle with a white hole, i.e. an outlined box.</summary>
        private static void Frame(Graphics g, int x, int y, int width, int height, int border)
        {
            Rect(g, x, y, width, height);
            Cut(g, x + border, y + border, width - border * 2, height - border * 2);
        }

        /// <summary>A filled black polygon from a flat coordinate list.</summary>
        private static void Poly(Graphics g, params int[] points)
        {
            Fill(g, Black, points);
        }

        /// <summary>
        /// A filled WHITE polygon, for cutting a shape out of a black one. The
        /// counterpart of <see cref="Cut"/> for anything not rectangular.
        /// </summary>
        private static void CutPoly(Graphics g, params int[] points)
        {
            Fill(g, White, points);
        }

        private static void Fill(Graphics g, Brush brush, int[] points)
        {
            if (points == null || points.Length < 6 || points.Length % 2 != 0)
                return;

            Point[] polygon = new Point[points.Length / 2];

            for (int i = 0; i < polygon.Length; i++)
                polygon[i] = new Point(points[i * 2], points[i * 2 + 1]);

            g.FillPolygon(brush, polygon);
        }

        /// <summary>A black circle, drawn as a polygon so no anti-aliasing creeps in.</summary>
        private static void Disc(Graphics g, int centreX, int centreY, int radius)
        {
            FillCircle(g, Black, centreX, centreY, radius);
        }

        /// <summary>
        /// A WHITE circle, for cutting a hole out of a black one. The
        /// counterpart of <see cref="Disc"/>.
        /// </summary>
        private static void CutDisc(Graphics g, int centreX, int centreY, int radius)
        {
            FillCircle(g, White, centreX, centreY, radius);
        }

        private static void FillCircle(Graphics g, Brush brush, int centreX, int centreY, int radius)
        {
            int steps = Math.Max(12, radius * 4);
            Point[] points = new Point[steps];

            for (int i = 0; i < steps; i++)
            {
                double angle = (Math.PI * 2.0 * i) / steps;

                points[i] = new Point(
                    centreX + (int)Math.Round(Math.Cos(angle) * radius),
                    centreY + (int)Math.Round(Math.Sin(angle) * radius));
            }

            g.FillPolygon(brush, points);
        }

        /// <summary>A thick black line between two points.</summary>
        private static void Line(Graphics g, int x1, int y1, int x2, int y2, int thickness)
        {
            using (Pen pen = new Pen(Color.Black, thickness))
            {
                pen.StartCap = LineCap.Square;
                pen.EndCap = LineCap.Square;

                g.DrawLine(pen, x1, y1, x2, y2);
            }
        }

        // ------------------------------------------------------------------
        // The original three
        // ------------------------------------------------------------------

        /// <summary>Four thick squares in a 2x2 arrangement: the grid.</summary>
        private static void DrawGrid(Graphics g)
        {
            Frame(g, 2, 2, 13, 13, 5);
            Frame(g, 17, 2, 13, 13, 5);
            Frame(g, 2, 17, 13, 13, 5);
            Frame(g, 17, 17, 13, 13, 5);
        }

        /// <summary>A square split into three: the colour swatch.</summary>
        private static void DrawColor(Graphics g)
        {
            Rect(g, 4, 4, 24, 24);
            g.FillRectangle(White, 8, 8, 16, 16);

            // A diagonal, so it reads as "shade" rather than "empty box".
            Poly(g, 8, 24, 24, 8, 24, 12, 12, 24);
        }

        /// <summary>A question mark: the help icon.</summary>
        private static void DrawHelp(Graphics g)
        {
            Rect(g, 3, 3, 26, 26);
            g.FillRectangle(White, 6, 6, 20, 20);

            // The hook of the question mark.
            Rect(g, 12, 9, 8, 3);
            Rect(g, 17, 12, 3, 4);
            Rect(g, 14, 16, 6, 3);
            Rect(g, 14, 19, 3, 3);

            // The dot.
            Rect(g, 14, 23, 3, 3);
        }

        // ------------------------------------------------------------------
        // Simulation
        // ------------------------------------------------------------------

        /// <summary>A filled triangle pointing right: start.</summary>
        private static void DrawStart(Graphics g)
        {
            Poly(g, 8, 5, 8, 27, 26, 16);
        }

        /// <summary>A filled square: stop.</summary>
        private static void DrawStop(Graphics g)
        {
            Rect(g, 7, 7, 18, 18);
        }

        /// <summary>A triangle with a bar: step one.</summary>
        private static void DrawStep(Graphics g)
        {
            Poly(g, 5, 6, 5, 26, 20, 16);
            Rect(g, 23, 6, 4, 20);
        }

        /// <summary>A circular arrow: reset.</summary>
        private static void DrawReset(Graphics g)
        {
            // A ring, with a wedge cut out of it for the gap.
            Disc(g, 16, 16, 12);
            CutDisc(g, 16, 16, 7);

            CutPoly(g, 16, 16, 30, 0, 30, 14);

            // The arrowhead at the open end.
            Poly(g, 11, 4, 24, 7, 12, 13);
        }

        /// <summary>A dustbin: clear.</summary>
        private static void DrawClear(Graphics g)
        {
            Rect(g, 6, 4, 20, 4);
            Rect(g, 13, 1, 6, 3);

            Frame(g, 8, 9, 16, 20, 3);

            // Two slats, so it reads as a bin rather than a box.
            Rect(g, 13, 13, 2, 12);
            Rect(g, 18, 13, 2, 12);
        }

        /// <summary>Scattered dots: randomize.</summary>
        private static void DrawRandom(Graphics g)
        {
            Rect(g, 3, 3, 6, 6);
            Rect(g, 13, 8, 6, 6);
            Rect(g, 23, 4, 6, 6);
            Rect(g, 5, 14, 6, 6);
            Rect(g, 16, 17, 6, 6);
            Rect(g, 23, 23, 6, 6);
            Rect(g, 8, 24, 6, 6);
        }

        // ------------------------------------------------------------------
        // View
        // ------------------------------------------------------------------

        /// <summary>A magnifying glass with a plus: zoom in.</summary>
        private static void DrawZoomIn(Graphics g)
        {
            Disc(g, 14, 14, 10);
            CutDisc(g, 14, 14, 6);

            // The handle.
            Line(g, 21, 21, 29, 29, 5);

            // The plus, inside the lens, in black on the white middle.
            Rect(g, 12, 8, 4, 12);
            Rect(g, 8, 12, 12, 4);
        }

        /// <summary>A magnifying glass with a minus: zoom out.</summary>
        private static void DrawZoomOut(Graphics g)
        {
            Disc(g, 14, 14, 10);
            CutDisc(g, 14, 14, 6);

            Line(g, 21, 21, 29, 29, 5);

            Rect(g, 8, 12, 12, 4);
        }

        /// <summary>A panel down the left edge: the sidebar.</summary>
        private static void DrawSidebar(Graphics g)
        {
            Frame(g, 3, 5, 26, 22, 3);
            Rect(g, 6, 8, 8, 16);
        }

        /// <summary>A square with a trail behind it: the history layer.</summary>
        private static void DrawHistory(Graphics g)
        {
            Frame(g, 4, 4, 8, 8, 2);
            Frame(g, 12, 12, 8, 8, 2);
            Rect(g, 20, 20, 8, 8);
        }

        // ------------------------------------------------------------------
        // World and ecosystem
        // ------------------------------------------------------------------

        /// <summary>
        /// A globe: a black rim with the land inside it, so the shape reads at
        /// 16 by 16.
        /// </summary>
        private static void DrawWorld(Graphics g)
        {
            // The rim: a black disc with a white disc inside it.
            Disc(g, 16, 16, 13);
            CutDisc(g, 16, 16, 11);

            // The land, black on the white interior.
            Poly(g, 9, 12, 15, 8, 19, 13, 12, 18, 8, 15);
            Poly(g, 15, 20, 22, 18, 24, 23, 18, 26, 14, 23);

            // The equator, a black line across the disc.
            Rect(g, 3, 15, 26, 2);
            Cut(g, 6, 15, 20, 2);
        }

        /// <summary>Three leaves on a stem: the ecosystem.</summary>
        private static void DrawEcosystem(Graphics g)
        {
            Rect(g, 15, 12, 2, 18);

            Poly(g, 15, 14, 6, 6, 4, 14, 14, 16);
            Poly(g, 17, 14, 26, 6, 28, 14, 18, 16);
            Poly(g, 15, 8, 16, 1, 22, 6, 17, 10);
        }

        /// <summary>A paw print: a species.</summary>
        private static void DrawSpecies(Graphics g)
        {
            Disc(g, 16, 21, 7);

            Disc(g, 8, 12, 4);
            Disc(g, 14, 8, 4);
            Disc(g, 21, 8, 4);
            Disc(g, 26, 13, 4);
        }

        /// <summary>A square split into four biomes: a tile.</summary>
        private static void DrawTile(Graphics g)
        {
            Rect(g, 3, 3, 12, 12);
            g.FillRectangle(White, 6, 6, 6, 6);

            Rect(g, 17, 3, 12, 12);

            Rect(g, 3, 17, 12, 12);

            Rect(g, 17, 17, 12, 12);
            g.FillRectangle(White, 21, 21, 4, 4);
        }

        // ------------------------------------------------------------------
        // Wolves
        // ------------------------------------------------------------------

        /// <summary>
        /// A wolf's head, side on. The silhouette is black; the eye, the muzzle
        /// and the jaw are cut out in white, so they read at 16 by 16.
        /// </summary>
        private static void DrawWolf(Graphics g)
        {
            // Ears first, so the head overlaps their base.
            Poly(g, 7, 12, 9, 1, 15, 9);
            Poly(g, 16, 8, 21, 1, 23, 11);

            // Head and muzzle as one silhouette.
            Poly(g, 4, 10, 18, 6, 27, 11, 29, 16, 26, 21, 17, 26, 5, 22);

            // The muzzle, cut out so the snout reads.
            CutPoly(g, 21, 18, 29, 15, 29, 20, 23, 23);

            // The eye.
            Cut(g, 12, 13, 5, 4);

            // The jaw line, cut across the lower head.
            Cut(g, 6, 20, 14, 2);
        }

        /// <summary>Three wolf heads in a row: the pack.</summary>
        private static void DrawPack(Graphics g)
        {
            // Centre wolf, larger.
            Poly(g, 12, 12, 16, 10, 21, 15, 19, 23, 16, 26, 12, 20);
            Poly(g, 13, 12, 14, 7, 17, 11);
            Poly(g, 17, 11, 19, 7, 20, 12);

            // Two flankers.
            Poly(g, 3, 16, 9, 14, 11, 22, 7, 27, 3, 23);
            Poly(g, 21, 15, 27, 16, 28, 24, 24, 27, 20, 22);
        }

        /// <summary>Teeth: the hunt.</summary>
        private static void DrawHunt(Graphics g)
        {
            // Upper jaw.
            Rect(g, 3, 6, 26, 4);
            Poly(g, 5, 10, 10, 10, 7, 17);
            Poly(g, 12, 10, 17, 10, 14, 17);
            Poly(g, 19, 10, 24, 10, 21, 17);

            // Lower jaw.
            Rect(g, 3, 22, 26, 4);
            Poly(g, 7, 22, 12, 22, 9, 15);
            Poly(g, 14, 22, 19, 22, 16, 15);
            Poly(g, 21, 22, 26, 22, 23, 15);
        }

        /// <summary>A crescent moon: the full moon.</summary>
        private static void DrawMoon(Graphics g)
        {
            Disc(g, 16, 16, 13);

            // Cut the crescent by removing the dark side.
            CutDisc(g, 23, 13, 12);
        }

        // ------------------------------------------------------------------
        // Scripts, settings, machinery
        // ------------------------------------------------------------------

        /// <summary>A page with a folded corner: a script.</summary>
        private static void DrawScript(Graphics g)
        {
            Poly(g, 5, 3, 21, 3, 27, 9, 27, 29, 5, 29);

            // The fold, cut out.
            CutPoly(g, 21, 3, 27, 9, 21, 9);

            // Two lines of text.
            Cut(g, 9, 14, 14, 3);
            Cut(g, 9, 20, 14, 3);
        }

        /// <summary>A terminal: a box with a prompt.</summary>
        private static void DrawConsole(Graphics g)
        {
            Frame(g, 3, 5, 26, 22, 3);

            // The prompt and the caret, black on the white interior.
            Poly(g, 8, 11, 15, 15, 8, 19);
            Rect(g, 17, 17, 8, 3);
        }

        /// <summary>Two sliders: settings.</summary>
        private static void DrawSettings(Graphics g)
        {
            Rect(g, 3, 6, 26, 3);
            Rect(g, 3, 15, 26, 3);
            Rect(g, 3, 24, 26, 3);

            // The handles.
            Rect(g, 9, 3, 6, 9);
            Rect(g, 19, 12, 6, 9);
            Rect(g, 5, 21, 6, 9);
        }

        /// <summary>A clock face: a schedule.</summary>
        private static void DrawSchedule(Graphics g)
        {
            Disc(g, 16, 16, 13);
            CutDisc(g, 16, 16, 10);

            // Hands.
            Rect(g, 15, 9, 3, 8);
            Rect(g, 15, 15, 8, 3);
        }

        /// <summary>A downward arrow into a tray: an update.</summary>
        private static void DrawUpdate(Graphics g)
        {
            Rect(g, 14, 3, 5, 13);
            Poly(g, 8, 14, 25, 14, 16, 24);

            // The tray.
            Rect(g, 4, 25, 24, 3);
            Rect(g, 4, 21, 3, 7);
            Rect(g, 25, 21, 3, 7);
        }

        /// <summary>Three stacked cylinders: a database.</summary>
        private static void DrawDatabase(Graphics g)
        {
            Disc(g, 16, 7, 11);
            Rect(g, 5, 7, 22, 8);

            Disc(g, 16, 15, 11);
            Rect(g, 5, 15, 22, 8);

            Disc(g, 16, 23, 11);
            Rect(g, 5, 23, 22, 5);

            // White separators, so the three drums read as three.
            Cut(g, 5, 14, 22, 2);
            Cut(g, 5, 22, 22, 2);
        }

        /// <summary>A grid with a bar chart over it: a spreadsheet.</summary>
        private static void DrawExcel(Graphics g)
        {
            Frame(g, 3, 3, 26, 26, 3);

            // The cell dividers.
            Rect(g, 3, 11, 26, 2);
            Rect(g, 3, 20, 26, 2);
            Rect(g, 12, 3, 2, 26);
            Rect(g, 21, 3, 2, 26);

            // Three bars, filled in so it reads as data rather than a table.
            Rect(g, 5, 21, 6, 6);
            Rect(g, 14, 13, 6, 14);
            Rect(g, 23, 16, 6, 11);
        }

        /// <summary>A box with an arrow leaving it: an export.</summary>
        private static void DrawExport(Graphics g)
        {
            Rect(g, 3, 17, 3, 12);
            Rect(g, 3, 26, 26, 3);
            Rect(g, 26, 17, 3, 12);

            // The arrow, pointing up and out.
            Rect(g, 14, 8, 4, 12);
            Poly(g, 8, 9, 24, 9, 16, 1);
        }

        /// <summary>Lines of text: the event log.</summary>
        private static void DrawLog(Graphics g)
        {
            Rect(g, 3, 4, 26, 3);
            Rect(g, 3, 11, 26, 3);
            Rect(g, 3, 18, 26, 3);
            Rect(g, 3, 25, 16, 3);

            // A bullet beside each line.
            Rect(g, 1, 4, 1, 3);
        }

        // ------------------------------------------------------------------
        // Cell states
        // ------------------------------------------------------------------

        /// <summary>A solid black cell: alive.</summary>
        private static void DrawCellAlive(Graphics g)
        {
            Rect(g, 4, 4, 24, 24);
        }

        /// <summary>A hollow cell: dead.</summary>
        private static void DrawCellDead(Graphics g)
        {
            Frame(g, 4, 4, 24, 24, 4);
        }

        /// <summary>A half-filled cell: has been alive before.</summary>
        private static void DrawCellHistory(Graphics g)
        {
            Frame(g, 4, 4, 24, 24, 4);
            Rect(g, 8, 8, 16, 16);
        }

        public static string Describe()
        {
            return "IconGenerator";
        }
    }

    /// <summary>
    /// The entry point for the icon tool. It is in the same file so the tool
    /// compiles on its own with a single csc call and no project of its own.
    /// </summary>
    internal static class IconToolMain
    {
        /// <summary>
        /// Draws every icon into the directory named on the command line, or
        /// into Sources\Resources if none is given.
        /// </summary>
        /// <summary>
        /// Draws every icon into the directory named on the command line, or
        /// into Sources\\Resources if none is given.
        ///
        /// Named Draw rather than Main so it does not collide with the
        /// program's own entry point when this file is compiled as part of the
        /// project. See the note above the class for how to run it.
        /// </summary>
        [STAThread]
        internal static int Draw(string[] arguments)
        {
            string directory = arguments != null && arguments.Length > 0
                ? arguments[0]
                : "Sources\\Resources";

            try
            {
                List<string> written = IconGenerator.GenerateAll(directory);

                Console.WriteLine("Wrote " + written.Count + " icons to " + directory);

                for (int i = 0; i < written.Count; i++)
                    Console.WriteLine("  " + written[i]);

                return 0;
            }
            catch (Exception error)
            {
                Console.WriteLine("Failed: " + error.Message);
                return 1;
            }
        }
    }
}

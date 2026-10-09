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
using System.Windows.Forms;

namespace LifeEngine
{
    static class Program
    {
        [STAThread]
        static void Main(string[] arguments)
        {
            // The self-test harness lives in Tools/SelfTest.cs and has its own
            // entry point. Selecting it here, rather than at build time, keeps
            // both reachable from the one executable.
            if (arguments != null && arguments.Length > 0 &&
                (arguments[0] == "--selftest" || arguments[0] == "-t"))
            {
                Environment.ExitCode = Tools.SelfTest.Run(arguments);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // The new interface. If it cannot be constructed, fall back to the
            // original 2017 form rather than leaving the user with a crash and
            // nothing on screen.
            try
            {
                Application.Run(new Gui.MainForm());
            }
            catch (Exception error)
            {
                MessageBox.Show(
                    "The new interface failed to start, so the original window " +
                    "will open instead.\n\n" +
                    error.GetType().Name + ": " + error.Message,
                    "LifeEngine",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                Application.Run(new FormLife());
            }
        }
    }
}
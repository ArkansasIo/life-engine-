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
using System.Drawing;
using System.Reflection;

namespace LifeEngine
{
    /// <summary>
    /// The icon shown in the title bar and on the task bar.
    ///
    /// It is read back out of the running executable rather than from a resource
    /// file. The icon is attached to the assembly by ApplicationIcon in the
    /// project file, which is a Win32 icon resource the runtime reads natively —
    /// unlike the System.Drawing.Icon entry that used to live in FormLife.resx
    /// and which the SDK writes in a form .NET Framework cannot load.
    /// </summary>
    internal static class FormIcon
    {
        private static Icon m_icon;

        /// <summary>
        /// The application icon, or null if it cannot be read. Null is a
        /// perfectly acceptable value for Form.Icon: the form simply draws the
        /// default icon, which is far better than refusing to open.
        /// </summary>
        internal static Icon Load()
        {
            if (m_icon != null)
                return m_icon;

            try
            {
                // Assembly.Location is empty for a single-file or memory-loaded
                // assembly, so fall back to the executable that is running.
                string path = Assembly.GetExecutingAssembly().Location;

                if (string.IsNullOrEmpty(path))
                    path = System.Windows.Forms.Application.ExecutablePath;

                if (string.IsNullOrEmpty(path))
                    return null;

                m_icon = Icon.ExtractAssociatedIcon(path);
            }
            catch (Exception)
            {
                m_icon = null;
            }

            return m_icon;
        }
    }
}

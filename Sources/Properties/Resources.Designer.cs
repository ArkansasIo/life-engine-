﻿/**
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
 * This file replaces the Visual Studio generated Resources.Designer.cs.
 *
 * The generated version read its bitmaps through a ResourceManager over
 * Properties.Resources.resx. That .resx held System.Drawing.Bitmap entries, and
 * the SDK resource writer serialises those in a format the .NET Framework
 * runtime cannot read without a binding redirect for System.Resources.Extensions
 * — which the SDK does not generate, so the program crashed the first time a
 * menu asked for an icon.
 *
 * The icons are now embedded straight from the PNG files and loaded by name
 * with Assembly.GetManifestResourceStream. No .resx, no preserialized
 * resources, no extra assembly to bind, nothing to redirect. The public surface
 * is unchanged, so the designer file and the menus need no edits.
 *
 * The PNGs are embedded by the ItemGroup in LifeEngine.csproj.
 * ---------------------------------------------------------------------------
 */

using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace LifeEngine.Properties
{
    /// <summary>
    /// The icons the menus use. Each one is read once from the assembly's
    /// embedded resources and then cached, because a menu draws its icon every
    /// time it opens.
    /// </summary>
    internal static class Resources
    {
        private const string Namespace = "LifeEngine.Resources.";

        private static Bitmap m_iconColor;
        private static Bitmap m_iconGrid;
        private static Bitmap m_iconHelp;

        /// <summary>The colour swatch beside the three cell colour menu items.</summary>
        internal static Bitmap IconColor
        {
            get
            {
                if (m_iconColor == null)
                    m_iconColor = Load("IconColor.png");

                return m_iconColor;
            }
        }

        /// <summary>The grid icon beside the Show Grid menu item.</summary>
        internal static Bitmap IconGrid
        {
            get
            {
                if (m_iconGrid == null)
                    m_iconGrid = Load("IconGrid.png");

                return m_iconGrid;
            }
        }

        /// <summary>The icon beside the About menu item.</summary>
        internal static Bitmap IconHelp
        {
            get
            {
                if (m_iconHelp == null)
                    m_iconHelp = Load("IconHelp.png");

                return m_iconHelp;
            }
        }

        /// <summary>
        /// Reads an embedded PNG by file name. Returns null rather than throwing
        /// if it is missing: a menu without an icon is a blemish, and this runs
        /// while the form is being built, where an exception would stop the
        /// program from opening at all.
        /// </summary>
        private static Bitmap Load(string fileName)
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(Namespace + fileName);

                if (stream == null)
                    return null;

                using (stream)
                {
                    // Copy out of the stream first: a Bitmap keeps its stream
                    // open for its lifetime otherwise, and the resource stream
                    // cannot be closed while a menu still holds the image.
                    using (Bitmap loaded = new Bitmap(stream))
                        return new Bitmap(loaded);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

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
using System.Text;

namespace LifeEngine.Configuration
{
    /// <summary>
    /// How a release is delivered: a whole build, or a small change on top of
    /// one. The distinction matters because a patch carries only what changed.
    /// </summary>
    public enum ReleaseKind
    {
        /// <summary>A first release, or a build that supersedes everything.</summary>
        Full,

        /// <summary>A change on top of an existing build.</summary>
        Patch,

        /// <summary>A change that alters or removes existing behaviour.</summary>
        Breaking
    }

    /// <summary>
    /// One released build: what it is called, when it was made, how big it is,
    /// where to get it and what changed.
    ///
    /// A release is data. Nothing here downloads anything; the installer does
    /// that and only ever gets a path from this object.
    /// </summary>
    public sealed class Release
    {
        private string m_build;
        private string m_version;
        private string m_name;
        private string m_developer;
        private string m_releasedOn;
        private ReleaseKind m_kind;
        private string m_path;
        private string m_url;
        private string m_sha256;
        private long m_sizeBytes;
        private string m_notes;
        private List<string> m_changes;
        private Version m_minimumVersion;

        /// <summary>The build name, e.g. "2026.10.07-r3". Unique per release.</summary>
        public string Build
        {
            get { return m_build; }
            set { m_build = value; }
        }

        /// <summary>The dotted version, e.g. "1.0.0.0".</summary>
        public string Version
        {
            get { return m_version; }
            set { m_version = value; }
        }

        /// <summary>A human name for the release, e.g. "Rivers and Rain".</summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>Who built it.</summary>
        public string Developer
        {
            get { return m_developer; }
            set { m_developer = value; }
        }

        /// <summary>The date it was released, "yyyy-MM-dd".</summary>
        public string ReleasedOn
        {
            get { return m_releasedOn; }
            set { m_releasedOn = value; }
        }

        public ReleaseKind Kind
        {
            get { return m_kind; }
            set { m_kind = value; }
        }

        /// <summary>Where the package is: a local file, or a folder to install from.</summary>
        public string Path
        {
            get { return m_path; }
            set { m_path = value; }
        }

        /// <summary>Where it can be fetched from, if it is not already local.</summary>
        public string Url
        {
            get { return m_url; }
            set { m_url = value; }
        }

        /// <summary>The expected SHA-256 of the package, lowercase hex.</summary>
        public string Sha256
        {
            get { return m_sha256; }
            set { m_sha256 = value; }
        }

        public long SizeBytes
        {
            get { return m_sizeBytes; }
            set { m_sizeBytes = value; }
        }

        /// <summary>A sentence or two about the release.</summary>
        public string Notes
        {
            get { return m_notes; }
            set { m_notes = value; }
        }

        /// <summary>The individual changes, one line each.</summary>
        public IList<string> Changes
        {
            get { return m_changes; }
        }

        /// <summary>
        /// The oldest version this release can be applied to. Null means any.
        /// This is what stops a patch for 1.2 landing on a 1.0 install.
        /// </summary>
        public Version MinimumVersion
        {
            get { return m_minimumVersion; }
            set { m_minimumVersion = value; }
        }

        public Release()
        {
            m_kind = ReleaseKind.Patch;
            m_changes = new List<string>();
            m_developer = "ArkansasIo";
            m_version = "0.0.0.0";
            m_sizeBytes = 0;
        }

        public Release(string build, string version, string name)
            : this()
        {
            m_build = build;
            m_version = version;
            m_name = name;
        }

        /// <summary>Records one line of the change list.</summary>
        public void AddChange(string change)
        {
            if (!string.IsNullOrEmpty(change) && !m_changes.Contains(change))
                m_changes.Add(change);
        }

        /// <summary>Size in a readable form, e.g. "1.4 MB".</summary>
        public string SizeText()
        {
            double bytes = m_sizeBytes;

            if (bytes < 1024.0)
                return m_sizeBytes + " B";

            if (bytes < 1024.0 * 1024.0)
                return (bytes / 1024.0).ToString("0.0", CultureInfo.InvariantCulture) + " KB";

            if (bytes < 1024.0 * 1024.0 * 1024.0)
                return (bytes / (1024.0 * 1024.0)).ToString("0.0", CultureInfo.InvariantCulture) + " MB";

            return (bytes / (1024.0 * 1024.0 * 1024.0)).ToString("0.0", CultureInfo.InvariantCulture) + " GB";
        }

        public override string ToString()
        {
            return m_build + " " + m_name + " (" + m_kind + ")";
        }
    }

    /// <summary>
    /// The list of releases this build knows about, and the rules for choosing
    /// one. It never goes to the network itself: it is given a list, and it
    /// answers questions about it.
    /// </summary>
    public sealed class UpdateChannel
    {
        private string m_name;
        private string m_url;
        private List<Release> m_releases;
        private bool m_allowPrerelease;

        /// <summary>The channel name: "stable", "beta", "nightly".</summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>Where the release list lives.</summary>
        public string Url
        {
            get { return m_url; }
            set { m_url = value; }
        }

        /// <summary>Every release on the channel, newest first as far as it knows.</summary>
        public IList<Release> Releases
        {
            get { return m_releases; }
        }

        /// <summary>Whether releases newer than the newest full release are offered.</summary>
        public bool AllowPrerelease
        {
            get { return m_allowPrerelease; }
            set { m_allowPrerelease = value; }
        }

        public UpdateChannel(string name)
        {
            m_name = name;
            m_url = string.Empty;
            m_releases = new List<Release>();
            m_allowPrerelease = false;
        }

        public Release Add(Release release)
        {
            if (release == null)
                return null;

            if (!m_releases.Contains(release))
                m_releases.Add(release);

            return release;
        }

        public Release Find(string build)
        {
            for (int i = 0; i < m_releases.Count; i++)
            {
                if (string.Equals(m_releases[i].Build, build, StringComparison.OrdinalIgnoreCase))
                    return m_releases[i];
            }

            return null;
        }

        /// <summary>The most recent release on the channel, or null if it is empty.</summary>
        public Release GetLatest()
        {
            Release latest = null;

            for (int i = 0; i < m_releases.Count; i++)
            {
                if (!m_allowPrerelease && m_releases[i].Kind == ReleaseKind.Breaking)
                    continue;

                if (latest == null || BuildVersion.IsNewer(m_releases[i].Build, latest.Build))
                    latest = m_releases[i];
            }

            return latest;
        }

        /// <summary>
        /// The newest release that is actually newer than the build in hand, or
        /// null if the install is already current.
        /// </summary>
        public Release GetNewer(string currentBuild)
        {
            Release latest = GetLatest();

            if (latest == null)
                return null;

            if (!BuildVersion.IsNewer(latest.Build, currentBuild))
                return null;

            return latest;
        }

        /// <summary>
        /// Every release between the installed build and the newest one, oldest
        /// first, so a caller can decide whether to apply them one at a time.
        /// </summary>
        public List<Release> GetPending(string currentBuild)
        {
            List<Release> pending = new List<Release>();

            for (int i = 0; i < m_releases.Count; i++)
            {
                if (BuildVersion.IsNewer(m_releases[i].Build, currentBuild))
                    pending.Add(m_releases[i]);
            }

            pending.Sort(delegate (Release left, Release right)
            {
                return BuildVersion.Compare(left.Build, right.Build);
            });

            return pending;
        }

        public override string ToString()
        {
            return m_name + " (" + m_releases.Count + " releases)";
        }
    }

    /// <summary>
    /// Works out whether a release may be installed on the build in hand, and
    /// what the consequences are. All of the deciding, none of the doing: the
    /// installer asks this and follows the answer.
    /// </summary>
    public static class PatchPlanner
    {
        /// <summary>
        /// Can this release be applied to this build?
        /// </summary>
        /// <param name="currentBuild">The installed build name.</param>
        /// <param name="currentVersion">The installed version.</param>
        /// <param name="release">The release on offer.</param>
        /// <param name="reason">Why not, if the answer is no.</param>
        public static bool CanApply(string currentBuild, Version currentVersion, Release release, out string reason)
        {
            reason = string.Empty;

            if (release == null)
            {
                reason = "No release was given.";
                return false;
            }

            if (string.IsNullOrEmpty(release.Build))
            {
                reason = "The release has no build name.";
                return false;
            }

            if (!BuildVersion.IsNewer(release.Build, currentBuild))
            {
                reason = "Build " + release.Build + " is not newer than " + currentBuild + ".";
                return false;
            }

            if (release.MinimumVersion != null && currentVersion != null &&
                currentVersion < release.MinimumVersion)
            {
                reason = "This release needs version " + release.MinimumVersion +
                         " or later; you have " + currentVersion + ".";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Whether a release can be applied on its own, or whether everything
        /// before it has to be applied first.
        /// </summary>
        public static bool IsStandalone(Release release)
        {
            if (release == null)
                return false;

            return release.Kind == ReleaseKind.Full || release.MinimumVersion == null;
        }

        /// <summary>
        /// The order releases should be applied in, oldest first, with full
        /// releases kept and patches that a later full release already contains
        /// dropped.
        /// </summary>
        public static List<Release> PlanApplyOrder(List<Release> pending)
        {
            List<Release> plan = new List<Release>();

            if (pending == null)
                return plan;

            List<Release> sorted = new List<Release>(pending);

            sorted.Sort(delegate (Release left, Release right)
            {
                return BuildVersion.Compare(left.Build, right.Build);
            });

            for (int i = 0; i < sorted.Count; i++)
            {
                Release release = sorted[i];

                if (release.Kind == ReleaseKind.Full)
                {
                    // A full release makes everything before it unnecessary.
                    plan.Clear();
                    plan.Add(release);
                    continue;
                }

                plan.Add(release);
            }

            return plan;
        }

        /// <summary>A readable plan, for a dialog or a log.</summary>
        public static string DescribePlan(List<Release> plan)
        {
            StringBuilder builder = new StringBuilder();

            if (plan == null || plan.Count == 0)
            {
                builder.AppendLine("Nothing to apply: this build is current.");
                return builder.ToString();
            }

            builder.AppendLine("Steps to apply (" + plan.Count + ")");

            for (int i = 0; i < plan.Count; i++)
            {
                Release release = plan[i];

                builder.AppendLine("  " + (i + 1) + ". " + release.Build +
                                   "  " + release.Kind +
                                   "  " + release.SizeText());

                if (!string.IsNullOrEmpty(release.Name))
                    builder.AppendLine("     " + release.Name);

                for (int c = 0; c < release.Changes.Count; c++)
                    builder.AppendLine("     - " + release.Changes[c]);
            }

            return builder.ToString();
        }
    }
}

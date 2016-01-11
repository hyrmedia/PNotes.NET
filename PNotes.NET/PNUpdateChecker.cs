// PNotes.NET - open source desktop notes manager
// Copyright (C) 2015 Andrey Gruber

// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml.Linq;

namespace PNotes.NET
{
    internal class PNUpdateChecker
    {
        internal event EventHandler<NewVersionFoundEventArgs> NewVersionFound;
        internal event EventHandler IsLatestVersion;
        internal event EventHandler<PluginsUpdateFoundEventArgs> PluginsUpdateFound;
        internal event EventHandler<CriticalUpdatesFoundEventArgs> CriticalUpdatesFound;
        internal event EventHandler<ThemesUpdateFoundEventArgs> ThemesUpdateFound;

        private class _PluginClass
        {
            public string Name;
            public string Version;
            public int Type;
            public string FileName;
            public string MainProductVersion;
        }

        internal IEnumerable<DictData> GetListDictionaries()
        {
            try
            {
                var list = new List<DictData>();
                //var request = (FtpWebRequest) WebRequest.Create(PNStrings.URL_DICT_FTP + "/available.lst");
                ////request.Timeout = 1000;
                //using (var response = request.GetResponse())
                //{
                //    using (var stream = response.GetResponseStream())
                //    {
                //        if (stream != null)
                //        {
                //            using (var sr = new StreamReader(stream))
                //            {
                //                while (sr.Peek() != -1)
                //                {
                //                    var readLine = sr.ReadLine();
                //                    if (readLine != null)
                //                    {
                //                        var arr = readLine.Split(',');
                //                        if (arr.Length > 4)
                //                        {
                //                            list.Add(new DictData { LangName = arr[3], ZipFile = arr[4] });
                //                        }
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}

                using (var wc = new WebClient())
                {
                    var uri = new Uri(PNStrings.URL_DICT_FTP + "/available.lst");
                    using (var stream = wc.OpenRead(uri))
                    {
                        if (stream != null)
                        {
                            using (var sr = new StreamReader(stream))
                            {
                                while (sr.Peek() != -1)
                                {
                                    var readLine = sr.ReadLine();
                                    if (readLine != null)
                                    {
                                        var arr = readLine.Split(',');
                                        if (arr.Length > 4)
                                        {
                                            list.Add(new DictData { LangName = arr[3], ZipFile = arr[4] });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex, false);
                return null;
            }
        }

        internal CriticalUpdateAction CheckCriticalUpdates(string productVersion)
        {
            try
            {
                if (PNSingleton.Instance.CriticalChecking || PNSingleton.Instance.VersionChecking ||
                    PNSingleton.Instance.PluginsChecking || PNSingleton.Instance.PluginsDownload || PNSingleton.Instance.ThemesChecking ||
                    PNSingleton.Instance.ThemesDownload) return CriticalUpdateAction.None;
                PNSingleton.Instance.CriticalChecking = true;
                var uri = new Uri(PNStrings.URL_CRITICAL_UPDATES);
                var rq = (HttpWebRequest)WebRequest.Create(uri);
                rq.ReadWriteTimeout = 10000;
                using (var response = rq.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        var xdoc = XDocument.Load(stream);
                        return parsCriticalUpdate(xdoc, productVersion);
                    }
                }
                //using (var wc = new WebClient())
                //{
                //    var uri = new Uri(PNStrings.URL_CRITICAL_UPDATES);
                //    var xdoc = XDocument.Load(wc.OpenRead(uri));
                //    return parsCriticalUpdate(xdoc, productVersion);
                //}
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex, false);
                PNSingleton.Instance.CriticalChecking = false;
                return CriticalUpdateAction.None;
            }
        }

        internal void CheckCriticalUpdatesAsync(string productVersion)
        {
            try
            {
                if (PNSingleton.Instance.CriticalChecking || PNSingleton.Instance.VersionChecking ||
                    PNSingleton.Instance.PluginsChecking || PNSingleton.Instance.PluginsDownload || PNSingleton.Instance.ThemesChecking ||
                    PNSingleton.Instance.ThemesDownload) return;
                PNSingleton.Instance.CriticalChecking = true;
                using (var wc = new WebClient())
                {
                    wc.OpenReadCompleted += criticalOpenReadCompleted;
                    var uri = new Uri(PNStrings.URL_CRITICAL_UPDATES);
                    wc.OpenReadAsync(uri, productVersion);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex, false);
                PNSingleton.Instance.CriticalChecking = false;
            }
        }

        private void criticalOpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null || e.Cancelled) return;
                var xdoc = XDocument.Load(e.Result);
                var productVersion = e.UserState as string;
                if (productVersion == null) return;
                parsCriticalUpdate(xdoc, productVersion);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex, false);
                PNSingleton.Instance.CriticalChecking = false;
            }
        }

        private CriticalUpdateAction parsCriticalUpdate(XDocument xdoc, string productVersion)
        {
            try
            {
                var result = CriticalUpdateAction.None;
                if (string.IsNullOrWhiteSpace(productVersion)) return CriticalUpdateAction.None;
                if (xdoc.Root == null) return CriticalUpdateAction.None;
                string progFileName = null;
                var progs = xdoc.Root.Element("programs");
                if (progs != null)
                {
                    // current product version should be equal to version from critical.xml
                    foreach (var pg in progs.Elements("program").Where(pg => pg.Attribute("version") != null &&
                                                                             pg.Attribute("version").Value ==
                                                                             Application.ProductVersion))
                    {
                        progFileName = pg.Attribute("file").Value;
                        break;
                    }
                }
                var plugins = xdoc.Root.Element("plugins");
                if (plugins == null) return CriticalUpdateAction.None;
                var criticalPlugins = (from pl in plugins.Elements("plugin")
                                       where
                                           pl.Attribute("type") != null && pl.Attribute("name") != null &&
                                           pl.Attribute("version") != null && pl.Attribute("file") != null &&
                                           pl.Attribute("progversion") != null
                                       select new _PluginClass
                                       {
                                           Type = Convert.ToInt32(pl.Attribute("type").Value),
                                           Name = pl.Attribute("name").Value,
                                           Version = pl.Attribute("version").Value,
                                           FileName = pl.Attribute("file").Value,
                                           MainProductVersion = pl.Attribute("progversion").Value
                                       }).ToList();
                // get the max version from critical_update.xml which is less or equal to current product version
                var maxVersion =
                    criticalPlugins.Where(p => String.CompareOrdinal(p.MainProductVersion, productVersion) <= 0)
                        .Select(p => p.MainProductVersion)
                        .OrderByDescending(s => s, new VersionComparer())
                        .FirstOrDefault();
                var pluginsList = new List<CriticalPluginUpdate>();
                if (!string.IsNullOrEmpty(maxVersion))
                {
                    // current main product version should equal to max version from critical.xml
                    // and critical plugin version should be more than current plugin version
                    pluginsList.AddRange(from pl in PNPlugins.Instance.SocialPlugins
                                         select
                                             criticalPlugins.FirstOrDefault(
                                                 p =>
                                                     p.Type == 0 && p.MainProductVersion == maxVersion &&
                                                     new Version(p.Version) > new Version(pl.Version) && p.Name == pl.ProductName)
                                             into cp
                                             where cp != null
                                             select new CriticalPluginUpdate { FileName = cp.FileName, ProductName = cp.Name });
                    pluginsList.AddRange(from pl in PNPlugins.Instance.SyncPlugins
                                         select
                                             criticalPlugins.FirstOrDefault(
                                                 p => p.Type == 1 && p.MainProductVersion == maxVersion &&
                                                      new Version(p.Version) > new Version(pl.Version) && p.Name == pl.ProductName)
                                             into cp
                                             where cp != null
                                             select new CriticalPluginUpdate { FileName = cp.FileName, ProductName = cp.Name });
                }
                if (string.IsNullOrWhiteSpace(progFileName) && !pluginsList.Any()) return CriticalUpdateAction.None;
                if (CriticalUpdatesFound == null) return CriticalUpdateAction.None;
                var ev = new CriticalUpdatesFoundEventArgs(progFileName, pluginsList);
                CriticalUpdatesFound(this, ev);
                if (!string.IsNullOrWhiteSpace(progFileName))
                    result |= CriticalUpdateAction.Program;
                if (pluginsList.Any())
                    result |= CriticalUpdateAction.Plugins;
                return ev.Accepted ? result : CriticalUpdateAction.None;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex, false);
                return CriticalUpdateAction.None;
            }
            finally
            {
                PNSingleton.Instance.CriticalChecking = false;
            }
        }

        internal void CheckNewVersion(string progVersion)
        {
            try
            {
                if (PNSingleton.Instance.CriticalChecking || PNSingleton.Instance.VersionChecking ||
                    PNSingleton.Instance.PluginsChecking || PNSingleton.Instance.PluginsDownload ||
                    PNSingleton.Instance.ThemesChecking ||
                    PNSingleton.Instance.ThemesDownload) return;
                PNSingleton.Instance.VersionChecking = true;
                using (var wc = new WebClient())
                {
                    wc.OpenReadCompleted += programVersionOpenReadCompleted;
                    var uri = new Uri(PNStrings.URL_UPDATE);
                    wc.OpenReadAsync(uri, progVersion);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                PNSingleton.Instance.VersionChecking = false;
            }
        }

        internal void CheckThemesNewVersion()
        {
            try
            {
                if (PNSingleton.Instance.CriticalChecking || PNSingleton.Instance.VersionChecking ||
                    PNSingleton.Instance.PluginsChecking || PNSingleton.Instance.PluginsDownload ||
                    PNSingleton.Instance.ThemesChecking || PNSingleton.Instance.ThemesDownload) return;
                PNSingleton.Instance.ThemesChecking = true;
                using (var wc = new WebClient())
                {
                    wc.OpenReadCompleted += themesVersion_OpenReadCompleted;
                    var uri = new Uri(PNStrings.URL_THEMES_UPDATE);
                    wc.OpenReadAsync(uri);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                PNSingleton.Instance.ThemesChecking = false;
            }
        }

        private void themesVersion_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null && !e.Cancelled)
                {
                    var listThemes = new List<ThemesUpdate>();
                    var xdoc = XDocument.Load(e.Result);
                    if (xdoc.Root == null) return;
                    foreach (var xe in xdoc.Root.Elements("theme"))
                    {
                        var name = xe.Attribute("name").Value;
                        if (PNStatic.Themes.Keys.Any(k => k == name))
                        {
                            var theme = PNStatic.Themes[name];
                            var version = new Version(xe.Attribute("version").Value);
                            if (theme.Item5 < version)
                            {
                                listThemes.Add(new ThemesUpdate
                                {
                                    Name = Path.GetFileNameWithoutExtension(xe.Attribute("filename").Value),
                                    FileName = xe.Attribute("filename").Value,
                                    Suffix = PNLang.Instance.GetCaptionText("upd_theme", "(update)"),
                                    FriendlyName = name
                                });
                            }
                        }
                        else
                        {
                            listThemes.Add(new ThemesUpdate
                            {
                                Name = Path.GetFileNameWithoutExtension(xe.Attribute("filename").Value),
                                FileName = xe.Attribute("filename").Value,
                                Suffix = PNLang.Instance.GetCaptionText("new_theme", "(new theme)"),
                                FriendlyName = name
                            });
                        }
                    }
                    if (listThemes.Count == 0)
                    {
                        if (IsLatestVersion != null)
                        {
                            IsLatestVersion(this, new EventArgs());
                        }
                        return;
                    }
                    if (ThemesUpdateFound != null) ThemesUpdateFound(this, new ThemesUpdateFoundEventArgs(listThemes));
                }
                else if (e.Error != null)
                {
                    throw e.Error;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                PNSingleton.Instance.ThemesChecking = false;
            }
        }

        internal void CheckPluginsNewVersion()
        {
            try
            {
                if (PNSingleton.Instance.CriticalChecking || PNSingleton.Instance.VersionChecking ||
                    PNSingleton.Instance.PluginsChecking || PNSingleton.Instance.PluginsDownload ||
                    PNSingleton.Instance.ThemesChecking || PNSingleton.Instance.ThemesDownload) return;
                PNSingleton.Instance.PluginsChecking = true;
                using (var wc = new WebClient())
                {
                    wc.OpenReadCompleted += pluginsVersionOpenReadCompleted;
                    var uri = new Uri(PNStrings.URL_PLUGINS_UPDATE);
                    wc.OpenReadAsync(uri);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                PNSingleton.Instance.PluginsChecking = false;
            }
        }

        private void pluginsVersionOpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null && !e.Cancelled)
                {
                    var versions = new Dictionary<string, string>();
                    var plugs = new List<_PluginClass>();
                    using (var reader = new StreamReader(e.Result))
                    {
                        while (reader.Peek() != -1)
                        {
                            var line = reader.ReadLine();
                            if (line == null) continue;
                            var arr = line.Split(',');
                            if (arr.Length > 2)
                            {
                                versions.Add(arr[0], arr[1]);
                                plugs.Add(new _PluginClass
                                {
                                    Name = arr[0],
                                    Version = arr[1],
                                    Type = int.Parse(arr[2], PNStatic.CultureInvariant)
                                });
                            }
                        }
                    }
                    if (plugs.Count == 0)
                    {
                        if (IsLatestVersion != null)
                        {
                            IsLatestVersion(this, new EventArgs());
                        }
                        return;
                    }
                    var listSyncExisting = (from pl in plugs
                                            let p =
                                                PNPlugins.Instance.SyncPlugins.FirstOrDefault(pg => pg.Name == pl.Name)
                                            where p != null
                                            let v1 = new Version(pl.Version)
                                            let v2 = new Version(p.Version)
                                            where v1 > v2
                                            select
                                                new PluginsUpdate
                                                {
                                                    Name = p.Name,
                                                    ProductName = p.ProductName,
                                                    Suffix = PNLang.Instance.GetCaptionText("upd_plugin", "(update)"),
                                                    Type = pl.Type
                                                })
                        .ToList();
                    var listPostExisting = (from pl in plugs
                                            let p =
                                                PNPlugins.Instance.SocialPlugins.FirstOrDefault(pg => pg.Name == pl.Name)
                                            where p != null
                                            let v1 = new Version(pl.Version)
                                            let v2 = new Version(p.Version)
                                            where v1 > v2
                                            select
                                                new PluginsUpdate
                                                {
                                                    Name = p.Name,
                                                    ProductName = p.ProductName,
                                                    Suffix = PNLang.Instance.GetCaptionText("upd_plugin", "(update)"),
                                                    Type = pl.Type
                                                })
                        .ToList();
                    var listSyncNew = (from vk in plugs
                                       where vk.Type == 1 && PNPlugins.Instance.SyncPlugins.All(p => p.Name != vk.Name)
                                       select
                                           new PluginsUpdate
                                           {
                                               Name = vk.Name,
                                               ProductName = "pn" + vk.Name.ToLower(),
                                               Suffix = PNLang.Instance.GetCaptionText("new_plugin", "(new plugin)"),
                                               Type = vk.Type
                                           })
                        .ToList();
                    var listPostNew = (from vk in plugs
                                       where
                                           vk.Type == 0 && PNPlugins.Instance.SocialPlugins.All(p => p.Name != vk.Name)
                                       select
                                           new PluginsUpdate
                                           {
                                               Name = vk.Name,
                                               ProductName = "pn" + vk.Name.ToLower(),
                                               Suffix = PNLang.Instance.GetCaptionText("new_plugin", "(new plugin)"),
                                               Type = vk.Type
                                           })
                        .ToList();

                    if (listPostExisting.Count > 0 || listSyncExisting.Count > 0 || listPostNew.Count > 0 ||
                        listSyncNew.Count > 0)
                    {
                        if (PluginsUpdateFound != null)
                        {
                            var listPlugins =
                                new List<PluginsUpdate>(listPostExisting.Count + listSyncExisting.Count +
                                                        listPostNew.Count + listSyncNew.Count);
                            listPlugins.AddRange(listPostExisting);
                            listPlugins.AddRange(listSyncExisting);
                            listPlugins.AddRange(listPostNew);
                            listPlugins.AddRange(listSyncNew);
                            PluginsUpdateFound(this, new PluginsUpdateFoundEventArgs(listPlugins));
                        }
                    }
                    else
                    {
                        if (IsLatestVersion != null)
                        {
                            IsLatestVersion(this, new EventArgs());
                        }
                    }
                }
                else if (e.Error != null)
                {
                    throw e.Error;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                PNSingleton.Instance.PluginsChecking = false;
            }
        }

        private void programVersionOpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null && !e.Cancelled)
                {
                    var progVersion = e.UserState as string;
                    if (progVersion == null) return;
                    using (var reader = new StreamReader(e.Result))
                    {
                        var temp = reader.ReadToEnd();
                        if (temp.Trim().Length > 0)
                        {
                            if (string.CompareOrdinal(progVersion, temp) < 0)
                            {
                                // product version is less than version in file
                                if (NewVersionFound != null)
                                {
                                    NewVersionFound(this, new NewVersionFoundEventArgs(temp));
                                }
                            }
                            else
                            {
                                if (IsLatestVersion != null)
                                {
                                    IsLatestVersion(this, new EventArgs());
                                }
                            }
                        }
                        else
                        {
                            if (IsLatestVersion != null)
                            {
                                IsLatestVersion(this, new EventArgs());
                            }
                        }
                    }
                }
                else if (e.Error != null)
                {
                    throw e.Error;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                PNSingleton.Instance.VersionChecking = false;
            }
        }

        private class VersionComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return string.CompareOrdinal(x, y);
            }
        }
    }
}

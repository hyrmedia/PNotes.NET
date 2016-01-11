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
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using SQLiteWrapper;

namespace PNotes.NET
{
    internal class PNMenus
    {
        internal static List<PNMenu> DefaultMainMenus = new List<PNMenu>();
        internal static List<PNMenu> DefaultEditMenus = new List<PNMenu>();
        internal static List<PNMenu> DefaultNoteMenus = new List<PNMenu>();
        internal static List<PNMenu> DefaultCPMenus = new List<PNMenu>();
        internal static List<PNMenu> CurrentMainMenus = new List<PNMenu>();
        internal static List<PNMenu> CurrentEditMenus = new List<PNMenu>();
        internal static List<PNMenu> CurrentNoteMenus = new List<PNMenu>();
        internal static List<PNMenu> CurrentCPMenus = new List<PNMenu>();

        internal static void CheckAndApplyNewMenusOrder(ContextMenu ctm)
        {
            try
            {
                var sb = new StringBuilder("SELECT COUNT(MENU_NAME) FROM MENUS_ORDER WHERE CONTEXT_NAME = '");
                sb.Append(ctm.Name);
                sb.Append("' AND ORDER_ORIGINAL <> ORDER_NEW");
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    var obj = oData.GetScalar(sb.ToString());
                    if (obj != null && !PNData.IsDBNull(obj) && Convert.ToInt32(obj) > 0)
                        RearrangeMenus(ctm);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void RearrangeMenus(ContextMenu ctm)
        {
            try
            {
                var tableList = new List<Tuple<string, string, int>>();
                var tempMenus = new List<Control>();
                var sb =
                    new StringBuilder("SELECT MENU_NAME, IFNULL(PARENT_NAME, '') AS PARENT_NAME, ORDER_NEW FROM MENUS_ORDER WHERE CONTEXT_NAME = '");
                sb.Append(ctm.Name);
                sb.Append("' ORDER BY ORDER_NEW");
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    using (var t = oData.FillDataTable(sb.ToString()))
                    {
                        tableList.AddRange(from DataRow r in t.Rows
                                           select
                                               Tuple.Create(Convert.ToString(r["MENU_NAME"]), Convert.ToString(r["PARENT_NAME"]),
                                                   Convert.ToInt32(r["ORDER_NEW"])));
                        tempMenus.AddRange(ctm.Items.Cast<Control>());
                        ctm.Items.Clear();
                        foreach (
                            var tm in
                                tableList.Where(tb => tb.Item2 == "")
                                    .Select(tp => tempMenus.FirstOrDefault(m => m.Name == tp.Item1))
                                    .Where(tm => tm != null))
                        {
                            ctm.Items.Add(tm);
                            addSubitem(tableList, tm);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private static void addSubitem(IEnumerable<Tuple<string, string, int>> tableList, Control item)
        {
            try
            {
                if (item.GetType() != typeof(MenuItem)) return;
                var tempMenus = new List<Control>();
                tempMenus.AddRange(((MenuItem)item).Items.Cast<Control>());
                ((MenuItem)item).Items.Clear();
                var enumerable = tableList as Tuple<string, string, int>[] ?? tableList.ToArray();
                foreach (
                    var tm in
                        enumerable.Where(tb => tb.Item2 == item.Name)
                            .Select(tp => tempMenus.FirstOrDefault(m => m.Name == tp.Item1))
                            .Where(tm => tm != null))
                {
                    ((MenuItem)item).Items.Add(tm);
                    addSubitem(enumerable, tm);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void PrepareDefaultMenuStrip(ContextMenu source, MenuType type, bool forCurrent)
        {
            try
            {
                List<PNMenu> menuItems;
                if (forCurrent)
                {
                    switch (type)
                    {
                        case MenuType.Main:
                            menuItems = CurrentMainMenus;
                            break;
                        case MenuType.Edit:
                            menuItems = CurrentEditMenus;
                            break;
                        case MenuType.Note:
                            menuItems = CurrentNoteMenus;
                            break;
                        case MenuType.ControlPanel:
                            menuItems = CurrentCPMenus;
                            break;
                        default:
                            return;
                    }
                }
                else
                {
                    switch (type)
                    {
                        case MenuType.Main:
                            menuItems = DefaultMainMenus;
                            break;
                        case MenuType.Edit:
                            menuItems = DefaultEditMenus;
                            break;
                        case MenuType.Note:
                            menuItems = DefaultNoteMenus;
                            break;
                        case MenuType.ControlPanel:
                            menuItems = DefaultCPMenus;
                            break;
                        default:
                            return;
                    }
                }
                menuItems.Clear();

                var arr = source.Items.Cast<Control>().ToArray();
                var tableList = new List<Tuple<string, string, int>>();
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    var sb =
                            new StringBuilder("SELECT MENU_NAME, ");
                    sb.Append(forCurrent ? "ORDER_NEW" : "ORDER_ORIGINAL");
                    sb.Append(", IFNULL(PARENT_NAME, '') AS PARENT_NAME FROM MENUS_ORDER WHERE CONTEXT_NAME = '");
                    sb.Append(source.Name);
                    sb.Append("' ORDER BY ");
                    sb.Append(forCurrent ? "ORDER_NEW" : "ORDER_ORIGINAL");
                    using (var t = oData.FillDataTable(sb.ToString()))
                    {
                        if (forCurrent)
                        {
                            tableList.AddRange(from DataRow r in t.Rows
                                               select
                                                   Tuple.Create(Convert.ToString(r["MENU_NAME"]), Convert.ToString(r["PARENT_NAME"]),
                                                       Convert.ToInt32(r["ORDER_NEW"])));
                        }
                        else
                        {
                            tableList.AddRange(from DataRow r in t.Rows
                                               select
                                                   Tuple.Create(Convert.ToString(r["MENU_NAME"]), Convert.ToString(r["PARENT_NAME"]),
                                                       Convert.ToInt32(r["ORDER_ORIGINAL"])));
                        }
                        var topItems = tableList.Where(tp => tp.Item2 == "");
                        foreach (var tp in topItems)
                        {
                            var si = arr.FirstOrDefault(a => a.Name == tp.Item1);
                            if (si == null) continue;
                            var pmi = new PNMenu(si.Name,
                                si.GetType() == typeof(MenuItem) ?((MenuItem) si).Header.ToString() : PNStrings.MENU_SEPARATOR_STRING, "", source.Name);
                            menuItems.Add(pmi);
                            var psi = si as MenuItem;
                            if (psi != null)
                                prepareSubitem(tableList, psi, pmi, source.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private static void prepareSubitem(IEnumerable<Tuple<string, string, int>> tableList, MenuItem parentItem, PNMenu parentPNItem, string context)
        {
            try
            {
                if (!parentItem.HasItems) return;
                var arr = parentItem.Items.Cast<Control>().ToArray();
                var enumerable = tableList as Tuple<string, string, int>[] ?? tableList.ToArray();
                var tuples = tableList as Tuple<string, string, int>[] ?? enumerable.ToArray();
                var topItems = tuples.Where(tp => tp.Item2 == parentItem.Name).OrderBy(tp => tp.Item3);
                foreach (var tp in topItems)
                {
                    var si = arr.FirstOrDefault(a => a.Name == tp.Item1);
                    if (si == null) continue;
                    if (si.GetType() != typeof(MenuItem) && si.GetType() != typeof(Separator)) continue;
                    var pmi = new PNMenu(si.Name, si.GetType() == typeof(MenuItem) ? ((MenuItem)si).Header.ToString() : PNStrings.MENU_SEPARATOR_STRING,
                        parentPNItem.Name, context);
                    parentPNItem.Items.Add(pmi);
                    var psi = si as MenuItem;
                    if (psi != null)
                        prepareSubitem(enumerable, psi, pmi, context);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}

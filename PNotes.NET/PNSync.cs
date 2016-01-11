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
using System.IO;
using System.Linq;
using System.Text;
using SQLiteWrapper;

namespace PNotes.NET
{
    internal class PNSync
    {
        internal event EventHandler<LocalSyncCompleteEventArgs> SyncComplete;


        //todo: table data
        private class _FieldData
        {
            public string Name;
            public string Type;
            public bool NotNull;
        }

        private class _NoteFile
        {
            public string Path;
            public string Name;
            public bool Copy;
        }

        private static readonly string[] _Tables =
            {
                "NOTES", "GROUPS", "NOTES_SCHEDULE", "LINKED_NOTES", "NOTES_TAGS",
                "CUSTOM_NOTES_SETTINGS"
            };

        //todo
        private bool exchangeNotes(SQLiteDataObject dataSrc, SQLiteDataObject dataDest, string id, KeyValuePair<string, List<_FieldData>> tableData)
        {
            try
            {
                using (var t1 = dataSrc.FillDataTable("SELECT * FROM NOTES WHERE ID = '" + id + "'"))
                {
                    using (var t2 = dataDest.FillDataTable("SELECT * FROM NOTES WHERE ID = '" + id + "'"))
                    {
                        if (t1.Rows.Count > 0 && t2.Rows.Count > 0)
                        {
                            var d1 = Convert.ToInt64(t1.Rows[0]["UPD_DATE"]);
                            var d2 = Convert.ToInt64(t2.Rows[0]["UPD_DATE"]);
                            if (d1 > d2)
                            {
                                return insertToNotes(dataDest, t1.Rows[0], tableData);
                            }
                            if (d2 > d1)
                            {
                                return insertToNotes(dataSrc, t2.Rows[0], tableData);
                            }
                        }
                        else if (t1.Rows.Count > 0)
                        {
                            return insertToNotes(dataDest, t1.Rows[0], tableData);
                        }
                        else if (t2.Rows.Count > 0)
                        {
                            return insertToNotes(dataSrc, t2.Rows[0], tableData);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool exchangeCustomNotesSettings(SQLiteDataObject dataSrc, SQLiteDataObject dataDest, string id, KeyValuePair<string, List<_FieldData>> tableData)
        {
            try
            {
                using (var t1 = dataSrc.FillDataTable("SELECT * FROM CUSTOM_NOTES_SETTINGS WHERE NOTE_ID = '" + id + "'"))
                {
                    using (var t2 = dataDest.FillDataTable("SELECT * FROM CUSTOM_NOTES_SETTINGS WHERE NOTE_ID = '" + id + "'"))
                    {
                        if (t1.Rows.Count > 0 && t2.Rows.Count > 0)
                        {
                            var d1 = Convert.ToInt64(t1.Rows[0]["UPD_DATE"]);
                            var d2 = Convert.ToInt64(t2.Rows[0]["UPD_DATE"]);
                            if (d1 > d2)
                            {
                                return insertToCustomNotesSettings(dataDest, t1.Rows[0], tableData);
                            }
                            if (d2 > d1)
                            {
                                return insertToCustomNotesSettings(dataSrc, t2.Rows[0], tableData);
                            }
                        }
                        else if (t1.Rows.Count > 0)
                        {
                            return insertToCustomNotesSettings(dataDest, t1.Rows[0], tableData);
                        }
                        else if (t2.Rows.Count > 0)
                        {
                            return insertToCustomNotesSettings(dataSrc, t2.Rows[0], tableData);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool executeTransaction(SQLiteDataObject oData, string sqlQuery)
        {
            var inTrans = false;
            try
            {
                inTrans = oData.BeginTransaction();
                if (!inTrans) return false;
                oData.ExecuteInTransaction(sqlQuery);
                oData.CommitTransaction();
                inTrans = false;
                return true;
            }
            catch (Exception ex)
            {
                if (inTrans)
                {
                    oData.RollbackTransaction();
                }
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool exchangeGroups(SQLiteDataObject dataSrc, SQLiteDataObject dataDest, KeyValuePair<string, List<_FieldData>> tableData)
        {
            try
            {
                using (var t1 = dataSrc.FillDataTable("SELECT * FROM GROUPS"))
                {
                    using (var t2 = dataDest.FillDataTable("SELECT * FROM GROUPS"))
                    {
                        var t = t1.Clone();
                        IEnumerable<DataRow> rows1 = t1.AsEnumerable();
                        IEnumerable<DataRow> rows2 = t2.AsEnumerable();
                        foreach (var r1 in rows1)
                        {
                            var r2 = rows2.FirstOrDefault(r => (int)r["GROUP_ID"] == (int)r1["GROUP_ID"]);
                            if (r2 != null)
                            {
                                if (Convert.ToInt64(r1["UPD_DATE"]) >= Convert.ToInt64(r2["UPD_DATE"]))
                                {
                                    t.Rows.Add(r1.ItemArray);
                                }
                                else if (Convert.ToInt64(r2["UPD_DATE"]) > Convert.ToInt64(r1["UPD_DATE"]))
                                {
                                    t.Rows.Add(r2.ItemArray);
                                }
                            }
                            else
                            {
                                t.Rows.Add(r1.ItemArray);
                            }
                        }
                        foreach (var r2 in rows2)
                        {
                            if (rows1.All(r => (int)r["GROUP_ID"] != (int)r2["GROUP_ID"]))
                            {
                                t.Rows.Add(r2.ItemArray);
                            }
                        }

                        var sqlList = new List<string> { "DELETE FROM GROUPS" };
                        sqlList.AddRange(from DataRow r in t.Rows select createInsert(r, tableData));

                        var inTrans1 = false;
                        var inTrans2 = false;
                        try
                        {
                            inTrans1 = dataSrc.BeginTransaction();
                            inTrans2 = dataDest.BeginTransaction();
                            if (inTrans1 && inTrans2)
                            {
                                foreach (string s in sqlList)
                                {
                                    dataSrc.ExecuteInTransaction(s);
                                    dataDest.ExecuteInTransaction(s);
                                }
                                dataSrc.CommitTransaction();
                                inTrans1 = false;
                                dataDest.CommitTransaction();
                                inTrans2 = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (inTrans1)
                            {
                                dataSrc.RollbackTransaction();
                            }
                            if (inTrans2)
                            {
                                dataDest.RollbackTransaction();
                            }
                            PNStatic.LogException(ex);
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool exchangeNotesSchedule(SQLiteDataObject dataSrc, SQLiteDataObject dataDest, string id, KeyValuePair<string, List<_FieldData>> tableData)
        {
            try
            {
                using (var t1 = dataSrc.FillDataTable("SELECT * FROM NOTES_SCHEDULE WHERE NOTE_ID = '" + id + "'"))
                {
                    using (var t2 = dataDest.FillDataTable("SELECT * FROM NOTES_SCHEDULE WHERE NOTE_ID = '" + id + "'"))
                    {
                        if (t1.Rows.Count > 0 && t2.Rows.Count > 0)
                        {
                            var d1 = Convert.ToInt64(t1.Rows[0]["UPD_DATE"]);
                            var d2 = Convert.ToInt64(t2.Rows[0]["UPD_DATE"]);
                            if (d1 > d2)
                            {
                                return insertToNotesSchedule(dataDest, t1.Rows[0], tableData);
                            }
                            if (d2 > d1)
                            {
                                return insertToNotesSchedule(dataSrc, t2.Rows[0], tableData);
                            }
                        }
                        else if (t1.Rows.Count > 0)
                        {
                            return insertToNotesSchedule(dataDest, t1.Rows[0], tableData);
                        }
                        else if (t2.Rows.Count > 0)
                        {
                            return insertToNotesSchedule(dataSrc, t2.Rows[0], tableData);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool exchangeLinkedNotes(SQLiteDataObject dataSrc, SQLiteDataObject dataDest, string id, KeyValuePair<string, List<_FieldData>> tableData)
        {
            try
            {
                using (var t1 = dataSrc.FillDataTable("SELECT * FROM LINKED_NOTES WHERE NOTE_ID = '" + id + "'"))
                {
                    using (var t2 = dataDest.FillDataTable("SELECT * FROM LINKED_NOTES WHERE NOTE_ID = '" + id + "'"))
                    {
                        if (t1.Rows.Count > 0 && t2.Rows.Count > 0)
                        {
                            var t = t1.Clone();

                            foreach (DataRow r1 in t1.Rows)
                            {
                                if (t.AsEnumerable().All(r => (string)r["LINK_ID"] != (string)r1["LINK_ID"]))
                                {
                                    t.Rows.Add(r1.ItemArray);
                                }
                            }
                            foreach (DataRow r2 in t2.Rows)
                            {
                                if (t.AsEnumerable().All(r => (string)r["LINK_ID"] != (string)r2["LINK_ID"]))
                                {
                                    t.Rows.Add(r2.ItemArray);
                                }
                            }
                            foreach (DataRow r in t.Rows)
                            {
                                if (!insertToLinkedNotes(dataDest, r, tableData))
                                {
                                    return false;
                                }
                                if (!insertToLinkedNotes(dataSrc, r, tableData))
                                {
                                    return false;
                                }
                            }
                        }
                        else if (t1.Rows.Count > 0)
                        {
                            if (t1.Rows.Cast<DataRow>().Any(r => !insertToLinkedNotes(dataDest, r, tableData)))
                            {
                                return false;
                            }
                        }
                        else if (t2.Rows.Count > 0)
                        {
                            if (t2.Rows.Cast<DataRow>().Any(r => !insertToLinkedNotes(dataSrc, r, tableData)))
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool exchangeNotesTags(SQLiteDataObject dataSrc, SQLiteDataObject dataDest, string id, KeyValuePair<string, List<_FieldData>> tableData)
        {
            try
            {
                using (var t1 = dataSrc.FillDataTable("SELECT * FROM NOTES_TAGS WHERE NOTE_ID = '" + id + "'"))
                {
                    using (var t2 = dataDest.FillDataTable("SELECT * FROM NOTES_TAGS WHERE NOTE_ID = '" + id + "'"))
                    {
                        if (t1.Rows.Count > 0 && t2.Rows.Count > 0)
                        {
                            var t = t1.Clone();

                            foreach (DataRow r1 in t1.Rows)
                            {
                                if (t.AsEnumerable().All(r => (string)r["TAG"] != (string)r1["TAG"]))
                                {
                                    t.Rows.Add(r1.ItemArray);
                                }
                            }
                            foreach (DataRow r2 in t2.Rows)
                            {
                                if (t.AsEnumerable().All(r => (string)r["TAG"] != (string)r2["TAG"]))
                                {
                                    t.Rows.Add(r2.ItemArray);
                                }
                            }
                            foreach (DataRow r in t.Rows)
                            {
                                if (!insertToNotesTags(dataDest, r, tableData))
                                {
                                    return false;
                                }
                                if (!insertToNotesTags(dataSrc, r, tableData))
                                {
                                    return false;
                                }
                            }
                        }
                        else if (t1.Rows.Count > 0)
                        {
                            if (t1.Rows.Cast<DataRow>().Any(r => !insertToNotesTags(dataDest, r, tableData)))
                            {
                                return false;
                            }
                        }
                        else if (t2.Rows.Count > 0)
                        {
                            if (t2.Rows.Cast<DataRow>().Any(r => !insertToNotesTags(dataSrc, r, tableData)))
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        //todo
        private bool exchangeData(SQLiteDataObject dataSrc, SQLiteDataObject dataDest, string id, Dictionary<string, List<_FieldData>> tablesData)
        {
            try
            {
                //todo
                if (!exchangeNotes(dataSrc, dataDest, id, tablesData.FirstOrDefault(td => td.Key == "NOTES")))
                {
                    return false;
                }
                if (!exchangeCustomNotesSettings(dataSrc, dataDest, id, tablesData.FirstOrDefault(td => td.Key == "CUSTOM_NOTES_SETTINGS")))
                {
                    return false;
                }
                if (!exchangeLinkedNotes(dataSrc, dataDest, id, tablesData.FirstOrDefault(td => td.Key == "LINKED_NOTES")))
                {
                    return false;
                }
                if (!exchangeNotesSchedule(dataSrc, dataDest, id, tablesData.FirstOrDefault(td => td.Key == "NOTES_SCHEDULE")))
                {
                    return false;
                }
                if (!exchangeNotesTags(dataSrc, dataDest, id, tablesData.FirstOrDefault(td => td.Key == "NOTES_TAGS")))
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        internal bool dropTriggers(string dbPath)
        {
            try
            {
                var connSrc = "data source = " + dbPath;
                using (var src = new SQLiteDataObject(connSrc))
                {
                    src.Execute(PNStrings.DROP_TRIGGERS);
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        internal bool restoreTriggers(string dbPath)
        {
            try
            {
                var connSrc = "data source = " + dbPath;
                using (var src = new SQLiteDataObject(connSrc))
                {
                    src.Execute(PNStrings.CREATE_TRIGGERS);
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        internal void SyncLocal(object localPaths)
        {
            var result = LocalSyncResult.Reload;
            var filesSrc = new List<_NoteFile>();
            var filesDest = new List<_NoteFile>();
            var tempDir = "";

            try
            {
                if (PNStatic.Settings.Network.SaveBeforeSync)
                {
                    PNStatic.FormMain.ApplyAction(MainDialogAction.SaveAll, null);
                }
                var paths = localPaths as string[];
                if (paths == null || paths.Length != 2)
                {
                    throw new ArgumentException("Invalid local paths");
                }
                var destDir = paths[0].Trim();
                var destDB = paths[1].Trim() != "" ? paths[1].Trim() : destDir;
                destDB = Path.Combine(destDB, PNStrings.DB_FILE);

                // create temp directory
                tempDir = createTempDir();
                var tempDBSrc = Path.Combine(tempDir, PNStrings.DB_FILE);
                var tempDBDest = Path.Combine(tempDir, PNStrings.TEMP_DB_FILE);

                // copy source db
                File.Copy(PNPaths.Instance.DBPath, tempDBSrc, true);

                // build source connection string
                var srcConnectionString = "data source=\"" + tempDBSrc + "\"";
                //string srcConnectionString = "metadata=res://*/PNModel.csdl|res://*/PNModel.ssdl|res://*/PNModel.msl;provider=System.Data.SQLite;provider connection string='data source=\"" + tempDBSrc + "\"'";
                using (var eSrc = new SQLiteDataObject(srcConnectionString))
                {
                    // drop triggers
                    eSrc.Execute(PNStrings.DROP_TRIGGERS);
                    // get listNames of all source notes files
                    var srcNotes = new DirectoryInfo(PNPaths.Instance.DataDir).GetFiles("*" + PNStrings.NOTE_EXTENSION);
                    filesSrc.AddRange(srcNotes.Select(fi => new _NoteFile { Path = fi.FullName, Name = fi.Name, Copy = false }));
                    // get deleted ids
                    if (!PNStatic.Settings.Network.IncludeBinInSync)
                    {
                        var deletedSrc = deletedIDs(eSrc);
                        filesSrc.RemoveAll(nf => deletedSrc.Contains(Path.GetFileNameWithoutExtension(nf.Name)));
                    }
                    // get listNames of all destination notes files
                    var destNotes = new DirectoryInfo(destDir).GetFiles("*" + PNStrings.NOTE_EXTENSION);
                    filesDest.AddRange(destNotes.Select(fir => new _NoteFile { Path = fir.FullName, Name = fir.Name, Copy = false }));

                    if (File.Exists(destDB))
                    {
                        // copy destination db to temp directory
                        File.Copy(destDB, tempDBDest, true);
                        // build connection string
                        var destConnectionString = "data source=\"" + tempDBDest + "\"";
                        using (var eDest = new SQLiteDataObject(destConnectionString))
                        {
                            // drop triggers
                            eDest.Execute(PNStrings.DROP_TRIGGERS);
                            if (areTablesDifferent(eSrc, eDest))
                            {
                                if (SyncComplete != null)
                                {
                                    SyncComplete(this, new LocalSyncCompleteEventArgs(LocalSyncResult.AbortVersion));
                                }
                                return;
                            }

                            //todo: create tables data
                            var tablesData = new Dictionary<string, List<_FieldData>>();
                            foreach (var tn in _Tables)
                            {
                                var td = new List<_FieldData>();
                                var sb = new StringBuilder("pragma table_info('");
                                sb.Append(tn);
                                sb.Append("')");
                                using (var t = eSrc.FillDataTable(sb.ToString()))
                                {
                                    td.AddRange(from DataRow r in t.Rows
                                                select new _FieldData
                                                {
                                                    Name = Convert.ToString(r["name"]),
                                                    Type = Convert.ToString(r["type"]),
                                                    NotNull = Convert.ToBoolean(r["notnull"])
                                                });
                                }
                                tablesData.Add(tn, td);
                            }

                            // get deleted ids
                            if (!PNStatic.Settings.Network.IncludeBinInSync)
                            {
                                var deletedDest = deletedIDs(eDest);
                                filesDest.RemoveAll(nf => deletedDest.Contains(Path.GetFileNameWithoutExtension(nf.Name)));
                            }
                            foreach (var sf in filesSrc)
                            {
                                var id = Path.GetFileNameWithoutExtension(sf.Name);
                                // find destination file with same name
                                var df = filesDest.FirstOrDefault(f => f.Name == sf.Name);
                                if (df == null)
                                {
                                    sf.Copy = true;
                                    //todo
                                    if (!insertToAllTables(eSrc, eDest, id, tablesData))
                                    {
                                        if (SyncComplete != null)
                                        {
                                            SyncComplete(this, new LocalSyncCompleteEventArgs(LocalSyncResult.Error));
                                        }
                                        return;
                                    }
                                }
                                else
                                {
                                    // check which note is more up to date
                                    var dSrc = File.GetLastWriteTime(sf.Path);
                                    var dDest = File.GetLastWriteTime(df.Path);
                                    if (dSrc > dDest)
                                    {
                                        // compare two files
                                        if (areFilesDifferent(sf.Path, df.Path))
                                        {
                                            // local file is younger then remote - copy it to remote client
                                            sf.Copy = true;
                                        }
                                    }
                                    else if (dSrc < dDest)
                                    {
                                        // compare two files
                                        if (areFilesDifferent(sf.Path, df.Path))
                                        {
                                            // remote file is younger than local - copy it to local directory
                                            df.Copy = true;
                                        }
                                    }
                                    //todo
                                    if (!exchangeData(eSrc, eDest, id, tablesData))
                                    {
                                        if (SyncComplete != null)
                                        {
                                            SyncComplete(this, new LocalSyncCompleteEventArgs(LocalSyncResult.Error));
                                        }
                                        return;
                                    }
                                }
                            }
                            // check remaining destination files
                            var remDest = filesDest.Where(df => !df.Copy);
                            foreach (var df in remDest)
                            {
                                if (filesSrc.All(sf => sf.Name != df.Name))
                                {
                                    df.Copy = true;
                                    var id = Path.GetFileNameWithoutExtension(df.Name);
                                    //todo
                                    if (!exchangeData(eSrc, eDest, id, tablesData))
                                    {
                                        if (SyncComplete != null)
                                        {
                                            SyncComplete(this, new LocalSyncCompleteEventArgs(LocalSyncResult.Error));
                                        }
                                        return;
                                    }
                                }
                            }
                            // synchronize groups
                            if (!exchangeGroups(eSrc, eDest, tablesData.FirstOrDefault(td => td.Key == "GROUPS")))
                            {
                                if (SyncComplete != null)
                                {
                                    SyncComplete(this, new LocalSyncCompleteEventArgs(LocalSyncResult.Error));
                                }
                                return;
                            }
                            // restore triggers
                            eSrc.Execute(PNStrings.CREATE_TRIGGERS);
                            eDest.Execute(PNStrings.CREATE_TRIGGERS);
                        }
                        // copy files
                        var filesToCopy = filesSrc.Where(sf => sf.Copy);
                        foreach (var sf in filesToCopy)
                        {
                            var newPath = Path.Combine(destDir, sf.Name);
                            File.Copy(sf.Path, newPath, true);
                        }
                        filesToCopy = filesDest.Where(df => df.Copy);
                        foreach (var df in filesToCopy)
                        {
                            var newPath = Path.Combine(PNPaths.Instance.DataDir, df.Name);
                            File.Copy(df.Path, newPath, true);
                        }
                        if (filesDest.Count(df => df.Copy) == 0) result = LocalSyncResult.None;
                        // copy synchronized db files
                        File.Copy(tempDBSrc, PNPaths.Instance.DBPath, true);
                        File.Copy(tempDBDest, destDB, true);
                    }
                    else
                    {
                        // restore triggers
                        eSrc.Execute(PNStrings.CREATE_TRIGGERS);
                        // just copy all notes files and db file to remote client
                        File.Copy(PNPaths.Instance.DBPath, destDB, true);
                        foreach (var sf in filesSrc)
                        {
                            var newPath = Path.Combine(destDir, sf.Name);
                            File.Copy(sf.Path, newPath, true);
                        }
                        result = LocalSyncResult.None;
                    }
                }
                if (SyncComplete != null)
                {
                    SyncComplete(this, new LocalSyncCompleteEventArgs(result));
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                if (SyncComplete != null)
                {
                    SyncComplete(this, new LocalSyncCompleteEventArgs(LocalSyncResult.Error));
                }
            }
            finally
            {
                if (tempDir != "" && Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        private List<string> deletedIDs(SQLiteDataObject oData)
        {
            var list = new List<string>();

            try
            {
                using (var t = oData.FillDataTable("SELECT ID FROM NOTES WHERE GROUP_ID = " + (int)SpecialGroups.RecycleBin))
                {
                    list.AddRange(from DataRow r in t.Rows select (string)r["ID"]);
                }
                return list;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return list;
            }
        }

        private string createTempDir()
        {
            try
            {
                // create temp directory
                var tempDir = Path.Combine(Path.GetTempPath(), PNStrings.TEMP_SYNC_DIR);
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
                Directory.CreateDirectory(tempDir);
                return tempDir;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return "";
            }
        }

        private object getUpdateValue(string tableName, string columnName)
        {
            try
            {
                switch (tableName)
                {
                    case "GROUPS":
                        switch (columnName)
                        {
                            case "IS_DEFAULT_IMAGE":
                                return 1;
                        }
                        break;
                }
                return null;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private bool areTablesDifferent(SQLiteDataObject dataSrc, SQLiteDataObject dataDest)
        {
            try
            {
                foreach (var tableName in _Tables)
                {
                    var sqlQuery = "pragma table_info('" + tableName + "')";
                    using (var ts = dataSrc.FillDataTable(sqlQuery))
                    {
                        using (var td = dataDest.FillDataTable(sqlQuery))
                        {
                            var upper = ts.Rows.Count == td.Rows.Count
                                ? ts.Rows.Count
                                : Math.Min(ts.Rows.Count, td.Rows.Count);
                            for (var i = 0; i < upper; i++)
                            {
                                if (Convert.ToString(ts.Rows[i]["name"]) != Convert.ToString(td.Rows[i]["name"]) ||
                                    Convert.ToString(ts.Rows[i]["type"]) != Convert.ToString(td.Rows[i]["type"]))
                                {
                                    return true;
                                }
                            }
                            if (upper >= ts.Rows.Count) continue;
                            for (var i = upper; i < ts.Rows.Count; i++)
                            {
                                var r = ts.Rows[i];
                                var sb = new StringBuilder("ALTER TABLE ");
                                sb.Append(tableName);
                                sb.Append(" ADD COLUMN [");
                                sb.Append(r["name"]);
                                sb.Append("] ");
                                sb.Append(r["type"]);
                                if (!DBNull.Value.Equals(r["dflt_value"]))
                                {
                                    sb.Append(" DEFAULT (");
                                    sb.Append(r["dflt_value"]);
                                    sb.Append(")");
                                    sb.Append("; UPDATE ");
                                    sb.Append(tableName);
                                    sb.Append(" SET ");
                                    sb.Append(r["name"]);
                                    sb.Append(" = ");
                                    if (Convert.ToString(r["type"]).ToUpper() == "TEXT")
                                    {
                                        sb.Append("'");
                                        sb.Append(Convert.ToString(r["dflt_value"]).Replace("'", "''"));
                                    }
                                    else
                                    {
                                        sb.Append(r["dflt_value"]);
                                    }
                                    
                                    if (Convert.ToString(r["type"]).ToUpper() == "TEXT")
                                    {
                                        sb.Append("'");
                                    }
                                }
                                else
                                {
                                    var updValue = getUpdateValue(tableName, Convert.ToString(r["name"]));
                                    if (updValue != null)
                                    {
                                        sb.Append("; UPDATE ");
                                        sb.Append(tableName);
                                        sb.Append(" SET ");
                                        sb.Append(r["name"]);
                                        sb.Append(" = ");
                                        if (Convert.ToString(r["type"]).ToUpper() == "TEXT")
                                        {
                                            sb.Append("'");
                                            sb.Append(Convert.ToString(updValue).Replace("'", "''"));
                                        }
                                        else
                                        {
                                            sb.Append(updValue);
                                        }
                                        
                                        if (Convert.ToString(r["type"]).ToUpper() == "TEXT")
                                        {
                                            sb.Append("'");
                                        }
                                    }
                                }
                                dataDest.Execute(sb.ToString());
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool areFilesDifferent(string path1, string path2)
        {
            try
            {
                using (var fs1 = new FileStream(path1, FileMode.Open, FileAccess.Read))
                {
                    using (var fs2 = new FileStream(path2, FileMode.Open, FileAccess.Read))
                    {
                        var l1 = fs1.Length;
                        var l2 = fs2.Length;
                        if (l1 != l2)
                        {
                            return true;
                        }
                        while (fs1.Position < l1 && fs2.Position < l2)
                        {
                            if (fs1.ReadByte() != fs2.ReadByte())
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        #region Insert procedures

        private string createInsert(DataRow r, KeyValuePair<string, List<_FieldData>> tableData)
        {
            try
            {
                var fieldsList = string.Join(", ", tableData.Value.Select(f => f.Name));
                var sb = new StringBuilder();
                sb.Append("INSERT INTO ");
                sb.Append(tableData.Key);
                sb.Append(" (");
                sb.Append(fieldsList);
                sb.Append(") VALUES(");
                foreach (var f in tableData.Value)
                {
                    if (f.NotNull)
                    {
                        if (f.Type == "TEXT")
                            sb.Append("'");
                        switch (f.Type)
                        {
                            case "BOOLEAN":
                                sb.Append(Convert.ToInt32(r[f.Name]));
                                break;
                            case "TEXT":
                                sb.Append(Convert.ToString(r[f.Name]).Replace("'", "''"));
                                break;
                            default:
                                sb.Append(r[f.Name]);
                                break;
                        }
                        if (f.Type == "TEXT")
                            sb.Append("'");
                    }
                    else
                    {
                        if (!DBNull.Value.Equals(r[f.Name]))
                        {
                            if (f.Type == "TEXT")
                                sb.Append("'");
                            switch (f.Type)
                            {
                                case "BOOLEAN":
                                    sb.Append(Convert.ToInt32(r[f.Name]));
                                    break;
                                case "TEXT":
                                    sb.Append(Convert.ToString(r[f.Name]).Replace("'", "''"));
                                    break;
                                default:
                                    sb.Append(r[f.Name]);
                                    break;
                            }
                            if (f.Type == "TEXT")
                                sb.Append("'");
                        }
                        else
                        {
                            sb.Append("NULL");
                        }
                    }
                    sb.Append(", ");
                }
                sb.Length -= 2;
                sb.Append(")");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return "";
            }
        }

        private bool insertToNotesTags(SQLiteDataObject dataDest, DataRow r, KeyValuePair<string, List<_FieldData>> tableData)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("DELETE FROM NOTES_TAGS WHERE NOTE_ID = '");
                sb.Append(r["NOTE_ID"]);
                sb.Append("' AND TAG = '");
                sb.Append(r["TAG"]);
                sb.Append("'; ");

                sb.Append(createInsert(r, tableData));

                return executeTransaction(dataDest, sb.ToString());
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        //todo
        private bool insertToNotes(SQLiteDataObject dataDest, DataRow r, KeyValuePair<string, List<_FieldData>> tableData)
        {
            try
            {


                var sb = new StringBuilder();
                sb.Append("DELETE FROM NOTES WHERE ID = '");
                sb.Append(r["ID"]);
                sb.Append("'; ");

                sb.Append(createInsert(r, tableData));

                return executeTransaction(dataDest, sb.ToString());
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool insertToCustomNotesSettings(SQLiteDataObject dataDest, DataRow r, KeyValuePair<string, List<_FieldData>> tableData)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("DELETE FROM CUSTOM_NOTES_SETTINGS WHERE NOTE_ID = '");
                sb.Append(r["NOTE_ID"]);
                sb.Append("'; ");

                sb.Append(createInsert(r, tableData));

                return executeTransaction(dataDest, sb.ToString());
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool insertToLinkedNotes(SQLiteDataObject dataDest, DataRow r, KeyValuePair<string, List<_FieldData>> tableData)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("DELETE FROM LINKED_NOTES WHERE NOTE_ID = '");
                sb.Append(r["NOTE_ID"]);
                sb.Append("' AND LINK_ID = '");
                sb.Append(r["LINK_ID"]);
                sb.Append("'; ");

                sb.Append(createInsert(r, tableData));

                return executeTransaction(dataDest, sb.ToString());
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        //todo: table data
        private bool insertToAllTables(SQLiteDataObject dataSrc, SQLiteDataObject dataDest, string id, Dictionary<string, List<_FieldData>> tablesData)
        {
            try
            {
                using (var t = dataSrc.FillDataTable("SELECT * FROM NOTES WHERE ID = '" + id + "'"))
                {
                    if (t.Rows.Count > 0)
                    {
                        if (!insertToNotes(dataDest, t.Rows[0], tablesData.FirstOrDefault(td => td.Key == "NOTES")))
                        {
                            return false;
                        }
                    }
                }
                using (var t = dataSrc.FillDataTable("SELECT * FROM CUSTOM_NOTES_SETTINGS WHERE NOTE_ID = '" + id + "'"))
                {
                    if (t.Rows.Count > 0)
                    {
                        if (!insertToCustomNotesSettings(dataDest, t.Rows[0], tablesData.FirstOrDefault(td => td.Key == "CUSTOM_NOTES_SETTINGS")))
                        {
                            return false;
                        }
                    }
                }
                using (var t = dataSrc.FillDataTable("SELECT * FROM LINKED_NOTES WHERE NOTE_ID = '" + id + "'"))
                {
                    if (t.Rows.Count > 0)
                    {
                        if (!insertToLinkedNotes(dataDest, t.Rows[0], tablesData.FirstOrDefault(td => td.Key == "LINKED_NOTES")))
                        {
                            return false;
                        }
                    }
                }
                using (var t = dataSrc.FillDataTable("SELECT * FROM NOTES_SCHEDULE WHERE NOTE_ID = '" + id + "'"))
                {
                    if (t.Rows.Count > 0)
                    {
                        if (!insertToNotesSchedule(dataDest, t.Rows[0], tablesData.FirstOrDefault(td => td.Key == "NOTES_SCHEDULE")))
                        {
                            return false;
                        }
                    }
                }
                using (var t = dataSrc.FillDataTable("SELECT * FROM NOTES_TAGS WHERE NOTE_ID = '" + id + "'"))
                {
                    if (t.Rows.Count > 0)
                    {
                        if (!insertToNotesTags(dataDest, t.Rows[0], tablesData.FirstOrDefault(td => td.Key == "NOTES_TAGS")))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool insertToNotesSchedule(SQLiteDataObject dataDest, DataRow r, KeyValuePair<string, List<_FieldData>> tableData)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("DELETE FROM NOTES_SCHEDULE WHERE NOTE_ID = '");
                sb.Append(r["NOTE_ID"]);
                sb.Append("'; ");

                sb.Append(createInsert(r, tableData));

                return executeTransaction(dataDest, sb.ToString());
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }
        #endregion
    }
}

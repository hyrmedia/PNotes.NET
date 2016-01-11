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
using System.Text;
using PNContacts;
using PNEncryption;
using SQLiteWrapper;

namespace PNotes.NET
{
    internal class PNGoogle
    {
        internal static List<Tuple<string, string>> GetGoogleContacts()
        {
            var list = new List<Tuple<string, string>>();
            try
            {
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    var update = false;
                    using (var t = oData.FillDataTable("SELECT * FROM SERVICES WHERE APP_NAME = 'PNContactsLoader'"))
                    {
                        if (t.Rows.Count == 0) return list;
                        var row = t.Rows[0];
                        using (var enc = new PNEncryptor(PNKeys.ENC_KEY))
                        {
                            var gc = new GContacts(Convert.ToString(row["CLIENT_ID"]),
                                enc.DecryptString(Convert.ToString(row["CLIENT_SECRET"])),
                                Convert.ToString(row["APP_NAME"]));
                            if (!PNData.IsDBNull(row["ACCESS_TOKEN"]))
                                gc.AccessToken = enc.DecryptString(Convert.ToString(row["ACCESS_TOKEN"]));
                            if (!PNData.IsDBNull(row["REFRESH_TOKEN"]))
                                gc.RefreshToken = enc.DecryptString(Convert.ToString(row["REFRESH_TOKEN"]));
                            if (!PNData.IsDBNull(row["TOKEN_EXPIRY"]))
                            {
                                var expDate = Convert.ToDateTime(Convert.ToString(row["TOKEN_EXPIRY"]));
                                if (expDate <= DateTime.Now && !string.IsNullOrWhiteSpace(gc.RefreshToken) &&
                                    !string.IsNullOrWhiteSpace(gc.AccessToken))
                                {
                                    if (!string.IsNullOrWhiteSpace(gc.RefreshToken))
                                        update = gc.RefreshAccessToken();
                                    if (!update)
                                        update = gc.Authenticate();
                                    if (!update)
                                        return list;
                                }
                            }
                            if (string.IsNullOrWhiteSpace(gc.AccessToken))
                                update = gc.Authenticate();

                            while (true)
                            {
                                try
                                {
                                    list = gc.GetContacts();
                                    if (update)
                                    {
                                        var sb = new StringBuilder("UPDATE SERVICES SET ACCESS_TOKEN = '");
                                        sb.Append(enc.EncryptString(gc.AccessToken));
                                        sb.Append("', REFRESH_TOKEN = '");
                                        sb.Append(enc.EncryptString(gc.RefreshToken));
                                        sb.Append("', TOKEN_EXPIRY = '");
                                        sb.Append(gc.TokenExpiry.ToString("dd MMM yyyy HH:mm:ss"));
                                        sb.Append("' WHERE APP_NAME = 'PNContactsLoader'");
                                        oData.Execute(sb.ToString());
                                    }
                                    break;
                                }
                                catch (PNContactsException pnex)
                                {
                                    if (pnex.AdditionalInfo.ToUpper().Contains("UNAUTHORIZED"))
                                    {
                                        if (!string.IsNullOrWhiteSpace(gc.RefreshToken))
                                            update = gc.RefreshAccessToken();
                                        if (update) continue;
                                        update = gc.Authenticate();
                                        if (update) continue;
                                        PNStatic.LogException(pnex);
                                        break;
                                    }
                                    PNStatic.LogException(pnex);
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    PNStatic.LogException(ex);
                                    break;
                                }
                            }
                        }
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return list;
            }
        }
    }
}

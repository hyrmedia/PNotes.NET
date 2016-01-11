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
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;

namespace PNotes.NET
{
    internal class PNConnections
    {
        internal static ContactConnection CheckContactConnection(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress)) return ContactConnection.Disconnected;

            if (!PNStatic.Settings.Network.AllowPing) return ContactConnection.Connected;

            var ping = new Ping();
            var timeOut = 500;
            try
            {
                var reply = ping.Send(ipAddress, timeOut);
                if (reply == null) return ContactConnection.Disconnected;
                if (reply.Status == IPStatus.TimedOut)
                {
                    do
                    {
                        timeOut *= 2;
                        reply = ping.Send(ipAddress, timeOut);
                        if (reply == null) return ContactConnection.Disconnected;
                        if (reply.Status == IPStatus.Success) return ContactConnection.Connected;
                        if (reply.Status != IPStatus.TimedOut) return ContactConnection.Disconnected;
                    } while (timeOut <= 5000);
                }
                if (reply.Status == IPStatus.Success)
                    return ContactConnection.Connected;
            }
            catch (PingException)
            {
                return ContactConnection.Disconnected;
            }
            //var scope = new ManagementScope(string.Format("\\\\{0}\\root\\CIMV2", ipAddress), null);
            //var query = new ObjectQuery("SELECT * FROM Win32_Process");
            //var searcher = new ManagementObjectSearcher(scope, query);
            //foreach (var s in from ManagementBaseObject s in searcher.Get()
            //    where
            //        s["Name"].ToString().ToUpper().StartsWith("PNOTES.NET") &&
            //        s["Name"].ToString().ToUpper().EndsWith(".EXE")
            //    select s)
            //{
            //    return = ContactConnection.WithPnotes;
            //}
            return ContactConnection.Disconnected;
        }

        private static void PingCompletedCallback(object sender, PingCompletedEventArgs e)
        {
            
        }

    }
}

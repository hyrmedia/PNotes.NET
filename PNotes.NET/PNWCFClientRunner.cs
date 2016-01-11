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

using PNWCFLib;
using System;
using System.Collections.Generic;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace PNotes.NET
{
    internal class PNWCFClientRunner
    {
        internal event PNDataErrorEventHandler PNDataError;
        internal event EventHandler<NotesSentEventArgs> NotesSent;

        internal bool SendNotes(string contactName, string ipAddress, string message, string port, List<PNote> notes)
        {
            try
            {
                var endPointAddr = "net.tcp://" + ipAddress + ":" + port + "/PNService";
                var tcpBinding = new NetTcpBinding { TransactionFlow = false };
                tcpBinding.Security.Transport.ProtectionLevel = ProtectionLevel.EncryptAndSign;
                tcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
                tcpBinding.Security.Mode = SecurityMode.None;
                tcpBinding.SendTimeout = TimeSpan.FromSeconds(600);

                var endPointAddress = new EndpointAddress(endPointAddr);

                var proxy = ChannelFactory<IPNService>.CreateChannel(tcpBinding, endPointAddress);

                var sb = new StringBuilder();
                var hostName = Environment.MachineName;
                var pos = hostName.IndexOf(".", StringComparison.Ordinal);
                if (pos > -1)
                {
                    hostName = hostName.Substring(0, pos);
                }
                sb.Append(hostName);
                sb.Append(PNStrings.END_OF_ADDRESS);
                sb.Append(message);
                sb.Append(PNStrings.END_OF_FILE);

                var attempts = 0;

                try
                {
                    string result;
                    while (true)
                    {
                        try
                        {
                            result = proxy.GetNote(sb.ToString());
                            break;
                        }
                        catch (EndpointNotFoundException epnfex)
                        {
                            if (epnfex.Message.Contains("10061"))
                            {
                                attempts++;
                                if (attempts <= 5) continue;
                            }
                            if (PNDataError != null)
                            {
                                PNDataError(this, new PNDataErrorEventArgs(epnfex));
                            }
                            return false;
                        }
                    }
                    if (result != "SUCCESS") return false;
                    if (NotesSent != null)
                    {
                        NotesSent(this, new NotesSentEventArgs(notes, contactName));
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    if (PNDataError != null)
                    {
                        PNDataError(this, new PNDataErrorEventArgs(ex));
                    }
                    return false;
                }
                finally
                {
                    switch (((IChannel)proxy).State)
                    {
                        case CommunicationState.Opened:
                            ((IChannel)proxy).Close();
                            break;
                        case CommunicationState.Faulted:
                            ((IChannel)proxy).Abort();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (PNDataError != null)
                {
                    PNDataError(this, new PNDataErrorEventArgs(ex));
                }
                return false;
            }
        }
    }
}

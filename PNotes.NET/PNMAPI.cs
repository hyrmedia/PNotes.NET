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
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PNotes.NET
{
    /// <summary>
    /// Represents an email message to be sent through MAPI.
    /// </summary>
    public class MapiMailMessage
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private class MapiFileDescriptor
        {
            public int reserved = 0;
            public int flags = 0;
            public int position = 0;
            public string path = null;
            public string name = null;
            public IntPtr type = IntPtr.Zero;
        }

        #region Enums

        /// <summary>
        /// Specifies the valid RecipientTypes for a Recipient.
        /// </summary>
        public enum RecipientType
        {
            /// <summary>
            /// Recipient will be in the TO listNames.
            /// </summary>
            To = 1,

            /// <summary>
            /// Recipient will be in the CC listNames.
            /// </summary>
            CC = 2,

            /// <summary>
            /// Recipient will be in the BCC listNames.
            /// </summary>
            BCC = 3
        };

        #endregion Enums

        #region Member Variables

        private readonly RecipientCollection _RecipientCollection;
        private readonly List<string> _Files = new List<string>();
        private readonly ManualResetEvent _ManualResetEvent;
        private readonly TaskScheduler _TaskScheduler;

        #endregion Member Variables

        #region Constructors

        /// <summary>
        /// Creates a blank mail message.
        /// </summary>
        public MapiMailMessage()
        {
            _RecipientCollection = new RecipientCollection();
            _ManualResetEvent = new ManualResetEvent(false);
            _TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }


        /// <summary>
        /// Creates a new mail message with the specified subject.
        /// </summary>
        public MapiMailMessage(string subject)
            : this()
        {
            Subject = subject;
        }

        /// <summary>
        /// Creates a new mail message with the specified subject and body.
        /// </summary>
        public MapiMailMessage(string subject, string body)
            : this()
        {
            Subject = subject;
            Body = body;
        }

        #endregion Constructors

        #region Public Properties

        /// <summary>
        /// Gets or sets the subject of this mail message.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the body of this mail message.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets the recipient listNames for this mail message.
        /// </summary>
        public RecipientCollection Recipients
        {
            get { return _RecipientCollection; }
        }

        /// <summary>
        /// Gets the file listNames for this mail message.
        /// </summary>
        public List<string> Files
        {
            get { return _Files; }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Displays the mail message dialog asynchronously.
        /// </summary>
        public void ShowDialog()
        {
            // Create the mail message in an STA thread
            var t = new Thread(showMail) { IsBackground = true };
            t.SetApartmentState(ApartmentState.STA);
            t.Start();

            // only return when the new thread has built it's interop representation
            _ManualResetEvent.WaitOne();
            _ManualResetEvent.Reset();
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Sends the mail message.
        /// </summary>
        private void showMail()
        {
            var message = new MAPIHelperInterop.MapiMessage();

            using (var interopRecipients = _RecipientCollection.GetInteropRepresentation())
            {

                message.Subject = Subject;
                message.NoteText = Body;

                message.Recipients = interopRecipients.Handle;
                message.RecipientCount = _RecipientCollection.Count;

                // Check if we need to add attachments
                if (_Files.Count > 0)
                {
                    // Add attachments
                    message.Files = allocAttachments(out message.FileCount);
                }

                // Signal the creating thread (make the remaining code async)
                _ManualResetEvent.Set();

                const int MAPI_DIALOG = 0x8;

                var error = MAPIHelperInterop.MAPISendMail(IntPtr.Zero, IntPtr.Zero, message, MAPI_DIALOG, 0);

                if (_Files.Count > 0)
                {
                    // Deallocate the files
                    deallocFiles(message);
                }

                // Check for error
                string errorDescription = getErrorDescription(error);
                switch ((MAPIErrorCode)error)
                {
                    case MAPIErrorCode.MAPI_SUCCESS:
                    case MAPIErrorCode.MAPI_USER_ABORT:
                        //do nothing
                        break;
                    case MAPIErrorCode.MAPI_E_NO_MAIL_CLIENT:
                        {
                            //todo - add description to lang file in next version
                            var msg = PNLang.Instance.GetMessageText("mail_error", "MAPI error:") + '\n' +
                                         error.ToString(CultureInfo.InvariantCulture) + '\n' + "No default mail client installed";
                            Task.Factory.StartNew(
                                () =>
                                {
                                    PNMessageBox.Show(msg, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                                },
                                CancellationToken.None,
                                TaskCreationOptions.None,
                                _TaskScheduler);
                            break;
                        }
                    default:
                        {
                            var msg = PNLang.Instance.GetMessageText("mail_error", "MAPI error:") + '\n' +
                                         error.ToString(CultureInfo.InvariantCulture) + '\n' + errorDescription;
                            Task.Factory.StartNew(
                                () =>
                                {
                                    PNMessageBox.Show(msg, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                                },
                                CancellationToken.None,
                                TaskCreationOptions.None,
                                _TaskScheduler);
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Deallocates the files in a message.
        /// </summary>
        /// <param name="message">The message to deallocate the files from.</param>
        private void deallocFiles(MAPIHelperInterop.MapiMessage message)
        {
            if (message.Files == IntPtr.Zero) return;
            var fileDescType = typeof(MapiFileDescriptor);
            var fsize = Marshal.SizeOf(fileDescType);

            // Get the ptr to the files
            var runptr = (int)message.Files;
            // Release each file
            for (var i = 0; i < message.FileCount; i++)
            {
                Marshal.DestroyStructure((IntPtr)runptr, fileDescType);
                runptr += fsize;
            }
            // Release the file
            Marshal.FreeHGlobal(message.Files);
        }

        /// <summary>
        /// Allocates the file attachments
        /// </summary>
        /// <param name="fileCount"></param>
        /// <returns></returns>
        private IntPtr allocAttachments(out int fileCount)
        {
            fileCount = 0;
            if (_Files == null)
            {
                return IntPtr.Zero;
            }
            if ((_Files.Count <= 0) || (_Files.Count > 100))
            {
                return IntPtr.Zero;
            }

            var atype = typeof(MapiFileDescriptor);
            var asize = Marshal.SizeOf(atype);
            var ptra = Marshal.AllocHGlobal(_Files.Count * asize);

            var mfd = new MapiFileDescriptor { position = -1 };
            var runptr = (int)ptra;
            foreach (var path in _Files)
            {
                mfd.name = Path.GetFileName(path);
                mfd.path = path;
                Marshal.StructureToPtr(mfd, (IntPtr)runptr, false);
                runptr += asize;
            }

            fileCount = _Files.Count;
            return ptra;
        }

        /// <summary>
        /// Logs any Mapi errors.
        /// </summary>
        private string getErrorDescription(int errorCode)
        {
            switch ((MAPIErrorCode)errorCode)
            {
                case MAPIErrorCode.MAPI_USER_ABORT:
                    return "User Aborted.";
                case MAPIErrorCode.MAPI_E_FAILURE:
                    return "MAPI Failure.";
                case MAPIErrorCode.MAPI_E_LOGIN_FAILURE:
                    return "Login Failure.";
                case MAPIErrorCode.MAPI_E_DISK_FULL:
                    return "MAPI Disk full.";
                case MAPIErrorCode.MAPI_E_INSUFFICIENT_MEMORY:
                    return "MAPI Insufficient memory.";
                case MAPIErrorCode.MAPI_E_BLK_TOO_SMALL:
                    return "MAPI Block too small.";
                case MAPIErrorCode.MAPI_E_TOO_MANY_SESSIONS:
                    return "MAPI Too many sessions.";
                case MAPIErrorCode.MAPI_E_TOO_MANY_FILES:
                    return "MAPI too many files.";
                case MAPIErrorCode.MAPI_E_TOO_MANY_RECIPIENTS:
                    return "MAPI too many recipients.";
                case MAPIErrorCode.MAPI_E_ATTACHMENT_NOT_FOUND:
                    return "MAPI Attachment not found.";
                case MAPIErrorCode.MAPI_E_ATTACHMENT_OPEN_FAILURE:
                    return "MAPI Attachment open failure.";
                case MAPIErrorCode.MAPI_E_ATTACHMENT_WRITE_FAILURE:
                    return "MAPI Attachment Write Failure.";
                case MAPIErrorCode.MAPI_E_UNKNOWN_RECIPIENT:
                    return "MAPI Unknown recipient.";
                case MAPIErrorCode.MAPI_E_BAD_RECIPTYPE:
                    return "MAPI Bad recipient type.";
                case MAPIErrorCode.MAPI_E_NO_MESSAGES:
                    return "MAPI No messages.";
                case MAPIErrorCode.MAPI_E_INVALID_MESSAGE:
                    return "MAPI Invalid message.";
                case MAPIErrorCode.MAPI_E_TEXT_TOO_LARGE:
                    return "MAPI Text too large.";
                case MAPIErrorCode.MAPI_E_INVALID_SESSION:
                    return "MAPI Invalid session.";
                case MAPIErrorCode.MAPI_E_TYPE_NOT_SUPPORTED:
                    return "MAPI Type not supported.";
                case MAPIErrorCode.MAPI_E_AMBIGUOUS_RECIPIENT:
                    return "MAPI Ambiguous recipient.";
                case MAPIErrorCode.MAPI_E_MESSAGE_IN_USE:
                    return "MAPI Message in use.";
                case MAPIErrorCode.MAPI_E_NETWORK_FAILURE:
                    return "MAPI Network failure.";
                case MAPIErrorCode.MAPI_E_INVALID_EDITFIELDS:
                    return "MAPI Invalid edit fields.";
                case MAPIErrorCode.MAPI_E_INVALID_RECIPS:
                    return "MAPI Invalid Recipients.";
                case MAPIErrorCode.MAPI_E_NOT_SUPPORTED:
                    return "MAPI Not supported.";
                case MAPIErrorCode.MAPI_E_NO_LIBRARY:
                    return "MAPI No Library.";
                case MAPIErrorCode.MAPI_E_INVALID_PARAMETER:
                    return "MAPI Invalid parameter.";
            }

            return "";
        }
        #endregion Private Methods

        #region Private MAPIHelperInterop Class

        /// <summary>
        /// Internal class for calling MAPI APIs
        /// </summary>
        internal class MAPIHelperInterop
        {
            #region Constructors

            /// <summary>
            /// Private constructor.
            /// </summary>
            private MAPIHelperInterop()
            {
                // Intenationally blank
            }

            #endregion Constructors

            #region Constants

            public const int MAPI_LOGON_UI = 0x1;

            #endregion Constants

            #region APIs

            [DllImport("MAPI32.DLL", CharSet = CharSet.Ansi)]
            public static extern int MAPILogon(IntPtr hwnd, string prf, string pw, int flg, int rsv, ref IntPtr sess);

            [DllImport("MAPI32.DLL")]
            public static extern int MAPISendMail(IntPtr session, IntPtr hwnd, MapiMessage message, int flg, int rsv);
            #endregion APIs

            #region Structs

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public class MapiMessage
            {
                public int Reserved = 0;
                public string Subject = null;
                public string NoteText = null;
                public string MessageType = null;
                public string DateReceived = null;
                public string ConversationID = null;
                public int Flags = 0;
                public IntPtr Originator = IntPtr.Zero;
                public int RecipientCount;
                public IntPtr Recipients = IntPtr.Zero;
                public int FileCount;
                public IntPtr Files = IntPtr.Zero;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public class MapiRecipDesc
            {
                public int Reserved = 0;
                public int RecipientClass;
                public string Name = null;
                public string Address = null;
                public int eIDSize = 0;
                public IntPtr EntryID = IntPtr.Zero;
            }

            #endregion Structs
        }

        #endregion Private MAPIHelperInterop Class
    }

    /// <summary>
    /// Represents a Recipient for a MapiMailMessage.
    /// </summary>
    public class Recipient
    {
        #region Public Properties

        /// <summary>
        /// The email address of this recipient.
        /// </summary>
        public string Address;

        /// <summary>
        /// The display name of this recipient.
        /// </summary>
        public string DisplayName;

        /// <summary>
        /// How the recipient will receive this message (To, CC, BCC).
        /// </summary>
        public MapiMailMessage.RecipientType RecipientType = MapiMailMessage.RecipientType.To;

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Creates a new recipient with the specified address.
        /// </summary>
        public Recipient(string address)
        {
            Address = address;
        }

        /// <summary>
        /// Creates a new recipient with the specified address and display name.
        /// </summary>
        public Recipient(string address, string displayName)
        {
            Address = address;
            DisplayName = displayName;
        }

        /// <summary>
        /// Creates a new recipient with the specified address and recipient type.
        /// </summary>
        public Recipient(string address, MapiMailMessage.RecipientType recipientType)
        {
            Address = address;
            RecipientType = recipientType;
        }

        /// <summary>
        /// Creates a new recipient with the specified address, display name and recipient type.
        /// </summary>
        public Recipient(string address, string displayName, MapiMailMessage.RecipientType recipientType)
        {
            Address = address;
            DisplayName = displayName;
            RecipientType = recipientType;
        }

        #endregion Constructors

        #region Internal Methods

        /// <summary>
        /// Returns an interop representation of a recepient.
        /// </summary>
        /// <returns></returns>
        internal MapiMailMessage.MAPIHelperInterop.MapiRecipDesc GetInteropRepresentation()
        {
            var interop = new MapiMailMessage.MAPIHelperInterop.MapiRecipDesc();

            if (DisplayName == null)
            {
                interop.Name = Address;
            }
            else
            {
                interop.Name = DisplayName;
                interop.Address = Address;
            }

            interop.RecipientClass = (int)RecipientType;

            return interop;
        }

        #endregion Internal Methods
    }

    /// <summary>
    /// Represents a colleciton of recipients for a mail message.
    /// </summary>
    public class RecipientCollection : List<Recipient>
    {
        /// <summary>
        /// Adds a new recipient with the specified address to this collection.
        /// </summary>
        public void Add(string address)
        {
            Add(new Recipient(address));
        }

        /// <summary>
        /// Adds a new recipient with the specified address and display name to this collection.
        /// </summary>
        public void Add(string address, string displayName)
        {
            Add(new Recipient(address, displayName));
        }

        /// <summary>
        /// Adds a new recipient with the specified address and recipient type to this collection.
        /// </summary>
        public void Add(string address, MapiMailMessage.RecipientType recipientType)
        {
            Add(new Recipient(address, recipientType));
        }

        /// <summary>
        /// Adds a new recipient with the specified address, display name and recipient type to this collection.
        /// </summary>
        public void Add(string address, string displayName, MapiMailMessage.RecipientType recipientType)
        {
            Add(new Recipient(address, displayName, recipientType));
        }

        internal InteropRecipientCollection GetInteropRepresentation()
        {
            return new InteropRecipientCollection(this);
        }

        /// <summary>
        /// Struct which contains an interop representation of a colleciton of recipients.
        /// </summary>
        internal struct InteropRecipientCollection : IDisposable
        {
            #region Member Variables

            private IntPtr _Handle;
            private int _Count;

            #endregion Member Variables

            #region Constructors

            /// <summary>
            /// Default constructor for creating InteropRecipientCollection.
            /// </summary>
            /// <param name="outer"></param>
            public InteropRecipientCollection(RecipientCollection outer)
            {
                _Count = outer.Count;

                if (_Count == 0)
                {
                    _Handle = IntPtr.Zero;
                    return;
                }

                // allocate enough memory to hold all recipients
                var size = Marshal.SizeOf(typeof(MapiMailMessage.MAPIHelperInterop.MapiRecipDesc));
                _Handle = Marshal.AllocHGlobal(_Count * size);

                // place all interop recipients into the memory just allocated
                var ptr = (int)_Handle;
                foreach (var native in outer)
                {
                    var interop = native.GetInteropRepresentation();

                    // stick it in the memory block
                    Marshal.StructureToPtr(interop, (IntPtr)ptr, false);
                    ptr += size;
                }
            }

            #endregion Costructors

            #region Public Properties

            public IntPtr Handle
            {
                get { return _Handle; }
            }

            #endregion Public Properties

            #region Public Methods

            /// <summary>
            /// Disposes of resources.
            /// </summary>
            public void Dispose()
            {
                if (_Handle == IntPtr.Zero) return;
                var type = typeof(MapiMailMessage.MAPIHelperInterop.MapiRecipDesc);
                var size = Marshal.SizeOf(type);

                // destroy all the structures in the memory area
                var ptr = (int)_Handle;
                for (var i = 0; i < _Count; i++)
                {
                    Marshal.DestroyStructure((IntPtr)ptr, type);
                    ptr += size;
                }

                // free the memory
                Marshal.FreeHGlobal(_Handle);

                _Handle = IntPtr.Zero;
                _Count = 0;
            }

            #endregion Public Methods
        }
    }
}

﻿#pragma checksum "..\..\WndMailContact.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "C8DF6E2681E0B6135151E0602624464B"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace PNotes.NET {
    
    
    /// <summary>
    /// WndMailContact
    /// </summary>
    public partial class WndMailContact : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 4 "..\..\WndMailContact.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal PNotes.NET.WndMailContact DlgMailContact;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\WndMailContact.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock lblMailDisplayName;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\WndMailContact.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtMailDisplayName;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\WndMailContact.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock lblMailAddress;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\WndMailContact.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtMailAddress;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\WndMailContact.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button cmdOK;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\WndMailContact.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button cmdCancel;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/PNotes.NET;component/wndmailcontact.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\WndMailContact.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.DlgMailContact = ((PNotes.NET.WndMailContact)(target));
            
            #line 18 "..\..\WndMailContact.xaml"
            this.DlgMailContact.Loaded += new System.Windows.RoutedEventHandler(this.DlgMailContact_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.lblMailDisplayName = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this.txtMailDisplayName = ((System.Windows.Controls.TextBox)(target));
            return;
            case 4:
            this.lblMailAddress = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 5:
            this.txtMailAddress = ((System.Windows.Controls.TextBox)(target));
            
            #line 24 "..\..\WndMailContact.xaml"
            this.txtMailAddress.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.txtMailAddress_TextChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            this.cmdOK = ((System.Windows.Controls.Button)(target));
            
            #line 30 "..\..\WndMailContact.xaml"
            this.cmdOK.Click += new System.Windows.RoutedEventHandler(this.cmdOK_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.cmdCancel = ((System.Windows.Controls.Button)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}


﻿#pragma checksum "..\..\WndImportMailContacts.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "056EDBC5418381A1109B2ED6BBBD2BA0"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using PNotes.NET;
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
    /// WndImportMailContacts
    /// </summary>
    public partial class WndImportMailContacts : System.Windows.Window, System.Windows.Markup.IComponentConnector, System.Windows.Markup.IStyleConnector {
        
        
        #line 5 "..\..\WndImportMailContacts.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal PNotes.NET.WndImportMailContacts DlgImportMailContacts;
        
        #line default
        #line hidden
        
        
        #line 20 "..\..\WndImportMailContacts.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel stkImport;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\WndImportMailContacts.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock pnsImportContacts;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\WndImportMailContacts.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView grdMailContacts;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\WndImportMailContacts.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chkAll;
        
        #line default
        #line hidden
        
        
        #line 40 "..\..\WndImportMailContacts.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chkNoDuplicates;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\WndImportMailContacts.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button cmdLoadImport;
        
        #line default
        #line hidden
        
        
        #line 48 "..\..\WndImportMailContacts.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button cmdOK;
        
        #line default
        #line hidden
        
        
        #line 49 "..\..\WndImportMailContacts.xaml"
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
            System.Uri resourceLocater = new System.Uri("/PNotes.NET;component/wndimportmailcontacts.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\WndImportMailContacts.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal System.Delegate _CreateDelegate(System.Type delegateType, string handler) {
            return System.Delegate.CreateDelegate(delegateType, this, handler);
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
            this.DlgImportMailContacts = ((PNotes.NET.WndImportMailContacts)(target));
            
            #line 19 "..\..\WndImportMailContacts.xaml"
            this.DlgImportMailContacts.Loaded += new System.Windows.RoutedEventHandler(this.DlgImportMailContacts_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.stkImport = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 3:
            this.pnsImportContacts = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            this.grdMailContacts = ((System.Windows.Controls.ListView)(target));
            return;
            case 5:
            this.chkAll = ((System.Windows.Controls.CheckBox)(target));
            
            #line 27 "..\..\WndImportMailContacts.xaml"
            this.chkAll.Checked += new System.Windows.RoutedEventHandler(this.HeaderChecked);
            
            #line default
            #line hidden
            
            #line 27 "..\..\WndImportMailContacts.xaml"
            this.chkAll.Unchecked += new System.Windows.RoutedEventHandler(this.HeaderChecked);
            
            #line default
            #line hidden
            return;
            case 7:
            this.chkNoDuplicates = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 8:
            this.cmdLoadImport = ((System.Windows.Controls.Button)(target));
            
            #line 47 "..\..\WndImportMailContacts.xaml"
            this.cmdLoadImport.Click += new System.Windows.RoutedEventHandler(this.cmdLoadImport_Click);
            
            #line default
            #line hidden
            return;
            case 9:
            this.cmdOK = ((System.Windows.Controls.Button)(target));
            
            #line 48 "..\..\WndImportMailContacts.xaml"
            this.cmdOK.Click += new System.Windows.RoutedEventHandler(this.cmdOK_Click);
            
            #line default
            #line hidden
            return;
            case 10:
            this.cmdCancel = ((System.Windows.Controls.Button)(target));
            return;
            }
            this._contentLoaded = true;
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        void System.Windows.Markup.IStyleConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 6:
            
            #line 31 "..\..\WndImportMailContacts.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Checked += new System.Windows.RoutedEventHandler(this.CheckBoxChecked);
            
            #line default
            #line hidden
            
            #line 31 "..\..\WndImportMailContacts.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Unchecked += new System.Windows.RoutedEventHandler(this.CheckBoxChecked);
            
            #line default
            #line hidden
            break;
            }
        }
    }
}


﻿#pragma checksum "..\..\WndAbout.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "69C5221FB146F51DCF240C2986205F08"
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
    /// WndAbout
    /// </summary>
    public partial class WndAbout : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 5 "..\..\WndAbout.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal PNotes.NET.WndAbout DlgAbout;
        
        #line default
        #line hidden
        
        
        #line 19 "..\..\WndAbout.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image imgAbout;
        
        #line default
        #line hidden
        
        
        #line 20 "..\..\WndAbout.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock lblInfo;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\WndAbout.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Documents.Run progName;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\WndAbout.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Documents.Run progDesc;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\WndAbout.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Documents.Run progCopy;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\WndAbout.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Documents.Run progMail;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\WndAbout.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock lblGPL;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\WndAbout.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal PNotes.NET.AboutControl cntAbout;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\WndAbout.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock cmdLicense;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\WndAbout.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock cmdOK;
        
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
            System.Uri resourceLocater = new System.Uri("/PNotes.NET;component/wndabout.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\WndAbout.xaml"
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
            this.DlgAbout = ((PNotes.NET.WndAbout)(target));
            
            #line 16 "..\..\WndAbout.xaml"
            this.DlgAbout.Loaded += new System.Windows.RoutedEventHandler(this.DlgAbout_Loaded);
            
            #line default
            #line hidden
            
            #line 17 "..\..\WndAbout.xaml"
            this.DlgAbout.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(this.DlgAbout_PreviewKeyDown);
            
            #line default
            #line hidden
            return;
            case 2:
            this.imgAbout = ((System.Windows.Controls.Image)(target));
            return;
            case 3:
            this.lblInfo = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            this.progName = ((System.Windows.Documents.Run)(target));
            return;
            case 5:
            this.progDesc = ((System.Windows.Documents.Run)(target));
            return;
            case 6:
            this.progCopy = ((System.Windows.Documents.Run)(target));
            return;
            case 7:
            this.progMail = ((System.Windows.Documents.Run)(target));
            
            #line 24 "..\..\WndAbout.xaml"
            this.progMail.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.progMail_MouseLeftButtonDown);
            
            #line default
            #line hidden
            return;
            case 8:
            this.lblGPL = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 9:
            this.cntAbout = ((PNotes.NET.AboutControl)(target));
            return;
            case 10:
            this.cmdLicense = ((System.Windows.Controls.TextBlock)(target));
            
            #line 28 "..\..\WndAbout.xaml"
            this.cmdLicense.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.cmdLicense_MouseLeftButtonDown);
            
            #line default
            #line hidden
            return;
            case 11:
            this.cmdOK = ((System.Windows.Controls.TextBlock)(target));
            
            #line 29 "..\..\WndAbout.xaml"
            this.cmdOK.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.cmdOK_MouseLeftButtonDown);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

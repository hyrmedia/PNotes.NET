﻿#pragma checksum "..\..\WndExchangeLists.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "16DAB1E22FA8F056816FAC9F353BE68E"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using PNColorPicker;
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
    /// WndExchangeLists
    /// </summary>
    public partial class WndExchangeLists : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 4 "..\..\WndExchangeLists.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal PNotes.NET.WndExchangeLists DlgTags;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\WndExchangeLists.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock lblAvailableTags;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\WndExchangeLists.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock lblCurrentTags;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\WndExchangeLists.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox lstAvailabe;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\WndExchangeLists.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button cmdAvToCurr;
        
        #line default
        #line hidden
        
        
        #line 34 "..\..\WndExchangeLists.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button cmdCurrToAv;
        
        #line default
        #line hidden
        
        
        #line 36 "..\..\WndExchangeLists.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox lstCurrent;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\WndExchangeLists.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button cmdOK;
        
        #line default
        #line hidden
        
        
        #line 43 "..\..\WndExchangeLists.xaml"
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
            System.Uri resourceLocater = new System.Uri("/PNotes.NET;component/wndexchangelists.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\WndExchangeLists.xaml"
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
            this.DlgTags = ((PNotes.NET.WndExchangeLists)(target));
            
            #line 17 "..\..\WndExchangeLists.xaml"
            this.DlgTags.Loaded += new System.Windows.RoutedEventHandler(this.DlgTags_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.lblAvailableTags = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this.lblCurrentTags = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            this.lstAvailabe = ((System.Windows.Controls.ListBox)(target));
            
            #line 31 "..\..\WndExchangeLists.xaml"
            this.lstAvailabe.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.lstAvailabe_SelectionChanged);
            
            #line default
            #line hidden
            
            #line 31 "..\..\WndExchangeLists.xaml"
            this.lstAvailabe.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(this.lstAvailabe_MouseDoubleClick);
            
            #line default
            #line hidden
            return;
            case 5:
            this.cmdAvToCurr = ((System.Windows.Controls.Button)(target));
            
            #line 33 "..\..\WndExchangeLists.xaml"
            this.cmdAvToCurr.Click += new System.Windows.RoutedEventHandler(this.cmdAvToCurr_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.cmdCurrToAv = ((System.Windows.Controls.Button)(target));
            
            #line 34 "..\..\WndExchangeLists.xaml"
            this.cmdCurrToAv.Click += new System.Windows.RoutedEventHandler(this.cmdCurrToAv_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.lstCurrent = ((System.Windows.Controls.ListBox)(target));
            
            #line 36 "..\..\WndExchangeLists.xaml"
            this.lstCurrent.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.lstCurrent_SelectionChanged);
            
            #line default
            #line hidden
            
            #line 36 "..\..\WndExchangeLists.xaml"
            this.lstCurrent.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(this.lstCurrent_MouseDoubleClick);
            
            #line default
            #line hidden
            return;
            case 8:
            this.cmdOK = ((System.Windows.Controls.Button)(target));
            
            #line 42 "..\..\WndExchangeLists.xaml"
            this.cmdOK.Click += new System.Windows.RoutedEventHandler(this.cmdOK_Click);
            
            #line default
            #line hidden
            return;
            case 9:
            this.cmdCancel = ((System.Windows.Controls.Button)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

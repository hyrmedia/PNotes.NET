﻿#pragma checksum "..\..\SkinnableFooter.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "4D3A9E391E8E45826EA7113CBA00AD4D"
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
    /// SkinnableFooter
    /// </summary>
    public partial class SkinnableFooter : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 23 "..\..\SkinnableFooter.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image ScheduleButton;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\SkinnableFooter.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image ChangeButton;
        
        #line default
        #line hidden
        
        
        #line 25 "..\..\SkinnableFooter.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image ProtectedButton;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\SkinnableFooter.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image PriorityButton;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\SkinnableFooter.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image CompleteButton;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\SkinnableFooter.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image PasswordButton;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\SkinnableFooter.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image PinButton;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\SkinnableFooter.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image MailButton;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\SkinnableFooter.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image EncryptedButton;
        
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
            System.Uri resourceLocater = new System.Uri("/PNotes.NET;component/skinnablefooter.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\SkinnableFooter.xaml"
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
            
            #line 10 "..\..\SkinnableFooter.xaml"
            ((PNotes.NET.SkinnableFooter)(target)).Initialized += new System.EventHandler(this.UserControl_Initialized);
            
            #line default
            #line hidden
            return;
            case 2:
            this.ScheduleButton = ((System.Windows.Controls.Image)(target));
            
            #line 23 "..\..\SkinnableFooter.xaml"
            this.ScheduleButton.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.MarkButton_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.ChangeButton = ((System.Windows.Controls.Image)(target));
            
            #line 24 "..\..\SkinnableFooter.xaml"
            this.ChangeButton.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.MarkButton_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.ProtectedButton = ((System.Windows.Controls.Image)(target));
            
            #line 25 "..\..\SkinnableFooter.xaml"
            this.ProtectedButton.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.MarkButton_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.PriorityButton = ((System.Windows.Controls.Image)(target));
            
            #line 26 "..\..\SkinnableFooter.xaml"
            this.PriorityButton.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.MarkButton_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.CompleteButton = ((System.Windows.Controls.Image)(target));
            
            #line 27 "..\..\SkinnableFooter.xaml"
            this.CompleteButton.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.MarkButton_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.PasswordButton = ((System.Windows.Controls.Image)(target));
            
            #line 28 "..\..\SkinnableFooter.xaml"
            this.PasswordButton.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.MarkButton_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.PinButton = ((System.Windows.Controls.Image)(target));
            
            #line 29 "..\..\SkinnableFooter.xaml"
            this.PinButton.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.MarkButton_Click);
            
            #line default
            #line hidden
            return;
            case 9:
            this.MailButton = ((System.Windows.Controls.Image)(target));
            
            #line 30 "..\..\SkinnableFooter.xaml"
            this.MailButton.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.MarkButton_Click);
            
            #line default
            #line hidden
            return;
            case 10:
            this.EncryptedButton = ((System.Windows.Controls.Image)(target));
            
            #line 31 "..\..\SkinnableFooter.xaml"
            this.EncryptedButton.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.MarkButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}


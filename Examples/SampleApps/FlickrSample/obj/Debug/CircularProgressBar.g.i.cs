﻿#pragma checksum "..\..\CircularProgressBar.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "A148A3AEDAE3C3FBEC105411B5CD727F"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
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


namespace FlickrSample {
    
    
    /// <summary>
    /// CircularProgressBar
    /// </summary>
    public partial class CircularProgressBar : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 6 "..\..\CircularProgressBar.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid LayoutRoot;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\CircularProgressBar.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Ellipse C0;
        
        #line default
        #line hidden
        
        
        #line 19 "..\..\CircularProgressBar.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Ellipse C1;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\CircularProgressBar.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Ellipse C2;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\CircularProgressBar.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Ellipse C3;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\CircularProgressBar.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Ellipse C4;
        
        #line default
        #line hidden
        
        
        #line 35 "..\..\CircularProgressBar.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Ellipse C5;
        
        #line default
        #line hidden
        
        
        #line 39 "..\..\CircularProgressBar.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Ellipse C6;
        
        #line default
        #line hidden
        
        
        #line 43 "..\..\CircularProgressBar.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Ellipse C7;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\CircularProgressBar.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Ellipse C8;
        
        #line default
        #line hidden
        
        
        #line 52 "..\..\CircularProgressBar.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.RotateTransform SpinnerRotate;
        
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
            System.Uri resourceLocater = new System.Uri("/FlickrSample;component/circularprogressbar.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\CircularProgressBar.xaml"
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
            
            #line 5 "..\..\CircularProgressBar.xaml"
            ((FlickrSample.CircularProgressBar)(target)).IsVisibleChanged += new System.Windows.DependencyPropertyChangedEventHandler(this.HandleVisibleChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            this.LayoutRoot = ((System.Windows.Controls.Grid)(target));
            return;
            case 3:
            
            #line 13 "..\..\CircularProgressBar.xaml"
            ((System.Windows.Controls.Canvas)(target)).Loaded += new System.Windows.RoutedEventHandler(this.HandleLoaded);
            
            #line default
            #line hidden
            
            #line 14 "..\..\CircularProgressBar.xaml"
            ((System.Windows.Controls.Canvas)(target)).Unloaded += new System.Windows.RoutedEventHandler(this.HandleUnloaded);
            
            #line default
            #line hidden
            return;
            case 4:
            this.C0 = ((System.Windows.Shapes.Ellipse)(target));
            return;
            case 5:
            this.C1 = ((System.Windows.Shapes.Ellipse)(target));
            return;
            case 6:
            this.C2 = ((System.Windows.Shapes.Ellipse)(target));
            return;
            case 7:
            this.C3 = ((System.Windows.Shapes.Ellipse)(target));
            return;
            case 8:
            this.C4 = ((System.Windows.Shapes.Ellipse)(target));
            return;
            case 9:
            this.C5 = ((System.Windows.Shapes.Ellipse)(target));
            return;
            case 10:
            this.C6 = ((System.Windows.Shapes.Ellipse)(target));
            return;
            case 11:
            this.C7 = ((System.Windows.Shapes.Ellipse)(target));
            return;
            case 12:
            this.C8 = ((System.Windows.Shapes.Ellipse)(target));
            return;
            case 13:
            this.SpinnerRotate = ((System.Windows.Media.RotateTransform)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}


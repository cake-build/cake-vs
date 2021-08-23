﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Text.Editor;

namespace Cake.VisualStudio.Adornments
{
    class LogoAdornment
    {
        private IAdornmentLayer _adornmentLayer;
        private Image _adornment;
        private readonly double _initOpacity;
        private double _currentOpacity;

        public LogoAdornment(IWpfTextView view,  bool isVisible, double initOpacity)
        {
            _adornmentLayer = view.GetAdornmentLayer(AdornmentLayer.LayerName);
            _currentOpacity = isVisible ? initOpacity : 0;
            _initOpacity = initOpacity;

            CreateImage();

            view.ViewportHeightChanged += SetAdornmentLocation;
            view.ViewportWidthChanged += SetAdornmentLocation;
            VisibilityChanged += ToggleVisibility;

            if (_adornmentLayer.IsEmpty)
            {
                _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, null, null, _adornment, null);
            }
        }

        private void ToggleVisibility(object sender, bool isVisible)
        {
            _adornment.Opacity = isVisible ? _initOpacity : 0;
            _currentOpacity = _adornment.Opacity;
        }

        private void CreateImage()
        {
            _adornment = new Image();
            _adornment.Source = GetImage();
            _adornment.ToolTip = "Click to toggle visibility";
            _adornment.Opacity = _currentOpacity;
            _adornment.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.HighQuality);

            _adornment.MouseEnter += (s, e) => { _adornment.Opacity = 1D; };
            _adornment.MouseLeave += (s, e) => { _adornment.Opacity = _currentOpacity; };
            _adornment.MouseLeftButtonUp += (s, e) => { OnVisibilityChanged(_currentOpacity == 0); };
        }

        private static ImageSource GetImage()
        {
            var assembly = Assembly.GetExecutingAssembly().Location;
            var folder = Path.GetDirectoryName(assembly);
            var file = Path.Combine(folder, "Resources\\icon.png");

            var url =  new Uri(file, UriKind.Absolute);
            return BitmapFrame.Create(url);
        }

        private void SetAdornmentLocation(object sender, EventArgs e)
        {
            var view = (IWpfTextView)sender;
            Canvas.SetLeft(_adornment, view.ViewportRight - _adornment.Source.Width - 20);
            Canvas.SetTop(_adornment, view.ViewportBottom - _adornment.Source.Height - 20);
        }

        public static event EventHandler<bool> VisibilityChanged;

        internal static void OnVisibilityChanged(bool isVisible)
        {
            if (VisibilityChanged != null)
            {
                VisibilityChanged(null, isVisible);
            }
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Audiotica.View
{
    public sealed partial class FirstRunPage
    {
        public FirstRunPage()
        {
            this.InitializeComponent();
        }

        public override void NavigatedTo(object parameter)
        {
            base.NavigatedTo(parameter);
            BackgroundAnimation.Begin();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            App.Navigator.GoTo<HomePage, ZoomOutTransition>(null);
        }

        private void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContinueButton == null || FlipView.SelectedIndex != 2) return;

            ContinueButton.Visibility = Visibility.Visible;
            ContinueButtonAnimation.Begin();
        }
    }
}

﻿#region

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Audiotica.Data.Collection.Model;

#endregion

namespace Audiotica
{
    public sealed partial class SongViewer
    {
        public SongViewer()
        {
            InitializeComponent();
        }

        private Song _song;
        private bool _queueMode;
        private bool _playlistMode;

        /// <summary> 
        /// This method visualizes the placeholder state of the data item. When  
        /// showing a placehlder, we set the opacity of other elements to zero 
        /// so that stale data is not visible to the end user.
        /// </summary> 
        public void ShowPlaceholder(Song song, bool queueMode = false, bool playlistMode = false)
        {
            _playlistMode = playlistMode;
            _queueMode = queueMode;
            DownloadOptionGrid.Visibility = Visibility.Collapsed;
            DownloadProgressGrid.Visibility = Visibility.Collapsed;

            DataContext = song;
            _song = song;
            SongNameTextBlock.Opacity = 0;
            ArtistAlbumNameTextBlock.Opacity = 0;
        }

        /// <summary> 
        /// Visualize the Title by updating the TextBlock for Title and setting Opacity 
        /// to 1. 
        /// </summary> 
        public void ShowTitle()
        {
            SongNameTextBlock.Text = _song.Name;
            SongNameTextBlock.Opacity = 1;
        }

        /// <summary> 
        /// Visualize artist information by updating the correct TextBlock and  
        /// setting Opacity to 1. 
        /// </summary> 
        public void ShowRest(bool withAlbumName = true)
        {
            var albumName = withAlbumName ? _song.Album.Name : "";

            if (!string.IsNullOrEmpty(albumName))
                albumName = " - " + albumName;

            ArtistAlbumNameTextBlock.Text = _song.ArtistName + albumName;
            ArtistAlbumNameTextBlock.Opacity = 1;

            ShowDownload();
        }

        public void ShowDownload()
        {
            if (_queueMode) return;

            DownloadOptionGrid.Visibility = Visibility.Visible;
            DownloadProgressGrid.Visibility = Visibility.Visible;
        }

        /// <summary> 
        /// Drop all refrences to the data item 
        /// </summary> 
        public void ClearData()
        {
            _queueMode = true;
            _playlistMode = true;
            _song = null;
            SongNameTextBlock.ClearValue(TextBlock.TextProperty);
            ArtistAlbumNameTextBlock.ClearValue(TextBlock.TextProperty);
            DataContext = null;
        } 

        #region Song user events

        private void Song_OnHolding(object sender, HoldingRoutedEventArgs e)
        {
            if (!_queueMode && !_playlistMode)
            {
                FlyoutBase.ShowAttachedFlyout(RootGrid);
            }
        }

        private void AddToMenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            App.Locator.Collection.Commands.AddToClickCommand.Execute(_song);
        }

        private void Picker_OnItemsPicked(ListPickerFlyout sender, ItemsPickedEventArgs args)
        {
            App.Locator.Collection.Commands.ItemPickedCommand.Execute(args.AddedItems[0]);
        }

        private void DeleteMenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            App.Locator.Collection.Commands.DeleteClickCommand.Execute(_song);
        }

        private void DownloadButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            App.Locator.Collection.Commands.DownloadClickCommand.Execute(_song);
        }

        private void CancelButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            App.Locator.Collection.Commands.CancelClickCommand.Execute(_song);
        }

        #endregion
    }
}
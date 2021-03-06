﻿#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media.Imaging;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection.SqlHelper;
using SQLite;

#endregion

namespace Audiotica.Data.Collection.Model
{
    public class Album : BaseEntry
    {
        private BitmapImage _artwork;
        private BitmapImage _smallArtwork;
        private BitmapImage _mediumArtwork;

        public Album()
        {
            Songs = new OptimizedObservableCollection<Song>();
            AddableTo = new ObservableCollection<AddableCollectionItem>();
        }

        public string ProviderId { get; set; }

        [Indexed]
        public int PrimaryArtistId { get; set; }


        public string Name { get; set; }

        public string Genre { get; set; }

        public DateTime ReleaseDate { get; set; }

        [Ignore]
        public OptimizedObservableCollection<Song> Songs { get; set; }

        [Ignore]
        public BitmapImage Artwork { get { return _artwork; }
            set
            {
                _artwork = value;
                OnPropertyChanged();
            } }
        [Ignore]
        public BitmapImage SmallArtwork
        {
            get { return _smallArtwork; }
            set
            {
                _smallArtwork = value;
                OnPropertyChanged();
            }
        }
        [Ignore]
        public BitmapImage MediumArtwork
        {
            get { return _mediumArtwork; }
            set
            {
                _mediumArtwork = value;
                OnPropertyChanged();
            }
        }

        [Ignore]
        public Artist PrimaryArtist { get; set; }

        public bool HasArtwork { get; set; }

        [Ignore]
        public ObservableCollection<AddableCollectionItem> AddableTo { get; set; }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Audiotica.Collection;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection.Model;
using SQLitePCL;

namespace Audiotica.Data.Collection.RunTime
{
    public class CollectionService : ICollectionService
    {
        private readonly ISqlService _service;
        private readonly CoreDispatcher _dispatcher;

        public CollectionService(ISqlService service, CoreDispatcher dispatcher)
        {
            _service = service;
            _dispatcher = dispatcher;
            Songs = new ObservableCollection<Song>();
            Artists = new ObservableCollection<Artist>();
            Albums = new ObservableCollection<Album>();
        }


        public ObservableCollection<Song> Songs { get; set; }
        public ObservableCollection<Album> Albums { get; set; }
        public ObservableCollection<Artist> Artists { get; set; }

        public void LoadLibrary()
        {
            var songs = new ObservableCollection<Song>(_service.SelectAll<Song>());
            var albums = new ObservableCollection<Album>(_service.SelectAll<Album>());
            var artists = new ObservableCollection<Artist>(_service.SelectAll<Artist>());

            foreach (var song in songs)
            {
                song.Artist = artists.FirstOrDefault(p => p.Id == song.ArtistId);
                song.Album = albums.FirstOrDefault(p => p.Id == song.AlbumId);
            }

            foreach (var album in albums)
            {
                album.Songs = songs.Where(p => p.AlbumId == album.Id).OrderBy(p => p.TrackNumber).ToList();
                album.PrimaryArtist = artists.FirstOrDefault(p => p.Id == album.PrimaryArtistId);

                if (_dispatcher != null)
// ReSharper disable AccessToForEachVariableInClosure
                    _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => album.Artwork = await GetArtworkAsync(album.Id)).AsTask().Wait();
// ReSharper restore AccessToForEachVariableInClosure
            }

            foreach (var artist in artists)
            {
                artist.Songs = songs.Where(p => p.ArtistId == artist.Id).ToList();
                artist.Albums = albums.Where(p => p.PrimaryArtistId == artist.Id).ToList();
            }

            

            Songs = songs;
            Artists = artists;
            Albums = albums;
            CleanupFiles();
        }

        /// <summary>
        /// Deleting unused files.
        /// Artworks, since deleting them when an album is delete can cause problems.
        /// </summary>
        private async void CleanupFiles()
        {
            var artworkFolder = await StorageHelper.GetFolderAsync("artworks");
            
            if (artworkFolder == null) return;;

            var artworks = await artworkFolder.GetFilesAsync();

            foreach (var file in from file in artworks let id = int.Parse(file.Name.Replace(".jpg", "")) where Albums.Count(p => p.Id == id) == 0 select file)
            {
                await file.DeleteAsync();
            }
        }

        private async Task<BitmapImage> GetArtworkAsync(long id)
        {
            var artworkPath = string.Format(CollectionConstant.ArtworkPath, id);

            var exists = await StorageHelper.FileExistsAsync(artworkPath);

            return exists 
                ? new BitmapImage(new Uri(CollectionConstant.LocalStorageAppPath + artworkPath)) 
                : CollectionConstant.MissingArtworkImage;
        }

        public Task LoadLibraryAsync()
        {
            //just return non async as a task
            return Task.Factory.StartNew(LoadLibrary);
        }

        public async Task AddSongAsync(Song song, string artworkUrl)
        {
            if (Songs.Count(p => p.ProviderId == song.ProviderId) > 0)
                throw new Exception("AlreadySavedToast".FromLanguageResource());

            #region create artist

            if (song.Artist.ProviderId == "lastid.")
                song.Artist.ProviderId = "autc.single." + song.ProviderId;

            var artist = Artists.FirstOrDefault(entry => entry.ProviderId == song.Artist.ProviderId);

            if (artist == null)
            {
                await _service.InsertAsync(song.Artist);

                if (song.Album != null)
                    song.Album.PrimaryArtistId = song.Artist.Id;
                Artists.Add(song.Artist);

                song.Artist.Songs = new List<Song>();
                song.Artist.Albums = new List<Album>();
            }

            else
            {
                song.Artist = artist;

                if (song.Album != null)
                    song.Album.PrimaryArtistId = artist.Id;
            }
            song.ArtistId = song.Artist.Id;

            #endregion

            #region create album

            if (song.Album == null)
            {
                song.Album = new Album()
                {
                    PrimaryArtistId = song.ArtistId,
                    Name = song.Name + " (Single)",
                    PrimaryArtist = song.Artist,
                    ProviderId = "autc.single." + song.ProviderId
                };
                await _service.InsertAsync(song.Album);
                Albums.Add(song.Album);
                song.Album.Songs = new List<Song>();
                song.Artist.Albums.Add(song.Album);
            }
            else
            {
                var album = Albums.FirstOrDefault(p => p.ProviderId == song.Album.ProviderId);

                if (album != null)
                    song.Album = album;
                else
                {
                    await _service.InsertAsync(song.Album);
                    Albums.Add(song.Album);
                    song.Album.Songs = new List<Song>();
                    song.Artist.Albums.Add(song.Album);
                }
            }

            song.AlbumId = song.Album.Id;

            #endregion

            #region Download artwork

            if (artworkUrl != null)
            {
//Use the album if one is available
                var filePath = string.Format(CollectionConstant.ArtworkPath, song.Album.Id);

                //Check if the album artwork has already been downloaded
                var artworkExists = await StorageHelper.FileExistsAsync(filePath);

                if (!artworkExists)
                {
                    try
                    {
                        using (var client = new HttpClient())
                        {
                            using (var stream = await client.GetStreamAsync(artworkUrl))
                            {
                                using (
                                    var fileStream =
                                        await
                                            (await StorageHelper.CreateFileAsync(filePath)).OpenStreamForWriteAsync()
                                    )
                                {
                                    await stream.CopyToAsync(fileStream);
                                    //now set it
                                    song.Album.Artwork = new BitmapImage(new Uri(CollectionConstant.LocalStorageAppPath + filePath));
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Some shit happened saving the artwork, here: " + e);
                    }
                }
            }

            if (song.Album.Artwork == null)
                song.Album.Artwork = CollectionConstant.MissingArtworkImage;

            #endregion

            //Insert to db
            await _service.InsertAsync(song);

            song.Artist.Songs.Add(song);
            song.Album.Songs.Add(song);

            Songs.Add(song);
        }

        public async Task DeleteSongAsync(Song song)
        {
            // remove it from artist and albums songs
            var artist = Artists.FirstOrDefault(p => p.Songs.Contains(song));
            var album = Albums.FirstOrDefault(p => p.Songs.Contains(song));

            var deleteArtwork = false;
            if (album != null)
            {
                album.Songs.Remove(song);
                if (album.Songs.Count == 0)
                {
                    await _service.DeleteItemAsync(album);
                    Albums.Remove(album);
                    deleteArtwork = true;
                }
            }

            if (artist != null)
            {
                artist.Songs.Remove(song);
                if (artist.Songs.Count == 0)
                {
                    await _service.DeleteItemAsync(artist);
                    Artists.Remove(artist);
                }
            }

            //good, now lets delete it from the db
            await _service.DeleteItemAsync(song);

            Songs.Remove(song);
        }
    }
}

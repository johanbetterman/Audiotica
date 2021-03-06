﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Audiotica.Core.Exceptions;
using Audiotica.Core.Utilities;
using Audiotica.Data.Model.Deezer;
using Audiotica.Data.Service.Interfaces;

namespace Audiotica.Data.Service.RunTime
{
    public class DeezerService : IDeezerService
    {
        private const string BaseApi = "http://api.deezer.com/";
        private const string SearchQuery = "?q={0}&index={1}&limit={2}";
        private const string SearchPath = BaseApi + "search";
        private const string SearchTracksPath = SearchPath + SearchQuery;
        private const string SearchArtistPath = SearchPath + "/artist" + SearchQuery;
        private const string SearchAlbumsPath = SearchPath + "/album" + SearchQuery;
        private const string ArtistTopTrackList = BaseApi + "artist/{0}/top?index={1}&limit={2}";
        private const string AlbumTrackList = BaseApi + "album/{0}/tracks";


        public Task<DeezerPageResponse<DeezerSong>> GetArtistTopSongsAsync(int id, int page = 0, int limit = 10)
        {
            return GetAsync<DeezerPageResponse<DeezerSong>>(string.Format(ArtistTopTrackList, id, page, limit));
        }

        public Task<DeezerPageResponse<DeezerSong>> GetAlbumTrackListAsync(int id)
        {
            return GetAsync<DeezerPageResponse<DeezerSong>>(string.Format(AlbumTrackList, id));
        }

        public Task<DeezerPageResponse<DeezerArtist>> SearchArtistsAsync(string query, int page = 0, int limit = 10)
        {
            return GetAsync<DeezerPageResponse<DeezerArtist>>(string.Format(SearchArtistPath, Uri.EscapeDataString(query), page, limit));
        }

        public Task<DeezerPageResponse<DeezerSong>> SearchSongsAsync(string query, int page = 0, int limit = 10)
        {
            return GetAsync<DeezerPageResponse<DeezerSong>>(string.Format(SearchTracksPath, Uri.EscapeDataString(query), page, limit));
        }

        public Task<DeezerPageResponse<DeezerAlbum>> SearchAlbumsAsync(string query, int page = 0, int limit = 10)
        {
            return GetAsync<DeezerPageResponse<DeezerAlbum>>(string.Format(SearchAlbumsPath, Uri.EscapeDataString(query), page, limit));
        }

        private async Task<T> GetAsync<T>(string url) where T : DeezerBaseResponse
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(url);
                var json = await resp.Content.ReadAsStringAsync();
                var parseResp = await json.DeserializeAsync<T>();

                if (parseResp.error != null)
                    throw new Exception(parseResp.error.message);
                if (!resp.IsSuccessStatusCode)
                    throw new NetworkException();

                return parseResp;
            }
        }
    }
}

﻿#region

using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace Audiotica.Data.Spotify.Models
{
    public class ChartTrack
    {
        public string date { get; set; }
        public string country { get; set; }

        public string track_id
        {
            get { return track_url.Replace("https://play.spotify.com/track/", ""); }
        }

        public string track_url { get; set; }

        [JsonProperty(PropertyName = "track_name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "artist_name")]
        public string ArtistName { get; set; }

        public string artist_url { get; set; }
        public string album_name { get; set; }
        public string album_url { get; set; }

        [JsonProperty(PropertyName = "artwork_url")]
        public string ArtworkUrl { get; set; }
        public int num_streams { get; set; }
        public string window_type { get; set; }
        public int percent_male { get; set; }
        public int percent_age_group_0_17 { get; set; }
        public int percent_age_group_18_24 { get; set; }
        public int percent_age_group_25_29 { get; set; }
        public int percent_age_group_30_34 { get; set; }
        public int percent_age_group_35_44 { get; set; }
        public int percent_age_group_45_54 { get; set; }
        public int percent_age_group_55_plus { get; set; }
    }

    public class SpotifyChartsRoot
    {
        public List<ChartTrack> tracks { get; set; }
        public string prevDate { get; set; }
    }
}
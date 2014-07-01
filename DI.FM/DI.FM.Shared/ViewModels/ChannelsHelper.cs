using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DI.FM.ViewModel
{
    public class ChannelsHelper
    {
        public const string TRACK_HISTORY_URL = "http://api.v2.audioaddict.com/v1/di/track_history/channel/{0}.json";
        public const string BATCH_UPDATE_URL = "http://api.audioaddict.com/v1/di/mobile/batch_update?stream_set_key={0}";
        public const string BATCH_USER = "ephemeron";
        public const string BATCH_PASS = "dayeiph0ne@pp";

        public static Dictionary<string, string[]> ChannelsAssets = new Dictionary<string, string[]>
        {
            {"ambient", new string[]{"ms-appx:///Assets/Channels/channel23.png", "#0f3645", "#3da5cc"}},
            {"bigroomhouse", new string[]{"ms-appx:///Assets/Channels/bigroomhouse.png", "#837873", "#ff5a1b"}},
            {"breaks", new string[]{"ms-appx:///Assets/Channels/channel28.png", "#9e3b09", "#ef580c"}},
            {"chillhop", new string[]{"ms-appx:///Assets/Channels/chillhop.png", "#01b4dd", "#0072aa"}},
            {"chillout", new string[]{"ms-appx:///Assets/Channels/chillout.png", "#01b4dd", "#0072aa"}},
            {"chilloutdreams", new string[]{"ms-appx:///Assets/Channels/channel38.png", "#e23d1e", "#ef9f00"}},
            {"chiptunes", new string[]{"ms-appx:///Assets/Channels/channel51.png", "#083147", "#4a96b0"}},
            {"classiceurodance", new string[]{"ms-appx:///Assets/Channels/channel40.png", "#180a0a", "#de1f25"}},
            {"classiceurodisco", new string[]{"ms-appx:///Assets/Channels/classiceurodisco.png", "#123c69", "#700c6b"}},
            {"classictechno", new string[]{"ms-appx:///Assets/Channels/channel25.png", "#451e26", "#687871"}},
            {"classictrance", new string[]{"ms-appx:///Assets/Channels/channel43.png", "#aeafb2", "#5f6062"}},
            {"classicvocaltrance", new string[]{"ms-appx:///Assets/Channels/channel44.png", "#833507", "#d36c03"}},
            {"club", new string[]{"ms-appx:///Assets/Channels/channel42.png", "#433c38", "#919091"}},
            {"clubdubstep", new string[]{"ms-appx:///Assets/Channels/channel46.png", "#002496", "#019f7e"}},
            {"cosmicdowntempo", new string[]{"ms-appx:///Assets/Channels/channel29.png", "#296150", "#8fbf8d"}},
            {"darkdnb", new string[]{"ms-appx:///Assets/Channels/darkdnb.png", "#380e01", "#aa350b"}},
            {"deephouse", new string[]{"ms-appx:///Assets/Channels/channel32.png", "#41001e", "#e90069"}},
            {"deepnudisco", new string[]{"ms-appx:///Assets/Channels/channel35.png", "#ffd85e", "#ff916e"}},
            {"deeptech", new string[]{"ms-appx:///Assets/Channels/deeptech.png", "#3a3a3a", "#c66a33"}},
            {"discohouse", new string[]{"ms-appx:///Assets/Channels/channel47.png", "#e9579e", "#010303"}},
            {"djmixes", new string[]{"ms-appx:///Assets/Channels/channel22.png", "#00313f", "#1fb6e8"}},
            {"downtempolounge", new string[]{"ms-appx:///Assets/Channels/downtempolounge.png", "#01b4dd", "#0072aa"}},
            {"drumandbass", new string[]{"ms-appx:///Assets/Channels/channel24.png", "#006262", "#434343"}},
            {"dubstep", new string[]{"ms-appx:///Assets/Channels/channel45.png", "#bf1600", "#590b00"}},
            {"eclectronica", new string[]{"ms-appx:///Assets/Channels/eclectronica.png", "#e3ba00", "#557000"}},
            {"electro", new string[]{"ms-appx:///Assets/Channels/electro.png", "#da9fbb", "#c575b0"}},
            {"epictrance", new string[]{"ms-appx:///Assets/Channels/channel26.png", "#1a8c03", "#00387b"}},
            {"eurodance", new string[]{"ms-appx:///Assets/Channels/eurodance.png", "#fe6a00", "#ffeb00"}},
            {"funkyhouse", new string[]{"ms-appx:///Assets/Channels/channel34.png", "#f1a1ba", "#e52e7e"}},
            {"futuresynthpop", new string[]{"ms-appx:///Assets/Channels/channel48.png", "#e66c1f", "#fcb016"}},
            {"glitchhop", new string[]{"ms-appx:///Assets/Channels/glitchhop.png", "#4ea1a1", "#00191a"}},
            {"goapsy", new string[]{"ms-appx:///Assets/Channels/goapsy.png", "#d67c0b", "#2ee3df"}},
            {"handsup", new string[]{"ms-appx:///Assets/Channels/channel41.png", "#5a164f", "#c01b7b"}},
            {"hardcore", new string[]{"ms-appx:///Assets/Channels/channel21.png", "#40a2c7", "#77c6eb"}},
            {"harddance", new string[]{"ms-appx:///Assets/Channels/harddance.png", "#e25300", "#fed362"}},
            {"hardstyle", new string[]{"ms-appx:///Assets/Channels/channel37.png", "#a20013", "#da0019"}},
            {"house", new string[]{"ms-appx:///Assets/Channels/house.png", "#ff3f00", "#ffa300"}},
            {"latinhouse", new string[]{"ms-appx:///Assets/Channels/channel49.png", "#da117c", "#f5b92b"}},
            {"liquiddnb", new string[]{"ms-appx:///Assets/Channels/channel39.png", "#49c6f3", "#004881"}},
            {"liquiddubstep", new string[]{"ms-appx:///Assets/Channels/liquiddubstep.png", "#2f203a", "#3874ab"}},
            {"lounge", new string[]{"ms-appx:///Assets/Channels/lounge.png", "#009fc6", "#422d14"}},
            {"mainstage", new string[]{"ms-appx:///Assets/Channels/mainstage.png", "#336d84", "#d00001"}},
            {"minimal", new string[]{"ms-appx:///Assets/Channels/minimal.png", "#8da639", "#e2dd46"}},
            {"oldschoolacid", new string[]{"ms-appx:///Assets/Channels/channel50.png", "#226576", "#f8c000"}},
            {"progressive", new string[]{"ms-appx:///Assets/Channels/progressive.png", "#221238", "#7f0e98"}},
            {"progressivepsy", new string[]{"ms-appx:///Assets/Channels/channel20.png", "#032d46", "#5cc6dc"}},
            {"psychill", new string[]{"ms-appx:///Assets/Channels/psychill.png", "#1bbea1", "#3b31ee"}},
            {"russianclubhits", new string[]{"ms-appx:///Assets/Channels/russianclubhits.png", "#f43937", "#b41f73"}},
            {"sankeys", new string[]{"ms-appx:///Assets/Channels/sankeysradio.png", "#01b4dd", "#0072aa"}},
            {"soulfulhouse", new string[]{"ms-appx:///Assets/Channels/channel31.png", "#003244", "#36b0d7"}},
            {"spacemusic", new string[]{"ms-appx:///Assets/Channels/channel36.png", "#003a94", "#5aa8d6"}},
            {"techhouse", new string[]{"ms-appx:///Assets/Channels/techhouse.png", "#ffa100", "#a10e00"}},
            {"techno", new string[]{"ms-appx:///Assets/Channels/channel30.png", "#007bc1", "#6dcaf2"}},
            {"trance", new string[]{"ms-appx:///Assets/Channels/trance.png", "#22a0d2", "#0071ae"}},
            {"trap", new string[]{"ms-appx:///Assets/Channels/trap.png", "#01b4dd", "#0072aa"}},
            {"tribalhouse", new string[]{"ms-appx:///Assets/Channels/channel33.png", "#3f0f0f", "#faa919"}},
            {"ukgarage", new string[]{"ms-appx:///Assets/Channels/channel27.png", "#1a3db9", "#f70b0b"}},
            {"umfradio", new string[]{"ms-appx:///Assets/Channels/umfradio.png", "#120f22", "#4f3f88"}},
            {"vocalchillout", new string[]{"ms-appx:///Assets/Channels/vocalchillout.png", "#d06a00", "#24797e"}},
            {"vocaltrance", new string[]{"ms-appx:///Assets/Channels/vocaltrance.png", "#94ac2e", "#e03176"}},
        };

        public static string[][] FreeStreamFormats = new string[][]
        {
            new string[] {"MP3 96kbps", "public3"},
            //new string[] {"WMA 40kbps", "public5"}, - Not working mms:// protocol
            new string[] {"AAC 40kbps", "public2"},
        };

        public static string[][] PremiumStreamFormats = new string[][]
        {
            new string[] {"MP3 256kbps", "premium_high"},
            new string[] {"AAC 128kbps", "premium"},
            new string[] {"AAC 64kbps", "premium_medium"},
            new string[] {"AAC 40kbps", "premium_low"},
            //new string[] {"WMA 128kbps", "premium_wma"}, - Not working mms:// protocol
            //new string[] {"WMA 64kbps", "premium_wma_low"}, - Not working mms:// protocol
        };

        public static async Task<string> DownloadJson(string url)
        {
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            try { return await client.GetStringAsync(url); }
            catch { return null; }
        }
    }
}

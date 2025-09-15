using System.Collections.Generic;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.DirectShow.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public PluginConfiguration()
        {
            Channels = new List<ChannelConfig>();
            FFmpegPath = "ffmpeg";
            DefaultVideoBitrate = 2000000;
            DefaultAudioBitrate = 128000;
            DefaultResolution = "1280x720";
            DefaultFramerate = 30;
        }

        public List<ChannelConfig> Channels { get; set; }
        public string FFmpegPath { get; set; }
        public int DefaultVideoBitrate { get; set; }
        public int DefaultAudioBitrate { get; set; }
        public string DefaultResolution { get; set; }
        public int DefaultFramerate { get; set; }
    }

    public class ChannelConfig
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string VideoDevice { get; set; }
        public string AudioDevice { get; set; }
        public string VideoCodec { get; set; } = "libx264";
        public string AudioCodec { get; set; } = "aac";
        public int VideoBitrate { get; set; } = 2000000;
        public int AudioBitrate { get; set; } = 128000;
        public string Resolution { get; set; } = "1280x720";
        public int Framerate { get; set; } = 30;
        public bool IsEnabled { get; set; } = true;
    }
}
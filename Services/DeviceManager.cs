using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DirectShow.Services
{
    public class DeviceManager
    {
        private readonly ILogger<DeviceManager> _logger;

        public DeviceManager(ILogger<DeviceManager> logger)
        {
            _logger = logger;
        }

        public async Task<List<string>> GetDirectShowVideoDevices()
        {
            return await GetDirectShowDevices("video");
        }

        public async Task<List<string>> GetDirectShowAudioDevices()
        {
            return await GetDirectShowDevices("audio");
        }

        private async Task<List<string>> GetDirectShowDevices(string deviceType)
        {
            var devices = new List<string>();

            try
            {
                // استخدام FFmpeg للحصول على قائمة أجهزة DirectShow
                var ffmpegPath = "ffmpeg"; // أو المسار الكامل إذا كان في مكان محدد

                var args = $"-list_devices true -f dshow -i dummy -hide_banner";

                using (var process = new Process())
                {
                    process.StartInfo.FileName = ffmpegPath;
                    process.StartInfo.Arguments = args;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;

                    process.Start();
                    var output = await process.StandardError.ReadToEndAsync();
                    process.WaitForExit(5000);

                    ParseDevicesFromOutput(output, deviceType, devices);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting DirectShow devices");
            }

            return devices;
        }

        private void ParseDevicesFromOutput(string output, string deviceType, List<string> devices)
        {
            var lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var isInCorrectSection = false;

            foreach (var line in lines)
            {
                if (line.Contains("DirectShow video devices"))
                {
                    isInCorrectSection = deviceType == "video";
                    continue;
                }

                if (line.Contains("DirectShow audio devices"))
                {
                    isInCorrectSection = deviceType == "audio";
                    continue;
                }

                if (isInCorrectSection && line.Contains("\""))
                {
                    var startIndex = line.IndexOf('"') + 1;
                    var endIndex = line.IndexOf('"', startIndex);
                    if (endIndex > startIndex)
                    {
                        var deviceName = line.Substring(startIndex, endIndex - startIndex);
                        if (!devices.Contains(deviceName))
                        {
                            devices.Add(deviceName);
                        }
                    }
                }
            }
        }
    }
}
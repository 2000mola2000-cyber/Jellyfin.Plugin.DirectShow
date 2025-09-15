using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DirectShow.Services
{
    public class FFmpegManager
    {
        private readonly ILogger<FFmpegManager> _logger;
        private Process _ffmpegProcess;

        public FFmpegManager(ILogger<FFmpegManager> logger)
        {
            _logger = logger;
        }

        public async Task<bool> StartStreaming(ChannelConfig config, string outputUrl, CancellationToken cancellationToken)
        {
            try
            {
                var ffmpegPath = "ffmpeg"; // يمكن جلب هذا من الإعدادات

                var videoDevice = $"video=\"{config.VideoDevice}\"";
                var audioDevice = $"audio=\"{config.AudioDevice}\"";

                var args = $"-f dshow -i {videoDevice}:{audioDevice} " +
                           $"-vcodec {config.VideoCodec} -b:v {config.VideoBitrate} " +
                           $"-acodec {config.AudioCodec} -b:a {config.AudioBitrate} " +
                           $"-s {config.Resolution} -r {config.Framerate} " +
                           $"-f mpegts \"{outputUrl}\"";

                _logger.LogInformation($"Starting FFmpeg with args: {args}");

                _ffmpegProcess = new Process();
                _ffmpegProcess.StartInfo.FileName = ffmpegPath;
                _ffmpegProcess.StartInfo.Arguments = args;
                _ffmpegProcess.StartInfo.UseShellExecute = false;
                _ffmpegProcess.StartInfo.RedirectStandardOutput = true;
                _ffmpegProcess.StartInfo.RedirectStandardError = true;
                _ffmpegProcess.StartInfo.CreateNoWindow = true;

                _ffmpegProcess.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        _logger.LogInformation($"FFmpeg output: {e.Data}");
                };

                _ffmpegProcess.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        _logger.LogInformation($"FFmpeg error: {e.Data}");
                };

                _ffmpegProcess.Start();
                _ffmpegProcess.BeginOutputReadLine();
                _ffmpegProcess.BeginErrorReadLine();

                // الانتظار حتى الإلغاء أو انتهاء العملية
                await Task.Run(() =>
                {
                    while (!cancellationToken.IsCancellationRequested && !_ffmpegProcess.HasExited)
                    {
                        Thread.Sleep(1000);
                    }

                    if (!_ffmpegProcess.HasExited)
                    {
                        _ffmpegProcess.Kill();
                    }
                }, cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting FFmpeg stream");
                return false;
            }
        }

        public void StopStreaming()
        {
            try
            {
                if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
                {
                    _ffmpegProcess.Kill();
                    _ffmpegProcess = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping FFmpeg stream");
            }
        }
    }
}
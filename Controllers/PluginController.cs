using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.DirectShow.Configuration;
using Jellyfin.Plugin.DirectShow.Services;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DirectShow.Controllers
{
    [ApiController]
    [Route("DirectShow")]
    public class PluginController : ControllerBase
    {
        private readonly ILogger<PluginController> _logger;
        private readonly DeviceManager _deviceManager;
        private readonly StreamingService _streamingService;
        private readonly ILibraryManager _libraryManager;

        public PluginController(
            ILogger<PluginController> logger,
            DeviceManager deviceManager,
            StreamingService streamingService,
            ILibraryManager libraryManager)
        {
            _logger = logger;
            _deviceManager = deviceManager;
            _streamingService = streamingService;
            _libraryManager = libraryManager;
        }

        [HttpGet("Devices/Video")]
        public async Task<ActionResult<List<string>>> GetVideoDevices()
        {
            try
            {
                var devices = await _deviceManager.GetDirectShowVideoDevices();
                return Ok(devices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting video devices");
                return StatusCode(500, "Error getting video devices");
            }
        }

        [HttpGet("Devices/Audio")]
        public async Task<ActionResult<List<string>>> GetAudioDevices()
        {
            try
            {
                var devices = await _deviceManager.GetDirectShowAudioDevices();
                return Ok(devices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audio devices");
                return StatusCode(500, "Error getting audio devices");
            }
        }

        [HttpPost("Channels")]
        public ActionResult AddChannel([FromBody] ChannelConfig config)
        {
            try
            {
                var plugin = Plugin.Instance;
                config.Id = Guid.NewGuid().ToString();
                plugin.Configuration.Channels.Add(config);
                plugin.SaveConfiguration();

                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding channel");
                return StatusCode(500, "Error adding channel");
            }
        }

        [HttpPut("Channels/{id}")]
        public ActionResult UpdateChannel(string id, [FromBody] ChannelConfig config)
        {
            try
            {
                var plugin = Plugin.Instance;
                var existingConfig = plugin.Configuration.Channels.Find(c => c.Id == id);
                if (existingConfig != null)
                {
                    plugin.Configuration.Channels.Remove(existingConfig);
                    plugin.Configuration.Channels.Add(config);
                    plugin.SaveConfiguration();

                    return Ok(config);
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating channel");
                return StatusCode(500, "Error updating channel");
            }
        }

        [HttpDelete("Channels/{id}")]
        public ActionResult DeleteChannel(string id)
        {
            try
            {
                var plugin = Plugin.Instance;
                var config = plugin.Configuration.Channels.Find(c => c.Id == id);
                if (config != null)
                {
                    plugin.Configuration.Channels.Remove(config);
                    plugin.SaveConfiguration();

                    return Ok();
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting channel");
                return StatusCode(500, "Error deleting channel");
            }
        }

        [HttpGet("Channels")]
        public ActionResult<List<ChannelConfig>> GetChannels()
        {
            try
            {
                var plugin = Plugin.Instance;
                return Ok(plugin.Configuration.Channels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting channels");
                return StatusCode(500, "Error getting channels");
            }
        }

        [HttpPost("Stream/Start/{channelId}")]
        public async Task<ActionResult> StartStream(string channelId)
        {
            try
            {
                var plugin = Plugin.Instance;
                var config = plugin.Configuration.Channels.Find(c => c.Id == channelId);
                if (config == null)
                {
                    return NotFound("Channel not found");
                }

                var streamUrl = $"http://localhost:8096/LiveTv/LiveStreamFiles/{channelId}/stream.ts";
                var cancellationTokenSource = new CancellationTokenSource();

                var success = await _streamingService.StartStreaming(config, streamUrl, cancellationTokenSource.Token);
                if (success)
                {
                    return Ok(new { StreamUrl = streamUrl });
                }

                return StatusCode(500, "Failed to start stream");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting stream");
                return StatusCode(500, "Error starting stream");
            }
        }

        [HttpPost("Stream/Stop/{channelId}")]
        public ActionResult StopStream(string channelId)
        {
            try
            {
                _streamingService.StopStreaming(channelId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping stream");
                return StatusCode(500, "Error stopping stream");
            }
        }
    }
}
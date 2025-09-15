using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;
using Jellyfin.Plugin.DirectShow.Configuration;

namespace Jellyfin.Plugin.DirectShow
{
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        public static Plugin Instance { get; private set; }

        private readonly ILogger<Plugin> _logger;
        private readonly IApplicationPaths _applicationPaths;

        public override string Name => "DirectShow Tuner";
        public override Guid Id => Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        public Plugin(
            IApplicationPaths applicationPaths,
            IXmlSerializer xmlSerializer,
            ILogger<Plugin> logger)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
            _applicationPaths = applicationPaths;
            _logger = logger;

            // إنشاء مجلدات التخزين إذا لم تكن موجودة
            EnsureDirectoriesExist();
        }

        private void EnsureDirectoriesExist()
        {
            try
            {
                var configPath = Path.Combine(_applicationPaths.PluginConfigurationsPath, "DirectShow");
                if (!Directory.Exists(configPath))
                {
                    Directory.CreateDirectory(configPath);
                }

                var logPath = Path.Combine(_applicationPaths.LogDirectoryPath, "DirectShow");
                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating plugin directories");
            }
        }

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "directshow",
                    EmbeddedResourcePath = GetType().Namespace + ".Web.configuration.html",
                    EnableInMainMenu = true,
                    DisplayName = "DirectShow Tuner"
                },
                new PluginPageInfo
                {
                    Name = "directshowjs",
                    EmbeddedResourcePath = GetType().Namespace + ".Web.configuration.js"
                }
            };
        }
    }
}
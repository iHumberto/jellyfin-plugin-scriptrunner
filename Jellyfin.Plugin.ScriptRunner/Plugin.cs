using System;
using System.Collections.Generic;
using Jellyfin.Plugin.ScriptRunner.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.ScriptRunner;

public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public static Plugin? Instance { get; private set; }

    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    public override string Name => "ScriptRunner";
    public override Guid Id => Guid.Parse("b7c9e1d2-1234-4abc-9def-123456789abd");
    public override string Description => "Gerencia e executa scripts quando a biblioteca é atualizada.";

    // Registra a página de configuração na dashboard
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
            new PluginPageInfo
            {
                Name = "ScriptRunner",
                EmbeddedResourcePath = $"{GetType().Namespace}.Configuration.config.html"
            }
        };
    }
}
using System.Collections.Generic;
using MediaBrowser.Model.Plugins;
using Jellyfin.Plugin.ScriptRunner.Configuration;

namespace Jellyfin.Plugin.ScriptRunner.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>Diretório onde os scripts .sh serão salvos</summary>
    public string ScriptsDirectory { get; set; } = "/config/plugins/ScriptRunner/scripts";

    public List<ScriptEntry> Scripts { get; set; } = new();
}
using System.Collections.Generic;
using System.Xml.Serialization;
using MediaBrowser.Model.Plugins;
using Jellyfin.Plugin.ScriptRunner.Configuration;

namespace Jellyfin.Plugin.ScriptRunner.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    public string ScriptsDirectory { get; set; } = "/config/plugins/scripts";

    [XmlArray("Scripts")]
    [XmlArrayItem("ScriptEntry")]
    public List<ScriptEntry> Scripts { get; set; } = new();
}
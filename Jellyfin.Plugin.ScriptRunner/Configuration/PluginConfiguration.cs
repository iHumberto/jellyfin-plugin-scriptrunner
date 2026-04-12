using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.ScriptRunner.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    public string ScriptPath { get; set; } = "/jellyfin/id_updater.sh";
    public string ScriptArguments { get; set; } = string.Empty;
    public int DebounceSeconds { get; set; } = 30;
    public bool TriggerOnItemAdded { get; set; } = true;
    public bool TriggerOnItemUpdated { get; set; } = false;
}
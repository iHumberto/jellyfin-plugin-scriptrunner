using System;

namespace Jellyfin.Plugin.ScriptRunner.Configuration;

public class ScriptEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Nome do arquivo, ex: "id_updater" (sem .sh)</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Conteúdo completo do script (shebang + comandos)</summary>
    public string Content { get; set; } = string.Empty;

    public int DebounceSeconds { get; set; } = 30;
    public bool TriggerOnItemAdded { get; set; } = true;
    public bool TriggerOnItemUpdated { get; set; } = false;
}
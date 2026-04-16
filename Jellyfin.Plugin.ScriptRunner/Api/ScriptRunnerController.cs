using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Jellyfin.Plugin.ScriptRunner.Configuration;

namespace Jellyfin.Plugin.ScriptRunner.Api;

[ApiController]
[Route("ScriptRunner")]
[Authorize(Policy = "RequiresElevation")]
public class ScriptRunnerController : ControllerBase
{
    private readonly ILogger<ScriptRunnerController> _logger;

    public ScriptRunnerController(ILogger<ScriptRunnerController> logger)
    {
        _logger = logger;
    }

    private static Plugin Plg => Plugin.Instance!;

    // ─── GET /ScriptRunner/scripts ────────────────────────────────
    [HttpGet("scripts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<ScriptEntry>> GetScripts()
    {
        var config = Plg.Configuration;
        var dir = config.ScriptsDirectory;

        if (Directory.Exists(dir))
        {
            var diskFiles = Directory.GetFiles(dir, "*.sh");
            bool dirty = false;

            // Importa arquivos do disco que não estão na config
            foreach (var filePath in diskFiles)
            {
                var scriptName = Path.GetFileNameWithoutExtension(filePath);

                var existing = config.Scripts.FirstOrDefault(s => s.Name == scriptName);
                if (existing is null)
                {
                    config.Scripts.Add(new ScriptEntry
                    {
                        Id = Guid.NewGuid(),
                        Name = scriptName,
                        Content = System.IO.File.ReadAllText(filePath),
                        DebounceSeconds = 30,
                        TriggerOnItemAdded = true,
                        TriggerOnItemUpdated = false
                    });
                    dirty = true;
                    _logger.LogInformation("[ScriptRunner] Script re-importado do disco: {File}", filePath);
                }
            }

            // Remove entradas órfãs (config aponta para .sh que não existe mais)
            var toRemove = config.Scripts
                .Where(s => !System.IO.File.Exists(Path.Combine(dir, s.Name + ".sh")))
                .ToList();

            foreach (var orphan in toRemove)
            {
                config.Scripts.Remove(orphan);
                dirty = true;
                _logger.LogInformation("[ScriptRunner] Entrada órfã removida da config: {Name}", orphan.Name);
            }

            if (dirty)
                Plg.SaveConfiguration();
        }

        return Ok(config.Scripts);
    }

    // ─── POST /ScriptRunner/scripts ───────────────────────────────
    [HttpPost("scripts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult SaveScript([FromBody] ScriptEntry entry)
    {
        if (string.IsNullOrWhiteSpace(entry.Name))
            return BadRequest("Nome do script é obrigatório.");

        // Sanitiza o nome (apenas alnum + _ + -)
        var safeName = System.Text.RegularExpressions.Regex
            .Replace(entry.Name.Trim(), @"[^\w\-]", "_");

        var dir = Plg.Configuration.ScriptsDirectory;
        Directory.CreateDirectory(dir);

        var filePath = Path.Combine(dir, safeName + ".sh");

        // Escreve o arquivo físico
        System.IO.File.WriteAllText(filePath, entry.Content);

        // Garante permissão de execução
        try
        {
            var chmod = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "chmod",
                Arguments = $"+x \"{filePath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            System.Diagnostics.Process.Start(chmod)?.WaitForExit();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[ScriptRunner] Não foi possível definir chmod +x em {Path}", filePath);
        }

        // Atualiza ou insere na lista de configuração
        var existing = Plg.Configuration.Scripts
            .FirstOrDefault(s => s.Id == entry.Id);

        if (existing is not null)
        {
            existing.Name = safeName;
            existing.Content = entry.Content;
            existing.DebounceSeconds = entry.DebounceSeconds;
            existing.TriggerOnItemAdded = entry.TriggerOnItemAdded;
            existing.TriggerOnItemUpdated = entry.TriggerOnItemUpdated;
        }
        else
        {
            entry.Id = Guid.NewGuid();
            entry.Name = safeName;
            Plg.Configuration.Scripts.Add(entry);
        }

        Plg.SaveConfiguration();

        _logger.LogInformation("[ScriptRunner] Script salvo: {File}", filePath);
        return Ok(new { filePath, entry.Id });
    }

    // ─── DELETE /ScriptRunner/scripts/{id} ────────────────────────
    [HttpDelete("scripts/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteScript(Guid id)
    {
        var entry = Plg.Configuration.Scripts.FirstOrDefault(s => s.Id == id);
        if (entry is null)
            return NotFound();

        var filePath = Path.Combine(
            Plg.Configuration.ScriptsDirectory,
            entry.Name + ".sh");

        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
            _logger.LogInformation("[ScriptRunner] Script excluído: {File}", filePath);
        }

        Plg.Configuration.Scripts.Remove(entry);
        Plg.SaveConfiguration();

        return NoContent();
    }
}
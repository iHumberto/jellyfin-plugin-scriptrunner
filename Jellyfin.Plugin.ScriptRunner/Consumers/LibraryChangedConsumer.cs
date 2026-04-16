using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ScriptRunner.Consumers;

public class LibraryChangedConsumer : IHostedService
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<LibraryChangedConsumer> _logger;

    // Um timer por script (chave = ScriptEntry.Id)
    private readonly Dictionary<Guid, Timer> _timers = new();
    private readonly object _timerLock = new();

    public LibraryChangedConsumer(
        ILibraryManager libraryManager,
        ILogger<LibraryChangedConsumer> logger)
    {
        _libraryManager = libraryManager;
        _logger = logger;
    }

    // ── Ciclo de vida ────────────────────────────────────────────────

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _libraryManager.ItemAdded   += OnItemAdded;
        _libraryManager.ItemUpdated += OnItemUpdated;

        _logger.LogInformation("[ScriptRunner] Plugin v2 iniciado. Scripts cadastrados: {Count}",
            Plugin.Instance?.Configuration.Scripts.Count ?? 0);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _libraryManager.ItemAdded   -= OnItemAdded;
        _libraryManager.ItemUpdated -= OnItemUpdated;

        // Descarta todos os timers pendentes
        lock (_timerLock)
        {
            foreach (var timer in _timers.Values)
                timer.Dispose();

            _timers.Clear();
        }

        return Task.CompletedTask;
    }

    // ── Handlers de evento ───────────────────────────────────────────

    private void OnItemAdded(object? sender, ItemChangeEventArgs e)
        => HandleEvent(e, triggerType: "Added");

    private void OnItemUpdated(object? sender, ItemChangeEventArgs e)
        => HandleEvent(e, triggerType: "Updated");

    private void HandleEvent(ItemChangeEventArgs e, string triggerType)
    {
        if (e.Item is not (Movie or Series))
            return;

        var config = Plugin.Instance?.Configuration;
        if (config is null || config.Scripts.Count == 0)
            return;

        foreach (var script in config.Scripts)
        {
            // Verifica se este script deve reagir ao tipo de evento
            bool shouldRun = triggerType == "Added"   && script.TriggerOnItemAdded
                          || triggerType == "Updated" && script.TriggerOnItemUpdated;

            if (!shouldRun)
                continue;

            var filePath = Path.Combine(config.ScriptsDirectory, script.Name + ".sh");

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("[ScriptRunner] Arquivo não encontrado: {Path}", filePath);
                continue;
            }

            _logger.LogDebug(
                "[ScriptRunner] [{Script}] Item {Type}: '{Item}'. Agendando em {Sec}s.",
                script.Name, triggerType, e.Item.Name, script.DebounceSeconds);

            ScheduleScript(script.Id, script.Name, filePath, script.DebounceSeconds);
        }
    }

    // ── Debounce por script ──────────────────────────────────────────

    private void ScheduleScript(Guid scriptId, string scriptName, string filePath, int debounceSeconds)
    {
        lock (_timerLock)
        {
            // Cancela o timer anterior deste script (se houver)
            if (_timers.TryGetValue(scriptId, out var existing))
            {
                existing.Dispose();
                _timers.Remove(scriptId);
            }

            // Cria novo timer — dispara uma única vez após o debounce
            _timers[scriptId] = new Timer(
                _ => ExecuteScript(scriptId, scriptName, filePath),
                state: null,
                dueTime: TimeSpan.FromSeconds(debounceSeconds),
                period: Timeout.InfiniteTimeSpan);
        }
    }

    // ── Execução do script ───────────────────────────────────────────

    private void ExecuteScript(Guid scriptId, string scriptName, string filePath)
    {
        // Remove o timer da lista (já disparou)
        lock (_timerLock)
        {
            if (_timers.TryGetValue(scriptId, out var t))
            {
                t.Dispose();
                _timers.Remove(scriptId);
            }
        }

        _logger.LogInformation("[ScriptRunner] [{Script}] Executando: {Path}", scriptName, filePath);

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName               = "/bin/bash",
                Arguments              = $"-c \"{filePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true
            };

            using var process = Process.Start(psi);

            if (process is null)
            {
                _logger.LogError("[ScriptRunner] [{Script}] Falha ao iniciar processo.", scriptName);
                return;
            }

            var output = process.StandardOutput.ReadToEnd();
            var error  = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrWhiteSpace(output))
                _logger.LogInformation("[ScriptRunner] [{Script}] stdout: {Output}", scriptName, output);

            if (process.ExitCode != 0)
                _logger.LogError("[ScriptRunner] [{Script}] Saiu com código {Code}: {Error}",
                    scriptName, process.ExitCode, error);
            else
                _logger.LogInformation("[ScriptRunner] [{Script}] Concluído com sucesso.", scriptName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ScriptRunner] [{Script}] Exceção durante execução.", scriptName);
        }
    }
}
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.ScriptRunner.Configuration;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ScriptRunner.Consumers;

public class LibraryChangedConsumer : IHostedService
{
    private const string ConfigPath = "/config/plugins/ScriptRunner/config.json";

    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<LibraryChangedConsumer> _logger;

    private Timer? _debounceTimer;
    private readonly object _timerLock = new();
    private bool _scriptQueued = false;

    public LibraryChangedConsumer(
        ILibraryManager libraryManager,
        ILogger<LibraryChangedConsumer> logger)
    {
        _libraryManager = libraryManager;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var config = LoadConfig();

        if (config.TriggerOnItemAdded)
            _libraryManager.ItemAdded += OnItemChanged;

        if (config.TriggerOnItemUpdated)
            _libraryManager.ItemUpdated += OnItemChanged;

        _logger.LogInformation("[ScriptRunner] Plugin iniciado. Script: {Path}", config.ScriptPath);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _libraryManager.ItemAdded -= OnItemChanged;
        _libraryManager.ItemUpdated -= OnItemChanged;
        _debounceTimer?.Dispose();
        return Task.CompletedTask;
    }

    private void OnItemChanged(object? sender, ItemChangeEventArgs e)
    {
        if (e.Item is not (Movie or Series))
            return;

        var config = LoadConfig();
        if (string.IsNullOrWhiteSpace(config.ScriptPath))
        {
            _logger.LogWarning("[ScriptRunner] ScriptPath vazio no config.json.");
            return;
        }

        _logger.LogDebug("[ScriptRunner] Item alterado: {Name}. Debounce: {Sec}s.", e.Item.Name, config.DebounceSeconds);

        lock (_timerLock)
        {
            _scriptQueued = true;
            _debounceTimer?.Dispose();
            _debounceTimer = new Timer(
                _ => ExecuteScript(),
                null,
                TimeSpan.FromSeconds(config.DebounceSeconds),
                Timeout.InfiniteTimeSpan);
        }
    }

    private void ExecuteScript()
    {
        lock (_timerLock)
        {
            if (!_scriptQueued)
                return;

            _scriptQueued = false;
        }

        var config = LoadConfig();

        try
        {
            _logger.LogInformation("[ScriptRunner] Executando: {Path} {Args}", config.ScriptPath, config.ScriptArguments);

            var psi = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{config.ScriptPath} {config.ScriptArguments}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                _logger.LogError("[ScriptRunner] Falha ao iniciar processo.");
                return;
            }

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrWhiteSpace(output))
                _logger.LogInformation("[ScriptRunner] Output: {Output}", output);

            if (process.ExitCode != 0)
                _logger.LogError("[ScriptRunner] Erro (exit {Code}): {Error}", process.ExitCode, error);
            else
                _logger.LogInformation("[ScriptRunner] Script concluído com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ScriptRunner] Exceção ao executar o script.");
        }
    }

    private PluginConfiguration LoadConfig()
    {
        try
        {
            if (!File.Exists(ConfigPath))
            {
                _logger.LogWarning("[ScriptRunner] config.json não encontrado em {Path}. Usando defaults.", ConfigPath);
                return new PluginConfiguration();
            }

            var json = File.ReadAllText(ConfigPath);
            var config = JsonSerializer.Deserialize<PluginConfiguration>(json);

            return config ?? new PluginConfiguration();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ScriptRunner] Erro ao ler config.json. Usando defaults.");
            return new PluginConfiguration();
        }
    }
}
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MyBet.Infrastructure.Ollama
{
    public sealed class OllamaLifecycle : IAsyncDisposable, IDisposable
    {
        private readonly HttpClient _http;
        private readonly OllamaOptions _options;

        private Process? _process;
        private bool _modelLoaded;
        private bool _disposed;

        public OllamaLifecycle(HttpClient http, IOptions<OllamaOptions> options)
        {
            _http = http;
            _options = options.Value;
        }

        public async Task StartAsync(CancellationToken ct = default)
        {
            if (!_options.ManageLifecycle) return;

            if (await IsServerUpAsync(ct))
            {
                Console.WriteLine("Ollama is already running (will be left running).");
            }
            else
            {
                Console.WriteLine("Starting Ollama...");
                StartServerProcess();
                await WaitForServerAsync(ct);
            }

            await EnsureModelInstalledAsync(ct);

            Console.WriteLine($"Loading model {_options.Model}...");
            await LoadModelAsync(ct);
            Console.WriteLine("Ollama ready.");
        }

        public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            if (!_options.ManageLifecycle) return;

            try
            {
                if (_process is { HasExited: false })
                {
                    Console.WriteLine("Stopping Ollama...");
                    _process.Kill(entireProcessTree: true);
                    await _process.WaitForExitAsync(new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);
                }
                else if (_modelLoaded)
                {
                    Console.WriteLine("Unloading model...");
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    await _http.PostAsJsonAsync("/api/generate",
                        new { model = _options.Model, keep_alive = 0 }, cts.Token);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ollama cleanup failed: {ex.Message}");
            }
            finally
            {
                _process?.Dispose();
                _process = null;
            }
        }

        private async Task<bool> IsServerUpAsync(CancellationToken ct)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(2));
                using var response = await _http.GetAsync("/api/tags", cts.Token);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException) { return false; }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested) { return false; }
        }

        private void StartServerProcess()
        {
            var exe = ResolveExecutable()
                ?? throw new FileNotFoundException(
                    "Could not find the Ollama executable. Install Ollama or set Ollama:ExecutablePath in appsettings.json.");

            var psi = new ProcessStartInfo(exe, "serve")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var uri = new Uri(_options.BaseUrl);
            psi.Environment["OLLAMA_HOST"] = $"{uri.Host}:{uri.Port}";

            _process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start the Ollama process.");

            _process.OutputDataReceived += (_, _) => { };
            _process.ErrorDataReceived += (_, _) => { };
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
        }

        private string? ResolveExecutable()
        {
            if (!string.IsNullOrWhiteSpace(_options.ExecutablePath))
                return File.Exists(_options.ExecutablePath) ? _options.ExecutablePath : null;

            var names = OperatingSystem.IsWindows() ? new[] { "ollama.exe" } : new[] { "ollama" };
            var pathDirs = (Environment.GetEnvironmentVariable("PATH") ?? "")
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

            foreach (var dir in pathDirs)
                foreach (var name in names)
                {
                    var candidate = Path.Combine(dir.Trim('"'), name);
                    if (File.Exists(candidate)) return candidate;
                }

            if (OperatingSystem.IsWindows())
            {
                var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var installed = Path.Combine(local, "Programs", "Ollama", "ollama.exe");
                if (File.Exists(installed)) return installed;
            }

            return null;
        }

        private async Task WaitForServerAsync(CancellationToken ct)
        {
            var deadline = DateTime.UtcNow.AddSeconds(_options.StartupTimeoutSeconds);

            while (DateTime.UtcNow < deadline)
            {
                if (_process is { HasExited: true })
                    throw new InvalidOperationException($"Ollama exited during startup (exit code {_process.ExitCode}).");

                if (await IsServerUpAsync(ct)) return;
                await Task.Delay(500, ct);
            }

            throw new TimeoutException($"Ollama did not respond within {_options.StartupTimeoutSeconds} seconds.");
        }

        private async Task EnsureModelInstalledAsync(CancellationToken ct)
        {
            using var response = await _http.GetAsync("/api/tags", ct);
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
            var wanted = _options.Model;
            var wantedLatest = wanted.Contains(':') ? wanted : wanted + ":latest";

            var installed = doc.RootElement.TryGetProperty("models", out var models)
                && models.EnumerateArray().Any(m =>
                {
                    var name = m.GetProperty("name").GetString();
                    return name == wanted || name == wantedLatest;
                });

            if (!installed)
                throw new InvalidOperationException(
                    $"Model '{wanted}' is not installed. Run: ollama pull {wanted}");
        }

        private async Task LoadModelAsync(CancellationToken ct)
        {
            using var response = await _http.PostAsJsonAsync("/api/generate", new
            {
                model = _options.Model,
                keep_alive = _options.KeepAlive,
                options = new { num_ctx = _options.NumCtx }
            }, ct);

            response.EnsureSuccessStatusCode();
            _modelLoaded = true;
        }
    }
}

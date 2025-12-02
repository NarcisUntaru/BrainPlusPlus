using System.Diagnostics;
using System.Text;
using BrainPlusPlus.Server.Models;
using BrainPlusPlus.Server.Services.Interfaces;

namespace BrainPlusPlus.Server.Services;

public class ProcessService : Interfaces.IProcessService
{
    private const int TimeoutSeconds = 5;

    public async Task<ProcessResult> RunProcessAsync(string fileName, string arguments, string workingDirectory, string? input = null)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = !string.IsNullOrEmpty(input),
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processStartInfo };
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                outputBuilder.AppendLine(e.Data);
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                errorBuilder.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        if (!string.IsNullOrEmpty(input))
        {
            await process.StandardInput.WriteAsync(input);
            process.StandardInput.Close();
        }

        var completed = await process.WaitForExitAsync(TimeSpan.FromSeconds(TimeoutSeconds));
        
        if (!completed)
        {
            process.Kill(true);
            return new ProcessResult
            {
                ExitCode = -1,
                Output = outputBuilder.ToString(),
                Error = "Execution timed out"
            };
        }

        return new ProcessResult
        {
            ExitCode = process.ExitCode,
            Output = outputBuilder.ToString(),
            Error = errorBuilder.ToString()
        };
    }
}

internal static class ProcessExtensions
{
    public static async Task<bool> WaitForExitAsync(this Process process, TimeSpan timeout)
    {
        return await Task.Run(() => process.WaitForExit((int)timeout.TotalMilliseconds));
    }
}
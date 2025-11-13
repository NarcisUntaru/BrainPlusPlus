using Brain__.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Brain__.Services
{

    public class CppCompilerService : ICppCompilerService
    {
        private readonly ILogger<CppCompilerService> _logger;
        private readonly string _workingDirectory;

        public CppCompilerService(ILogger<CppCompilerService> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _workingDirectory = Path.Combine(env.ContentRootPath, "CppWorkspace");
            Directory.CreateDirectory(_workingDirectory);
        }

        public async Task<CompilationResult> CompileAndRunAsync(string cppCode, string input = "")
        {
            var result = new CompilationResult();
            var fileName = $"program_{Guid.NewGuid():N}";
            var sourceFile = Path.Combine(_workingDirectory, $"{fileName}.cpp");
            var outputFile = Path.Combine(_workingDirectory, fileName);

            try
            {
                // Write C++ code to file
                await File.WriteAllTextAsync(sourceFile, cppCode);

                // Compile
                var compileResult = await RunProcessAsync("g++", $"-std=c++17 -o \"{outputFile}\" \"{sourceFile}\"");
                
                if (compileResult.ExitCode != 0)
                {
                    result.Success = false;
                    result.Error = compileResult.Error;
                    return result;
                }

                if (!OperatingSystem.IsWindows())
                {
                    var chmodResult = await RunProcessAsync("chmod", $"+x \"{outputFile}\"");
                    if (chmodResult.ExitCode != 0)
                    {
                        _logger.LogWarning("Failed to chmod executable: {Error}", chmodResult.Error);
                    }
                }

                // Run the compiled program
                var runResult = await RunProcessAsync(outputFile, "", input);
                result.Success = runResult.ExitCode == 0;
                result.Output = runResult.Output;
                result.Error = runResult.Error;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error compiling/running C++ code");
                result.Success = false;
                result.Error = ex.Message;
                return result;
            }
            finally
            {
                // Cleanup
                try
                {
                    if (File.Exists(sourceFile)) File.Delete(sourceFile);
                    if (File.Exists(outputFile)) File.Delete(outputFile);
                }
                catch { }
            }
        }

        private async Task<ProcessResult> RunProcessAsync(string fileName, string arguments, string input = "")
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = !string.IsNullOrEmpty(input),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = _workingDirectory
                }
            };

            var output = new StringBuilder();
            var error = new StringBuilder();

            process.OutputDataReceived += (sender, e) => { if (e.Data != null) output.AppendLine(e.Data); };
            process.ErrorDataReceived += (sender, e) => { if (e.Data != null) error.AppendLine(e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (!string.IsNullOrEmpty(input))
            {
                await process.StandardInput.WriteAsync(input);
                process.StandardInput.Close();
            }

            await process.WaitForExitAsync();

            return new ProcessResult
            {
                ExitCode = process.ExitCode,
                Output = output.ToString(),
                Error = error.ToString()
            };
        }
    }

    public class CompilationResult
    {
        public bool Success { get; set; }
        public string Output { get; set; } = "";
        public string Error { get; set; } = "";
    }

    public class ProcessResult
    {
        public int ExitCode { get; set; }
        public string Output { get; set; } = "";
        public string Error { get; set; } = "";
    }
}
using Microsoft.AspNetCore.Mvc;
using BrainPlusPlus.Server.Models;
using BrainPlusPlus.Server.Services.Interfaces;

namespace BrainPlusPlus.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompilerController : ControllerBase
{
    private readonly ILogger<CompilerController> _logger;
    private readonly IProcessService _processExecutor;

    public CompilerController(ILogger<CompilerController> logger, IProcessService processExecutor)
    {
        _logger = logger;
        _processExecutor = processExecutor;
    }

    [HttpPost("execute")]
    public async Task<ActionResult<CompileResult>> ExecuteCode([FromBody] CompileRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest(new CompileResult 
            { 
                Success = false, 
                Error = "Code cannot be empty" 
            });
        }

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var sourceFile = Path.Combine(tempDir, "main.cpp");
            var outputFile = Path.Combine(tempDir, "program");

            // Write source code to file
            await System.IO.File.WriteAllTextAsync(sourceFile, request.Code);

            // Compile
            var compileResult = await _processExecutor.RunProcessAsync("g++", $"{sourceFile} -o {outputFile}", tempDir);
            
            if (compileResult.ExitCode != 0)
            {
                return Ok(new CompileResult
                {
                    Success = false,
                    Error = compileResult.Error,
                    CompilationOutput = compileResult.Output
                });
            }

            // Execute
            var executeResult = await _processExecutor.RunProcessAsync(outputFile, "", tempDir, request.Input);

            return Ok(new CompileResult
            {
                Success = executeResult.ExitCode == 0,
                Output = executeResult.Output,
                Error = executeResult.Error,
                ExitCode = executeResult.ExitCode
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing C++ code");
            return Ok(new CompileResult
            {
                Success = false,
                Error = $"Internal error: {ex.Message}"
            });
        }
        finally
        {
            // Cleanup
            try
            {
                Directory.Delete(tempDir, true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cleanup temp directory: {TempDir}", tempDir);
            }
        }
    }
}
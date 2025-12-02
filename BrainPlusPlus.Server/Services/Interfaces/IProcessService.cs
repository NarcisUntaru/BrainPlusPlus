using BrainPlusPlus.Server.Models;

namespace BrainPlusPlus.Server.Services.Interfaces;

public interface IProcessService
{
    Task<ProcessResult> RunProcessAsync(string fileName, string arguments, string workingDirectory, string? input = null);
}
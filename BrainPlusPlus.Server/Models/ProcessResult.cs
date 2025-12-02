namespace BrainPlusPlus.Server.Models;

public record ProcessResult
{
    public int ExitCode { get; init; }
    public string Output { get; init; } = string.Empty;
    public string Error { get; init; } = string.Empty;
}
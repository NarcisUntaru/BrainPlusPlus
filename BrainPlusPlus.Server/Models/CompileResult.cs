namespace BrainPlusPlus.Server.Models;

public record CompileResult
{
    public bool Success { get; init; }
    public string? Output { get; init; }
    public string? Error { get; init; }
    public string? CompilationOutput { get; init; }
    public int? ExitCode { get; init; }
}
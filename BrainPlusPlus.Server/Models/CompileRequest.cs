namespace BrainPlusPlus.Server.Models;

public record CompileRequest
{
    public string Code { get; init; } = string.Empty;
    public string? Input { get; init; }
}
using Brain__.Services;
using System.Threading.Tasks;

namespace Brain__.Services.Interfaces
{
    public interface ICppCompilerService
    {
        Task<CompilationResult> CompileAndRunAsync(string cppCode, string input = "");
    }
}

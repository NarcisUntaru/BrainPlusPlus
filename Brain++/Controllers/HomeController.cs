using System.Diagnostics;
using System.Threading.Tasks;
using Brain__.Models;
using Brain__.Services;
using Brain__.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Brain__.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICppCompilerService _cppCompiler;

        public HomeController(ILogger<HomeController> logger, ICppCompilerService cppCompiler)
        {
            _logger = logger;
            _cppCompiler = cppCompiler;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CompileCpp([FromBody] CppCodeRequest request)
        {
            var result = await _cppCompiler.CompileAndRunAsync(request.Code, request.Input);
            return Json(result);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class CppCodeRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Input { get; set; } = string.Empty;
    }
}

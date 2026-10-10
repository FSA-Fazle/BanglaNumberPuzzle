using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BanglaNumberPuzzle.Controllers
{
    public class TtsController : Controller
    {
        private static readonly HttpClient _http = new();
        private readonly IMemoryCache _cache;

        static TtsController()
        {
            _http.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }

        public TtsController(IMemoryCache cache) => _cache = cache;

        [HttpGet]
        public async Task<IActionResult> Index(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return BadRequest();

            if (_cache.TryGetValue(text, out byte[]? cached))
                return File(cached!, "audio/mpeg");

            var url = $"https://translate.google.com/translate_tts?ie=UTF-8&q={Uri.EscapeDataString(text)}&tl=bn&client=tw-ob&ttsspeed=0.9";
            try
            {
                var bytes = await _http.GetByteArrayAsync(url);
                _cache.Set(text, bytes, TimeSpan.FromHours(24));
                return File(bytes, "audio/mpeg");
            }
            catch
            {
                return NoContent();
            }
        }
    }
}

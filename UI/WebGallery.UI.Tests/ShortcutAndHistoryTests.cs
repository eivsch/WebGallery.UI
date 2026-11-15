using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Infrastructure.Common;
using Infrastructure.MinimalApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.WebUtilities;
using WebGallery.UI.Controllers;

namespace WebGallery.UI.Tests;

public class ShortcutAndHistoryTests
{
    [Fact]
    public async Task Unwatched_shortcut_requests_one_random_long_video_and_opens_its_player()
    {
        using var api = new RecordingApi
        {
            Body = new[] { new SearchHitDTO { AlbumName = "a", MediaItem = new() { Id = "chosen-id" } } }
        };

        var result = Assert.IsType<RedirectResult>(
            await api.Home().RandomBio("mp4", minFileSize: 104857600, unwatchedOnly: true));

        Assert.Equal("/bio/id/chosen-id", result.Url);
        var request = Assert.Single(api.Requests);
        Assert.Equal("/users/tester/search", request.AbsolutePath);
        var query = QueryHelpers.ParseQuery(request.Query);
        Assert.Equal("mp4", query["fileExtensions"]);
        Assert.Equal("104857600", query["minFileSize"]);
        Assert.Equal("1", query["maxResults"]);
        Assert.Equal("true", query["random"]);
        Assert.Equal("true", query["unwatchedOnly"]);
        Assert.False(query.ContainsKey("tags"));
    }

    [Fact]
    public async Task Existing_random_shortcuts_keep_their_search_behavior()
    {
        using var api = new RecordingApi
        {
            Body = new[] { new SearchHitDTO { AlbumName = "a", MediaItem = new() { Id = "short-id" } } }
        };

        var result = Assert.IsType<RedirectResult>(
            await api.Home().RandomBio("mp4", maxFileSize: 104857599));

        Assert.Equal("/bio/id/short-id", result.Url);
        var query = QueryHelpers.ParseQuery(Assert.Single(api.Requests).Query);
        Assert.Equal("1000", query["maxResults"]);
        Assert.Equal("104857599", query["maxFileSize"]);
        Assert.False(query.ContainsKey("random"));
        Assert.False(query.ContainsKey("unwatchedOnly"));
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.NoContent)]
    public async Task Empty_unwatched_search_returns_home_with_an_explicit_message(HttpStatusCode status)
    {
        using var api = new RecordingApi { Status = status };
        var controller = api.Home();

        var result = Assert.IsType<RedirectToActionResult>(
            await controller.RandomBio("mp4", minFileSize: 104857600, unwatchedOnly: true));

        Assert.Equal("Index", result.ActionName);
        Assert.Equal("No unwatched videos match these filters.", controller.TempData["HomeMessage"]);
    }

    [Fact]
    public async Task Failed_search_is_not_reported_as_no_unwatched_videos()
    {
        using var api = new RecordingApi { Status = HttpStatusCode.InternalServerError };
        var controller = api.Home();

        await Assert.ThrowsAsync<Exception>(() => controller.RandomBio("mp4", unwatchedOnly: true));

        Assert.False(controller.TempData.ContainsKey("HomeMessage"));
    }

    [Theory]
    [InlineData(0, 51, false, true)]
    [InlineData(500, 51, true, true)]
    [InlineData(1000, 50, true, false)]
    [InlineData(1050, 0, true, false)]
    public async Task History_pages_are_bounded_but_have_no_total_500_entry_ceiling(
        int from, int count, bool hasPrevious, bool hasMore)
    {
        using var api = new RecordingApi
        {
            Body = Enumerable.Range(from, count)
                .Select(i => new WatchHistoryItemDTO { MediaId = $"v{i}" }).ToList()
        };
        var controller = new HistoryController(api.Proxy, new UsernameResolver(api.Accessor));

        var result = Assert.IsType<ViewResult>(await controller.Index(from));
        var items = Assert.IsType<List<WatchHistoryItemDTO>>(result.Model);

        Assert.Equal(Math.Min(count, 50), items.Count);
        if (count > 0) Assert.Equal($"v{from}", items[0].MediaId);
        Assert.Equal(hasPrevious, (bool)controller.ViewBag.HasPrevious);
        Assert.Equal(hasMore, (bool)controller.ViewBag.HasMore);
        Assert.Equal(Math.Max(0, from - 50), (int)controller.ViewBag.PreviousFrom);
        Assert.Equal(from + 50, (int)controller.ViewBag.NextFrom);
        var query = QueryHelpers.ParseQuery(Assert.Single(api.Requests).Query);
        Assert.Equal(from.ToString(), query["from"]);
        Assert.Equal("51", query["size"]);
    }

    sealed class RecordingApi : HttpMessageHandler
    {
        readonly HttpClient _client;
        public readonly HttpContextAccessor Accessor;
        public readonly MinimalApiProxy Proxy;
        public readonly List<Uri> Requests = [];
        public object Body = Array.Empty<SearchHitDTO>();
        public HttpStatusCode Status = HttpStatusCode.OK;

        public RecordingApi()
        {
            Accessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Sid, "tester")], "test"))
                }
            };
            _client = new HttpClient(this, disposeHandler: false) { BaseAddress = new Uri("http://api.test/") };
            Proxy = new MinimalApiProxy(new WebGalleryApiClient(_client, Accessor));
        }

        public HomeController Home() => new(Proxy, Accessor)
        {
            TempData = new TempDataDictionary(Accessor.HttpContext, new EmptyTempDataProvider())
        };

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request.RequestUri);
            return Task.FromResult(new HttpResponseMessage(Status)
            {
                Content = new StringContent(JsonSerializer.Serialize(Body, JsonSerializerOptions.Web), Encoding.UTF8, "application/json")
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _client.Dispose();
            base.Dispose(disposing);
        }
    }

    sealed class EmptyTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}

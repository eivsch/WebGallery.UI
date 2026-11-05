using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Infrastructure.Common;
using Infrastructure.MinimalApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using WebGallery.UI.Controllers;
using WebGallery.UI.ViewModels.Bio;

namespace WebGallery.UI.Tests;

public class BioSourceVideoTests
{
    [Fact]
    public async Task Initial_and_ajax_screencaps_have_the_same_encoded_source_link_beyond_first_page()
    {
        using var catalog = new Catalog("trip & snow");
        catalog.SourceItems.AddRange(Enumerable.Range(0, 205).Select(i => Media($"other{i}.mp4")));
        catalog.SourceItems.Add(Media("clip.mp4"));
        var controller = catalog.Controller();

        var view = Assert.IsType<ViewResult>(await controller.Index(catalog.ScreencapAlbum, 0));
        var initial = Assert.IsType<BioViewModel>(view.Model).BioPictureViewModel;
        var partial = Assert.IsType<PartialViewResult>(await controller.Switch(catalog.ScreencapAlbum, 0));
        var switched = Assert.IsType<BioPictureViewModel>(partial.Model);

        Assert.Equal("/Bio/trip%20%26%20snow/205?start=62.345", initial.SourceVideoUrl);
        Assert.Equal("00:01:02.345", initial.SourceVideoTimestamp);
        Assert.Equal(initial.SourceVideoUrl, switched.SourceVideoUrl);
        Assert.Contains(catalog.Requests, r => r.Contains("from=200"));
    }

    [Fact]
    public async Task Id_based_bio_also_resolves_the_source()
    {
        using var catalog = new Catalog();
        catalog.SourceItems.Add(Media("clip.mp4"));

        var result = Assert.IsType<ViewResult>(await catalog.Controller().Index("screencap-id"));
        var model = Assert.IsType<BioViewModel>(result.Model);

        Assert.Equal("/Bio/trip/0?start=62.345", model.BioPictureViewModel.SourceVideoUrl);
    }

    [Fact]
    public async Task Source_video_endpoint_resolves_redirect_only_when_requested()
    {
        using var catalog = new Catalog("trip & snow");
        catalog.SourceItems.AddRange(Enumerable.Range(0, 205).Select(i => Media($"other{i}.mp4")));
        catalog.SourceItems.Add(Media("clip.mp4"));

        var result = Assert.IsType<RedirectResult>(
            await catalog.Controller().OpenSourceVideo(catalog.ScreencapAlbum, catalog.Picture.Name));

        Assert.Equal("/Bio/trip%20%26%20snow/205?start=62.345", result.Url);
        Assert.Contains(catalog.Requests, r => r.Contains("from=200"));
    }

    [Fact]
    public async Task Source_video_endpoint_rejects_names_that_do_not_match_screencap_pattern()
    {
        using var catalog = new Catalog();

        var result = await catalog.Controller().OpenSourceVideo(catalog.ScreencapAlbum, "not-a-screencap.jpg");

        Assert.IsType<NotFoundResult>(result);
        Assert.Empty(catalog.Requests);
    }

    [Fact]
    public async Task Duplicate_on_a_later_page_is_not_treated_as_an_unambiguous_source()
    {
        using var catalog = new Catalog();
        catalog.SourceItems.Add(Media("clip.mp4"));
        catalog.SourceItems.AddRange(Enumerable.Range(0, 205).Select(i => Media($"other{i}.mp4")));
        catalog.SourceItems.Add(Media("clip.mp4"));

        var result = Assert.IsType<PartialViewResult>(await catalog.Controller().Switch(catalog.ScreencapAlbum, 0));

        Assert.Null(Assert.IsType<BioPictureViewModel>(result.Model).SourceVideoUrl);
        Assert.Contains(catalog.Requests, r => r.Contains("from=200"));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Missing_or_ambiguous_source_has_no_link(bool albumExists, bool duplicate)
    {
        using var catalog = new Catalog { SourceAlbumExists = albumExists };
        if (duplicate) catalog.SourceItems.AddRange([Media("clip.mp4"), Media("CLIP.MP4")]);
        else catalog.SourceItems.Add(Media("clip.mp4.backup.mp4"));

        var result = Assert.IsType<PartialViewResult>(await catalog.Controller().Switch(catalog.ScreencapAlbum, 0));
        Assert.Null(Assert.IsType<BioPictureViewModel>(result.Model).SourceVideoUrl);
    }

    [Fact]
    public async Task Ordinary_image_never_queries_the_source_album()
    {
        using var catalog = new Catalog();
        catalog.Picture.Tags.Clear();

        var result = Assert.IsType<PartialViewResult>(await catalog.Controller().Switch(catalog.ScreencapAlbum, 0));

        Assert.Null(Assert.IsType<BioPictureViewModel>(result.Model).SourceVideoUrl);
        Assert.Single(catalog.Requests);
    }

    [Fact]
    public async Task Source_api_failure_is_not_hidden()
    {
        using var catalog = new Catalog { SourceStatus = HttpStatusCode.InternalServerError };
        await Assert.ThrowsAsync<Exception>(() => catalog.Controller().Switch(catalog.ScreencapAlbum, 0));
    }

    [Theory]
    [InlineData("0", 0.0)]
    [InlineData("62.345", 62.345)]
    [InlineData("-1", null)]
    [InlineData("NaN", null)]
    [InlineData("Infinity", null)]
    [InlineData("1,5", null)]
    [InlineData("bad", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public async Task Explicit_position_is_validated_invariantly_and_only_applied_to_videos(string start, double? expected)
    {
        using var catalog = new Catalog();
        catalog.Picture = Media("clip.MP4");
        var oldCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("nb-NO");
            var result = Assert.IsType<ViewResult>(await catalog.Controller().Index(catalog.ScreencapAlbum, 0, start));
            Assert.Equal(expected, Assert.IsType<BioViewModel>(result.Model).BioPictureViewModel.StartPositionSeconds);

            catalog.Picture = Media("image.jpg");
            var image = Assert.IsType<ViewResult>(await catalog.Controller().Index(catalog.ScreencapAlbum, 0, start));
            Assert.Null(Assert.IsType<BioViewModel>(image.Model).BioPictureViewModel.StartPositionSeconds);
        }
        finally
        {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    [Fact]
    public async Task Overflowed_position_is_ignored()
    {
        using var catalog = new Catalog();
        catalog.Picture = Media("clip.mp4");
        var result = Assert.IsType<ViewResult>(await catalog.Controller().Index(catalog.ScreencapAlbum, 0, new string('9', 400)));
        Assert.Null(Assert.IsType<BioViewModel>(result.Model).BioPictureViewModel.StartPositionSeconds);
    }

    private static MediaDTO Media(string name) => new() { Id = name, Name = name, Tags = [] };

    private sealed class Catalog : HttpMessageHandler
    {
        private readonly HttpClient _client;
        private readonly HttpContextAccessor _accessor;
        public readonly string Album;
        public string ScreencapAlbum => Album + "_video_images";
        public bool SourceAlbumExists = true;
        public HttpStatusCode SourceStatus = HttpStatusCode.OK;
        public MediaDTO Picture = new() { Id = "screencap-id", Name = "clip_000102.345.jpg", Tags = [new() { TagName = "screencap" }] };
        public List<MediaDTO> SourceItems = [];
        public List<string> Requests = [];

        public Catalog(string album = "trip")
        {
            Album = album;
            _client = new HttpClient(this, disposeHandler: false) { BaseAddress = new Uri("http://gallery.test") };
            _accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
            _accessor.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Sid, "tester")]));
        }

        public BioController Controller() => new(new MinimalApiProxy(new WebGalleryApiClient(_client, _accessor)), null, new UsernameResolver(_accessor));

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request.RequestUri.PathAndQuery);
            string path = Uri.UnescapeDataString(request.RequestUri.AbsolutePath);
            var query = QueryHelpers.ParseQuery(request.RequestUri.Query);
            int from = query.TryGetValue("from", out var fromValue) ? int.Parse(fromValue.ToString()) : 0;
            int size = query.TryGetValue("size", out var sizeValue) ? int.Parse(sizeValue.ToString()) : 200;
            object body;
            var status = HttpStatusCode.OK;
            if (path == "/users/tester/albums")
            {
                var albums = new List<AlbumMetaDTO> { new() { AlbumName = ScreencapAlbum, Tags = [] } };
                if (SourceAlbumExists) albums.Add(new() { AlbumName = Album, Tags = [] });
                // A similar album must not substitute for the exact source album.
                albums.Add(new() { AlbumName = Album + "-backup", Tags = [] });
                body = new { TotalCount = albums.Count, Albums = albums.Skip(from).Take(size).ToList() };
            }
            else if (path == "/users/tester/search")
                body = new[] { new SearchHitDTO { AlbumName = ScreencapAlbum, MediaAlbumIndex = 0, MediaItem = Picture } };
            else if (path == "/users/tester/albums/" + ScreencapAlbum)
                body = new AlbumContentsDTO { TotalCount = 1, Items = [Picture] };
            else if (path == "/users/tester/albums/" + Album)
            {
                body = new AlbumContentsDTO { TotalCount = SourceItems.Count, Items = SourceItems.Skip(from).Take(size).ToList() };
                status = SourceStatus;
            }
            else throw new InvalidOperationException("Unexpected API request: " + request.RequestUri);

            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(JsonSerializer.Serialize(body, JsonSerializerOptions.Web), Encoding.UTF8, "application/json")
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _client.Dispose();
            base.Dispose(disposing);
        }
    }
}

using WebGallery.UI.Helpers;

namespace WebGallery.UI.Tests;

public class ScreencapSourceTests
{
    [Theory]
    [InlineData("clip_000000.000.jpg", "clip.mp4", 0, "00:00:00.000")]
    [InlineData("clip_with.dots_010203.456.jpg", "clip_with.dots.mp4", 3723.456, "01:02:03.456")]
    [InlineData("clip_1000203.004.jpg", "clip.mp4", 360123.004, "100:02:03.004")]
    [InlineData("clip_235959.999.JPG", "clip.mp4", 86399.999, "23:59:59.999")]
    public void Parses_generated_names(string name, string videoName, double position, string timestamp)
    {
        var source = ScreencapSource.Parse("trip_video_images_video_images", name, ["screencap"]);

        Assert.NotNull(source);
        Assert.Equal("trip_video_images", source.Album);
        Assert.Equal(videoName, source.VideoName);
        Assert.Equal(position, source.PositionSeconds, 6);
        Assert.Equal(timestamp, source.Timestamp);
    }

    [Theory]
    [InlineData("clip_006000.000.jpg")]
    [InlineData("clip_000060.000.jpg")]
    [InlineData("clip_00x000.000.jpg")]
    [InlineData("clip_000000.0x0.jpg")]
    [InlineData("clip_000000.00.jpg")]
    [InlineData("clip_000000.jpg")]
    [InlineData("clip_00000.000.jpg")]
    [InlineData("clip_-00000.000.jpg")]
    [InlineData("clip_000000.000.png")]
    [InlineData("_000000.000.jpg")]
    [InlineData("clip_000000.000.jpg\n")]
    [InlineData("")]
    [InlineData(null)]
    public void Rejects_malformed_or_unsupported_names(string name)
    {
        Assert.Null(ScreencapSource.Parse("trip_video_images", name, ["screencap"]));
    }

    [Theory]
    [InlineData("trip", "screencap")]
    [InlineData("trip_video_images_backup", "screencap")]
    [InlineData("_video_images", "screencap")]
    [InlineData("trip_video_images", "holiday")]
    [InlineData(null, "screencap")]
    public void Does_not_infer_relationships_from_ordinary_images(string album, string tag)
    {
        Assert.Null(ScreencapSource.Parse(album, "clip_000001.000.jpg", [tag]));
    }

    [Fact]
    public void Requires_tags_and_rejects_overflow()
    {
        Assert.Null(ScreencapSource.Parse("trip_video_images", "clip_000001.000.jpg", null));
        Assert.Null(ScreencapSource.Parse("trip_video_images", "clip_" + new string('9', 400) + "0000.000.jpg", ["screencap"]));
    }
}

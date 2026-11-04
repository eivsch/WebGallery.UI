using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Infrastructure.MinimalApi;

namespace WebGallery.UI.Helpers;

public record ScreencapSource(string Album, string VideoName, double PositionSeconds, string Timestamp)
{
    private const string AlbumSuffix = "_video_images";
    private static readonly Regex FilePattern = new(
        @"\A(?<stem>.+)_(?<hours>[0-9]{2,})(?<minutes>[0-5][0-9])(?<seconds>[0-5][0-9])\.(?<milliseconds>[0-9]{3})\.jpg\z",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static ScreencapSource Parse(string album, string name, IEnumerable<string> tags)
    {
        if (string.IsNullOrEmpty(album) || string.IsNullOrEmpty(name) ||
            !album.EndsWith(AlbumSuffix, StringComparison.OrdinalIgnoreCase) ||
            album.Length <= AlbumSuffix.Length ||
            tags?.Any(t => string.Equals(t, "screencap", StringComparison.OrdinalIgnoreCase)) != true)
            return null;

        Match match = FilePattern.Match(name);
        if (!match.Success ||
            !double.TryParse(match.Groups["hours"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out double hours))
            return null;

        int minutes = int.Parse(match.Groups["minutes"].Value, CultureInfo.InvariantCulture);
        int seconds = int.Parse(match.Groups["seconds"].Value, CultureInfo.InvariantCulture);
        int milliseconds = int.Parse(match.Groups["milliseconds"].Value, CultureInfo.InvariantCulture);
        double position = hours * 3600 + minutes * 60 + seconds + milliseconds / 1000.0;
        if (!double.IsFinite(position)) return null;

        string timestamp = FormattableString.Invariant($"{match.Groups["hours"].Value}:{minutes:00}:{seconds:00}.{milliseconds:000}");
        return new ScreencapSource(album[..^AlbumSuffix.Length], match.Groups["stem"].Value + ".mp4", position, timestamp);
    }

    public async Task<int?> FindVideoIndexAsync(MinimalApiProxy api, string username)
    {
        const int pageSize = 200;
        int from = 0;
        int? matchIndex = null;
        while (true)
        {
            AlbumContentsDTO page = await api.GetAlbumContents(username, Uri.EscapeDataString(Album), from, pageSize);
            if (page?.Items == null || page.Items.Count == 0) break;

            for (int i = 0; i < page.Items.Count; i++)
            {
                if (!string.Equals(page.Items[i].Name, VideoName, StringComparison.OrdinalIgnoreCase)) continue;
                if (matchIndex.HasValue) return null;
                matchIndex = from + i;
            }

            from += page.Items.Count;
            if (from >= page.TotalCount) break;
        }

        return matchIndex;
    }
}

using System.Globalization;
using System.Runtime.CompilerServices;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// A tag for each artwork object a fixture composes sheets from (#2604), the same tag every time it is asked for and a
/// different one for every other object: the fixtures keep the sheets an object composes by it, and tell the
/// installation's own artwork, whose sheets are the installed ones, by it. The import keeps no identity of the content
/// it composes sheets from, so an object is told apart as itself, never by what it holds.
/// </summary>
internal static class FixtureArtworkTags
{
    /// <summary>Each object's tag, given on its first use.</summary>
    private static readonly ConditionalWeakTable<object, string> tags = new();

    /// <summary>The number of the last tag given.</summary>
    private static long last;

    /// <summary>The tag of <paramref name="artwork"/>.</summary>
    internal static string Of(object artwork)
    {
        ArgumentNullException.ThrowIfNull(artwork);
        return tags.GetValue(artwork, _ => Interlocked.Increment(ref last).ToString("X", CultureInfo.InvariantCulture));
    }
}

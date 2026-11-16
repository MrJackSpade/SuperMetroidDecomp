using System.Collections.Frozen;
using System.Collections.Immutable;
using SuperMetroid.Core;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>Cartridge values and helpers of <see cref="MusicTrackNumbers"/> that only verification reads.</summary>
internal static class MusicTrackNumbersConstants
{
    /// <summary>The track numbers of the shared songs, which name the same song whichever data set is loaded.</summary>
    internal static IEnumerable<int> SharedTrackNumbers => MusicTrackNumbers.SharedNumbers.Values;
}

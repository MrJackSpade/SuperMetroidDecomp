using System.Collections.Frozen;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The key each enemy frame's picture has in the installed enemy sheets, in players' override sheets and in the
/// installed artwork's content identity (#1412). The sheets were first installed keyed by the frames' identity names,
/// which then spelled each frame's composition name in Pascal case (<c>draygon_oam_A2DF</c> was keyed
/// <c>DraygonOamA2DF</c>). The key stays derived from the composition name, the installed artwork document's own key, so
/// renaming a frame's identity never changes a key on disk.
/// </summary>
internal static class EnemyFrameSheetKeys
{
    /// <summary>Each frame's key.</summary>
    private static readonly FrozenDictionary<EnemyFrameId, string> Keys = Build();

    /// <summary>The installed sheet key of <paramref name="frame"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is not an installed frame.</exception>
    internal static string Of(EnemyFrameId frame) => Keys.TryGetValue(frame, out string? key)
        ? key
        : throw new ArgumentOutOfRangeException(nameof(frame), frame, "The frame has no installed composition.");

    /// <summary>Keys every installed composition, and the two shared empty frames by their own names.</summary>
    private static FrozenDictionary<EnemyFrameId, string> Build()
    {
        var keys = new Dictionary<EnemyFrameId, string>
        {
            [EnemyFrameId.Empty] = "Empty",
            [EnemyFrameId.EmptyExtended] = "EmptyExtended",
        };
        foreach (EnemySpritemapDefinition composition in EnemySpritemapCompositions.All.Concat(EnemyExtendedFrameCompositions.All))
            keys.Add(composition.Frame, PascalCase(composition.Name));
        return keys.ToFrozenDictionary();
    }

    /// <summary><paramref name="compositionName"/>'s underscore-separated words, each capitalized, joined.</summary>
    private static string PascalCase(string compositionName) => string.Concat(compositionName.Split('_')
        .Select(word => word.Length == 0 ? word : char.ToUpperInvariant(word[0]) + word[1..]));
}

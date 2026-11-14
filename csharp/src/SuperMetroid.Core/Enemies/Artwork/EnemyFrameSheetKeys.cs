using System.Collections.Frozen;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The key each enemy frame's picture has in the installed enemy sheets and in the
/// installed artwork's content identity (#1412): its document name (<see cref="EnemyFrameNameDefinitions"/>), which names
/// the frame for its owner and meaning (#1465), in Pascal case (<c>draygon_arms_facing_left_idle_first</c> is keyed
/// <c>"DraygonArmsFacingLeftIdleFirst"</c>). The key stays derived from the document name, the installed artwork document's own key, so renaming a frame's identity
/// never changes a key on disk.
/// </summary>
internal static class EnemyFrameSheetKeys
{
    /// <summary>Each frame's key.</summary>
    private static readonly FrozenDictionary<EnemyFrameId, string> Keys = Build();

    /// <summary>The installed sheet key of <paramref name="frame"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is not an installed frame.</exception>
    internal static string Of(EnemyFrameId frame) => Keys.TryGetValue(frame, out string? key)
        ? key
        : throw new ArgumentOutOfRangeException(nameof(frame), frame, "The frame is not installed.");

    /// <summary>Keys every installed frame by its document name, and the shared empty frame by its own name.</summary>
    private static FrozenDictionary<EnemyFrameId, string> Build()
    {
        var keys = new Dictionary<EnemyFrameId, string>
        {
            [EnemyFrameId.Empty] = "Empty",
        };
        foreach ((EnemyFrameId frame, string name) in EnemyFrameNameDefinitions.Names)
            keys.Add(frame, PascalCase(name));
        return keys.ToFrozenDictionary();
    }

    /// <summary><paramref name="compositionName"/>'s underscore-separated words, each capitalized, joined.</summary>
    private static string PascalCase(string compositionName) => string.Concat(compositionName.Split('_')
        .Select(segment => segment.Length == 0 ? segment : char.ToUpperInvariant(segment[0]) + segment[1..]));
}

using System.Text;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The current names of the pictures in a player's override enemy sheet written before enemy frames were named for their
/// owner and meaning (#1465). A picture's name holds its frame's sheet key, which then spelled where the cartridge stores
/// the frame, so each such older key in a picture's name becomes that frame's current key and the rest of the name stays.
/// </summary>
internal static class EnemySheetLegacyFrameNames
{
    /// <summary>Each older sheet key and the current key of its frame.</summary>
    private static readonly Lazy<Dictionary<string, string>> CurrentKeys = new(() =>
        EnemyFrameLegacyNameDefinitions.OrdinaryCompositions.Concat(EnemyFrameLegacyNameDefinitions.ExtendedCompositions)
            .ToDictionary(entry => PascalCase(entry.Value), entry => EnemyFrameSheetKeys.Of(entry.Key), StringComparer.Ordinal));

    /// <summary><paramref name="picture"/> with each older frame key in it replaced by that frame's current key.</summary>
    internal static string Current(string picture)
    {
        Ensure.NotNull(picture);
        var current = new StringBuilder(picture.Length);
        int start = 0;
        while (start < picture.Length)
        {
            bool word = char.IsAsciiLetterOrDigit(picture[start]);
            int end = start;
            while (end < picture.Length && char.IsAsciiLetterOrDigit(picture[end]) == word)
                end++;
            string run = picture[start..end];
            current.Append(word && CurrentKeys.Value.TryGetValue(run, out string? key) ? key : run);
            start = end;
        }
        return current.ToString();
    }

    /// <summary>An older composition name's underscore-separated words, each capitalized, joined: its older sheet key.</summary>
    private static string PascalCase(string name) => string.Concat(name.Split('_')
        .Select(word => word.Length == 0 ? word : char.ToUpperInvariant(word[0]) + word[1..]));
}

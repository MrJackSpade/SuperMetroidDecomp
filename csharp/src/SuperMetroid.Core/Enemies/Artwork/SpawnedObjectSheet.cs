using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The pictures a room graphics set draws one owner's spawned objects from (#1392, #1513), pre-coloured in the colours its
/// spawners draw in and in every shade they can show (#1512). Common art, such as an explosion, a drop or a projectile many
/// sets draw alike, is held once on a common sheet that every set drawing it reads; the set's own sheet holds only the
/// pictures particular to it. Each picture is found on exactly one of those sheets.
/// </summary>
public sealed class SpawnedObjectSheet
{
    /// <summary>Each picture the owner's objects draw, by name, from the sheet holding it.</summary>
    internal readonly Dictionary<string, SpriteSheetFrame> frames;

    /// <summary>
    /// The pictures of <paramref name="own"/>, the set's sheet of the pictures particular to it, and each picture
    /// <paramref name="common"/> names on the common sheet it names.
    /// </summary>
    /// <param name="own">The sheet of the pictures particular to the set; none when every picture is common.</param>
    /// <param name="common">The common sheet holding each of the owner's other pictures, by picture name.</param>
    /// <exception cref="InvalidDataException">A common sheet does not hold its picture, or a picture is on the set's own sheet too.</exception>
    public SpawnedObjectSheet(SpriteSheet? own, IReadOnlyDictionary<string, SpriteSheet> common)
    {
        Ensure.NotNull(common);
        frames = own is null ? new Dictionary<string, SpriteSheetFrame>(StringComparer.Ordinal)
            : new Dictionary<string, SpriteSheetFrame>(own.frames, StringComparer.Ordinal);
        var sheets = new List<SpriteSheet>();
        if (own is not null)
            sheets.Add(own);
        foreach ((string picture, SpriteSheet sheet) in common.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            if (!frames.TryAdd(picture, Ensure.NotNull(sheet).Frame(picture)))
                throw new InvalidDataException($"Spawned-object picture {picture} is on the set's own sheet and on common sheet {sheet.Image.Name}.");
            if (!sheets.Contains(sheet))
                sheets.Add(sheet);
        }
        Sheets = [.. sheets];
    }

    /// <summary>Every installed sheet the owner's pictures are on: the set's own first, then each common sheet once.</summary>
    public IReadOnlyList<SpriteSheet> Sheets { get; }

    /// <summary>The picture named <paramref name="picture"/>, from the sheet holding it.</summary>
    /// <exception cref="InvalidDataException">No sheet of the owner's holds the picture.</exception>
    public SpriteSheetFrame Frame(string picture) =>
        frames.TryGetValue(picture, out SpriteSheetFrame? frame) ? frame
            : throw new InvalidDataException($"No spawned-object sheet of {string.Join(", ", Sheets.Select(sheet => sheet.Image.Name))} holds picture {picture}.");
}

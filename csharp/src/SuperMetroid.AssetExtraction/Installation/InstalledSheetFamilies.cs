using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// The sheet families one installation run composes (#1635): each family's composer adds its converted sheets, on whatever
/// thread it runs, and the picture store then takes them in one fixed order, so the store's pages and every manifest are
/// the same bytes whichever family finishes first.
/// </summary>
internal sealed class InstalledSheetFamilies
{
    /// <summary>The order the store writes the families in, by directory name: sprite, room-block, boss-body, enemy.</summary>
    private static readonly string[] WritingOrder =
    [
        GameInstallationLayout.SpriteSheetDirectoryName,
        GameInstallationLayout.RoomBlockSheetDirectoryName,
        GameInstallationLayout.BossBodySheetDirectoryName,
        GameInstallationLayout.EnemySheetDirectoryName,
    ];

    /// <summary>Taken around every read and write of <see cref="added"/>.</summary>
    private readonly Lock gate = new();

    /// <summary>Each family added so far, by directory name.</summary>
    private readonly Dictionary<string, InstalledSheetFamily> added = new(StringComparer.Ordinal);

    /// <summary>Adds one family's sheets.</summary>
    /// <exception cref="ArgumentException">The family is no sheet family, or was added before.</exception>
    internal void Add(InstalledSheetFamily family)
    {
        Ensure.NotNull(family);
        string name = family.DirectoryName;
        if (!WritingOrder.Contains(name, StringComparer.Ordinal))
            throw new ArgumentException($"Directory {family.Directory} holds no sheet family the picture store writes.", nameof(family));
        lock (gate)
        {
            if (!added.TryAdd(name, family))
                throw new ArgumentException($"Sheet family {name} was composed twice in one installation.", nameof(family));
        }
    }

    /// <summary>Every sheet family, in the order the store writes them, as an installation writes its store.</summary>
    /// <exception cref="InvalidOperationException">A family has not been composed.</exception>
    internal IReadOnlyList<InstalledSheetFamily> Every()
    {
        lock (gate)
        {
            if (WritingOrder.FirstOrDefault(name => !added.ContainsKey(name)) is { } missing)
                throw new InvalidOperationException($"Sheet family {missing} was not composed before the picture store was written.");
            return [.. WritingOrder.Select(name => added[name])];
        }
    }
}

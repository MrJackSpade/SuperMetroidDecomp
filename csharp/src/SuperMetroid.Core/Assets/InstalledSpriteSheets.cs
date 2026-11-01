namespace SuperMetroid.Core.Assets;

/// <summary>Every installed pre-coloured sprite sheet (#1392); the installation loader checks each holds the frames its family draws.</summary>
public sealed class InstalledSpriteSheets
{
    /// <summary>The sheets by identity.</summary>
    private readonly Dictionary<SpriteSheetId, SpriteSheet> sheets;

    /// <summary>Keeps the sheets the import pipeline loaded, one for each identity it installs.</summary>
    public InstalledSpriteSheets(IReadOnlyDictionary<SpriteSheetId, SpriteSheet> sheets) =>
        this.sheets = new Dictionary<SpriteSheetId, SpriteSheet>(Ensure.NotNull(sheets));

    /// <summary>The installed sheet <paramref name="sheet"/>.</summary>
    public SpriteSheet Get(SpriteSheetId sheet) => sheets[sheet];

    /// <summary>SHA-256 of every sheet's pixels and frames, in identity order.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledSpriteSheets), content =>
    {
        foreach (SpriteSheetId sheet in Enum.GetValues<SpriteSheetId>())
            sheets[sheet].AppendIdentity(content);
    });
}

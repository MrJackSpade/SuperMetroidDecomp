namespace SuperMetroid.Core.Assets;

/// <summary>Every installed pre-coloured sprite sheet (#1392); the installation loader checks each holds the frames its family draws.</summary>
public sealed class InstalledSpriteSheets
{
    /// <summary>The sheets by identity.</summary>
    private readonly Dictionary<SpriteSheetId, SpriteSheet> sheets;

    /// <summary>Requires every sheet.</summary>
    /// <exception cref="InvalidDataException">A sheet is missing.</exception>
    public InstalledSpriteSheets(IReadOnlyDictionary<SpriteSheetId, SpriteSheet> sheets)
    {
        Ensure.NotNull(sheets);
        foreach (SpriteSheetId sheet in SpriteSheetDefinitions.Sheets)
        {
            if (!sheets.ContainsKey(sheet))
                throw new InvalidDataException($"The installation has no {sheet} sprite sheet.");
        }
        this.sheets = new Dictionary<SpriteSheetId, SpriteSheet>(sheets);
    }

    /// <summary>The installed sheet <paramref name="sheet"/>.</summary>
    public SpriteSheet Get(SpriteSheetId sheet) => sheets[sheet];

    /// <summary>SHA-256 of every sheet's pixels and frames, in identity order.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledSpriteSheets), content =>
    {
        foreach (SpriteSheetId sheet in SpriteSheetDefinitions.Sheets)
            sheets[sheet].AppendIdentity(content);
    });
}

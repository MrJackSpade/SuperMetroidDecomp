namespace SuperMetroid.Core.Assets;

/// <summary>Editable starting CGRAM image and common sprite colors restored on room entry.</summary>
public sealed class GameplayBasePaletteCatalog
{
    /// <summary>Owned 256-color CGRAM image loaded at gameplay startup.</summary>
    internal readonly RgbColor[] initial;
    /// <summary>Owned sixteen-color palette reused by common room sprites.</summary>
    internal readonly RgbColor[] commonSprites;

    /// <summary>Retains validated RGB5 arrays detached from the editable document.</summary>
    internal GameplayBasePaletteCatalog(RgbColor[] initial, RgbColor[] commonSprites)
    {
        this.initial = initial;
        this.commonSprites = commonSprites;
    }

    /// <summary>Identity of the decoded selected colors, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(GameplayBasePaletteCatalog), content =>
    {
        content.AppendColors("initial", initial);
        content.AppendColors("common sprites", commonSprites);
    });

    /// <summary>Loads the complete selected 256-colour starting image into gameplay's colours.</summary>
    public void LoadInitial(GameplayColours colours) => Ensure.NotNull(colours).LoadStartingImage(initial);

    /// <summary>Loads the complete selected 256-colour starting image into a screen's colours.</summary>
    public void LoadInitial(IPaletteColors colours)
    {
        Ensure.NotNull(colours);
        new CgramPalettes(colours).LoadImage(initial);
    }

    /// <summary>Loads the selected sixteen common sprite colours into <paramref name="set"/>.</summary>
    public void LoadCommonSprites(ColourRun set) => set.CopyFrom(commonSprites);

    /// <summary>Copies the sixteen enemy-projectile colours of the initial image into <paramref name="set"/>.</summary>
    public void LoadEnemyProjectileSprites(ColourRun set) =>
        set.CopyFrom(initial.AsSpan(GameplayBasePaletteFormat.EnemyProjectileInitialColor, GameplayBasePaletteFormat.SpriteColorCount));
}

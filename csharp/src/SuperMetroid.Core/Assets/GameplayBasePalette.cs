namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable starting colours and the common sprite colours restored on room entry. The import pipeline decodes the
/// installed starting image into its owners' colours (#1464); this catalog never addresses a colour by its position.
/// </summary>
public abstract class GameplayBasePalette
{
    /// <summary>The starting colours, by owner.</summary>
    internal readonly GameplayStartingColours starting;

    /// <summary>Owned sixteen-color palette reused by common room sprites.</summary>
    internal readonly RgbColor[] commonSprites;

    /// <summary>Retains the decoded starting colours and common sprite colours, detached from the editable document.</summary>
    /// <param name="starting">The starting colours, by owner.</param>
    /// <param name="commonSprites">The sixteen common sprite colours.</param>
    internal GameplayBasePalette(GameplayStartingColours starting, RgbColor[] commonSprites)
    {
        this.starting = Ensure.NotNull(starting);
        this.commonSprites = Ensure.NotNull(commonSprites);
    }

    /// <summary>Gives every owner of gameplay's colours its starting colours.</summary>
    public void LoadInitial(GameplayColours colours) => Ensure.NotNull(colours).LoadStarting(starting);

    /// <summary>Loads the selected sixteen common sprite colours into <paramref name="set"/>.</summary>
    public void LoadCommonSprites(ColourRun set) => set.CopyFrom(commonSprites);

    /// <summary>Copies the sixteen starting enemy-projectile colours into <paramref name="set"/>.</summary>
    public void LoadEnemyProjectileSprites(ColourRun set) => set.CopyFrom(starting.EnemyProjectiles);
}

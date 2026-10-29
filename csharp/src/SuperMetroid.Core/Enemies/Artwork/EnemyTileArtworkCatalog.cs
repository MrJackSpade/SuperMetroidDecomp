using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The installed enemy artwork gameplay reads (#1463): the palette each enemy is coloured with, the extended frames its
/// draw selects, and the enemies' colour cycles. The pictures enemies draw are pre-coloured sheets; the characters and
/// compositions they are composed from belong to the import pipeline's subclass.
/// </summary>
public abstract class EnemyTileArtworkCatalog
{
    /// <summary>The installed palettes, extended frames and colour catalogs.</summary>
    private EnemyGameplayArtworkParts installed;

    /// <summary>Copies the installed palettes and catalogs.</summary>
    /// <param name="parts">The installed artwork gameplay reads.</param>
    protected EnemyTileArtworkCatalog(EnemyGameplayArtworkParts parts)
    {
        Ensure.NotNull(parts);
        Ensure.NotNull(parts.Palettes);
        installed = parts with { Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(parts.Palettes) };
    }

    /// <summary>
    /// Complete selected enemy presentation, including legacy-override merges, display
    /// bindings, special uploads and colors. Gameplay definitions are excluded.
    /// This is computed rather than persisted in an exact debugger snapshot.
    /// </summary>
    public abstract string ContentIdentity { get; }

    /// <summary>Finds the installed palette of <paramref name="enemy"/>.</summary>
    internal bool TryGetPalette(EnemyDefinitionId enemy, [NotNullWhen(true)] out EnemyPaletteSheet? palette) =>
        installed.Palettes.TryGetValue(enemy, out palette);

    /// <summary>Editable face-block, corpse-sidehopper, and Golden Torizo health colors.</summary>
    public EnemyAuxiliaryColorCatalog? AuxiliaryColors => installed.AuxiliaryColors;

    /// <summary>Installed extended visual frames; hitbox and AI data stay engine-owned.</summary>
    public EnemyExtendedFrameCatalog? ExtendedFrames
    {
        get => installed.ExtendedFrames;
        internal set => installed = installed with { ExtendedFrames = value };
    }

    /// <summary>Four editable color frames; the Magdollite draw hook retains timing.</summary>
    public MagdollitePaletteCycle? MagdollitePaletteCycle => installed.MagdollitePaletteCycle;

    /// <summary>Six editable Work Robot color records; timer/terminator stay compiled.</summary>
    public WorkRobotPaletteCycle? WorkRobotPaletteCycle => installed.WorkRobotPaletteCycle;

    /// <summary>Editable Chozo statue and n00b-tube crack sprite-palette pairs.</summary>
    public ChozoAndTubeColorCatalog? ChozoAndTubeColors => installed.ChozoAndTubeColors;

    /// <summary>Editable Dachora default, speed and shine sprite colors.</summary>
    public DachoraColorCatalog? DachoraColors => installed.DachoraColors;

    /// <summary>Loads the sixteen indexed colors selected by a room graphics-set record.</summary>
    public void LoadPaletteTo(EnemyDefinitionId definitionId, ColourRun destination) =>
        EnemyTileArtwork.LoadPaletteTo(this, definitionId, destination);
}

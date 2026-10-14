using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>Editable RGB5 payloads for Crocomire's five palette transfers; each channel must be 0–31, and the native seventeen-word initialization overlaps remain explicit inputs.</summary>
public sealed record CrocomireColorDocument
{
    /// <summary>Color schema revision; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Eight body/head BG inks corresponding to $A4:B89D, restored at CGRAM 112–119 on nonwhite hurt-flash updates.</summary>
    public required PaletteRgb5[] FightBody { get; init; }
    /// <summary>Seventeen ordered wall initialization words corresponding to $A4:B8BD; word 16 overlaps the stock projectile source's first word but remains independently editable here.</summary>
    public required PaletteRgb5[] InitialWall { get; init; }
    /// <summary>Seventeen ordered projectile initialization words corresponding to $A4:B8DD; word 16 overlaps the stock skeleton-arm source's first word but remains independently editable here.</summary>
    public required PaletteRgb5[] InitialProjectile { get; init; }
    /// <summary>Sixteen skeleton-arm OBJ inks corresponding to $A4:B8FD, installed in OBJ palette 1 during wall break.</summary>
    public required PaletteRgb5[] SkeletonArm { get; init; }
    /// <summary>Sixteen wall-spike OBJ inks corresponding to $A4:B91D, installed in OBJ palette 3 at the rumble terminator.</summary>
    public required PaletteRgb5[] WallSpikes { get; init; }
}

using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Editable Mother Brain room-color schema; every supplied entry is a nonnull RGB5 color with channels 0..31, and nested arrays remain caller-mutable until compilation.</summary>
public sealed record MotherBrainRoomColorDocument
{
    /// <summary>Current revision 3, or revision 1/2 only when loading with explicit current-stock compatibility data.</summary>
    public required int Version { get; init; }
    /// <summary>Fourteen rows for native timed entries $A9:D046..D07A. Each row has 24 colors: twelve for CGRAM 52..63, then twelve for 83..94 mirrored to 115..126; timing is owned by the compiled program.</summary>
    public required PaletteRgb5[][] Flash { get; init; }
    /// <summary>Twenty-four final-room colors corresponding to $A9:D082: the same two twelve-color source slices and destinations as <see cref="Flash"/>.</summary>
    public required PaletteRgb5[] FinalRoom { get; init; }
    /// <summary>Fifteen nontransparent attack colors sourced natively from $A9:94B4 and installed at CGRAM 161..175 before phase two.</summary>
    public required PaletteRgb5[] PhaseTwoAttack { get; init; }
    /// <summary>Fifteen nontransparent rear-leg colors sourced natively from $A9:9494 and installed at CGRAM 177..191 before phase two, replacing the room-entry glass palette.</summary>
    public required PaletteRgb5[] PhaseTwoRearLeg { get; init; }
    /// <summary>Fifteen glass-shard colors from $A9:9514 for CGRAM 177..191; required in versions 2/3, but inherited from current stock when loading version 1.</summary>
    public PaletteRgb5[]? InitialGlassShard { get; init; }
    /// <summary>Fifteen tube-projectile colors from $A9:94F4 for CGRAM 241..255; required in versions 2/3, but inherited from current stock when loading version 1.</summary>
    public PaletteRgb5[]? InitialTubeProjectile { get; init; }
    /// <summary>Seven ordered 28-color images: fourteen for CGRAM 49..62 followed by fourteen for 81..94. Required in version 3; versions 1/2 inherit current-stock recovery artwork.</summary>
    public PaletteRgb5[][]? RecoveryLights { get; init; }
}

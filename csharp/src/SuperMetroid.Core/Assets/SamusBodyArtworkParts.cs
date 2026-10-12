using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Samus's body pointer tables, frame tiles, offsets and dependent artwork catalogs (#633).</summary>
public sealed record SamusBodyArtworkParts
{
    /// <summary>Bank-$92 pointers to upper-body definition groups.</summary>
    public required ushort[] TopPointers { get; init; }

    /// <summary>Bank-$92 pointers to lower-body definition groups.</summary>
    public required ushort[] BottomPointers { get; init; }

    /// <summary>Per-pose pointers to four-byte frame-selector lists.</summary>
    public required ushort[] PosePointers { get; init; }

    /// <summary>Per-pose signed vertical art origins.</summary>
    public required sbyte[] GraphicsYOffsets { get; init; }

    /// <summary>The contiguous native frame-selector interval.</summary>
    public required SamusBodyFrameSelection[] Frames { get; init; }

    /// <summary>Upper-body tile definitions grouped by pointer table entry.</summary>
    public required SamusBodyTileDefinition[][] Top { get; init; }

    /// <summary>Lower-body tile definitions grouped by pointer table entry.</summary>
    public required SamusBodyTileDefinition[][] Bottom { get; init; }

    /// <summary>Editable Samus OAM compositions.</summary>
    public required SamusSpritemapArtworkCatalog Spritemaps { get; init; }

    /// <summary>Editable atmospheric-effect OBJ attributes.</summary>
    public required SamusAtmosphericArtworkCatalog Atmosphere { get; init; }

    /// <summary>Editable fatal-damage and death palettes.</summary>
    public required SamusDeathPaletteArtworkCatalog DeathPalettes { get; init; }

    /// <summary>Editable death-explosion tile characters.</summary>
    public required SamusDeathTileAtlas DeathTiles { get; init; }

    /// <summary>Editable arm-cannon placement and cover artwork.</summary>
    public required SamusArmCannonArtworkCatalog ArmCannon { get; init; }

    /// <summary>Native landing vertical-offset bytes.</summary>
    public required ushort[] LandingYOffsets { get; init; }

    /// <summary>Native posture-transition vertical offsets.</summary>
    public required sbyte[] PostureYOffsets { get; init; }

    /// <summary>Native drained-state vertical offsets.</summary>
    public required sbyte[] DrainedYOffsets { get; init; }
}

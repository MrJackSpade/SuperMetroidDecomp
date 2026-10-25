using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>The Ceres escape's two installed character pages, in the order its transfer lists show them.</summary>
internal static class CeresEscapeTileArtworkDefinitions
{
    /// <summary>The escape warning text's 72 characters: nine rows of eight.</summary>
    internal static readonly TileSheetDefinition WarningText =
        new(72 * RoomCharacterAtlasFormat.BytesPerTile, "ceres-escape-warning-tiles.png");

    /// <summary>The Ceres escape doors' 48 characters.</summary>
    internal static readonly TileSheetDefinition Doors =
        new(48 * RoomCharacterAtlasFormat.BytesPerTile, "ceres-escape-door-tiles.png");

    /// <summary>The two fixed Ceres escape tile pages in transfer order: the warning text, then the doors.</summary>
    internal static ImmutableArray<TileSheetDefinition> All { get; } = [WarningText, Doors];
}

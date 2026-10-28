using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>The five gunship takeoff uploads, in takeoff order.</summary>
internal static class GunshipLiftoffTransferDefinitions
{
    /// <summary>One takeoff upload contains $400 bytes of 4-bpp OBJ characters.</summary>
    internal const ushort ByteCount = 0x0400;

    /// <summary>The takeoff's five uploads, showing five consecutive installed pages.</summary>
    internal static readonly GunshipLiftoffTransferDefinition[] Frames =
    [
        new(InstalledArtworkId.GunshipLiftoffFirstTiles),
        new(InstalledArtworkId.GunshipLiftoffSecondTiles),
        new(InstalledArtworkId.GunshipLiftoffThirdTiles),
        new(InstalledArtworkId.GunshipLiftoffFourthTiles),
        new(InstalledArtworkId.GunshipLiftoffFifthTiles),
    ];

    /// <summary>The five takeoff pages, in takeoff order (#1392, #1406).</summary>
    internal static IReadOnlyList<SpriteArtworkSource> Pages { get; } = [.. Frames.Select(frame => new InstalledArtworkCharacters(frame.Asset))];
}

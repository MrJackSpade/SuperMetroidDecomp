using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps the characters of the room-FX atlas's pictures (#1415): each object's first picture, the
/// rest following one frame transfer apart, and the shared Tourian statue strip. The import pipeline's private
/// knowledge, decoded once into the installed atlas.
/// </summary>
[CpuBusBoundary("catalog: room-FX animated-tile cartridge sources")]
internal static class RoomFxAnimatedTileSourceDefinitions
{
    /// <summary>$87:9D84, the three horizontal-spike pictures played 0/1/2/1 by $87:817E.</summary>
    internal static readonly SnesAddress HorizontalSpikes = new(0x87, 0x9d84);

    /// <summary>$87:91E4, the four ceiling-sand pictures selected by $87:8221.</summary>
    internal static readonly SnesAddress MaridiaSandCeiling = new(0x87, 0x91e4);

    /// <summary>$87:9164, the four falling-sand pictures selected by $87:8235.</summary>
    internal static readonly SnesAddress MaridiaSandFalling = new(0x87, 0x9164);

    /// <summary>$87:A564, the five lava pictures selected by $87:8293.</summary>
    internal static readonly SnesAddress Lava = new(0x87, 0xa564);

    /// <summary>$87:A6A4, the five acid pictures selected by $87:82B1.</summary>
    internal static readonly SnesAddress Acid = new(0x87, 0xa6a4);

    /// <summary>$87:A874, the five rain pictures selected by $87:82CF.</summary>
    internal static readonly SnesAddress Rain = new(0x87, 0xa874);

    /// <summary>$87:A7E4, the three spore pictures selected by $87:82ED.</summary>
    internal static readonly SnesAddress Spores = new(0x87, 0xa7e4);

    /// <summary>$87:8E64, the four rightward Wrecked Ship treadmill pictures, which the leftward loop shows reversed.</summary>
    internal static readonly SnesAddress WreckedShipTreadmill = new(0x87, 0x8e64);

    /// <summary>$87:9364, the shared strip whose windows the four Tourian statues show.</summary>
    internal static readonly SnesAddress TourianStatueStrip = new(0x87, 0x9364);
}

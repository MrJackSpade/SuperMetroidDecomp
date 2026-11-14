namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// The screens of Crocomire's room, one row of eight, whose scroll states his fight and death sequence change, and the
/// Samus position the bridge's scroll setup tests.
/// </summary>
internal static class CrocomireCameraDefinitions
{
    /// <summary>The screen immediately left of the melting viewport, which the bridge's scroll setup writes.</summary>
    internal static readonly Position16<RoomScreens> BridgeLeftScreen = new(4, 0);

    /// <summary>The bridge's own screen, the next one right, which the same scroll setup keeps open.</summary>
    internal static readonly Position16<RoomScreens> BridgeScreen = new(5, 0);

    /// <summary>The screen the skeleton's hidden wall closes as it rises, the one left of <see cref="BridgeLeftScreen"/>.</summary>
    internal static readonly Position16<RoomScreens> HiddenWallScreen = new(3, 0);

    /// <summary>The room's two leftmost screens, which the living fight closes behind his wall.</summary>
    internal static readonly Position16<RoomScreens>[] BehindWallScreens = [new(0, 0), new(1, 0)];

    /// <summary>The room's four leftmost screens, in order, which a defeated room and the skeleton's end open.</summary>
    internal static readonly Position16<RoomScreens>[] ArenaScreens = [new(0, 0), new(1, 0), new(2, 0), new(3, 0)];

    /// <summary>The bridge's scroll setup compares Samus's X against 1312 before closing the screen left of the bridge.</summary>
    internal const int BridgeVisibleSamusX = 1312;
}

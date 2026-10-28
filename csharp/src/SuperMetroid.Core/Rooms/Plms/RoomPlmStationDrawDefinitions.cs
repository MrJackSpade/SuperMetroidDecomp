
namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Complete bank-$84 draw lists selected by station animation programs. The native
/// level words are physical room mutations; replaceable artwork remains separate.
/// </summary>
internal static class RoomPlmStationDrawDefinitions
{
    /// <summary>Published enumeration order: frames by station, then save and access states.</summary>
    internal static readonly StationDraw[] PublishedOrder =
    [
        StationDraw.MapFrame0, StationDraw.EnergyFrame0, StationDraw.MissileFrame0,
        StationDraw.MapFrame1, StationDraw.EnergyFrame1, StationDraw.MissileFrame1,
        StationDraw.MapFrame2, StationDraw.EnergyFrame2, StationDraw.MissileFrame2,
        StationDraw.SaveIdle, StationDraw.SaveActive, StationDraw.SaveAlternate,
        StationDraw.MapRightRetracted, StationDraw.MapRightExtended,
        StationDraw.MapLeftRetracted, StationDraw.MapLeftExtended,
        StationDraw.ResourceRightRetracted, StationDraw.ResourceRightExtended,
        StationDraw.ResourceLeftRetracted, StationDraw.ResourceLeftExtended,
    ];

    /// <summary>The visual block of the map display's first frame, top row; each frame steps one block row.</summary>
    internal const int MapFirstBlock = 0x10c;
    /// <summary>The visual block of the energy display's first frame.</summary>
    internal const int EnergyFirstBlock = 0x0c4;
    /// <summary>The visual block of the missile display's first frame.</summary>
    internal const int MissileFirstBlock = 0x0c7;
    /// <summary>The visual block of the save pod's top and base; the active pod's steps one block.</summary>
    internal const int SavePodBlock = 0x059;
    /// <summary>The visual block of the save pod's shaft; the active pod's steps one block.</summary>
    internal const int SaveShaftBlock = 0x05b;
    /// <summary>The visual block of the extended map access arm.</summary>
    internal const int MapAccessExtendedBlock = 0x129;
    /// <summary>The visual block of the retracted map access arm.</summary>
    internal const int MapAccessRetractedBlock = 0x128;
    /// <summary>The visual block of the extended resource access arm.</summary>
    internal const int ResourceAccessExtendedBlock = 0x0c1;
    /// <summary>The visual block of the retracted resource access arm.</summary>
    internal const int ResourceAccessRetractedBlock = 0x0c3;
}

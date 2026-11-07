
namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Complete draw lists selected by station animation programs. The drawn
/// block are physical room mutations; replaceable artwork remains separate.
/// </summary>
internal static class RoomObjectStationDrawDefinitions
{
    /// <summary>The visual block of the map display's first frame, top row; each frame steps one block row.</summary>
    internal const int MapFirstBlock = 268;
    /// <summary>The visual block of the energy display's first frame.</summary>
    internal const int EnergyFirstBlock = 196;
    /// <summary>The visual block of the missile display's first frame.</summary>
    internal const int MissileFirstBlock = 199;
    /// <summary>The visual block of the save pod's top and base; the active pod's steps one block.</summary>
    internal const int SavePodBlock = 89;
    /// <summary>The visual block of the save pod's shaft; the active pod's steps one block.</summary>
    internal const int SaveShaftBlock = 91;
    /// <summary>The visual block of the extended map access arm.</summary>
    internal const int MapAccessExtendedBlock = 297;
    /// <summary>The visual block of the retracted map access arm.</summary>
    internal const int MapAccessRetractedBlock = 296;
    /// <summary>The visual block of the extended resource access arm.</summary>
    internal const int ResourceAccessExtendedBlock = 193;
    /// <summary>The visual block of the retracted resource access arm.</summary>
    internal const int ResourceAccessRetractedBlock = 195;
}

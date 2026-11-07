using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>The draw and runs of one roomObject content identity frame.</summary>
/// <param name="Draw">The draw.</param>
/// <param name="Runs">The runs.</param>
internal readonly record struct RoomObjectContentIdentityFrame(RoomObjectDraw Draw, RoomBlockAppearance[][] Runs);

using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>The draw and word of one roomObject content identity definition.</summary>
/// <param name="Draw">The draw.</param>
/// <param name="Appearance">The draw's block appearance.</param>
internal readonly record struct RoomObjectContentIdentityDefinition(RoomObjectDraw Draw, RoomBlockAppearance Appearance);

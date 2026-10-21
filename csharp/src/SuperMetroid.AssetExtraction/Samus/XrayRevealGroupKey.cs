using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>A collision type and the x-ray reveal it groups under.</summary>
/// <param name="Type">The type.</param>
/// <param name="Definition">The definition.</param>
public readonly record struct XrayRevealGroupKey(RoomCollisionType Type, XrayRevealDefinition Definition);

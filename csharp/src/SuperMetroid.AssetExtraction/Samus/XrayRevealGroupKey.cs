using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>A collision type and the x-ray reveal it groups under.</summary>
/// <param name="Type">The type.</param>
/// <param name="Definition">The definition.</param>
/// <param name="Copy">The shape of the copy the reveal draws for the type.</param>
public readonly partial record struct XrayRevealGroupKey(RoomCollisionType Type, XrayRevealDefinition Definition, XrayRevealCopyShape Copy);

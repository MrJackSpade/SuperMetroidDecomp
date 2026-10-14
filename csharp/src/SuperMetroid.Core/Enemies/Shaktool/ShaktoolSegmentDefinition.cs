namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Fixed initialization metadata for one of Shaktool's seven linked segments.</summary>
internal readonly record struct ShaktoolSegmentDefinition(
    ushort PropertyMask,
    ushort OwnerNativeOffset,
    ushort InitialOrbitAngle,
    ushort InitialInstruction,
    ushort Layer,
    ShaktoolPreInstruction PreInstruction,
    ushort AngularVelocity);

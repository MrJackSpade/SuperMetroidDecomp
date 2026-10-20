namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Fixed initialization metadata for one of Shaktool's seven linked segments.</summary>
internal readonly record struct ShaktoolSegmentDefinition(
    EnemyProperties PropertyMask,
    ushort OwnerNativeOffset,
    SnesAngle InitialOrbitAngle,
    ShaktoolProgram InitialProgram,
    ushort Layer,
    ShaktoolPreInstruction PreInstruction,
    ushort AngularVelocity);

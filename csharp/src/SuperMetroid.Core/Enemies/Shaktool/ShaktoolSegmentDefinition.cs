namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Fixed initialization metadata for one of Shaktool's seven linked segments.</summary>
internal readonly record struct ShaktoolSegmentDefinition(
    EnemyProperties PropertyMask,
    int OwnerSlotDistance,
    RotationAngle InitialOrbitAngle,
    ShaktoolProgram InitialProgram,
    int Layer,
    ShaktoolPreInstruction PreInstruction,
    int AngularVelocity);

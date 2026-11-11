namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Fixed initialization metadata for one of Shaktool's seven linked segments.</summary>
internal readonly record struct ShaktoolSegmentDefinition(
    EnemyProcessing Processing,
    EnemyContact Contact,
    int OwnerSlotDistance,
    RotationAngle InitialOrbitAngle,
    ShaktoolProgram InitialProgram,
    ShaktoolFrameBehavior FrameBehavior,
    int AngularVelocity);

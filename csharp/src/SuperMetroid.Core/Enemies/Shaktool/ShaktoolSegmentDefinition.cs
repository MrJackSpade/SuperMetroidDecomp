namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Fixed initialization metadata for one of Shaktool's seven linked segments.</summary>
/// <param name="Processing">How the segment is processed.</param>
/// <param name="Contact">How the segment meets Samus.</param>
/// <param name="OwnerPlaceDistance">How many places after the chain's leading saw the room lists the segment.</param>
/// <param name="InitialOrbitAngle">The segment's first angle around the segment before it.</param>
/// <param name="InitialAnimation">The animation the segment starts.</param>
/// <param name="Motion">The segment's motion.</param>
/// <param name="AngularVelocity">The turn the segment curls by each update.</param>
internal readonly record struct ShaktoolSegmentDefinition(
    EnemyProcessing Processing,
    EnemyContact Contact,
    int OwnerPlaceDistance,
    RotationAngle InitialOrbitAngle,
    ShaktoolSegment InitialAnimation,
    ShaktoolSegmentMotion Motion,
    RotationAngle AngularVelocity);

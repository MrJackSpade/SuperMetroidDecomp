namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Fixed initialization metadata for one of Shaktool's seven linked segments; every one is processed alike (<see cref="ShaktoolSegmentDefinitions.Processing"/>).</summary>
/// <param name="Contact">How the segment meets Samus.</param>
/// <param name="InitialOrbitAngle">The segment's first angle around the segment before it.</param>
/// <param name="InitialAnimation">The animation the segment starts.</param>
/// <param name="Motion">The segment's motion.</param>
/// <param name="AngularVelocity">The turn the segment curls by each update.</param>
/// <param name="CollisionAnimation">The head bob the segment plays as the chain recovers from striking a wall.</param>
internal readonly record struct ShaktoolSegmentDefinition(
    EnemyContact Contact,
    RotationAngle InitialOrbitAngle,
    ShaktoolSegment InitialAnimation,
    ShaktoolSegmentMotion Motion,
    RotationAngle AngularVelocity,
    ShaktoolSegment CollisionAnimation);

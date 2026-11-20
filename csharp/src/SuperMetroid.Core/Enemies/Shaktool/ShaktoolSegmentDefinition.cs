namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Fixed initialization metadata for one of Shaktool's seven linked segments; every one is processed alike (<see cref="ShaktoolSegmentDefinitions.Processing"/>).</summary>
/// <param name="Contact">How the segment meets Samus.</param>
/// <param name="InitialOrbitAngle">The segment's first angle around the segment before it.</param>
/// <param name="InitialAnimation">The animation the segment starts.</param>
/// <param name="Motion">The segment's motion.</param>
/// <param name="AngularVelocity">The turn the segment curls by each update.</param>
/// <param name="CollisionAnimation">The head bob the segment plays as the chain recovers from striking a wall.</param>
/// <param name="AnchorsChain">Whether the segment is the chain's fixed anchor rather than centred on the segment before it.</param>
/// <param name="IsSaw">Whether the segment is one of the chain's two saws, which the swap's fractions of a pixel pass by.</param>
[IndependentFlag("independent roles of one segment: whether it anchors the chain and whether it is the saw")]
internal readonly record struct ShaktoolSegmentDefinition(
    EnemyContact Contact,
    RotationAngle InitialOrbitAngle,
    ShaktoolSegment InitialAnimation,
    ShaktoolSegmentMotion Motion,
    RotationAngle AngularVelocity,
    ShaktoolSegment CollisionAnimation,
    bool AnchorsChain = false,
    bool IsSaw = false);

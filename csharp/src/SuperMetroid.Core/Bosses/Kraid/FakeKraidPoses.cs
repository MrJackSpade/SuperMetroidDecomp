using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The poses of a fake Kraid facing one way (#2557): his steps and his spit are written once over a facing's poses.
/// </summary>
/// <param name="Facing">The way it faces.</param>
/// <param name="SteppingFeetTogether">Its stepping feet together picture.</param>
/// <param name="SteppingFrontFootLifting">Its stepping front foot lifting picture.</param>
/// <param name="SteppingFrontFootForward">Its stepping front foot forward picture.</param>
/// <param name="SteppingFeetPlanted">Its stepping feet planted picture.</param>
/// <param name="MouthClosed">Its mouth closed picture.</param>
/// <param name="MouthOpening">Its mouth opening picture.</param>
/// <param name="MouthWide">Its mouth wide picture.</param>
internal sealed record FakeKraidPoses(
    HorizontalDirection Facing,
    EnemyFrameId SteppingFeetTogether,
    EnemyFrameId SteppingFrontFootLifting,
    EnemyFrameId SteppingFrontFootForward,
    EnemyFrameId SteppingFeetPlanted,
    EnemyFrameId MouthClosed,
    EnemyFrameId MouthOpening,
    EnemyFrameId MouthWide)
{
    /// <summary>The poses facing left.</summary>
    internal static FakeKraidPoses Left { get; } = new(
        HorizontalDirection.Left,
        EnemyFrameId.FakeKraidSteppingFacingLeftFeetTogether,
        EnemyFrameId.FakeKraidSteppingFacingLeftFrontFootLifting,
        EnemyFrameId.FakeKraidSteppingFacingLeftFrontFootForward,
        EnemyFrameId.FakeKraidSteppingFacingLeftFeetPlanted,
        EnemyFrameId.FakeKraidFacingLeftMouthClosed,
        EnemyFrameId.FakeKraidFacingLeftMouthOpening,
        EnemyFrameId.FakeKraidFacingLeftMouthWide);

    /// <summary>The poses facing right.</summary>
    internal static FakeKraidPoses Right { get; } = new(
        HorizontalDirection.Right,
        EnemyFrameId.FakeKraidSteppingFacingRightFeetTogether,
        EnemyFrameId.FakeKraidSteppingFacingRightFrontFootLifting,
        EnemyFrameId.FakeKraidSteppingFacingRightFrontFootForward,
        EnemyFrameId.FakeKraidSteppingFacingRightFeetPlanted,
        EnemyFrameId.FakeKraidFacingRightMouthClosed,
        EnemyFrameId.FakeKraidFacingRightMouthOpening,
        EnemyFrameId.FakeKraidFacingRightMouthWide);
}

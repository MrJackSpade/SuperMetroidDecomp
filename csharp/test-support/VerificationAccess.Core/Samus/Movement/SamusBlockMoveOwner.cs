using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The Samus a check's block move is hers when the check moves a bare body (#2554): Samus as a new game starts her,
/// neither boosting nor attacking, in the pose she starts in.
/// </summary>
internal static class SamusBlockMoveOwner
{
    /// <summary>A new Samus, whose block move a check's bare body makes.</summary>
    internal static SamusState NewSamus => RoomEnemyFrameFixtures.StandaloneSamus();

    /// <summary>The pose a new Samus starts in.</summary>
    internal static SamusPoseId NewSamusPose => NewSamus.Pose;
}

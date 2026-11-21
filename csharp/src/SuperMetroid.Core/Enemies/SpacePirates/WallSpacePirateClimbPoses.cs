using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The poses of a wall Space Pirate's climb on one wall (#2451), which its climbs up and down that wall both show. Each climb
/// is written once over a wall's poses, as a Torizo's animations are written once over a side (#2411).
/// </summary>
/// <param name="Wall">The wall the pirate clings to.</param>
/// <param name="LegsHanging">Its legs hanging.</param>
/// <param name="KneesBending">Its knees bending.</param>
/// <param name="KneesTucked">Its knees tucked.</param>
/// <param name="LegsPushing">Its legs pushing.</param>
/// <param name="LegsStretched">Its legs stretched.</param>
internal sealed partial record WallSpacePirateClimbPoses(HorizontalDirection Wall, F LegsHanging, F KneesBending, F KneesTucked, F LegsPushing, F LegsStretched)
{
    /// <summary>The poses on the left wall.</summary>
    internal static WallSpacePirateClimbPoses LeftWall { get; } = new(
        HorizontalDirection.Left, F.WallPirateOnLeftWallLegsHanging, F.WallPirateOnLeftWallKneesBending, F.WallPirateOnLeftWallKneesTucked,
        F.WallPirateOnLeftWallLegsPushing, F.WallPirateOnLeftWallLegsStretched);

    /// <summary>The poses on the right wall.</summary>
    internal static WallSpacePirateClimbPoses RightWall { get; } = new(
        HorizontalDirection.Right, F.WallPirateOnRightWallLegsHanging, F.WallPirateOnRightWallKneesBending, F.WallPirateOnRightWallKneesTucked,
        F.WallPirateOnRightWallLegsPushing, F.WallPirateOnRightWallLegsStretched);
}

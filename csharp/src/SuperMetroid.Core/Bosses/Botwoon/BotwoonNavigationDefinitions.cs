namespace SuperMetroid.Core.Bosses.Botwoon;
/// <summary>The fixed geometry and paths of Botwoon's navigation through his room.</summary>
internal static partial class BotwoonNavigationDefinitions
{

    /// <summary>
    /// Whether Botwoon, leaving a hole at full speed after his first action, follows an authored path rather than spitting
    /// from the hole: three chances in eight.
    /// </summary>
    internal static readonly RandomChance FollowsAuthoredPath = RandomChance.Lowest(3, BotwoonRandomRolls.PathChoice);

    /// <summary>Whether Botwoon starts its next path inside the hole (#2479): one path in two.</summary>
    internal static readonly RandomChance StartsInsideHole = RandomChance.Highest(1, RandomSampleField.OneOf(2));

    /// <summary>The holes in the order a walk's choices list them: left, bottom, top and right.</summary>
    private static readonly BotwoonHole[] HoleOrder = [BotwoonHole.Left, BotwoonHole.Bottom, BotwoonHole.Top, BotwoonHole.Right];

    /// <summary>
    /// The hole a walk in the open leads to from each hole, chosen from four rolls of the walk's random draw (#2510): the
    /// other three holes in turn, then back into the hole he left.
    /// </summary>
    internal static readonly IReadOnlyDictionary<BotwoonHole, WeightedRandomChoice<BotwoonHole>> OpenDestinations =
        BotwoonWalkDestinations.InTheOpen(HoleOrder, BotwoonRandomRolls.WalkDestination);

    /// <summary>
    /// The hole a walk inside the wall leads to from each hole, chosen from four rolls of the walk's random draw (#2510): the
    /// other three holes in turn, the last of them taken by two rolls, since no wall path leads back into the hole he left.
    /// </summary>
    internal static readonly IReadOnlyDictionary<BotwoonHole, WeightedRandomChoice<BotwoonHole>> WallDestinations =
        BotwoonWalkDestinations.InsideTheWall(HoleOrder, BotwoonRandomRolls.WalkDestination);

    /// <summary>
    /// The room holes in the order the game tests the head against them, last first: left, bottom, top and right, each at
    /// the centre its definition holds.
    /// </summary>
    internal static readonly BotwoonHoleDefinition[] Holes =
    [
        new(BotwoonHole.Left, new(64 - 4, 112 - 4)),
        new(BotwoonHole.Bottom, new(128 - 4, 176 - 4)),
        new(BotwoonHole.Top, new(160 - 4, 96 - 4)),
        new(BotwoonHole.Right, new(224 - 4, 144 - 4)),
    ];

    /// <summary>
    /// How many steps before an authored path's end a backward walk starts. The walk begins on the path's
    /// second-to-last step, so it never takes the last one, as the game's backward walks do.
    /// </summary>
    internal const int BackwardStartStepsBeforeEnd = 2;

    /// <summary>The farthest a hole's distance on either axis is measured for its bearing, in pixels.</summary>
    internal const int HoleBearingReach = 255;

    /// <summary>The one-pixel movement each step of a path stands for: one pixel along each axis the step points along.</summary>
    internal static IReadOnlyDictionary<BotwoonPathStep, Offset16<RoomPixels>> StepMovements { get; } =
        new Dictionary<BotwoonPathStep, Offset16<RoomPixels>>
        {
            [BotwoonPathStep.Up] = new(0, -1),
            [BotwoonPathStep.UpRight] = new(1, -1),
            [BotwoonPathStep.Right] = new(1, 0),
            [BotwoonPathStep.DownRight] = new(1, 1),
            [BotwoonPathStep.Down] = new(0, 1),
            [BotwoonPathStep.DownLeft] = new(-1, 1),
            [BotwoonPathStep.Left] = new(-1, 0),
            [BotwoonPathStep.UpLeft] = new(-1, -1),
        };
}

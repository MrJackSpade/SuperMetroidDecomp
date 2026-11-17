namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>How the Mama Turtle and her babies move, wait and touch Samus (#2043).</summary>
internal static class MamaTurtleMotionDefinitions
{
    /// <summary>Half the Mama Turtle's height once she has left her shell, in pixels.</summary>
    internal const int AwakeHalfHeight = 16;

    /// <summary>How long the quake lasts when the hovering Mama Turtle strikes a wall, in updates.</summary>
    internal const int WallQuakeDuration = 16;

    /// <summary>The fastest the hovering Mama Turtle carries a landing Samus leftward, in whole pixels an update.</summary>
    internal const int LargestLeftwardCarry = 16;

    /// <summary>The downward speed a Mama Turtle knocked out of her hover starts falling at, in whole pixels an update.</summary>
    internal const int KnockedDownFallSpeed = 2;

    /// <summary>How long the Mama Turtle rises before she hovers, in updates.</summary>
    internal const int RiseToHoverDuration = 16;

    /// <summary>
    /// How far the Mama Turtle's damaging box reaches past her body on either side, in pixels: it reaches that far wider
    /// and <see cref="DamageBoxInset"/> shorter than her body.
    /// </summary>
    internal const int DamageBoxReach = 8;

    /// <summary>How far inside her body the Mama Turtle's damaging box stops at the top and bottom, in pixels.</summary>
    internal const int DamageBoxInset = 4;

    /// <summary>How long a hiding baby waits after Samus steps off it before it starts its wait to crawl again, in updates.</summary>
    internal const int BabyRiderLeftDelay = 4;

    /// <summary>How long a hiding baby Samus has left waits before it crawls again, in updates.</summary>
    internal const int BabyHideDuration = 60;

    /// <summary>The velocity a hiding baby Samus lands on spins off at, for Samus facing right; facing left mirrors it.</summary>
    internal static Offset16<RoomPixels> BabySpinVelocity { get; } = new(3, 1);

    /// <summary>How fast a baby crawls along its heading, in whole pixels an update (#2338).</summary>
    internal const int BabyCrawlSpeed = 1;

    /// <summary>How many babies crawl on the Mama Turtle's shell; the room lists them right after her.</summary>
    internal const int BabyCount = 4;

    /// <summary>How far a baby crawls from where it spawned before it turns back, in pixels.</summary>
    internal const int BabyTravelDistance = 48;

    /// <summary>How far each step out of her shell raises the Mama Turtle, in pixels.</summary>
    internal const int LeavingShellClimb = 1;

    /// <summary>How far each step out of her shell moves a Samus standing on the Mama Turtle, taken from Samus's extra displacement.</summary>
    internal static Offset16<RoomPixels> LeavingShellRiderShift { get; } = new(1, 0);

    /// <summary>How fast the Mama Turtle rises before she hovers, carrying a Samus standing on her, in whole pixels an update.</summary>
    internal const int RiseToHoverSpeed = 1;

    /// <summary>How fast the Mama Turtle drifts sideways as she rises to hover, in whole pixels an update.</summary>
    internal const int RiseToHoverDrift = 1;

    /// <summary>How much the hovering Mama Turtle's sideways speed changes each update, toward Samus's side: a sixteenth of a pixel.</summary>
    internal static SignedSixteenSixteen HoverAcceleration { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(1);

    /// <summary>The fastest the hovering Mama Turtle moves sideways, in whole pixels an update.</summary>
    internal const int FastestHover = 3;

    /// <summary>The room height a Mama Turtle carrying Samus rises to before she pauses, in room pixels.</summary>
    internal const int RiddenAscentPeak = 488;

    /// <summary>How fast a Mama Turtle carrying Samus rises toward the peak, carrying her, in whole pixels an update.</summary>
    internal const int RiddenAscentSpeed = 7;

    /// <summary>How long the Mama Turtle holds at the peak before she falls, in updates.</summary>
    internal const int PeakPause = 30;

    /// <summary>How much the falling Mama Turtle's speed grows each update, an eighth of a pixel.</summary>
    internal static SignedSixteenSixteen FallAcceleration { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(2);

    /// <summary>The fall speed from which the Mama Turtle stops accelerating, in whole pixels an update.</summary>
    internal const int FastestFall = 4;
}

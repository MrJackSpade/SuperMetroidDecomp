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
}

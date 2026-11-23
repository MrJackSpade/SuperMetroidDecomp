namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// How the Yard moves (#2554), holding what its movement needs: an airborne Yard holds the animation it hides with once it
/// lands, which the airborne animation it flies with names, so a landing resumes the animation its flight holds.
/// </summary>
internal abstract partial class YardMotion
{
    /// <summary>Only the motions below derive from it.</summary>
    private YardMotion(YardMovement movement) => Movement = movement;

    /// <summary>The movement this motion is.</summary>
    internal YardMovement Movement { get; }

    /// <summary>The Yard holds still: placed in the room, turning a corner or landing.</summary>
    internal static YardMotion HoldingStill { get; } = new Grounded(YardMovement.HoldingStill);

    /// <summary>The Yard hides in its shell, watching whether its surface still holds it.</summary>
    internal static YardMotion Hiding { get; } = new Grounded(YardMovement.Hiding);

    /// <summary>The Yard crawls along its surface.</summary>
    internal static YardMotion Crawling { get; } = new Grounded(YardMovement.Crawling);

    /// <inheritdoc/>
    public override string ToString() => Movement.ToString();

    /// <summary>A movement on or against a surface, which needs nothing beyond the Yard's state.</summary>
    /// <param name="movement">The movement.</param>
    private sealed partial class Grounded(YardMovement movement) : YardMotion(movement)
    {
        /// <summary>The <c>movement</c> the object was made with, which it keeps so it can be made again from what it holds (#2602).</summary>
        private readonly YardMovement movement = movement;
    }

    /// <summary>The Yard's shell flies, falling, bouncing and slowing until it lands.</summary>
    /// <param name="landing">The animation the Yard hides with once it lands.</param>
    internal sealed partial class Airborne(YardSegment landing) : YardMotion(YardMovement.Airborne)
    {
        /// <summary>The animation the Yard hides with once it lands, which its airborne animation names.</summary>
        internal YardSegment Landing { get; } = landing;
    }
}

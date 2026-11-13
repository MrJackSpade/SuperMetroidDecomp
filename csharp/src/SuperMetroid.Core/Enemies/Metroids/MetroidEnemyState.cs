using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// The state of an ordinary Metroid, with its three extra values: the two outer-body sprite objects it spawned and
/// the drain counter.
/// </summary>
public sealed class MetroidEnemyState
{
    /// <summary>Connects the Metroid's common slot to its two live outer-body sprite objects.</summary>
    internal MetroidEnemyState(RoomSpriteObject outerBodyA,
        RoomSpriteObject outerBodyB)
    {
        OuterBodyA = outerBodyA;
        OuterBodyB = outerBodyB;
    }

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<MetroidSegment> Playback { get; set; }

    /// <summary>Signed 16.16 velocity in pixels per movement update, positive right and down.</summary>
    public FixedVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>Four-frame escape timer and frozen-shake countdown.</summary>
    public int EscapeTimer { get; internal set; }

    /// <summary>Zero-based movement dispatcher index 0..3, not an AI behaviour.</summary>
    public MetroidAiFunction Function { get; internal set; }

    /// <summary>The electricity sprite object the Metroid spawned; its owner repositions, freezes and ends it alongside the central body.</summary>
    public RoomSpriteObject OuterBodyA { get; }
    /// <summary>The shell sprite object the Metroid spawned; distinct from both the electricity layer and the central body.</summary>
    public RoomSpriteObject OuterBodyB { get; }

    /// <summary>
    /// Fractional health-drain counter, 0 through 65535 in 1/65536ths of one energy. A borrow from subtracting the
    /// suit-selected rate removes exactly one energy from Samus (<see cref="DrainBorrows"/>).
    /// </summary>
    public int DrainAccumulator { get; internal set; }

    /// <summary>The counter's range: one energy in fractional steps.</summary>
    private const int DrainFractionSteps = ushort.MaxValue + 1;

    /// <summary>
    /// Subtracts one update's drain <paramref name="rate"/> from the counter, wrapping below zero back into its
    /// range, and reports whether it borrowed: a borrow drains one energy.
    /// </summary>
    /// <param name="rate">The suit-selected drain per update, in the counter's fractional steps.</param>
    internal bool DrainBorrows(int rate)
    {
        int remaining = DrainAccumulator - rate;
        bool borrowed = remaining < 0;
        DrainAccumulator = borrowed ? remaining + DrainFractionSteps : remaining;
        return borrowed;
    }

    /// <summary>
    /// The ice damage the Metroid still absorbs before freezing, from the authored value; a shot that
    /// reaches it freezes the Metroid and empties it.
    /// </summary>
    public int IceDamageToFreeze
    {
        get;
        internal set => field = Ensure.AtLeastZero(value);
    }
}

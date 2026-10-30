namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>One of Samus's four atmospheric effect slots: a splash, bubbles, lava spray or dust.</summary>
public sealed class SamusAtmosphericEffectSlot
{
    /// <summary>The slot's effect; <see cref="SamusAtmosphericEffectType.Inactive"/> when the slot is empty.</summary>
    public SamusAtmosphericEffectType Type { get; private set; }

    /// <summary>The zero-based frame within the effect's animation.</summary>
    public int AnimationFrame { get; private set; }

    /// <summary>Whether the slot shows no effect.</summary>
    public bool IsEmpty => Type == SamusAtmosphericEffectType.Inactive;

    /// <summary>Starts an effect at an animation frame.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The effect is <see cref="SamusAtmosphericEffectType.Inactive"/>.</exception>
    internal void SetEffect(SamusAtmosphericEffectType type, int animationFrame)
    {
        if (type == SamusAtmosphericEffectType.Inactive)
            throw new ArgumentOutOfRangeException(nameof(type), type, "An effect slot is emptied, not started as inactive.");
        Type = type;
        AnimationFrame = Ensure.BetweenInclusive(0, byte.MaxValue, animationFrame);
    }

    /// <summary>Advances to the effect's next animation frame.</summary>
    internal void AdvanceFrame() => AnimationFrame++;

    /// <summary>Empties the slot, leaving its timer and anchor behind.</summary>
    internal void ClearEffect()
    {
        Type = SamusAtmosphericEffectType.Inactive;
        AnimationFrame = 0;
    }

    /// <summary>
    /// Countdown in update calls. A negative countdown suppresses motion and drawing until it reaches the delayed-start
    /// marker, which reloads the current frame's duration.
    /// </summary>
    public WrappingCountdown AnimationTimer { get; internal set; }

    /// <summary>
    /// Anchor in whole room pixels; the camera is subtracted only when drawing, motion wraps at sixteen bits, diving
    /// splashes follow the live FX surface and lava spray and dust rise one pixel per active update.
    /// </summary>
    public Position16<RoomPixels> Position { get; internal set; }

    /// <summary>Empties the slot and resets its delayed timer and anchor during room or Samus initialization.</summary>
    internal void Clear()
    {
        ClearEffect();
        AnimationTimer = WrappingCountdown.Start(0);
        Position = Position16<RoomPixels>.Zero;
    }
}

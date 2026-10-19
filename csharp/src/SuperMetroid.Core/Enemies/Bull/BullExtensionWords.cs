namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// The Bull words kept per physical slot for the room rather than per occupant. InitAI_Bull writes only the speed and
/// interval words, so the angles, shot-reaction guard and previous health keep the previous occupant's values when a
/// Bull is re-initialized in the same slot (a zero guard timer first wraps to $FFFF).
/// </summary>
internal sealed class BullExtensionWords
{
    /// <summary>Population-selected terminal speed, in 256ths of a pixel per update.</summary>
    internal int MaxSpeed { get; set; }

    /// <summary>Latest target bearing toward Samus, retained separately from the flight angle.</summary>
    internal SnesAngle AngleToSamus { get; set; }

    /// <summary>Committed travel bearing in 256ths of a turn, used while integrating the Bull's position.</summary>
    internal int Angle { get; set; }

    /// <summary>Suppresses repeated immune-shot knockback during its reaction window.</summary>
    internal ushort ShotReactionDisableFlag { get; set; }

    /// <summary>Population-selected interval between acceleration steps, in eligible AI calls.</summary>
    internal int AccelerationTimerReset { get; set; }

    /// <summary>Population-selected interval between deceleration steps, in eligible AI calls.</summary>
    internal int DecelerationTimerReset { get; set; }

    /// <summary>Counts down the post-hit period before shot reactions may run again.</summary>
    internal ushort ShotReactionDisableTimer { get; set; }

    /// <summary>Health captured before common hit processing for immune-hit detection.</summary>
    internal int PreviousHealth { get; set; }
}

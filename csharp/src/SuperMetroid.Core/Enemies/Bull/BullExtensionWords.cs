namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// The Bull words kept per physical slot for the room rather than per occupant. InitAI_Bull writes only the speed and
/// interval words, so the angles, shot-reaction guard and previous health keep the previous occupant's values when a
/// Bull is re-initialized in the same slot (a zero guard timer first wraps to $FFFF).
/// </summary>
internal sealed class BullExtensionWords
{
    /// <summary>Population-selected terminal speed.</summary>
    internal ushort MaxSpeed { get; set; }

    /// <summary>Latest target bearing toward Samus, retained separately from the flight angle.</summary>
    internal ushort AngleToSamus { get; set; }

    /// <summary>Committed travel bearing used while integrating the Bull's position.</summary>
    internal ushort Angle { get; set; }

    /// <summary>Suppresses repeated immune-shot knockback during its reaction window.</summary>
    internal ushort ShotReactionDisableFlag { get; set; }

    /// <summary>Population-selected interval between acceleration steps.</summary>
    internal ushort AccelerationTimerReset { get; set; }

    /// <summary>Population-selected interval between deceleration steps.</summary>
    internal ushort DecelerationTimerReset { get; set; }

    /// <summary>Counts down the post-hit period before shot reactions may run again.</summary>
    internal ushort ShotReactionDisableTimer { get; set; }

    /// <summary>Health captured before common hit processing for immune-hit detection.</summary>
    internal ushort PreviousHealth { get; set; }
}

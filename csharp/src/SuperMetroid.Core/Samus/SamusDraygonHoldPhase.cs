namespace SuperMetroid.Core.Samus;

/// <summary>Where Samus is in Draygon's hold (<c>Phase</c>).</summary>
public enum SamusDraygonHoldPhase
{
    /// <summary>Draygon does not hold her, and no release waits for Draygon to see it.</summary>
    Inactive,

    /// <summary>Draygon holds her and keeps her still; the escape check counts her presses of the directions.</summary>
    Held,

    /// <summary>
    /// Draygon still holds her and the escape check keeps counting, but another owner has taken her movement, so the hold no
    /// longer keeps her still.
    /// </summary>
    HeldMovementTakenOver,

    /// <summary>
    /// She has been released, and Draygon has not yet seen it: the next time Draygon carries her, it places her once more,
    /// sees the release and starts flying upward.
    /// </summary>
    ReleasedUnseen,
}

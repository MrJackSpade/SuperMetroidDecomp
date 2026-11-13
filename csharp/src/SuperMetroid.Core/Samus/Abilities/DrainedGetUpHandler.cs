namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The three timer/hack handlers relevant to draining.</summary>
public enum DrainedGetUpHandler
{
    /// <summary>The drained Up-input behaviour is no longer installed, either after a successful stand request or because another timer/hack owner replaced it.</summary>
    Inactive,
    /// <summary>Accepts a fresh Up press only in left crouching <c>DrainedCrouchingLeftPose</c> at animation index eight or later, starts index thirteen with timer one, then disables itself.</summary>
    AbleToStand,
    /// <summary>Accepts a fresh Up press at animation indices eight through eleven, starts failed-stand index eighteen with timer one, and stays installed for later attempts.</summary>
    UnableToStand,
}

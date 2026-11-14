namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>How drained Samus answers a fresh Up press in the check that runs after each of her movements.</summary>
public enum DrainedGetUpCheck
{
    /// <summary>Up no longer gets her up, either after a successful stand request or because the escape countdown's display took the check's place.</summary>
    Inactive,
    /// <summary>Accepts a fresh Up press only in left crouching <see cref="Poses.SamusPoseId.DrainedCrouchingLeftPose"/> at animation index eight or later, starts index thirteen with timer one, then ends itself.</summary>
    AbleToStand,
    /// <summary>Accepts a fresh Up press at animation indices eight through eleven, starts failed-stand index eighteen with timer one, and stays for later attempts.</summary>
    UnableToStand,
}

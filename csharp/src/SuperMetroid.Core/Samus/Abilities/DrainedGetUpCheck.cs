namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>How drained Samus answers a fresh Up press in the check that runs after each of her movements.</summary>
public enum DrainedGetUpCheck
{
    /// <summary>Up no longer gets her up, either after a successful stand request or because the escape countdown's display took the check's place.</summary>
    Inactive,
    /// <summary>Accepts a fresh Up press only in left crouching <see cref="Poses.SamusPoseId.DrainedCrouchingLeftPose"/> from <see cref="SamusDrainedAnimationDefinitions.AbleToStandFirstFrame"/> on, starts the stand-up at <see cref="SamusDrainedAnimationDefinitions.AbleToStandFrame"/> for one tick, then ends itself.</summary>
    AbleToStand,
    /// <summary>Accepts a fresh Up press from <see cref="SamusDrainedAnimationDefinitions.UnableToStandFirstFrame"/> through <see cref="SamusDrainedAnimationDefinitions.UnableToStandLastFrame"/>, starts the failed stand at <see cref="SamusDrainedAnimationDefinitions.UnableToStandFrame"/> for one tick, and stays for later attempts.</summary>
    UnableToStand,
}

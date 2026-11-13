namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The posture Mother Brain's body animation cues set.</summary>
public enum MotherBrainBodyPose
{
    /// <summary>Standing, which allows posture changes and ordinary attack and walking decisions.</summary>
    Standing,
    /// <summary>Walking, while the walking animation owns the body's displacement.</summary>
    Walking,
    /// <summary>Crouching or rising; posture changes wait for the animation to reach an endpoint.</summary>
    CrouchingTransition,
    /// <summary>At the crouched endpoint; standing up selects the matching rising animation.</summary>
    Crouched,
    /// <summary>Firing the red death beam from her body.</summary>
    DeathBeam,
    /// <summary>At the leaning-down endpoint; standing up requires its distinct recovery animation.</summary>
    LeaningDown,
}

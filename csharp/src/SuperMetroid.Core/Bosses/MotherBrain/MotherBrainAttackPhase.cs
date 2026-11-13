namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The step of one of Mother Brain's phase-two attacks. Its values are arbitrary.</summary>
public enum MotherBrainAttackPhase
{
    /// <summary>Selects the next head/body attack using Samus movement and sampled RNG, then seeds the 64-update cooldown. A new attack starts here.</summary>
    ChooseAttack,
    /// <summary>Decrements the unsigned attack cooldown until zero while the chosen head animation runs independently.</summary>
    Cooldown,
    /// <summary>Resets the attack steps and returns the body to ordinary phase-two thinking.</summary>
    EndAttack,
}

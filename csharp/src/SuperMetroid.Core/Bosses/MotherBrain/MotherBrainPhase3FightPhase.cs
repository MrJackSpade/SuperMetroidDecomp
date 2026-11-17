namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The phases of Mother Brain's third-phase fight (#2208), from her recovery to her attacks against the Hyper Beam. Its values are arbitrary.</summary>
public enum MotherBrainPhase3FightPhase
{
    /// <summary>Changes her form to <see cref="MotherBrainForm.ThirdPhase"/>, requests a fourteen-pixel backward target, and starts the combat-setup wait.</summary>
    RecoverMakeSomeDistance,

    /// <summary>When the wait runs out, installs the normal neck and walking steps and immediately runs the first phase-three fighting call.</summary>
    RecoverSetupForFighting,

    /// <summary>Checks head death before neck/walking work, then uses standing pose, attack admission, and sampled RNG to choose bombs or four onion rings.</summary>
    FightingMain,

    /// <summary>Pauses the neck and walking steps through the attack's cooldown and resumes main fighting only as it runs out.</summary>
    FightingAttackCooldown,
}

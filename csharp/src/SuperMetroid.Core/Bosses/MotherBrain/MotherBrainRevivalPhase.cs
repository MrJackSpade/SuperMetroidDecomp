namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The phases of Mother Brain's revival (#2208): coming back to life and attacking the Baby Metroid until its last blow. Its values are arbitrary.</summary>
public enum MotherBrainRevivalPhase
{
    /// <summary>Starts the 768-count revival wait, whose first step comes on the next update.</summary>
    InanimateGrey,

    /// <summary>When the wait runs out, re-enables breath/drool, clears the colour step counter, and starts the 224-count color-restoration delay.</summary>
    ShowSignsOfLife,

    /// <summary>Reverses the eight drained-grey colour steps when the timer runs out, then re-enables brain colour handling and falls into wake-up.</summary>
    TransitionFromGrey,

    /// <summary>Waits until she stands, then extends the neck and immediately begins a 16-count wake-up stretch.</summary>
    WakeUp,

    /// <summary>Expires the short stretch delay, starts the phase-three stretching head animation, and seeds a 128-count walking delay.</summary>
    WakeUpStretch,

    /// <summary>After the wait repeatedly requests forward walking until X passes 80, then enables health-based colours and prepares the neck.</summary>
    WalkUpToBaby,

    /// <summary>Clears the baby attack counter and configures both neck targets before falling into the stand-up handshake.</summary>
    PrepareNeckForBabyDeath,

    /// <summary>Waits for standing, requests really-slow forward walking, and immediately enters the baby-attack chooser.</summary>
    FinishPreparingForBabyDeath,

    /// <summary>A random number may change her posture or fire the targeted onion rings, aimed at Samus when there is no Baby.</summary>
    MurderBabyAttack,

    /// <summary>Waits for the 64-count timer to pass zero before returning to baby-attack selection.</summary>
    MurderBabyAttackCooldown,

    /// <summary>Repeatedly requests standing and backward walking to X 64; the baby owns replacement of this body phase.</summary>
    PrepareForFinalBabyAttack,

    /// <summary>Installs the final targeted head volley and selects the following inert step.</summary>
    ExecuteFinalBabyAttack,

    /// <summary>Holds while the head volley and baby actor independently own the final damage, death, and recovery sequence.</summary>
    FinalBabyAttackHolding,
}

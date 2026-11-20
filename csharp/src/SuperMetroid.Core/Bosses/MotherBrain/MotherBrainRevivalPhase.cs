namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The phases of Mother Brain's revival (#2208): coming back to life and attacking the Baby Metroid until its last blow. Its values are arbitrary.</summary>
public enum MotherBrainRevivalPhase
{
    /// <summary>Starts the revival wait, whose first update is the next.</summary>
    InanimateGrey,

    /// <summary>When the wait runs out, starts her breath and drool again, starts her colours' return from its first step, and starts the wait before that step.</summary>
    ShowSignsOfLife,

    /// <summary>
    /// Returns her brain from grey step by step, one each time its wait runs out; then her brain's colours follow it
    /// again and her waking runs on the same update.
    /// </summary>
    TransitionFromGrey,

    /// <summary>Waits until she stands, then extends the neck and immediately begins the wake-up stretch.</summary>
    WakeUp,

    /// <summary>Waits out the short stretch, starts the phase-three stretching head animation, and starts the wait before walking.</summary>
    WakeUpStretch,

    /// <summary>After the wait repeatedly requests forward walking until she passes <see cref="MotherBrainLateFightDefinitions.BabyWalkUpX"/>, then enables health-based colours and prepares the neck.</summary>
    WalkUpToBaby,

    /// <summary>Clears the baby attack counter and configures both neck targets before she stands up.</summary>
    PrepareNeckForBabyDeath,

    /// <summary>Waits for standing, requests really-slow forward walking, and immediately enters the baby-attack chooser.</summary>
    FinishPreparingForBabyDeath,

    /// <summary>A random number may change her posture or fire the targeted onion rings, aimed at Samus when there is no Baby.</summary>
    MurderBabyAttack,

    /// <summary>Waits out the attack's cooldown before returning to baby-attack selection.</summary>
    MurderBabyAttackCooldown,

    /// <summary>Repeatedly requests standing and backward walking to her final retreat; the baby owns replacement of this body phase.</summary>
    PrepareForFinalBabyAttack,

    /// <summary>Her head fires its last volley at the Baby, and her body then holds.</summary>
    ExecuteFinalBabyAttack,

    /// <summary>Holds while the head volley and baby actor independently own the final damage, death, and recovery sequence.</summary>
    FinalBabyAttackHolding,
}

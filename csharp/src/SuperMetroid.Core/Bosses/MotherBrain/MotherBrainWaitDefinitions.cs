namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// How long Mother Brain's waits last through her fake death, her rise, her second phase's laser and her finishing Samus
/// off (#2311): each is the updates it lasts, its last being the update it runs out on (<see cref="BossCountdown.LastFor"/>).
/// </summary>
internal static partial class MotherBrainWaitDefinitions
{
    /// <summary>The updates her fake death waits once her first phase ends before it locks Samus in the arena.</summary>
    internal const int BeforeLockUpdates = 65;

    /// <summary>The updates it waits after locking Samus in before the music stops and her own song loads.</summary>
    internal const int BeforeMusicUpdates = 33;

    /// <summary>The updates it waits after her song loads before Samus is free to move again.</summary>
    internal const int BeforeUnlockUpdates = 13;

    /// <summary>The updates it waits after Samus is free before the room starts to flash and the tubes collapse.</summary>
    internal const int BeforeFlashUpdates = 9;

    /// <summary>The updates each step of her fake death's fade to grey lasts.</summary>
    internal const int FakeDeathGreyStepUpdates = 9;

    /// <summary>The updates between the explosions about her tank as she fakes her death.</summary>
    internal const int FakeDeathExplosionIntervalUpdates = 9;

    /// <summary>The updates of the suspense before she prepares to rise, her brain set up on its neck.</summary>
    internal const int SuspenseUpdates = 129;

    /// <summary>The updates she prepares to rise before the floor starts to hide her legs.</summary>
    internal const int BeforeRisingUpdates = 33;

    /// <summary>
    /// The updates from the floor starting to hide her legs to her pause before rising, the first being the update it
    /// starts and the last the pause's first.
    /// </summary>
    internal const int BeforePauseUpdates = 12;

    /// <summary>The updates of the pause before her hidden body is placed to rise.</summary>
    internal const int StandingBodyPauseUpdates = 257;

    /// <summary>The updates each step of her colours' return from grey lasts, once she has risen.</summary>
    internal const int FromGreyStepUpdates = 5;

    /// <summary>The updates her head shakes menacingly before she stretches.</summary>
    internal const int MenacingShakeUpdates = 24;

    /// <summary>The updates of her stretch before she brings her head back up.</summary>
    internal const int StretchUpdates = 257;

    /// <summary>The updates after bringing her head back up before her second phase starts thinking.</summary>
    internal const int AfterStretchUpdates = 65;

    /// <summary>The updates her neck turns quickly toward Samus before it slows and she fires her laser.</summary>
    internal const int LaserQuickAimUpdates = 5;

    /// <summary>The updates after her laser fires before her neck bobs again and she thinks again.</summary>
    internal const int LaserRecoveryUpdates = 17;

    /// <summary>The updates her second phase cools down after choosing an attack before it thinks again.</summary>
    internal const int AttackCooldownUpdates = 64;

    /// <summary>The updates she stands admiring the weakened Samus before she stretches for her final beam.</summary>
    internal const int AdmireUpdates = 17;

    /// <summary>The updates she stretches before charging her final beam.</summary>
    internal const int FinalBeamChargeUpdates = 257;

    /// <summary>
    /// The updates from her final beam's charge starting to the Baby Metroid's arrival, the first being the stretch's last.
    /// </summary>
    internal const int BabyArrivalUpdates = 4;

    /// <summary>The updates from the Baby's arrival to her final beam's firing.</summary>
    internal const int FinalBeamFireDelayUpdates = 257;
}

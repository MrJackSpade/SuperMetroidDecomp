namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The waits, reaches and limits of Mother Brain's late fight (#2254): her rainbow beam, her revival and the Baby's last
/// attacks, her third phase's walks and recoils, and her third-phase death through the escape's start. Each wait is the
/// updates it lasts, its last being the update it runs out on (#2311).
/// </summary>
internal static class MotherBrainLateFightDefinitions
{
    /// <summary>The energy at or above which Samus can stand as the rainbow beam takes her.</summary>
    internal const int SamusStandsInBeamHealth = 700;

    /// <summary>The updates her neck extends before the rainbow beam's charge starts.</summary>
    internal const int NeckExtendUpdates = 257;

    /// <summary>The updates she waits, head drawn back, for the rainbow beam's charge.</summary>
    internal const int ChargeWaitUpdates = 257;

    /// <summary>The updates her neck lowers before the rainbow beam fires (the Japanese and North American value; the European release lowers it for 13).</summary>
    internal const int NeckLoweringUpdates = 17;

    /// <summary>The updates between the rainbow beam's explosions on Samus.</summary>
    internal const int BeamExplosionIntervalUpdates = 9;

    /// <summary>The energy the rainbow beam drains each update from Samus wearing the Varia Suit.</summary>
    internal const int BeamDrainWithVaria = 1;

    /// <summary>The energy the rainbow beam drains each update from Samus without the Varia Suit.</summary>
    internal const int BeamDrainWithoutVaria = 2;

    /// <summary>The updates the room shakes as the rainbow beam pins Samus.</summary>
    internal const int BeamPinShakeUpdates = 8;

    /// <summary>The updates the rainbow beam drains Samus.</summary>
    internal const int DrainUpdates = 300;

    /// <summary>The updates the room shakes as the rainbow beam drains Samus: all of the drain but its last update.</summary>
    internal const int DrainShakeUpdates = 299;

    /// <summary>The updates she lowers her head after the rainbow beam before deciding what to do next.</summary>
    internal const int LowerHeadUpdates = 129;

    /// <summary>The energy below which Samus, after a rainbow beam, is finished off.</summary>
    internal const int FinishOffSamusHealth = 400;

    /// <summary>How far forward the walk that starts the finishing off aims.</summary>
    internal const int FinishOffStepX = 16;

    /// <summary>The Baby attack count the counter stops at.</summary>
    internal const int BabyAttackCountLimit = 12;

    /// <summary>The updates she lies grey and inanimate before showing signs of life.</summary>
    internal const int InanimateUpdates = 769;

    /// <summary>The updates before the first step of her revival from grey.</summary>
    internal const int RevivalFirstGreyStepUpdates = 225;

    /// <summary>The updates she stretches as she wakes up.</summary>
    internal const int WakeUpStretchUpdates = 17;

    /// <summary>The updates before she walks up to the Baby.</summary>
    internal const int BeforeWalkUpToBabyUpdates = 129;

    /// <summary>Where she walks up to the Baby: her walks toward it aim here, and she has reached it once past it.</summary>
    internal const int BabyWalkUpX = 80;

    /// <summary>The updates she waits after each attack on the Baby.</summary>
    internal const int BabyAttackCooldownUpdates = 65;

    /// <summary>Where she walks back to as the Baby prepares its final attack.</summary>
    internal const int FinalBabyAttackRetreatX = 64;

    /// <summary>How far back each of her third phase's retreats steps from where it starts.</summary>
    internal const int Phase3RetreatPixels = 14;

    /// <summary>The walk credit each standing turn earns as her third phase tries to inch forward.</summary>
    internal const int Phase3CreditPerTurn = 32;

    /// <summary>The walk credit she keeps after her third phase's retreats.</summary>
    internal const int Phase3AfterRetreatCredit = 64;

    /// <summary>The updates her neck recovers from a recoil before her next attack.</summary>
    internal const int RecoilRecoveryUpdates = 17;

    /// <summary>The updates her neck recoils from a Hyper Beam hit.</summary>
    internal const int HyperBeamRecoilUpdates = 12;

    /// <summary>The updates her third phase sets up for fighting after making some distance.</summary>
    internal const int RecoverSetupUpdates = 33;

    /// <summary>The updates she waits after each of her third phase's attacks.</summary>
    internal const int Phase3AttackCooldownUpdates = 65;

    /// <summary>The updates she idles, exploding, at the back of the room as she dies.</summary>
    internal const int IdleExplodingUpdates = 129;

    /// <summary>Where she stumbles to, in the middle of the room, as she dies.</summary>
    internal const int DyingStumbleX = 96;

    /// <summary>The updates before her dying brain's effects stop.</summary>
    internal const int BeforeBrainEffectsStopUpdates = 33;

    /// <summary>The updates each step of her dying body's fade lasts.</summary>
    internal const int BodyFadeStepUpdates = 17;

    /// <summary>The updates of her last few explosions after her body is gone.</summary>
    internal const int FinalExplosionsUpdates = 17;

    /// <summary>The updates the room shakes as her severed brain lands.</summary>
    internal const int BrainLandingShakeUpdates = 20;

    /// <summary>The updates her landed brain settles, from the update after it lands, before she lies still to grey.</summary>
    internal const int BrainSettlesUpdates = 6;

    /// <summary>The updates before her corpse turns grey.</summary>
    internal const int BeforeCorpseGreyUpdates = 33;

    /// <summary>The updates her grey corpse tips over.</summary>
    internal const int CorpseTipsOverUpdates = 257;

    /// <summary>
    /// Where each dust cloud of her rotting corpse shows from her brain before its random spread to the right: sixteen
    /// pixels left of it and sixteen below.
    /// </summary>
    internal static readonly Offset16<RoomPixels> CorpseDustFromBrain = new(-16, 16);

    /// <summary>The updates of the pause before the escape, once her corpse has rotted away.</summary>
    internal const int BeforeEscapeUpdates = 21;

    /// <summary>The updates the room holds, from the update after the brain is parked, before the escape starts.</summary>
    internal const int HoldBeforeEscapeUpdates = 7;

    /// <summary>
    /// The updates the escape's start waits, the first being the hold's last, before its music, quake, red flashing and
    /// text start.
    /// </summary>
    internal const int EscapeStartUpdates = 2;

    /// <summary>The updates before the escape's subtitle or text starts.</summary>
    internal const int EscapeTextDelayUpdates = 33;

    /// <summary>The updates the escape door's dust bursts before the escape timer starts.</summary>
    internal const int EscapeDoorCountdownUpdates = 33;
}

using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Ending;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Verification's native view of the cinematic actors' motions (#1903): the bank-$8B pre-instruction routine each Core
/// motion, cue and actor role reproduces. Core's actors carry no routine; the ROM oracles compare a definition's routine
/// word, and the routines an animation installs, with the motion, the cue or the role Core names through this table.
/// </summary>
internal static class CinematicActorNativeMotions
{
    /// <summary>The shared routine that does nothing: an actor with no motion of its own.</summary>
    internal const ushort NoMotion = 0x93d9;

    /// <summary>The Metroid egg's routines, by what the egg waits for.</summary>
    internal static readonly IReadOnlyDictionary<MetroidEggBehavior, ushort> MetroidEgg = new Dictionary<MetroidEggBehavior, ushort>
    {
        [MetroidEggBehavior.WaitsForSamus] = 0xa8e8,
        [MetroidEggBehavior.Hatches] = NoMotion,
        [MetroidEggBehavior.WaitsForCrossFadeEnd] = 0xa903,
    };

    /// <summary>The egg shell fragments' flight.</summary>
    internal const ushort EggShellFragment = 0xa994;

    /// <summary>The egg's slime drops' flight.</summary>
    internal const ushort EggSlimeDrop = 0xaab3;

    /// <summary>The delivered baby's routine; the laboratory scene moves the page and the baby itself.</summary>
    internal const ushort DeliveredBaby = 0xad68;

    /// <summary>The examined baby's routine; the laboratory scene moves the page and the baby itself.</summary>
    internal const ushort ExaminedBaby = 0xada6;

    /// <summary>Mother Brain's cross-fade into page two.</summary>
    internal const ushort MotherBrainCrossFade = 0xb82e;

    /// <summary>The intro Rinkas' flights.</summary>
    internal static readonly IReadOnlyDictionary<IntroRinkaFlight, ushort> RinkaFlights = new Dictionary<IntroRinkaFlight, ushort>
    {
        [IntroRinkaFlight.StrikesSamus] = 0xb8d8,
        [IntroRinkaFlight.MissesSamus] = 0xb93b,
    };

    /// <summary>The confused baby's motions.</summary>
    internal static readonly IReadOnlyDictionary<ConfusedBabyMotion, ushort> ConfusedBaby = new Dictionary<ConfusedBabyMotion, ushort>
    {
        [ConfusedBabyMotion.WaitingForHatch] = 0xba5e,
        [ConfusedBabyMotion.Rising] = 0xba73,
        [ConfusedBabyMotion.Idling] = 0xbb0d,
        [ConfusedBabyMotion.Dancing] = 0xbb24,
    };

    /// <summary>The opening star field's rush toward Ceres.</summary>
    internal const ushort CeresFrontStars = 0xbeb5;

    /// <summary>The Ceres rear view's actors' drift, by role.</summary>
    internal static readonly IReadOnlyDictionary<CeresRearViewActorRole, ushort> CeresRearView = new Dictionary<CeresRearViewActorRole, ushort>
    {
        [CeresRearViewActorRole.LargeAsteroids] = 0xbf35,
        [CeresRearViewActorRole.StationUnderAttack] = 0xbf5f,
        [CeresRearViewActorRole.SmallAsteroids] = 0xbf89,
        [CeresRearViewActorRole.Vortex] = 0xbfc6,
        [CeresRearViewActorRole.RearStars] = 0xbfc6,
    };

    /// <summary>The Ceres destruction's actors' drift, by role: the vortex holds still.</summary>
    internal static readonly IReadOnlyDictionary<CeresDestructionActorRole, ushort> CeresDestruction = new Dictionary<CeresDestructionActorRole, ushort>
    {
        [CeresDestructionActorRole.LargeAsteroids] = 0xbf35,
        [CeresDestructionActorRole.SmallAsteroids] = 0xbf89,
        [CeresDestructionActorRole.Vortex] = 0xbfd9,
    };

    /// <summary>The Ceres repeating explosions' spawning.</summary>
    internal const ushort CeresRepeatingExplosions = 0xc489;

    /// <summary>A Ceres explosion's drift with the fleeing gunship.</summary>
    internal const ushort CeresExplosion = 0xc582;

    /// <summary>The Zebes reveal's actors' wait before the scene slides, by role.</summary>
    internal static readonly IReadOnlyDictionary<ZebesRevealActorRole, ushort> ZebesRevealWait = new Dictionary<ZebesRevealActorRole, ushort>
    {
        [ZebesRevealActorRole.Planet] = 0xc84e,
        [ZebesRevealActorRole.StarsUpperLeft] = 0xc8f9,
        [ZebesRevealActorRole.StarsUpperRight] = 0xc8f9,
        [ZebesRevealActorRole.StarsLowerLeft] = 0xc8f9,
        [ZebesRevealActorRole.StarsLowerRight] = 0xc8aa,
        [ZebesRevealActorRole.Title] = NoMotion,
        [ZebesRevealActorRole.JapaneseSubtitle] = NoMotion,
    };

    /// <summary>The Zebes reveal's actors' slide away, by role; the captions do not slide.</summary>
    internal static readonly IReadOnlyDictionary<ZebesRevealActorRole, ushort> ZebesRevealSlide = new Dictionary<ZebesRevealActorRole, ushort>
    {
        [ZebesRevealActorRole.Planet] = 0xc85d,
        [ZebesRevealActorRole.StarsUpperLeft] = 0xc908,
        [ZebesRevealActorRole.StarsUpperRight] = 0xc908,
        [ZebesRevealActorRole.StarsLowerLeft] = 0xc908,
        [ZebesRevealActorRole.StarsLowerRight] = 0xc8b9,
    };

    /// <summary>The logo's S halves moving into place, upper then lower.</summary>
    internal static readonly IReadOnlyList<ushort> LogoHalves = [0xf1e7, 0xf227];

    /// <summary>The exploding planet's stars' flyaway motion.</summary>
    internal static readonly IReadOnlyDictionary<EndingStarsMotion, ushort> FlyawayStars = new Dictionary<EndingStarsMotion, ushort>
    {
        [EndingStarsMotion.WaitingForFlyaway] = 0xf35a,
        [EndingStarsMotion.Moving] = 0xf375,
    };

    /// <summary>The suitless reward's upper body's routine, which does nothing.</summary>
    internal const ushort RewardSuitlessUpper = 0xf51c;

    /// <summary>The reward Samus's body's motions on her jump.</summary>
    internal static readonly IReadOnlyDictionary<EndingRewardJumpMotion, ushort> RewardBody = new Dictionary<EndingRewardJumpMotion, ushort>
    {
        [EndingRewardJumpMotion.Rising] = 0xf528,
        [EndingRewardJumpMotion.Falling] = 0xf5dd,
    };

    /// <summary>The suited reward Samus's head's flight.</summary>
    internal const ushort RewardHeadFlight = 0xf57f;

    /// <summary>Every bank-$8B routine the table names.</summary>
    internal static IEnumerable<ushort> NativeAddresses =>
    [
        NoMotion, .. MetroidEgg.Values, EggShellFragment, EggSlimeDrop, DeliveredBaby, ExaminedBaby, MotherBrainCrossFade,
        .. RinkaFlights.Values, .. ConfusedBaby.Values, CeresFrontStars, .. CeresRearView.Values, .. CeresDestruction.Values,
        CeresRepeatingExplosions, CeresExplosion, .. ZebesRevealWait.Values, .. ZebesRevealSlide.Values, .. LogoHalves,
        .. FlyawayStars.Values, RewardSuitlessUpper, .. RewardBody.Values, RewardHeadFlight,
    ];
}

using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Verification.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>
/// The native bank-$8B lists each typed cinematic animation replaces, for the #641 ROM oracle (#2197): where each animation an
/// actor starts begins, and the owner instructions each family's lists hold, by the family's own cue.
/// </summary>
internal static class CinematicSpriteNativeListAddresses
{
    /// <summary>The cinematic sprite lists' bank.</summary>
    internal const byte Bank = 0x8b;

    /// <summary>
    /// Where each animation an actor starts begins natively, the address an actor's definition names: its counter load where
    /// one precedes its list, else its list.
    /// </summary>
    internal static readonly IReadOnlyDictionary<Enum, ushort> Starts = new Dictionary<Enum, ushort>
    {
        [IntroMotherBrainAnimation.Pulsing] = 0xcb05,
        [IntroMotherBrainAnimation.StartingNextPage] = 0xcb19,
        [IntroBabyAnimation.Egg] = 0xcb33,
        [IntroBabyAnimation.EggHatching] = 0xcb3b,
        [IntroBabyAnimation.DeliveredBaby] = 0xcb9f,
        [IntroBabyAnimation.ExaminedBaby] = 0xcbcd,
        [IntroNarrationAnimation.Caret] = 0xcbfb,
        [IntroNarrationAnimation.CaretBlinking] = 0xcc03,
        [IntroNarrationAnimation.NextPageArrow] = 0xcc0f,
        [IntroNarrationAnimation.NextPageArrowDelayed] = 0xcc23,
        [IntroBabyAnimation.ConfusedBaby] = 0xcc2b,
        [CeresAnimation.DestructionLargeAsteroid] = 0xcc3f,
        [CeresAnimation.StationUnderAttack] = 0xcc47,
        [CeresAnimation.SmallAsteroid] = 0xcc4f,
        [CeresAnimation.Vortex] = 0xcc57,
        [CeresAnimation.ZebesPlanet] = 0xccab,
        [CeresAnimation.ZebesTitle] = 0xccbb,
        [CeresAnimation.ZebesJapaneseTitle] = 0xccd5,
        [CeresAnimation.SmallExplosion] = 0xccdb,
        [CeresAnimation.RepeatingSmallExplosion] = 0xccf5,
        [CeresAnimation.LargeExplosion] = 0xcd1b,
        [IntroEggShellFragment.UpperLeft] = 0xcd39,
        [IntroEggShellFragment.UpperRight] = 0xcd41,
        [IntroEggShellFragment.Left] = 0xcd49,
        [IntroEggShellFragment.Centre] = 0xcd51,
        [IntroEggShellFragment.Right] = 0xcd59,
        [IntroEggShellFragment.Lower] = 0xcd61,
        [IntroEggEffectAnimation.SlimeDrop] = 0xcd69,
        [IntroEggEffectAnimation.SlimeImpact] = 0xcd71,
        [CeresAnimation.ZebesStarsUpperLeft] = 0xcd83,
        [CeresAnimation.ZebesStarsUpperRight] = 0xcd8b,
        [CeresAnimation.ZebesStarsLowerLeft] = 0xcd93,
        [CeresAnimation.ZebesStarsLowerRight] = 0xcd9b,
        [CeresAnimation.Stars] = 0xcda3,
        [IntroMotherBrainExplosionAnimation.Big] = 0xcdab,
        [IntroMotherBrainExplosionAnimation.Small] = 0xcdcb,
        [IntroRinkaAnimation.Rinka] = 0xcdeb,
        [CeresAnimation.StationBlast] = 0xce1b,
        [CeresAnimation.ExplosionSpawner] = 0xce35,
        [CeresAnimation.LargeAsteroid] = 0xce4b,
        [EndingSceneAnimation.ExplodingPlanet] = 0xeb0f,
        [EndingSceneAnimation.ExplosionGlow] = 0xeb3d,
        [EndingSceneAnimation.ExplosionStars] = 0xeb51,
        [EndingSceneAnimation.ExplosionLava] = 0xeb59,
        [EndingSceneAnimation.ExplosionSilhouette] = 0xeb69,
        [EndingSceneAnimation.ExplosionRightStars] = 0xeb71,
        [EndingSceneAnimation.ExplosionLeftStars] = 0xeb81,
        [EndingSceneAnimation.ExplosionAfterglow] = 0xeb89,
        [EndingTypedText.OperationWas] = 0xeb91,
        [EndingTypedText.CompletedSuccessfully] = 0xebd7,
        [EndingTypedText.ClearTime] = 0xec35,
        [EndingSceneAnimation.ClearTimeDigit0] = 0xec81,
        [EndingSceneAnimation.ClearTimeDigit1] = 0xec89,
        [EndingSceneAnimation.ClearTimeDigit2] = 0xec91,
        [EndingSceneAnimation.ClearTimeDigit3] = 0xec99,
        [EndingSceneAnimation.ClearTimeDigit4] = 0xeca1,
        [EndingSceneAnimation.ClearTimeDigit5] = 0xeca9,
        [EndingSceneAnimation.ClearTimeDigit6] = 0xecb1,
        [EndingSceneAnimation.ClearTimeDigit7] = 0xecb9,
        [EndingSceneAnimation.ClearTimeDigit8] = 0xecc1,
        [EndingSceneAnimation.ClearTimeDigit9] = 0xecc9,
        [EndingSceneAnimation.ClearTimeColon] = 0xecd1,
        [EndingSceneAnimation.AnimalEscape] = 0xecd9,
        [EndingSceneAnimation.TopCloudPattern] = 0xeced,
        [EndingSceneAnimation.TopCloudEdge] = 0xecf5,
        [EndingSceneAnimation.BottomCloudEdge] = 0xecfd,
        [EndingSceneAnimation.BottomCloudPattern] = 0xed05,
        [EndingSceneAnimation.RightSideCloud] = 0xed0d,
        [EndingSceneAnimation.LeftSideCloud] = 0xed15,
        [EndingSceneAnimation.SuitlessIdle] = 0xed1d,
        [EndingSceneAnimation.SuitlessLegsIdle] = 0xed25,
        [EndingRewardAnimation.HairRelease] = 0xed2d,
        [EndingRewardAnimation.HairReleaseLegs] = 0xed59,
        [EndingRewardAnimation.SuitlessJump] = 0xed7f,
        [EndingRewardAnimation.Falling] = 0xed95,
        [EndingRewardAnimation.Landing] = 0xed9d,
        [EndingSceneAnimation.SuitedIdle] = 0xedb1,
        [EndingSceneAnimation.HelmetIdle] = 0xedb9,
        [EndingSceneAnimation.HeadIdle] = 0xedc1,
        [EndingRewardAnimation.GestureBody] = 0xedc9,
        [EndingRewardAnimation.GestureArm] = 0xedd3,
        [EndingRewardAnimation.GestureHelmet] = 0xee0f,
        [EndingRewardAnimation.GestureHead] = 0xee15,
        [EndingRewardAnimation.SuitedJump] = 0xee27,
        [EndingRewardAnimation.HelmetJump] = 0xee3d,
        [EndingRewardAnimation.HeadJump] = 0xee4d,
        [EndingLogoAnimation.UpperS] = 0xee5d,
        [EndingLogoAnimation.LowerS] = 0xee65,
        [EndingLogoAnimation.UpperCircle] = 0xee6d,
        [EndingLogoAnimation.LowerCircle] = 0xee87,
    };

    /// <summary>What the intro Mother Brain's lists have her owner do.</summary>
    internal static NativeCinematicCues<IntroMotherBrainCue> MotherBrainCues { get; } = new(
        new Dictionary<ushort, IntroMotherBrainCue> { [0xb336] = IntroMotherBrainCue.StartNextPage },
        new Dictionary<ushort, IntroMotherBrainCue> { [CinematicActorNativeMotions.MotherBrainCrossFade] = IntroMotherBrainCue.StartCrossFade });

    /// <summary>What the discovery scene egg's lists have their owner do.</summary>
    internal static NativeCinematicCues<IntroDiscoveryCue> DiscoveryCues { get; } = new(
        new Dictionary<ushort, IntroDiscoveryCue> { [0xa918] = IntroDiscoveryCue.SpawnShellPieces, [0xb33e] = IntroDiscoveryCue.StartNextPage },
        new Dictionary<ushort, IntroDiscoveryCue>
        {
            [CinematicActorNativeMotions.MetroidEgg[MetroidEggBehavior.WaitsForCrossFadeEnd]] = IntroDiscoveryCue.DeleteEggAfterCrossFade,
        });

    /// <summary>What the laboratory Baby's lists have their owner do.</summary>
    internal static NativeCinematicCues<IntroLaboratoryCue> LaboratoryCues { get; } = new(
        new Dictionary<ushort, IntroLaboratoryCue> { [0xb346] = IntroLaboratoryCue.StartNextPage, [0xb34e] = IntroLaboratoryCue.StartNextPage },
        new Dictionary<ushort, IntroLaboratoryCue>());

    /// <summary>What the intro Rinka's list has its owner do.</summary>
    internal static NativeCinematicCues<IntroRinkaCue> RinkaCues { get; } = new(
        new Dictionary<ushort, IntroRinkaCue> { [0xb8c5] = IntroRinkaCue.StartMoving }, new Dictionary<ushort, IntroRinkaCue>());

    /// <summary>
    /// The original's invisible intro Rinka spawner list (#2561): the port launches the flashback's Rinkas on a schedule of
    /// waits instead (<see cref="IntroRinkaDefinitions.Launches"/>), which the list's cues time.
    /// </summary>
    internal const ushort RinkaSpawnerList = 0xce0d;

    /// <summary>What the original's intro Rinka spawner list has its owner do: launch the striking pair, then the missing pair.</summary>
    internal enum NativeRinkaSpawnerCue
    {
        /// <summary>Launches the pair whose first Rinka strikes Samus.</summary>
        SpawnStrikingPair,
        /// <summary>Launches the pair that both miss Samus.</summary>
        SpawnMissingPair,
    }

    /// <summary>What the intro Rinka spawner's list has its owner do.</summary>
    internal static NativeCinematicCues<NativeRinkaSpawnerCue> RinkaSpawnerCues { get; } = new(
        new Dictionary<ushort, NativeRinkaSpawnerCue> { [0xba21] = NativeRinkaSpawnerCue.SpawnStrikingPair, [0xba36] = NativeRinkaSpawnerCue.SpawnMissingPair },
        new Dictionary<ushort, NativeRinkaSpawnerCue>());

    /// <summary>What the Ceres scenes' lists have their owner do: the PLANET ZEBES title's cues.</summary>
    internal static NativeCinematicCues<CeresCue> CeresCues { get; } = new(
        new Dictionary<ushort, CeresCue>
        {
            [0xc9a5] = CeresCue.FadeInZebesTitle,
            [0xc9af] = CeresCue.SpawnJapaneseTitleIfNeeded,
            [0xc9bd] = CeresCue.FadeOutZebesTitle,
            [0xc9c7] = CeresCue.StartFlyingToZebes,
        },
        new Dictionary<ushort, CeresCue>());

    /// <summary>
    /// What the Ceres explosion spawner's list has its owner do: its two waves, and the repeating blasts the routine it
    /// installs spawns (#2489).
    /// </summary>
    internal static NativeCinematicCues<CeresExplosionSpawnerCue> ExplosionSpawnerCues { get; } = new(
        new Dictionary<ushort, CeresExplosionSpawnerCue> { [0xc404] = CeresExplosionSpawnerCue.SpawnInitialWave, [0xc50c] = CeresExplosionSpawnerCue.SpawnFinalWave },
        new Dictionary<ushort, CeresExplosionSpawnerCue>())
    {
        Routines = new Dictionary<ushort, Func<IImportCartridgeSource, INativeCinematicRoutine<CeresExplosionSpawnerCue>>>
        {
            [CinematicActorNativeMotions.CeresRepeatingExplosions] = cartridge => new NativeCeresRepeatingExplosions(cartridge),
        },
    };

    /// <summary>What the ending scenes' lists have the escape scenes do: the explosion's and the result text's cues.</summary>
    internal static NativeCinematicCues<EndingSceneCue> EndingSceneCues { get; } = new(
        new Dictionary<ushort, EndingSceneCue>
        {
            [0xf32b] = EndingSceneCue.EndZebesExplosion,
            [0xf2fa] = EndingSceneCue.ExplosionFinale,
            [0xf284] = EndingSceneCue.FadeExplosionColours,
            [0xf295] = EndingSceneCue.SpawnExplosionSilhouette,
            [0xf2b7] = EndingSceneCue.StartZebesExplosion,
            [0xf3b0] = EndingSceneCue.SpawnCompletedText,
            [0xf3ce] = EndingSceneCue.SpawnClearTime,
            [0xf41b] = EndingSceneCue.SpawnHoursTens,
            [0xf424] = EndingSceneCue.SpawnHoursUnits,
            [0xf42d] = EndingSceneCue.SpawnColon,
            [0xf436] = EndingSceneCue.SpawnMinutesTens,
            [0xf43f] = EndingSceneCue.SpawnMinutesUnits,
            [0xf448] = EndingSceneCue.TransitionToCredits,
        },
        new Dictionary<ushort, EndingSceneCue> { [CinematicActorNativeMotions.FlyawayStars[EndingStarsMotion.WaitingForFlyaway]] = EndingSceneCue.WaitForFlyaway });

    /// <summary>What the reward gesture's lists have their owner do: the suitless and the suited hand-off to the jump.</summary>
    internal static NativeCinematicCues<EndingRewardGestureCue> RewardGestureCues { get; } = new(
        new Dictionary<ushort, EndingRewardGestureCue> { [0xf51d] = EndingRewardGestureCue.StartJump, [0xf554] = EndingRewardGestureCue.StartJump },
        new Dictionary<ushort, EndingRewardGestureCue>());

    /// <summary>What the reward jump's lists have their owner do.</summary>
    internal static NativeCinematicCues<EndingRewardJumpCue> RewardJumpCues { get; } = new(
        new Dictionary<ushort, EndingRewardJumpCue>
        {
            [0xf651] = EndingRewardJumpCue.Launch,
            [0xf5ba] = EndingRewardJumpCue.LaunchHead,
            [0xf597] = EndingRewardJumpCue.PrepareHead,
            [0xf604] = EndingRewardJumpCue.Shoot,
        },
        new Dictionary<ushort, EndingRewardJumpCue>
        {
            [CinematicActorNativeMotions.RewardBody[EndingRewardJumpMotion.Rising]] = EndingRewardJumpCue.StartBodyFlight,
            [CinematicActorNativeMotions.RewardHeadFlight] = EndingRewardJumpCue.StartHeadFlight,
        });

    /// <summary>What the logo's list has its owner do.</summary>
    internal static NativeCinematicCues<EndingLogoCue> LogoCues { get; } = new(
        new Dictionary<ushort, EndingLogoCue> { [0xf25e] = EndingLogoCue.GreyOut }, new Dictionary<ushort, EndingLogoCue>());

    /// <summary>Every owner instruction any family's lists hold.</summary>
    internal static IEnumerable<ushort> OwnerInstructions =>
        new IEnumerable<ushort>[]
        {
            MotherBrainCues.Instructions.Keys, DiscoveryCues.Instructions.Keys, LaboratoryCues.Instructions.Keys, RinkaCues.Instructions.Keys,
            RinkaSpawnerCues.Instructions.Keys, CeresCues.Instructions.Keys, ExplosionSpawnerCues.Instructions.Keys,
            EndingSceneCues.Instructions.Keys, RewardGestureCues.Instructions.Keys, RewardJumpCues.Instructions.Keys, LogoCues.Instructions.Keys,
        }.SelectMany(words => words).Distinct();

    /// <summary>
    /// Whether <paramref name="address"/> lies in the bank-$8B cinematic sprite list regions, $8B:CB05..CE55 and
    /// $8B:EB0F..EE9B, which production never reads now that the lists are typed animations.
    /// </summary>
    internal static bool InListRegion(int address) => address is >= 0x8bcb05 and < 0x8bce55 or >= 0x8beb0f and < 0x8bee9b;

    /// <summary>Where <paramref name="animation"/> begins natively, the address an actor's definition names.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No actor starts <paramref name="animation"/>.</exception>
    internal static ushort Start(Enum animation) => Starts.TryGetValue(animation, out ushort start)
        ? start
        : throw new ArgumentOutOfRangeException(nameof(animation), animation, "No native cinematic list starts this animation.");
}

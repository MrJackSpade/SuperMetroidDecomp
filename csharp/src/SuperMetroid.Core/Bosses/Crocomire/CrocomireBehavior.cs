using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The Crocomire species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class CrocomireBehavior : IEnemySpecies, IEnemyEntityFactory
{

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Crocomire, EnemyDefinitionId.CrocomireTongue];

    /// <summary>Crocomire's body, or his tongue (#2146).</summary>
    /// <exception cref="InvalidOperationException">The place holds no Crocomire definition.</exception>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        EnemyDefinitionId definition = Ensure.NotNull(place).DefinitionId;
        if (definition == EnemyDefinitionId.Crocomire)
            return new CrocomireBodyEntity(this, place, common, _bossArtwork);
        if (definition == EnemyDefinitionId.CrocomireTongue)
            return new CrocomireTongueEntity(this, place, common);
        throw new InvalidOperationException($"The Crocomire species creates no {place.DefinitionId}.");
    }

    /// <summary>Accesses the active room, enemy places, and shared RNG.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Crocomire's body layer: where it is placed, the canvas his death assembles it on, the tongue placed with it and the acid smoke drawn with it.</summary>
    internal CrocomireBodyLayer BodyLayer { get; }

    /// <summary>Crocomire's setup, its body and tongue animations and their cues, and the invisible wall that holds Samus back.</summary>
    internal CrocomireFight Fight { get; }

    /// <summary>Crocomire's state lookups, body animation start, room objects, dust and random numbers, and the sound and music it last requested.</summary>
    internal CrocomireEncounterAccess Access { get; }

    /// <summary>Crocomire's hitboxes, touch, projectiles and hurt flash, and what Samus's weapons do to it.</summary>
    internal CrocomireCombat Combat { get; }

    /// <summary>The bridge giving way under Crocomire: its fragments, the sink into the acid, the rise and the final sink.</summary>
    internal CrocomireBridgeCollapse BridgeCollapse { get; }

    /// <summary>Crocomire melting in the acid: the melt images and the dissolve passes.</summary>
    internal CrocomireMelting Melting { get; }

    /// <summary>Crocomire's skeleton: waiting behind the wall, breaking through, its arc and collapse, and the fight's end.</summary>
    internal CrocomireSkeleton Skeleton { get; }
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;



    /// <summary>Connects Crocomire AI to room collision, rendering, projectiles, and boss-status owners.</summary>
    internal CrocomireBehavior(CrocomireBehaviorDependencies dependencies)
    {
        _bossArtwork = dependencies.Presentation.Artwork;
        _room = dependencies.Room;
        Access = dependencies.Access;
        BodyLayer = dependencies.BodyLayer;
        Combat = new CrocomireCombat(dependencies.Projectiles, dependencies.FrameContext, dependencies.Collision, dependencies.DeathsAndDrops, Access);
        Fight = new CrocomireFight(new CrocomireFightDependencies { Room = _room, Terrain = dependencies.Terrain, Projectiles = dependencies.Projectiles, Earthquake = dependencies.Earthquake, Presentation = dependencies.Presentation.Surfaces, Collision = dependencies.Collision, CameraDistance = dependencies.CameraDistance, RoomStatus = dependencies.RoomStatus, Access = Access, Combat = Combat });
        BridgeCollapse = new CrocomireBridgeCollapse(_room, dependencies.Projectiles, dependencies.CameraDistance, BodyLayer, Access, Combat);
        Melting = new CrocomireMelting(BodyLayer, Access, _bossArtwork);
        Skeleton = new CrocomireSkeleton(_room, dependencies.Projectiles, dependencies.Presentation.Surfaces, dependencies.DeathsAndDrops, dependencies.CameraDistance, Access, Combat);
    }

    /// <summary>Body X threshold at which Crocomire begins breaking the arena bridge.</summary>
    internal const int CrocomireBridgeThreshold = 1600;
    /// <summary>Body X threshold at the spiked wall that starts the death sequence.</summary>
    internal const int CrocomireSpikeWallThreshold = 768;

    /// <summary>The tongue's horizontal offset from the body, in pixels, set when the tongue initializes.</summary>
    internal const int CrocomireTongueBodyXOffset = 23;

    /// <summary>Acceleration the sink adds each frame: three and a half pixels per frame per frame.</summary>
    internal static readonly SignedEightEight SinkAccelerationIncrement = SignedEightEight.FromParts(3, 128);
    /// <summary>The sink's acceleration stops growing at forty-eight whole pixels, keeping its fraction.</summary>
    internal const int MaximumSinkAccelerationWhole = 48;
    /// <summary>The sink's speed stops growing at three whole pixels per frame, keeping its fraction.</summary>
    internal const int MaximumSinkSpeedWhole = 3;
    /// <summary>The rise's acceleration stops growing at thirty-one pixels per frame per frame.</summary>
    internal static readonly SignedEightEight MaximumRiseAcceleration = SignedEightEight.FromPixels(31);
    /// <summary>The speed a rise keeps when its deceleration would take it below zero: 255/256 pixel per frame.</summary>
    internal static readonly SignedEightEight RiseSpeedAfterStalling = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(255);
    /// <summary>Starting acceleration of every sink: eight pixels per frame per frame.</summary>
    internal static readonly SignedEightEight SinkStartingAcceleration = SignedEightEight.FromPixels(8);
    /// <summary>The skeleton's fixed fall through its arc: 224/256 pixel per frame.</summary>
    internal static readonly SignedSixteenSixteen SkeletonArcFallSpeed =
        SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(224);

    /// <summary>Runs the live-fight phase of Crocomire's main AI for one update.</summary>
    internal void RunCrocomireMain(
        RoomEnemyPlace place,
        SamusState samus,
        PseudoScrewCharge pseudoScrew,
        Position16<RoomPixels> camera)
    {
        CrocomireEnemyState state = Access.RequireCrocomire(place);

        if (state.DeathPhase == CrocomireDeathPhase.Fighting)
        {
            BridgeCollapse.HandleCrocomireBridgeThreshold(state);
            // The scroll write runs even on the frame that starts bridge collapse.
            // Later death phases retain these zones until the skeleton enters the river.
            _room.Services.RoomScroll.Write(CrocomireCameraDefinitions.BridgeLeftScreen,
                CoordinateAxis.Difference(samus.Position.X.Whole, CrocomireCameraDefinitions.BridgeVisibleSamusX) >= 0
                    ? RoomScrollState.Closed : RoomScrollState.OpenAlignedToTop);
            _room.Services.RoomScroll.Write(CrocomireCameraDefinitions.BridgeScreen, RoomScrollState.OpenAlignedToTop);
            // The fight's update always finishes by placing his tongue at his body, including the update in which the bridge
            // approach starts his death sequence.
            CrocomireBodyLayer.PlaceCrocomireTongue(state);
        }
        else
        {
            RunCrocomireDeathSequence(state, samus, camera);
        }

        Fight.HandleCrocomireInvisibleWall(place, state, samus, pseudoScrew);
        Combat.ApplyCrocomireHurtFlash(place);
    }

    /// <summary>Number of dust puffs emitted across the bridge-collapse path.</summary>
    internal const int CrocomireBridgeDustCount = 7;
    /// <summary>
    /// Runs Crocomire's sinking, skeleton, wall-break, and arena-completion phases; the acid hides his sinking body below
    /// a row of <paramref name="camera"/>, the camera his main update runs with.
    /// </summary>
    private void RunCrocomireDeathSequence(CrocomireEnemyState state, SamusState samus, Position16<RoomPixels> camera)
    {
        switch (state.DeathPhase)
        {
            case CrocomireDeathPhase.CrumbleBridgeAndSink:
                BridgeCollapse.SpawnNextCrocomireBridgeFragment();
                BridgeCollapse.RunCrocomireAcidSoundTimer();
                BridgeCollapse.RunCrocomireSinkingComposite(state, camera);
                return;
            case CrocomireDeathPhase.FirstSubmergedPause:
                BridgeCollapse.RunCrocomireAcidSoundTimer();
                CrocomireBodyLayer.PlaceCrocomireTongue(state);
                CrocomireBridgeCollapse.RunCrocomireSubmergedPause(state);
                return;
            case CrocomireDeathPhase.FirstHopRise:
                BridgeCollapse.RunCrocomireAcidSoundTimer();
                BridgeCollapse.RunCrocomireRisingComposite(state);
                return;
            case CrocomireDeathPhase.FirstHopSink:
                BridgeCollapse.RunCrocomireAcidSoundTimer();
                BridgeCollapse.RunCrocomireSinkingComposite(state, camera);
                return;
            case CrocomireDeathPhase.SecondSubmergedPause:
                BridgeCollapse.RunCrocomireAcidSoundTimer();
                CrocomireBodyLayer.PlaceCrocomireTongue(state);
                CrocomireBridgeCollapse.RunCrocomireSubmergedPause(state);
                return;
            case CrocomireDeathPhase.SecondHopRise:
                BridgeCollapse.RunCrocomireAcidSoundTimer();
                BridgeCollapse.RunCrocomireRisingComposite(state);
                return;
            case CrocomireDeathPhase.SecondHopSink:
                BridgeCollapse.RunCrocomireAcidSoundTimer();
                BridgeCollapse.RunCrocomireSinkingComposite(state, camera);
                return;
            case CrocomireDeathPhase.InstallFirstMeltImage:
                Melting.ShowFirstCrocomireMeltImage(state);
                return;
            case CrocomireDeathPhase.BeginFirstMeltPass:
                Melting.BeginCrocomireMeltPass(state, CrocomireMeltingIllustration.First);
                return;
            case CrocomireDeathPhase.ShowFirstMeltBands:
                Melting.ShowNextCrocomireMeltBand(state);
                return;
            case CrocomireDeathPhase.ThirdHopRise:
                BridgeCollapse.RunCrocomireFirstMeltRisingComposite(state);
                return;
            case CrocomireDeathPhase.StartFirstDissolve:
                Access.RequestCrocomireSound(CrocomireSoundDefinitions.FirstMeltDissolve);
                Melting.BeginCrocomireMelting(state);
                return;
            case CrocomireDeathPhase.DissolveFirstImage:
                Melting.RunCrocomireMelting(state);
                return;
            case CrocomireDeathPhase.ClearFirstMeltImage:
                Melting.FinishCrocomireMeltingPass(state);
                return;
            case CrocomireDeathPhase.FourthHopSink:
                BridgeCollapse.RunCrocomireFirstMeltSinkingComposite(state, camera);
                return;
            case CrocomireDeathPhase.ThirdSubmergedPause:
                CrocomireBridgeCollapse.RunCrocomireSubmergedPause(state);
                return;
            case CrocomireDeathPhase.FourthHopRise:
                BridgeCollapse.RunCrocomireFirstMeltRisingComposite(state);
                return;
            case CrocomireDeathPhase.FifthHopSink:
                BridgeCollapse.RunCrocomireFirstMeltSinkingComposite(state, camera);
                return;
            case CrocomireDeathPhase.FourthSubmergedPause:
                CrocomireBridgeCollapse.RunCrocomireSubmergedPause(state);
                return;
            case CrocomireDeathPhase.FifthHopRise:
                BridgeCollapse.RunCrocomireFirstMeltRisingComposite(state);
                return;
            case CrocomireDeathPhase.SixthHopSink:
                BridgeCollapse.RunCrocomireFirstMeltSinkingComposite(state, camera);
                return;
            case CrocomireDeathPhase.InstallSecondMeltImage:
                Melting.ShowSecondCrocomireMeltImage(state);
                return;
            case CrocomireDeathPhase.BeginSecondMeltPass:
                Melting.BeginCrocomireMeltPass(state, CrocomireMeltingIllustration.Second);
                return;
            case CrocomireDeathPhase.ShowSecondMeltBands:
                Melting.ShowNextCrocomireMeltBand(state);
                return;
            case CrocomireDeathPhase.WaitBeforeSixthHopRise:
                state.DeathPhase = state.DeathPhase.Next();
                return;
            case CrocomireDeathPhase.SixthHopRise:
                CrocomireBridgeCollapse.SelectCrocomireMeltAnimation(state, CrocomireMeltingIllustration.Second);
                BodyLayer.SpawnCrocomireAcidSmoke(state);
                BridgeCollapse.RunCrocomireRise(state);
                return;
            case CrocomireDeathPhase.StartSecondDissolve:
                Access.RequestCrocomireSound(CrocomireSoundDefinitions.SecondMeltDissolve);
                Melting.BeginCrocomireMelting(state);
                return;
            case CrocomireDeathPhase.DissolveSecondImage:
                Melting.RunCrocomireMelting(state);
                return;
            case CrocomireDeathPhase.ClearSecondMeltImage:
                Melting.FinishCrocomireMeltingPass(state);
                return;
            case CrocomireDeathPhase.FinalSink:
                BridgeCollapse.RunCrocomireFinalSink(state, camera);
                return;
            case CrocomireDeathPhase.WaitForSamusAtWall:
                Skeleton.RunCrocomireWaitBehindWall(state, samus);
                return;
            case CrocomireDeathPhase.RumbleHiddenWall:
                Skeleton.RunCrocomireWallRumble(state);
                return;
            case CrocomireDeathPhase.BreakSpikeWall:
                Skeleton.RunCrocomireSkeletonRevealAndWallBreak(state);
                return;
            case CrocomireDeathPhase.DelaySkeletonFall:
                Skeleton.RunCrocomireWallBreakDelay(state);
                return;
            case CrocomireDeathPhase.ArcSkeletonIntoArena:
                Skeleton.RunCrocomireSkeletonArc(state);
                return;
            case CrocomireDeathPhase.WaitForSkeletonTerminalImage:
                Skeleton.RunCrocomireSkeletonCollapse(state, samus);
                return;
            case CrocomireDeathPhase.ClearWallAndOpenScrolls:
                Skeleton.FinishCrocomireArenaScrolls(state);
                return;
            case CrocomireDeathPhase.WaitForStableSkeleton:
                if (CrocomireBodyAnimationGroups.SkeletonAtRest.Contains(state.BodyResidentAnimation))
                    state.DeathPhase = state.DeathPhase.Next();
                return;
            case CrocomireDeathPhase.WaitBeforeRecordingDefeat:
                state.DeathPhase = state.DeathPhase.Next();
                return;
            case CrocomireDeathPhase.PublishDefeatAndRestoreMusic:
                Skeleton.CompleteCrocomireBoss(state);
                return;
            case CrocomireDeathPhase.InertCorpse:
                return;
            case CrocomireDeathPhase.RiverSkeletonDetour:
                CrocomireSkeleton.RunCrocomireSkeletonRiver(state);
                return;
            case CrocomireDeathPhase.Fighting:
                throw new InvalidDataException("Crocomire's death sequence runs before its bridge collapse began.");
            default:
                throw new InvalidOperationException($"Undefined CrocomireDeathPhase {state.DeathPhase}.");
        }
    }

    /// <summary>Advances Crocomire's typed animation.</summary>
    internal void AdvanceCrocomireTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext context)
    {
        Fight.AdvanceCrocomireBodyAnimation(place, context.Samus, context.Level, context.Camera.X);
    }

    /// <summary>Runs Crocomire's main AI.</summary>
    internal void RunCrocomireMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunCrocomireMain(place, frame.Samus, frame.PseudoScrew, frame.Camera);
    }
}

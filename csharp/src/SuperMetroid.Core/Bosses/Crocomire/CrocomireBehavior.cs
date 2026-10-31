using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The Crocomire species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class CrocomireBehavior : IEnemySpecies, IEnemyEntityFactory
{

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. CrocomirePartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new CrocomireEntity(this, slot, common, _bossArtwork);

    /// <summary>Accesses the active room, enemy slots, and shared RNG.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Crocomire's body layer: where it is placed, the canvas his death assembles it on, the tongue placed with it and the acid smoke drawn with it.</summary>
    internal CrocomireBodyLayer BodyLayer { get; }

    /// <summary>Crocomire's setup, its body and tongue programs and their cues, and the invisible wall that holds Samus back.</summary>
    internal CrocomireFight Fight { get; }

    /// <summary>Crocomire's state lookups, body program start, PLMs, dust and random numbers, and the sound and music it last requested.</summary>
    internal CrocomireEncounterAccess Access { get; }

    /// <summary>Crocomire's hitboxes, touch, projectiles and hurt palette, and what Samus's weapons do to it.</summary>
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
        Access = new CrocomireEncounterAccess(dependencies.FrameResults, _room, dependencies.Projectiles, dependencies.Audio);
        BodyLayer = new CrocomireBodyLayer(dependencies.SpriteObjects, dependencies.Presentation.Surfaces, Access);
        Fight = new CrocomireFight(new CrocomireFightDependencies { Room = _room, Terrain = dependencies.Terrain, Projectiles = dependencies.Projectiles, Earthquake = dependencies.Earthquake, Presentation = dependencies.Presentation.Surfaces, Collision = dependencies.Collision, ZebetiteBehavior = dependencies.ZebetiteBehavior, RoomStatus = dependencies.RoomStatus, BodyLayer = BodyLayer, Access = Access , BossArtwork = _bossArtwork });
        Combat = new CrocomireCombat(dependencies.Projectiles, dependencies.Presentation.Surfaces, dependencies.FrameContext, dependencies.Collision, dependencies.DeathsAndDrops, Access, _bossArtwork);
        BridgeCollapse = new CrocomireBridgeCollapse(_room, dependencies.Projectiles, dependencies.ZebetiteBehavior, BodyLayer, Access);
        Melting = new CrocomireMelting(dependencies.Presentation.Surfaces, BodyLayer, Access, _bossArtwork);
        Skeleton = new CrocomireSkeleton(_room, dependencies.Projectiles, dependencies.Presentation.Surfaces, dependencies.DeathsAndDrops, dependencies.ZebetiteBehavior, Access, _bossArtwork);
    }

    /// <summary>Body X threshold at which Crocomire begins breaking the arena bridge.</summary>
    internal const ushort CrocomireBridgeThreshold = 1600;
    /// <summary>Body X threshold at the spiked wall that starts the death sequence.</summary>
    internal const ushort CrocomireSpikeWallThreshold = 768;
    /// <summary>The melt's BG2 distortion step stops growing at 80 pixels (8.8).</summary>
    internal const int MaximumCrocomireDistortionStep = 20480;

    /// <summary>The tongue's horizontal offset from the body, in pixels, set when the tongue initializes.</summary>
    internal const int CrocomireTongueBodyXOffset = 23;
    /// <summary>The projectile attack's last spawn parameter: its ninth shot, after which Crocomire steps forward.</summary>
    internal const int LastProjectileSpawnParameter = 18;

    /// <summary>Acceleration the sink adds each frame, in 1/256 pixel per frame per frame: three and a half pixels.</summary>
    internal const int SinkAccelerationIncrement = 896;
    /// <summary>The sink's acceleration stops growing at forty-eight whole pixels, keeping its fraction.</summary>
    internal const int MaximumSinkAccelerationWhole = 48;
    /// <summary>The sink's speed stops growing at three whole pixels per frame, keeping its fraction.</summary>
    internal const int MaximumSinkSpeedWhole = 3;
    /// <summary>The rise's acceleration stops growing at thirty-one pixels per frame per frame.</summary>
    internal const int MaximumRiseAccelerationWhole = 31;
    /// <summary>The speed a rise keeps when its deceleration would take it below zero: 255/256 pixel per frame.</summary>
    internal const int RiseSpeedAfterStalling = 255;
    /// <summary>Starting acceleration of every sink: eight pixels per frame per frame.</summary>
    internal const int SinkStartingAccelerationWhole = 8;
    /// <summary>The skeleton's fixed fall through its arc: 224/256 pixel per frame.</summary>
    internal static readonly SignedSixteenSixteen SkeletonArcFallSpeed =
        SignedSixteenSixteen.FromRaw(SixteenSixteen.FromEightEight(224));

    /// <summary>Runs the live-fight phase of <c>MainAI_Crocomire</c> at $A4:8C04.</summary>
    internal void RunCrocomireMain(
        RoomEnemySlot slot,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        CrocomireEnemyState state = Access.RequireCrocomire(slot);
        state.Camera = camera;

        if (state.DeathSequenceIndex == CrocomireDeathPhase.Fighting)
        {
            BridgeCollapse.HandleCrocomireBridgeThreshold(state);
            // The native word write runs even on the frame that starts bridge collapse.
            // Later death phases retain these zones until the skeleton enters the river.
            _room.Services.RoomScroll.Write(CrocomireCameraDefinitions.BridgeLeftScreen,
                samus is not null && unchecked((short)(samus.Position.X.Whole - CrocomireCameraDefinitions.BridgeVisibleSamusX)) >= 0
                    ? RoomScrollState.RedBoundary : RoomScrollState.Blue);
            _room.Services.RoomScroll.Write(CrocomireCameraDefinitions.BridgeScreen, RoomScrollState.Blue);
            // Main state zero always finishes through $A4:8B5B, including the frame in
            // which $8D5E changes the death index to two.
            BodyLayer.PlaceCrocomireBodyLayer(state);
        }
        else
        {
            RunCrocomireDeathSequence(state, samus);
        }

        Fight.HandleCrocomireInvisibleWall(slot, state, samus);
        Combat.ApplyCrocomireHurtPalette(slot);
    }

    /// <summary>Number of dust puffs emitted across the bridge-collapse path.</summary>
    internal const int CrocomireBridgeDustCount = 7;
    /// <summary>Dispatches Crocomire's sinking, skeleton, wall-break, and arena-completion phases.</summary>
    private void RunCrocomireDeathSequence(CrocomireEnemyState state, SamusState? samus)
    {
        switch (state.DeathSequenceIndex)
        {
            case CrocomireDeathPhase.CrumbleBridgeAndSink:
                BridgeCollapse.SpawnNextCrocomireBridgeFragment();
                BridgeCollapse.RunCrocomireAcidSoundTimer();
                BridgeCollapse.RunCrocomireSinkingComposite(state, samus);
                return;
            case CrocomireDeathPhase.FirstSubmergedPause:
                BridgeCollapse.RunCrocomireAcidSoundTimer();
                BodyLayer.PlaceCrocomireTongueAndBodyLayerHorizontally(state);
                CrocomireBridgeCollapse.RunCrocomireSubmergedPause(state);
                return;
            case CrocomireDeathPhase.FirstHopRise:
                BridgeCollapse.RunCrocomireAcidSoundTimer();
                BridgeCollapse.RunCrocomireRisingComposite(state, samus);
                return;
            case CrocomireDeathPhase.FirstHopSink:
                BridgeCollapse.RunCrocomireAcidSoundTimer();
                BridgeCollapse.RunCrocomireSinkingComposite(state, samus);
                return;
            case CrocomireDeathPhase.SecondSubmergedPause:
                BridgeCollapse.RunCrocomireAcidSoundTimer();
                BodyLayer.PlaceCrocomireTongueAndBodyLayerHorizontally(state);
                CrocomireBridgeCollapse.RunCrocomireSubmergedPause(state);
                return;
            case CrocomireDeathPhase.SecondHopRise:
                BridgeCollapse.RunCrocomireAcidSoundTimer();
                BridgeCollapse.RunCrocomireRisingComposite(state, samus);
                return;
            case CrocomireDeathPhase.SecondHopSink:
                BridgeCollapse.RunCrocomireAcidSoundTimer();
                BridgeCollapse.RunCrocomireSinkingComposite(state, samus);
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
                BridgeCollapse.RunCrocomireRisingComposite(state, samus);
                return;
            case CrocomireDeathPhase.StartFirstDissolve:
                Access.RequestCrocomireSound(CrocomireSoundDefinitions.FirstMeltDissolve);
                Melting.BeginCrocomireMelting(state);
                return;
            case CrocomireDeathPhase.DissolveFirstImage:
                Melting.RunCrocomireMelting(state, samus);
                return;
            case CrocomireDeathPhase.ClearFirstMeltImage:
                Melting.FinishCrocomireMeltingPass(state);
                return;
            case CrocomireDeathPhase.FourthHopSink:
                BridgeCollapse.RunCrocomireSinkingComposite(state, samus);
                return;
            case CrocomireDeathPhase.ThirdSubmergedPause:
                CrocomireBridgeCollapse.RunCrocomireSubmergedPause(state);
                return;
            case CrocomireDeathPhase.FourthHopRise:
                BridgeCollapse.RunCrocomireRisingComposite(state, samus);
                return;
            case CrocomireDeathPhase.FifthHopSink:
                BridgeCollapse.RunCrocomireSinkingComposite(state, samus);
                return;
            case CrocomireDeathPhase.FourthSubmergedPause:
                CrocomireBridgeCollapse.RunCrocomireSubmergedPause(state);
                return;
            case CrocomireDeathPhase.FifthHopRise:
                BridgeCollapse.RunCrocomireRisingComposite(state, samus);
                return;
            case CrocomireDeathPhase.SixthHopSink:
                BridgeCollapse.RunCrocomireSinkingComposite(state, samus);
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
            case CrocomireDeathPhase.ShippedSpacer:
                state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
                return;
            case CrocomireDeathPhase.SixthHopRise:
                CrocomireBridgeCollapse.SelectCrocomireRisingInstruction(state);
                BodyLayer.SpawnCrocomireAcidSmoke(state, samus);
                BridgeCollapse.RunCrocomireRise(state);
                return;
            case CrocomireDeathPhase.StartSecondDissolve:
                Access.RequestCrocomireSound(CrocomireSoundDefinitions.SecondMeltDissolve);
                Melting.BeginCrocomireMelting(state);
                return;
            case CrocomireDeathPhase.DissolveSecondImage:
                Melting.RunCrocomireMelting(state, samus);
                return;
            case CrocomireDeathPhase.ClearSecondMeltImage:
                Melting.FinishCrocomireMeltingPass(state);
                return;
            case CrocomireDeathPhase.FinalSink:
                BridgeCollapse.RunCrocomireFinalSink(state, samus);
                return;
            case CrocomireDeathPhase.WaitForSamusAtWall:
                Skeleton.RunCrocomireWaitBehindWall(state, samus);
                return;
            case CrocomireDeathPhase.RumbleHiddenWall:
                Skeleton.RunCrocomireWallRumble(state);
                return;
            case CrocomireDeathPhase.BreakSpikeWall:
                Skeleton.RunCrocomireSkeletonTileLoadAndWallBreak(state);
                return;
            case CrocomireDeathPhase.DelaySkeletonFall:
                Skeleton.RunCrocomireWallBreakDelay(state);
                return;
            case CrocomireDeathPhase.ArcSkeletonIntoArena:
                Skeleton.RunCrocomireSkeletonArc(state);
                return;
            case CrocomireDeathPhase.WaitForSkeletonTerminalImage:
                Skeleton.RunCrocomireSkeletonCollapse(state);
                return;
            case CrocomireDeathPhase.ClearWallAndOpenScrolls:
                Skeleton.FinishCrocomireArenaScrolls(state);
                return;
            case CrocomireDeathPhase.WaitForStableSkeleton:
                if (CrocomireBodyProgramGroups.SkeletonAtRest.Contains(state.BodyResidentProgram))
                    state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
                return;
            case CrocomireDeathPhase.WaitOneUpdate:
                state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
                return;
            case CrocomireDeathPhase.PublishDefeatAndRestoreMusic:
                Skeleton.CompleteCrocomireBoss(state);
                return;
            case CrocomireDeathPhase.InertCorpse:
                return;
            case CrocomireDeathPhase.DefeatedRoomAdvance:
                state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
                return;
            case CrocomireDeathPhase.PinDefeatedRoomBodyLayerPlacement:
                BodyLayer.Placement = Position16<BackgroundPixels>.Zero;
                return;
            case CrocomireDeathPhase.RiverSkeletonDetour:
                CrocomireSkeleton.RunCrocomireSkeletonRiver(state);
                return;
            case CrocomireDeathPhase.Fighting:
                throw new InvalidDataException("Crocomire's death sequence runs before its bridge collapse began.");
            default:
                throw new InvalidOperationException($"Undefined CrocomireDeathPhase {state.DeathSequenceIndex}.");
        }
    }

    /// <summary>Advances Crocomire's typed animation.</summary>
    internal void AdvanceCrocomireTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context)
    {
        Fight.AdvanceCrocomireBodyAnimation(slot, context.Samus, context.Level, context.Camera.X);
    }

    /// <summary>Ports Crocomire's main AI.</summary>
    internal void RunCrocomireMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunCrocomireMain(slot, frame.Samus, frame.Camera);
    }
}

using SuperMetroid.Core.Bosses.Artwork;
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
    public EnemyOccupant Create(RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        EnemyDefinitionId definition = Ensure.NotNull(place).DefinitionId;
        if (definition == EnemyDefinitionId.Crocomire)
            return EnemyOccupant.Of(new CrocomireBodyEntity(this, place, common, _bossArtwork));
        if (definition == EnemyDefinitionId.CrocomireTongue)
            return EnemyOccupant.Of(new CrocomireTongueEntity(this, place, common));
        throw new InvalidOperationException($"The Crocomire species creates no {place.DefinitionId}.");
    }

    /// <summary>Accesses the active room, enemy places, and shared RNG.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Crocomire's body layer: where it is placed, the canvas his death assembles it on, the tongue placed with it and the acid smoke drawn with it.</summary>
    internal CrocomireBodyLayer BodyLayer { get; }

    /// <summary>Crocomire's setup, its body and tongue animations and their cues, and the invisible wall that holds Samus back.</summary>
    internal CrocomireFight Fight { get; }

    /// <summary>Crocomire's body animation start, room objects, dust and random numbers, and the sound and music it last requested.</summary>
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
        Fight = new CrocomireFight(new CrocomireFightDependencies { Room = _room, Terrain = dependencies.Terrain, Projectiles = dependencies.Projectiles, Earthquake = dependencies.Earthquake, Presentation = dependencies.Presentation.Surfaces, Collision = dependencies.Collision, CameraDistance = dependencies.CameraDistance, RoomStatus = dependencies.RoomStatus, Access = Access, Combat = Combat, Places = dependencies.Places, Encounters = dependencies.Encounters });
        BridgeCollapse = new CrocomireBridgeCollapse(_room, dependencies.Projectiles, dependencies.CameraDistance, BodyLayer, Access, Combat);
        Melting = new CrocomireMelting(BodyLayer, _bossArtwork);
        Skeleton = new CrocomireSkeleton(_room, dependencies.Projectiles, dependencies.Presentation.Surfaces, dependencies.DeathsAndDrops, dependencies.CameraDistance, Access, Combat);
    }

    /// <summary>Runs Crocomire's main AI for one update: the fight, or the stage of his death he is at.</summary>
    internal void RunCrocomireMain(
        RoomEnemyPlace place,
        SamusState samus,
        PseudoScrewCharge pseudoScrew,
        Position16<RoomPixels> camera)
    {
        CrocomireEnemyState state = place.RequireSpeciesState<CrocomireEnemyState>();

        if (state.DeathStage.Entry is CrocomireDeathStage.Fighting)
        {
            BridgeCollapse.HandleCrocomireBridgeThreshold(state);
            // The scroll write runs even on the frame that starts bridge collapse.
            // Later death stages retain these zones until the skeleton enters the river.
            _room.Services.RoomScroll.Write(CrocomireCameraDefinitions.BridgeLeftScreen,
                samus.Position.X.Whole >= CrocomireCameraDefinitions.BridgeVisibleSamusX
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
        Combat.ApplyCrocomireHurtFlash(place, state);
    }

    /// <summary>
    /// Runs the stage of his death he is at for one update (<see cref="CrocomireDeathStageDefinitions.Stages"/>, #2540); the
    /// acid hides his sinking body below a row of <paramref name="camera"/>, the camera his main update runs with.
    /// </summary>
    private void RunCrocomireDeathSequence(CrocomireEnemyState state, SamusState samus, Position16<RoomPixels> camera)
    {
        switch (state.DeathStage.Entry)
        {
            case CrocomireDeathStage.SinkThroughBridge:
                BridgeCollapse.SpawnNextCrocomireBridgeFragment(state.Death);
                BridgeCollapse.RunCrocomireSinkingComposite(state, CrocomireBodyInAcid.Whole, camera);
                return;
            case CrocomireDeathStage.Sink sink:
                BridgeCollapse.RunCrocomireSinkingComposite(state, sink.Body, camera);
                return;
            case CrocomireDeathStage.SubmergedPause pause:
                BridgeCollapse.RunCrocomireSubmergedRest(state, pause.Body);
                return;
            case CrocomireDeathStage.Rise rise:
                BridgeCollapse.RunCrocomireRisingComposite(state, rise.Body);
                return;
            case CrocomireDeathStage.ShowMeltImage show:
                Melting.ShowCrocomireMeltImage(state, show.Picture);
                return;
            case CrocomireDeathStage.BeginMeltPass pass:
                Melting.BeginCrocomireMeltPass(state, pass.Picture);
                return;
            case CrocomireDeathStage.ShowMeltBands:
                CrocomireMelting.ShowNextCrocomireMeltBand(state);
                return;
            case CrocomireDeathStage.PauseOneUpdate:
                state.AdvanceDeathStage();
                return;
            case CrocomireDeathStage.StartDissolve dissolve:
                Access.RequestCrocomireSound(dissolve.Cry);
                CrocomireMelting.BeginCrocomireMelting(state);
                return;
            case CrocomireDeathStage.Dissolve:
                Melting.RunCrocomireMelting(state);
                return;
            case CrocomireDeathStage.ClearMeltImage:
                Melting.FinishCrocomireMeltingPass(state);
                return;
            case CrocomireDeathStage.FinalSink:
                BridgeCollapse.RunCrocomireFinalSink(state, camera);
                return;
            case CrocomireDeathStage.FlowDownRiver:
                CrocomireSkeleton.RunCrocomireSkeletonRiver(state);
                return;
            case CrocomireDeathStage.WaitForSamusAtWall:
                Skeleton.RunCrocomireWaitBehindWall(state, samus);
                return;
            case CrocomireDeathStage.RumbleHiddenWall:
                Skeleton.RunCrocomireWallRumble(state);
                return;
            case CrocomireDeathStage.BreakSpikeWall:
                Skeleton.RunCrocomireSkeletonRevealAndWallBreak(state);
                return;
            case CrocomireDeathStage.DelaySkeletonFall:
                CrocomireSkeleton.RunCrocomireWallBreakDelay(state);
                return;
            case CrocomireDeathStage.ArcSkeletonIntoArena:
                Skeleton.RunCrocomireSkeletonArc(state);
                return;
            case CrocomireDeathStage.CollapseSkeleton:
                Skeleton.RunCrocomireSkeletonCollapse(state, samus);
                return;
            case CrocomireDeathStage.ClearWallAndOpenScrolls:
                Skeleton.FinishCrocomireArenaScrolls(state);
                return;
            case CrocomireDeathStage.PublishDefeatAndRestoreMusic:
                Skeleton.CompleteCrocomireBoss(state);
                return;
            case CrocomireDeathStage.InertCorpse:
                return;
            case CrocomireDeathStage.Fighting:
                throw new InvalidDataException("Crocomire's death sequence runs before its bridge collapse began.");
            default:
                throw new InvalidOperationException($"Crocomire's death has no step for {state.DeathStage}.");
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

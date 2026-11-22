using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The Crocomire species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed partial class CrocomireBehavior : IEnemySpecies, IEnemySpawnFactory<EnemySpawnSettings>
{
    /// <inheritdoc/>
    [field: NonSerialized]
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Crocomire];

    /// <summary>
    /// Crocomire's body, built the same way wherever it is set up, which offers his tongue, listed right after it, the
    /// creation that builds it joining his encounter (#2146, #2400, #2554); a tongue no body offered is rejected where it
    /// is bound.
    /// </summary>
    /// <param name="registrar">The room's registry being composed.</param>
    public void OfferCreations(EnemySpeciesRegistrar registrar)
    {
        Ensure.NotNull(registrar).Spawns<EnemySpawnSettings>(Definitions, Create);
        registrar.Joins([EnemyDefinitionId.CrocomireTongue]);
    }

    /// <summary>Crocomire's body, which offers his tongue the creation that builds it joining his encounter.</summary>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        var body = new CrocomireBodyEntity(this, occupant, place, common, _bossArtwork);
        _partOffers.Offer<EnemySpawnSettings>(EnemyDefinitionId.CrocomireTongue, new TongueJoining(this, body.Encounter).CreateTongue);
        return EnemyOccupant.Running(body);
    }

    /// <summary>The creation his body offers his tongue (#2554): the tongue joining the encounter his body began.</summary>
    /// <param name="species">The Crocomire species owner.</param>
    /// <param name="encounter">The encounter his body began.</param>
    private sealed partial class TongueJoining(CrocomireBehavior species, CrocomireEnemyState encounter)
    {
        /// <summary>His tongue, joining his encounter.</summary>
        internal EnemyOccupant CreateTongue(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
            EnemyOccupant.Running(new CrocomireTongueEntity(species, occupant, place, common, encounter));
    }

    /// <summary>Where his body offers his tongue the creation that builds it joining his encounter.</summary>
    private readonly EnemyPartOffers _partOffers;

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
        _partOffers = dependencies.PartOffers;
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
        RoomEnemyOccupant crocomire, CrocomireEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew,
        Position16<RoomPixels> camera)
    {
        RoomEnemyPlace place = crocomire.Place;

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

        Fight.HandleCrocomireInvisibleWall(crocomire, state, samus, pseudoScrew);
        Combat.ApplyCrocomireHurtFlash(place, state);
    }

    /// <summary>
    /// Runs the stage of his death he is at for one update (<see cref="CrocomireDeathStageDefinitions.Stages"/>, #2540); the
    /// acid hides his sinking body below a row of <paramref name="camera"/>, the camera his main update runs with.
    /// </summary>
    private void RunCrocomireDeathSequence(CrocomireEnemyState state, SamusState samus, Position16<RoomPixels> camera)
    {
        state.DeathStage.Entry.React(RunCrocomireDeathSequenceReactions, new RunCrocomireDeathSequenceContext(state, camera, samus, this));
    }

    /// <summary>What <see cref="RunCrocomireDeathSequence"/> does on each <see cref="CrocomireDeathStage"/>, built once (#2553).</summary>
    private static CrocomireDeathStageReactions<RunCrocomireDeathSequenceContext> RunCrocomireDeathSequenceReactions { get; } = new(
        Fighting: static (_, _) =>
        {
            throw new InvalidDataException("Crocomire's death sequence runs before its bridge collapse began.");
        },
        SinkThroughBridge: static (_, context) =>
        {
            context.Owner.BridgeCollapse.SpawnNextCrocomireBridgeFragment(context.State.Death);
            context.Owner.BridgeCollapse.RunCrocomireSinkingComposite(context.State, CrocomireBodyInAcid.Whole, context.Camera);
        },
        Sink: static (sink, context) => context.Owner.BridgeCollapse.RunCrocomireSinkingComposite(context.State, sink.Body, context.Camera),
        SubmergedPause: static (pause, context) => context.Owner.BridgeCollapse.RunCrocomireSubmergedRest(context.State, pause.Body),
        Rise: static (rise, context) => context.Owner.BridgeCollapse.RunCrocomireRisingComposite(context.State, rise.Body),
        ShowMeltImage: static (show, context) => context.Owner.Melting.ShowCrocomireMeltImage(context.State, show.Picture),
        BeginMeltPass: static (pass, context) => CrocomireMelting.BeginCrocomireMeltPass(context.State, pass.Picture),
        ShowMeltBands: static (_, context) => CrocomireMelting.ShowNextCrocomireMeltBand(context.State),
        PauseOneUpdate: static (_, context) => context.State.AdvanceDeathStage(),
        StartDissolve: static (dissolve, context) =>
        {
            context.Owner.Access.RequestCrocomireSound(dissolve.Cry);
            CrocomireMelting.BeginCrocomireMelting(context.State);
        },
        Dissolve: static (_, context) => context.Owner.Melting.RunCrocomireMelting(context.State),
        ClearMeltImage: static (_, context) => CrocomireMelting.FinishCrocomireMeltingPass(context.State),
        FinalSink: static (_, context) => context.Owner.BridgeCollapse.RunCrocomireFinalSink(context.State, context.Camera),
        FlowDownRiver: static (_, context) => CrocomireSkeleton.RunCrocomireSkeletonRiver(context.State),
        WaitForSamusAtWall: static (_, context) => context.Owner.Skeleton.RunCrocomireWaitBehindWall(context.State, context.Samus),
        RumbleHiddenWall: static (_, context) => context.Owner.Skeleton.RunCrocomireWallRumble(context.State),
        BreakSpikeWall: static (_, context) => context.Owner.Skeleton.RunCrocomireSkeletonRevealAndWallBreak(context.State),
        DelaySkeletonFall: static (_, context) => CrocomireSkeleton.RunCrocomireWallBreakDelay(context.State),
        ArcSkeletonIntoArena: static (_, context) => context.Owner.Skeleton.RunCrocomireSkeletonArc(context.State),
        CollapseSkeleton: static (_, context) => context.Owner.Skeleton.RunCrocomireSkeletonCollapse(context.State, context.Samus),
        ClearWallAndOpenScrolls: static (_, context) => context.Owner.Skeleton.FinishCrocomireArenaScrolls(context.State),
        PublishDefeatAndRestoreMusic: static (_, context) => context.Owner.Skeleton.CompleteCrocomireBoss(context.State),
        InertCorpse: static (_, _) =>
        {
        });

    /// <summary>What the cases read besides the case (#2553), handed to them so they are built once.</summary>
    /// <param name="State">The state the cases read.</param>
    /// <param name="Camera">The camera the cases read.</param>
    /// <param name="Samus">The samus the cases read.</param>
    /// <param name="Owner">The <see cref="CrocomireBehavior"/> reacting.</param>
    private readonly partial record struct RunCrocomireDeathSequenceContext(CrocomireEnemyState State, Position16<RoomPixels> Camera, SamusState Samus, CrocomireBehavior Owner);

    /// <summary>Advances Crocomire's typed animation.</summary>
    internal void AdvanceCrocomireTypedAnimation(RoomEnemyPlace place, CrocomireEnemyState crocomireEnemyState, EnemyAnimationCueContext context)
    {
        Fight.AdvanceCrocomireBodyAnimation(place, crocomireEnemyState, context.Samus, context.Level, context.Camera.X);
    }

    /// <summary>Runs Crocomire's main AI.</summary>
    internal void RunCrocomireMainAi(RoomEnemyOccupant crocomire, CrocomireEnemyState crocomireEnemyState, IEnemyAiFrame frame)
    {
        _ = crocomire.Place;
        RunCrocomireMain(crocomire, crocomireEnemyState, frame.Samus, frame.PseudoScrew, frame.Camera);
    }
}

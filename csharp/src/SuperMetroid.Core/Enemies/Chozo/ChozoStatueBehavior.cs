using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>The Chozo statue species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class ChozoStatueBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. ChozoStatuePartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new ChozoStatueEntity(this, slot, common);

    /// <summary>Supplies room events, scroll grid and Samus-control service.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Clips walking-statue movement before the carried Samus position is written.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Allocates the Wrecked Ship statue's spike-footstep projectile.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Queues enemy-library footstep and walking sounds from cues.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Receives statue-authored effect overlay motion writes.</summary>
    private readonly EnemyRoomFx _roomFx;
    /// <summary>The room's Chozo statue, which its hand trigger wakes; null until the statue is placed.</summary>
    private RoomEnemySlot? _statue;

    /// <summary>Binds statue behavior to room, movement, FX, sound and projectile owners.</summary>
    internal ChozoStatueBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemyAudioQueue audio, EnemyRoomFx roomFx)
    {
        _room = room;
        _terrain = terrain;
        _projectiles = projectiles;
        _audio = audio;
        _roomFx = roomFx;
    }

    /// <summary>The room objects the statue has asked for and the room has still to spawn, in the order it asked.</summary>
    internal readonly List<RoomObjectSpawnRequest<ChozoStatueRoomObject>> ChozoStatueObjectRequests = new();

    /// <summary>Spawns each room object the statue asked for, once, in the order it asked.</summary>
    internal void ApplyPendingChozoStatueObjects(RoomLevelData level, ChozoStatueObjects chozoStatueObjects)
    {
        while (ChozoStatueObjectRequests.Count != 0)
        {
            chozoStatueObjects.TrySpawnChozoStatueObject(level, ChozoStatueObjectRequests[0]);
            ChozoStatueObjectRequests.RemoveAt(0);
        }
    }

    /// <summary>Enemy-library sound queued as the statue grabs Samus.</summary>
    private static readonly SoundEffectId ChozoGrabsSamusSound =
        EnemySoundEffectDefinitions.ChozoGrabsSamusSound;
    /// <summary>Enemy-library sound queued by the walking statue's footsteps.</summary>
    private static readonly SoundEffectId ChozoFootstepsSound =
        EnemySoundEffectDefinitions.ChozoFootstepsSound;

    /// <summary>Whether the statue last enabled Samus's controls rather than disabling them.</summary>
    public bool ChozoStatueSamusControlsEnabled { get; private set; } = true;

    /// <summary>Whether the Lower Norfair statue has started lowering the acid in this room.</summary>
    public bool ChozoStatueAcidLoweringStarted { get; private set; }

    /// <summary>
    /// Initializes the glass tube's cracks, which do nothing the game keeps: the record is deleted before any main-AI
    /// update, and the cracks' colours are the shade every tube projectile draws in
    /// (<see cref="ChozoAndTubeShades.TubeCracks"/>, #1512).
    /// </summary>
    internal static void InitializeN00bTubeCracks()
    {
    }

    /// <summary>Initializes either shipped statue.</summary>
    internal void InitializeChozoStatue(RoomEnemySlot statue)
    {
        ChozoStatueSite site = statue.SpawnSettings<ChozoStatueSpawnSettings>().Site;
        bool isWreckedShip = site switch
        {
            ChozoStatueSite.WreckedShip => true,
            ChozoStatueSite.LowerNorfair => false,
            _ => throw new InvalidOperationException($"Undefined {nameof(ChozoStatueSite)} {site}."),
        };

        // The initializer makes the statue solid to Samus and animates it, on screen or off.
        statue.Contact = statue.Contact.With(EnemyContact.SolidToSamus);
        statue.Processing = statue.Processing.With(EnemyProcessing.AnimationScript | EnemyProcessing.OffScreen);
        statue.Frame = EnemyFrameId.Empty;
        statue.FrameHold = WrappingCountdown.Start(1);
        statue.Timer = WrappingCountdown.Start(0);

        // The statue shows its own colours, the shade of its site's statue, which no other enemy shows (#1512).
        statue.Shading = PictureShading.Shaded(isWreckedShip ? ChozoAndTubeShades.WreckedShipStatue : ChozoAndTubeShades.LowerNorfairStatue);

        var state = new ChozoStatueState
        {
            FrameBehavior = ChozoStatueFrameBehavior.Idle,
            IsWreckedShip = isWreckedShip,
            HandTriggered = false,
        };
        statue.SetSpeciesState(state);
        _statue = statue;

        if (state.IsWreckedShip)
        {
            state.Playback = new(ChozoStatueAnimations.StartingWith(ChozoStatueProgram.WreckedShipInitial), AnimationPlace.StartOfAnimation);
            RequestChozoObject(ChozoStatueRoomObject.WreckedShipHand, ChozoStatueRoomDefinitions.WreckedShipHandBlock);
            RequestChozoObject(ChozoStatueRoomObject.SlopeAccessClosing, ChozoStatueRoomDefinitions.WreckedShipSlopeAccessBlock);
        }
        else
        {
            state.Playback = new(ChozoStatueAnimations.StartingWith(ChozoStatueProgram.LowerNorfairInitial), AnimationPlace.StartOfAnimation);
            RequestChozoObject(ChozoStatueRoomObject.LowerNorfairHand, ChozoStatueRoomDefinitions.LowerNorfairHandBlock);
        }
    }

    /// <summary>
    /// Represents the exact enemy-slot side effect produced by the Chozo hand
    /// trigger. Collision/pose/item admission remains the room object's responsibility; once
    /// admitted, both trigger setups write parameter one and disable Samus controls.
    /// </summary>
    public void ActivateChozoStatueHandTrigger(RoomLevelData level, Position16<RoomBlocks>? handBlock = null)
    {
        Ensure.NotNull(level);
        _room.EnsureLoaded();
        RoomEnemySlot statue = _statue ?? throw new InvalidOperationException("Chozo hand trigger requires a placed Chozo statue.");
        if (statue.DefinitionId != EnemyDefinitionId.Chozo)
            throw new InvalidOperationException("Chozo hand trigger requires the Chozo statue in its place.");

        ChozoStatueState state = statue.RequireSpeciesState<ChozoStatueState>();
        int triggerBlockX = state.IsWreckedShip ? ChozoStatueRoomDefinitions.WreckedShipHandBlock.X : ChozoStatueRoomDefinitions.LowerNorfairTriggerX;
        int triggerBlockY = state.IsWreckedShip ? ChozoStatueRoomDefinitions.WreckedShipHandBlock.Y : ChozoStatueRoomDefinitions.LowerNorfairTriggerY;
        Position16<RoomBlocks> triggerPosition = handBlock ?? level.GridBlock(new(CoordinateAxis.Wrap(triggerBlockX), CoordinateAxis.Wrap(triggerBlockY)));
        RoomCollisionBlock triggerBlock = level.GetCollisionBlock(triggerPosition);

        // Both setup steps clear only the block's collision kind. Its behaviour and
        // appearance survive until the room object the statue then spawns runs.
        level.SetTerrain(
            triggerPosition,
            triggerBlock.Terrain.WithCollision(RoomCollisionType.Air));
        state.HandTriggered = true;
        DisableChozoStatueSamusControls();
        if (!state.IsWreckedShip)
        {
            // Lower Norfair's hand trigger records the acid-lowered event before waking the actor.
            _room.Services.Events.Set(StoryEvent.LowerNorfairChozoLoweredAcid);
            RequestChozoObject(ChozoStatueRoomObject.PlugCrumbleAtHand, ChozoStatueRoomDefinitions.LowerNorfairHandBlock);
        }
        else
        {
            // The Wrecked Ship hand opens two pairs of the room's screens before handing the statue to its carry: two
            // green, then the two below them blue. The slope access's opening then queues its terrain change.
            foreach (Position16<RoomScreens> screen in ChozoStatueRoomDefinitions.WakingGreenScreens)
                _room.Services.RoomScroll.Write(screen, RoomScrollState.Green);
            foreach (Position16<RoomScreens> screen in ChozoStatueRoomDefinitions.WakingBlueScreens)
                _room.Services.RoomScroll.Write(screen, RoomScrollState.Blue);
            RequestChozoObject(ChozoStatueRoomObject.SlopeAccessOpening, ChozoStatueRoomDefinitions.WreckedShipSlopeAccessBlock);
        }
    }

    /// <summary>Dispatches statue phases, including boss-gated movement and authored scene transitions.</summary>
    private void RunChozoStatueMain(RoomEnemySlot statue, ChozoStatueState state)
    {
        switch (state.FrameBehavior)
        {
            case ChozoStatueFrameBehavior.Idle:
            case ChozoStatueFrameBehavior.Cleared:
            case ChozoStatueFrameBehavior.Walking:
                return;

            case ChozoStatueFrameBehavior.WaitForLowerNorfairHandTrigger:
                if (state.HandTriggered)
                {
                    state.Playback = new(ChozoStatueAnimations.StartingWith(ChozoStatueProgram.LowerNorfairActivated), AnimationPlace.StartOfAnimation);
                    statue.FrameHold = WrappingCountdown.Start(1);
                }
                return;

            case ChozoStatueFrameBehavior.WaitForWreckedShipHandTrigger:
                if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss) && state.HandTriggered)
                {
                    state.Playback = new(ChozoStatueAnimations.StartingWith(ChozoStatueProgram.WreckedShipActivated), AnimationPlace.StartOfAnimation);
                    statue.FrameHold = WrappingCountdown.Start(1);
                }
                return;

            default:
                throw new InvalidDataException(
                    $"Chozo statue frame behaviour {state.FrameBehavior?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>
    /// Carries out one cue of the Chozo statues' animations: every callback reachable from lists
    /// the four statue animations. Every cue runs on to the next step, so none redirects.
    /// </summary>
    private ChozoStatueProgram? ApplyChozoStatueCue(
        RoomEnemySlot statue,
        ChozoStatueCue cue,
        SamusState? samus,
        RoomLevelData? level)
    {
        ChozoStatueState state = statue.RequireSpeciesState<ChozoStatueState>();
        switch (cue)
        {
            case ChozoStatueCue.SetFrameBehavior setFrameBehavior:
                state.FrameBehavior = setFrameBehavior.FrameBehavior;
                return null;

            case ChozoStatueCue.Command { Kind: ChozoStatueCueKind.ClearFrameBehavior }:
                state.FrameBehavior = ChozoStatueFrameBehavior.Cleared;
                return null;

            case ChozoStatueCue.Command { Kind: ChozoStatueCueKind.StartLoweringAcid }:
                ChozoStatueAcidLoweringStarted = true;
                // The cue writes the liquid motion values consumed by the liquid handler once, when
                // it runs; the liquid owns them from then on.
                _roomFx.FxOverlay?.SetLiquidMotion(
                    timer: ChozoStatueFxData.LoweringDelay, yVelocity: ChozoStatueFxData.LoweringVelocity);
                return null;

            case ChozoStatueCue.Command { Kind: ChozoStatueCueKind.SetLoweredAcidPosition }:
                _roomFx.FxOverlay?.SetLiquidMotion(baseYPosition: ChozoStatueRoomDefinitions.LoweredAcidY);
                return null;

            case ChozoStatueCue.Command { Kind: ChozoStatueCueKind.UnlockSamus }:
                EnableChozoStatueSamusControls();
                return null;

            case ChozoStatueCue.Command { Kind: ChozoStatueCueKind.PlayGrabSound }:
                _audio.QueueEnemySound(ChozoGrabsSamusSound, priority: SoundRequestPriority.Normal);
                return null;

            case ChozoStatueCue.Command { Kind: ChozoStatueCueKind.PlayFootstepSound }:
                _audio.QueueEnemySound(ChozoFootstepsSound, priority: SoundRequestPriority.Normal);
                return null;

            case ChozoStatueCue.Footstep footstep:
                ProcessChozoStatueFootstep(
                    statue,
                    RequireChozoLevel(level),
                    footstep.Offset);
                return null;

            case ChozoStatueCue.Move move:
                ProcessChozoStatueMovement(
                    statue,
                    state,
                    RequireChozoSamus(samus),
                    RequireChozoLevel(level),
                    move.CarryRecord);
                return null;

            case ChozoStatueCue.Command { Kind: ChozoStatueCueKind.ReleaseSamus }:
                FinishWreckedShipChozoSequence();
                return null;

            default:
                throw new InvalidOperationException($"Undefined {nameof(ChozoStatueCueKind)} {cue}.");
        }
    }

    /// <summary>Moves the statue through terrain, then places Samus at the authored carry offset from its accepted position.</summary>
    internal void ProcessChozoStatueMovement(
        RoomEnemySlot statue,
        ChozoStatueState state,
        SamusState samus,
        RoomLevelData level,
        int carryRecord)
    {
        var motion = ChozoCarry.Read(carryRecord);
        state.CarryRecord = carryRecord;
        int signedVelocity = motion.Velocity;

        // The slope-ignoring horizontal mover receives a signed 8.8 speed promoted to 16.16 by
        // a shift by 256. The vertical mover receives its absolute magnitude in the same format.
        _ = _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            statue,
            SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(signedVelocity));
        _ = _terrain.MoveEnemyVertically(
            level,
            statue,
            SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(Math.Abs(signedVelocity)));
        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, statue);

        // The two 32-entry tables are joint offsets indexed by the same record.
        // Writing Samus after collision reproduces the cutscene's absolute ownership.
        samus.Position = samus.Position.WithWhole(new(CoordinateAxis.Wrap(statue.Position.X.Whole + motion.SamusOffset.X), CoordinateAxis.Wrap(statue.Position.Y.Whole + motion.SamusOffset.Y)));
    }

    /// <summary>Spawns a plug-crumble room object and spike-footstep only when the offset floor block has the special type.</summary>
    private void ProcessChozoStatueFootstep(
        RoomEnemySlot statue,
        RoomLevelData level,
        Offset16<RoomPixels> footstepOffset)
    {
        Position16<RoomPixels> probe = new(
            CoordinateAxis.Wrap(statue.Position.X.Whole + footstepOffset.X), CoordinateAxis.Wrap(statue.Position.Y.Whole + 28));
        Position16<RoomBlocks> footstep = level.GridBlock(probe.ToRoomBlock());
        Position16<RoomBlocks> below = footstep + new Offset16<RoomBlocks>(0, 1);
        if (!level.Size.Contains(below) || level.GetCollisionBlock(below).CollisionType != RoomCollisionType.SpikeBlock)
            return;

        RequestChozoObject(ChozoStatueRoomObject.PlugCrumbleUnderFootstep, footstep);
        SpawnWreckedShipChozoFootstep(statue, footstepOffset);
    }

    /// <summary>Places the Wrecked Ship footstep projectile beneath the statue at its signed horizontal offset.</summary>
    internal void SpawnWreckedShipChozoFootstep(RoomEnemySlot statue, Offset16<RoomPixels> footstepOffset)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep,
            graphics: default);
        projectile.Position = projectile.Position.WithWhole(new(CoordinateAxis.Wrap(statue.Position.X.Whole + footstepOffset.X), CoordinateAxis.Wrap(statue.Position.Y.Whole + 28)));
    }

    /// <summary>Restores Samus controls, closes and opens the room's screens behind her and queues the slope access's closing.</summary>
    private void FinishWreckedShipChozoSequence()
    {
        EnableChozoStatueSamusControls();

        // The release's overlapping writes leave five screens of the second row closed, then the third row's second
        // screen blue and its third closed.
        foreach (Position16<RoomScreens> screen in ChozoStatueRoomDefinitions.ReleaseClosedScreens)
            _room.Services.RoomScroll.Write(screen, RoomScrollState.RedBoundary);
        _room.Services.RoomScroll.Write(ChozoStatueRoomDefinitions.ReleaseBlueScreen, RoomScrollState.Blue);
        _room.Services.RoomScroll.Write(ChozoStatueRoomDefinitions.ReleaseClosedLastScreen, RoomScrollState.RedBoundary);

        RequestChozoObject(ChozoStatueRoomObject.SlopeAccessClosing, ChozoStatueRoomDefinitions.WreckedShipSlopeAccessBlock);
    }

    /// <summary>Gives Samus her controls back, in both the room service and observable state.</summary>
    private void EnableChozoStatueSamusControls()
    {
        ChozoStatueSamusControlsEnabled = true;
        _room.Services.SamusControls.EnableControls();
    }

    /// <summary>Takes Samus's controls away while the statue holds her, in both the room service and observable state.</summary>
    private void DisableChozoStatueSamusControls()
    {
        ChozoStatueSamusControlsEnabled = false;
        _room.Services.SamusControls.DisableControls();
    }

    /// <summary>Asks the room to spawn <paramref name="roomObject"/> at <paramref name="block"/>.</summary>
    private void RequestChozoObject(ChozoStatueRoomObject roomObject, Position16<RoomBlocks> block) =>
        ChozoStatueObjectRequests.Add(new RoomObjectSpawnRequest<ChozoStatueRoomObject>(block, roomObject));

    /// <summary>Requires the live Samus actor for cutscene carry movement.</summary>
    private static SamusState RequireChozoSamus(SamusState? samus) =>
        samus ?? throw new InvalidOperationException(
            "Chozo statue movement requires the active Samus actor.");

    /// <summary>Requires loaded terrain before statue movement or footstep collision probes.</summary>
    private static RoomLevelData RequireChozoLevel(RoomLevelData? level) =>
        level ?? throw new InvalidOperationException(
            "Chozo statue movement requires the active room level.");

    /// <summary>Runs ChozoStatue's main AI.</summary>
    internal void RunChozoStatueMainAi(RoomEnemySlot slot, IEnemyAiFrame _)
    {
        RunChozoStatueMain(slot, slot.RequireSpeciesState<ChozoStatueState>());
    }

    /// <summary>Advances ChozoStatue's typed animation.</summary>
    internal void AdvanceChozoStatueTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context) =>
        EnemyProgramAnimation.Advance(slot, ChozoStatueAnimations.Segments,
            static (ChozoStatueState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyChozoStatueCue(slot, cue, context.Samus, context.Level) is { } target
                ? new AnimationContinuation<ChozoStatueProgram>(ChozoStatueAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
}

using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.FaceBlocks.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>The blue Brinstar face block species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class BlueBrinstarFaceBlockBehavior : IEnemySpecies, IEnemyEntityFactory, IRoomGlow
{
    /// <summary>The pass-through shot reaction for this species' private shot reaction.</summary>
    internal static EnemyShotReaction PassThroughShot { get; } =
        new()
        {
            AfterSuperMissileQuake = ProjectilePassThrough.Shot,
        };

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.FaceBlock];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.FromSurroundings(new BlueBrinstarFaceBlockEntity(this, occupant, place, common));

    /// <summary>The face blocks the room has set up, in the order they were placed; all show the room-wide glow (#2283).</summary>
    private readonly List<RoomEnemyPlace> _blocks = [];

    /// <summary>Samus's passage through a door, which pauses the face blocks' glow.</summary>
    private readonly DoorPassage _doorPassage;

    /// <summary>Binds the species to Samus's passage through a door.</summary>
    internal BlueBrinstarFaceBlockBehavior(DoorPassage doorPassage) => _doorPassage = doorPassage;

    /// <summary>
    /// Whether a face block has started the room's glow, which it does once Samus has collected the Morph Ball. The room
    /// has one glow, not one per block.
    /// </summary>
    private bool _glowStarted;

    /// <summary>
    /// The room's glow (#2477): the level every face block shows, kept when a block wakes, and the updates it has left. A
    /// room starts it on its last step, so the glow's first step shows its first.
    /// </summary>
    private readonly ShowingAnimationPlayer<FaceBlockGlowSegment, FaceBlockGlowLevel, NoAnimationCue> _glow =
        ShowingAnimationPlayer<FaceBlockGlowSegment, FaceBlockGlowLevel, NoAnimationCue>.StartShowing(FaceBlockGlowAnimations.Segments, FaceBlockGlowAnimations.Restarted);

    /// <summary>The glow every face block in the room shows once it has started (#1512).</summary>
    private PictureShading Glow => PictureShading.Shaded(FaceBlockShades.Glow(_glow.Showing));

    /// <summary>Initializes the Blue Brinstar face block.</summary>
    private void InitializeBlueBrinstarFaceBlock(RoomEnemyPlace place, SamusState samus)
    {
        var state = new BlueBrinstarFaceBlockEnemyState();
        place.SetSpeciesState(state);
        _blocks.Add(place);

        // The neutral animation shows the neutral picture and holds it. Every directional animation shows the same
        // neutral picture first.
        state.Playback = EnemyAnimationPlayback.Start(place, BlueBrinstarFaceBlockAnimations.StartingWith(BlueBrinstarFaceBlockSegment.LookingAhead));

        // The setup always restarts the glow's countdown, even before Samus has the Morph Ball, and starts the glow
        // only once she has collected it. Main AI makes the same test every update, so a block in the room where she
        // collects it starts the glow at once.
        if (HasCollectedMorphBall(samus))
            StartGlow(place);
        RestartGlowStep();
    }

    /// <summary>Runs the Blue Brinstar face block's main AI.</summary>
    private void RunBlueBrinstarFaceBlockMain(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        BlueBrinstarFaceBlockEnemyState state,
        SamusState samus)
    {
        // The collected items, not the equipped ones, gate both the glow and waking.
        if (!HasCollectedMorphBall(samus))
            return;

        StartGlow(place);
        if (state.Activated)
            return;

        if (Math.Abs(samus.Position.Y.Whole - place.Position.Y.Whole) >=
            occupant.SpawnSettings<FaceBlockSpawnSettings>().TriggerDistance)
            return;

        // How far Samus lies to the block's left or right.
        int horizontalDelta = samus.Position.X.Whole - place.Position.X.Whole;
        if (Math.Abs(horizontalDelta) >= occupant.SpawnSettings<FaceBlockSpawnSettings>().TriggerDistance)
            return;

        // The block wakes only for Samus on the side its spawn settings face it toward (#2277).
        HorizontalDirection samusSide = horizontalDelta < 0 ? HorizontalDirection.Left : HorizontalDirection.Right;
        if (samusSide != occupant.SpawnSettings<FaceBlockSpawnSettings>().Facing)
            return;

        state.Playback = EnemyAnimationPlayback.Start(place, BlueBrinstarFaceBlockAnimations.StartingWith(Wake(samusSide)));

        // Waking restarts the room's glow countdown at a frame's duration but keeps the glow's level.
        RestartGlowStep();
    }

    /// <summary>Shows the glow's level for a whole step's updates from now, as a block's setup and its waking do.</summary>
    private void RestartGlowStep() => _glow.HoldShownFrameFor(FaceBlockGlowDefinitions.FrameDuration);

    /// <summary>The turn a face block wakes into for Samus on <paramref name="side"/>, the side it faces.</summary>
    private static BlueBrinstarFaceBlockSegment Wake(HorizontalDirection side) =>
        side == HorizontalDirection.Left ? BlueBrinstarFaceBlockSegment.SamusLeft : BlueBrinstarFaceBlockSegment.SamusRight;

    /// <summary>Uses the collected items so unequipping Morph Ball does not put active blocks back to rest.</summary>
    private static bool HasCollectedMorphBall(SamusState samus) =>
        samus.CollectedItems.HasAny(SamusEquipmentFlags.MorphBall);

    /// <summary>
    /// Starts the room's glow and shows it on <paramref name="place"/>; every face block, and only a face block, shows the
    /// glow (#1512).
    /// </summary>
    private void StartGlow(RoomEnemyPlace place)
    {
        _glowStarted = true;
        place.Shading = Glow;
    }

    /// <summary>
    /// Counts down the room's glow once a block has started it, after the enemies' update; when the step's time is up
    /// the glow steps to its next level on every face block. A door transition pauses it.
    /// </summary>
    public void StepGlow()
    {
        if (!_glowStarted || _doorPassage.IsUnderWay || _glow.StepToNewFrame() is null)
            return;

        PictureShading glow = Glow;
        foreach (RoomEnemyPlace block in _blocks)
            block.Shading = glow;
    }

    /// <summary>Advances the blue Brinstar face block's typed animation.</summary>
    internal static void AdvanceBlueBrinstarFaceBlockTypedAnimation(RoomEnemyPlace place)
    {
        if (!place.AnimationClock.Expire())
            return;
        var state = place.RequireSpeciesState<BlueBrinstarFaceBlockEnemyState>();
        AnimationPlayback<BlueBrinstarFaceBlockSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, BlueBrinstarFaceBlockAnimations.Segments, ref playback,
            static cue => throw new InvalidOperationException($"A cue-free blue Brinstar face block animation raised {cue}."));
        state.Playback = playback;
    }

    /// <summary>Runs the blue Brinstar face block's initialization AI.</summary>
    internal void SetUpBlueBrinstarFaceBlock(RoomEnemyPlace place, EnemyPopulationSurroundings surroundings)
    {
        InitializeBlueBrinstarFaceBlock(place, surroundings.Samus);
    }

    /// <summary>Runs the blue Brinstar face block's main AI.</summary>
    internal void RunBlueBrinstarFaceBlockMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunBlueBrinstarFaceBlockMain(
            occupant, place,
            place.RequireSpeciesState<BlueBrinstarFaceBlockEnemyState>(),
            frame.Samus);
    }
}

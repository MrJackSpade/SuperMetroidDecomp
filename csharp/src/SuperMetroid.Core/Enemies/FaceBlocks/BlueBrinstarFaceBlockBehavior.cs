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
internal sealed class BlueBrinstarFaceBlockBehavior : IEnemySpecies, IEnemyEntityFactory
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
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new BlueBrinstarFaceBlockEntity(this, place, common);

    /// <summary>The room's enemy places, whose face blocks all show the room-wide glow.</summary>
    private readonly EnemyPlaces _places;

    /// <summary>Samus's passage through a door, which pauses the face blocks' glow.</summary>
    private readonly DoorPassage _doorPassage;

    /// <summary>Binds the species to the room's places and Samus's passage through a door.</summary>
    internal BlueBrinstarFaceBlockBehavior(EnemyPlaces places, DoorPassage doorPassage)
    {
        _doorPassage = doorPassage;
        _places = places;
    }

    /// <summary>The updates each frame of the face blocks' glow holds for.</summary>
    private const int GlowFrameUpdates = 16;

    /// <summary>
    /// Whether a face block has started the room's glow, which it does once Samus has collected the Morph Ball. The room
    /// has one glow, not one per block.
    /// </summary>
    private bool _glowStarted;
    /// <summary>Updates left before the glow steps, restarted at sixteen by each step and by a block waking.</summary>
    private int _glowStepCountdown;
    /// <summary>
    /// The frame of the glow every face block shows, 0 through <see cref="FaceBlockShades.GlowFrames"/> minus one, kept
    /// when a block wakes. It starts at the last frame, so the glow's first step shows its first.
    /// </summary>
    private int _glowFrame = FaceBlockShades.GlowFrames - 1;

    /// <summary>The glow every face block in the room shows once it has started (#1512).</summary>
    private PictureShading Glow => PictureShading.Shaded(FaceBlockShades.Glow(_glowFrame));

    /// <summary>Initializes the Blue Brinstar face block.</summary>
    internal void InitializeBlueBrinstarFaceBlock(RoomEnemyPlace place, SamusState samus)
    {
        var state = new BlueBrinstarFaceBlockEnemyState();
        place.SetSpeciesState(state);

        // The neutral animation is a one-frame neutral picture followed by the common rest. All
        // authored directional animations show the same neutral picture as their first frame.
        state.Playback = EnemyAnimationPlayback.Start(place, BlueBrinstarFaceBlockAnimations.StartingWith(BlueBrinstarFaceBlockSegment.Initial));

        // The initializer always restarts the glow's countdown, even before Samus has the Morph Ball, and starts the glow
        // only once she has collected it. Main AI makes the same test every update, so a block in the room where she
        // collects it starts the glow at once.
        if (HasCollectedMorphBall(samus))
            StartGlow(place);
        _glowStepCountdown = GlowFrameUpdates;

        // Keeps only the facing its spawn names. This looks
        // backwards until main AI is read: activation requires the Samus delta sign to be
        // *different* from this marker.
        state.FacesRight = place.SpawnSettings<FaceBlockSpawnSettings>().Facing == HorizontalDirection.Right;
        state.Activated = false;
    }

    /// <summary>Runs the Blue Brinstar face block's main AI.</summary>
    internal void RunBlueBrinstarFaceBlockMain(
        RoomEnemyPlace place,
        BlueBrinstarFaceBlockEnemyState state,
        SamusState samus)
    {
        // The collected items, not the equipped ones, gate both the glow and waking.
        if (!HasCollectedMorphBall(samus))
            return;

        StartGlow(place);
        if (state.Activated)
            return;

        if (CoordinateAxis.Distance(samus.Position.Y.Whole, place.Position.Y.Whole) >=
            place.SpawnSettings<FaceBlockSpawnSettings>().TriggerDistance)
            return;

        // The signed room-coordinate difference wraps across the room's wrapping axis.
        int horizontalDelta = CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole);
        if (Math.Abs(horizontalDelta) >= place.SpawnSettings<FaceBlockSpawnSettings>().TriggerDistance)
            return;

        bool samusToTheLeft = horizontalDelta < 0;
        if (samusToTheLeft == state.FacesRight)
            return;

        state.Playback = EnemyAnimationPlayback.Start(place, BlueBrinstarFaceBlockAnimations.StartingWith(samusToTheLeft
            ? BlueBrinstarFaceBlockSegment.SamusLeft
            : BlueBrinstarFaceBlockSegment.SamusRight));
        state.Activated = true;

        // Waking restarts the room's glow countdown at sixteen updates but keeps the glow's frame.
        _glowStepCountdown = GlowFrameUpdates;
    }

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
    /// Counts down the room's glow once a block has started it, after the enemies' update; when the frame's time is up
    /// the glow steps to its next frame on every face block. A door transition pauses it.
    /// </summary>
    internal void StepGlow()
    {
        if (!_glowStarted || _doorPassage.IsUnderWay)
            return;

        if (--_glowStepCountdown != 0)
            return;

        _glowStepCountdown = GlowFrameUpdates;
        _glowFrame = (_glowFrame + 1) % FaceBlockShades.GlowFrames;
        PictureShading glow = Glow;
        foreach (RoomEnemyPlace block in _places.InUpdateOrder)
        {
            if (block.DefinitionId == EnemyDefinitionId.FaceBlock)
                block.Shading = glow;
        }
    }

    /// <summary>Advances the blue Brinstar face block's typed animation.</summary>
    internal static void AdvanceBlueBrinstarFaceBlockTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _)
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
    internal void RunBlueBrinstarFaceBlockInitializationAi(RoomEnemyPlace place, EnemySpawnSurroundings surroundings)
    {
        InitializeBlueBrinstarFaceBlock(place, surroundings.IntroducedSamus);
    }

    /// <summary>Runs the blue Brinstar face block's main AI.</summary>
    internal void RunBlueBrinstarFaceBlockMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunBlueBrinstarFaceBlockMain(
            place,
            place.RequireSpeciesState<BlueBrinstarFaceBlockEnemyState>(),
            frame.Samus);
    }
}

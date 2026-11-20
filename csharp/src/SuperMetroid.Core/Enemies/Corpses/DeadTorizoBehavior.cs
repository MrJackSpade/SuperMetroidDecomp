using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The dead Torizo species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class DeadTorizoBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.CorpseTorizo];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new DeadTorizoEntity(this, occupant, place, common));

    /// <summary>Supplies the shared random seed.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Supplies the enemy-frame counter used to cadence rotting sounds.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Spawns dust when individual corpse rows finish rotting.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Draws the corpse's sand heap from its installed frame.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>The places of the corpses the room has set up, in the order they were placed, whose sand heaps it draws (#2283).</summary>
    private readonly List<RoomEnemyPlace> _corpses = [];

    /// <summary>Gives the corpse's rot the room's random numbers, the enemy pass's count, projectiles, drawing and
    /// audio.</summary>
    internal DeadTorizoBehavior(EnemyRoomBinding room, EnemyFrameContext frameContext, EnemyProjectileEngine projectiles, EnemyPresentation presentation, EnemyAudioQueue audio)
    {
        _audio = audio;
        _room = room;
        _frameContext = frameContext;
        _projectiles = projectiles;
        _presentation = presentation;
    }

    /// <summary>Initializes the dead Torizo, whose corpse keeps its own state on its place like the other corpses (#1808).</summary>
    internal void InitializeDeadTorizo(RoomEnemyPlace place)
    {
        _corpses.Add(place);
        place.Contact = place.Contact.With(EnemyContact.SolidToSamus);
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        AnimationPlayback<DeadTorizoSegment> playback = EnemyAnimationPlayback.Start(place, DeadTorizoAnimations.StartingWith(Corpses.DeadTorizoSegment.Stationary));

        place.SetSpeciesState(new DeadTorizoEnemyState(new CorpseRotSchedule(DeadTorizoCorpseDefinitions.RowCount), DeadTorizoPhase.Wait)
        {
            Playback = playback,
            FallSpeed = DeadTorizoCorpseDefinitions.StartingFallSpeed,
        });
    }

    /// <summary>Runs the dead Torizo's main AI.</summary>
    private void RunDeadTorizoMain(RoomEnemyPlace place, SamusState samus)
    {
        DeadTorizoEnemyState state = place.RequireSpeciesState<DeadTorizoEnemyState>();

        // The custom detector remains enabled while the intact corpse is solid. Rotting
        // disables interaction separately, after publishing Samus's external displacement.
        if (!place.Contact.HasAny(EnemyContact.IgnoresSamus) &&
            DeadTorizoCustomHitboxOverlaps(place, samus))
        {
            TriggerDeadTorizoRotting(place);
        }

        switch (state.Phase)
        {
            case DeadTorizoPhase.Wait:
                if (samus.Kinematics.DidCollideWithSolidEnemy(place) == true)
                    state.Phase = DeadTorizoPhase.PreRot;
                break;

            case DeadTorizoPhase.PreRot:
                if (state.PreRotDelay.Step())
                {
                    place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
                    state.Phase = DeadTorizoPhase.Rotting;
                    RunDeadTorizoRotting(place, state);
                }
                break;

            case DeadTorizoPhase.Rotting:
                RunDeadTorizoRotting(place, state);
                break;

            case DeadTorizoPhase.Finished:
                break;

            default:
                throw new InvalidDataException(
                    $"Dead Torizo has no behaviour for phase {state.Phase}.");
        }
    }


    /// <summary>Advances the corpse's fall and runs one rot update of its schedule.</summary>
    private void RunDeadTorizoRotting(RoomEnemyPlace place, DeadTorizoEnemyState state)
    {
        state.FallSpeed += DeadTorizoCorpseDefinitions.RotGravity;
        place.Position = place.Position.WithY(place.Position.Y.MovedBy(state.FallSpeed));
        state.SandForming.Step();
        bool stillRotting = state.Rot.Step(state.Rows.MoveRow, _ => FinishDeadTorizoCorpseRow());
        if (!stillRotting)
            state.Phase = DeadTorizoPhase.Finished;
    }

    /// <summary>The rest of the corpse's shot and touch reactions.</summary>
    private static void TriggerDeadTorizoRotting(RoomEnemyPlace place)
    {
        DeadTorizoEnemyState state = place.RequireSpeciesState<DeadTorizoEnemyState>();
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
        state.Phase = DeadTorizoPhase.Rotting;
    }

    /// <summary>The corpse's power bomb reaction.</summary>
    private static void TriggerDeadTorizoPowerBomb(RoomEnemyPlace place)
    {
        if (!place.Contact.HasAny(EnemyContact.IgnoresSamus))
            TriggerDeadTorizoRotting(place);
    }

    /// <summary>Checks asymmetric corpse touch boxes and publishes Samus's minimum separation displacement.</summary>
    private static bool DeadTorizoCustomHitboxOverlaps(RoomEnemyPlace place, SamusState samus)
    {
        SamusKinematicsState kinematics = samus.Kinematics;
        foreach (EnemyHitboxRectangle hitbox in DeadTorizoTouchDefinitions.TouchHitboxes)
        {
            // The game takes the magnitude of whichever signed edge faces Samus, so each edge
            // acts as a reach on its own side of the corpse origin.
            int verticalOffset = kinematics.Position.Y.Whole - place.Position.Y.Whole;
            int verticalReach = Math.Abs(verticalOffset >= 0 ? hitbox.Bottom : hitbox.Top);
            if (kinematics.Radius.Y + verticalReach - Math.Abs(verticalOffset) < 0)
                continue;

            int horizontalOffset = kinematics.Position.X.Whole - place.Position.X.Whole;
            int horizontalReach = Math.Abs(horizontalOffset >= 0 ? hitbox.Right : hitbox.Left);
            int overlap = kinematics.Radius.X + horizontalReach - Math.Abs(horizontalOffset);
            if (overlap < 0)
                continue;
            kinematics.ExtraDisplacement = FixedVector<RoomPixels>.FromWholePixels(new(
                Math.Max(overlap, DeadTorizoTouchDefinitions.LeastSidewaysPush),
                DeadTorizoTouchDefinitions.DownwardPush));
            return true;
        }

        return false;
    }

    /// <summary>Spawns a finished rot row's dust from shared RNG, and optionally publishes sound.</summary>
    private void FinishDeadTorizoCorpseRow()
    {
        int random = _room.Random.Read();
        _projectiles.SpawnDustOrExplosion(
            new(CorpseDustDefinitions.TorizoSpread.Of(random) + DeadTorizoCorpseDefinitions.DustOrigin.X, DeadTorizoCorpseDefinitions.DustOrigin.Y),
            DustExplosionAnimation.CorpseDustCloud);
        if (CorpseCadenceDefinitions.DustSound.IsDue(_frameContext.EnemyPassCount))
            _audio.QueueEnemySound(EnemySound.LiquidDamage, priority: SoundRequestPriority.Normal);
    }

    /// <summary>The body rows the dead Torizo in <paramref name="place"/> shows: its own, as its rot has left them (#1539).</summary>
    internal static CorpseRotRows ShownDeadTorizoRows(RoomEnemyPlace place) => place.RequireSpeciesState<DeadTorizoEnemyState>().Rows.Shown;

    /// <summary>
    /// The sand rows <paramref name="state"/>'s sand heap shows (#1539): only the rows its heap has formed into, as the corpse's
    /// own rot forms it. Each run of rot updates one more line of sand falls, from the heap's last row up, until all the
    /// heap's lines have fallen; the intact corpse has none.
    /// </summary>
    private static CorpseRotRows ShownSand(DeadTorizoEnemyState state)
    {
        int lines = state.SandForming.Age / DeadTorizoCorpseDefinitions.RotUpdatesPerSandLine;
        return CorpseRotRows.Showing(DeadTorizoCorpseDefinitions.SandRowCount, row => row >= DeadTorizoCorpseDefinitions.SandRowCount - lines);
    }

    /// <summary>
    /// Draws, after the room's enemies, the sand heap of each corpse still in the room, in the order they were placed,
    /// publishing its frame as the encounter's own (#1392), with the rows that corpse's own rot has formed (#1539).
    /// </summary>
    internal void DrawDeadTorizoSandHeap(SpriteFrameBuffer sprites, Position16<RoomPixels> camera)
    {
        foreach (RoomEnemyPlace place in _corpses)
        {
            if (place.SpeciesState<DeadTorizoEnemyState>() is not { } state)
                continue;

            // The heap is published wherever it lies; the display clips it to the view (#1541).
            _presentation.PublishEncounterFrame(sprites, new SpriteSource(SpriteSourceKind.DeadTorizoSandHeap), DeadTorizoArtworkDefinitions.SandHeapFrame,
                DeadTorizoArtworkDefinitions.SandHeapPlace.RelativeToCamera(camera), PictureShading.Standard, rows: ShownSand(state));
        }
    }

    /// <summary>Routes corpse bomb contact to rotting once the check for an ignoring reaction has passed.</summary>
    internal static void ReplaceDeadTorizoBombDamage(EnemyBombContext c) =>
        // Corpse shot reactions run directly after the collision mark. They do not consult bomb
        // vulnerability or health; they only hand the actor to its authored rotting state machine.
        TriggerDeadTorizoRotting(c.Enemy);

    /// <summary>Advances the dead Torizo's typed animation.</summary>
    internal static void AdvanceDeadTorizoTypedAnimation(RoomEnemyPlace place)
    {
        if (!place.AnimationClock.Expire())
            return;
        DeadTorizoEnemyState state = place.RequireSpeciesState<DeadTorizoEnemyState>();
        AnimationPlayback<DeadTorizoSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, DeadTorizoAnimations.Segments, ref playback,
            static cue => throw new InvalidOperationException($"A cue-free dead Torizo animation raised {cue}."));
        state.Playback = playback;
    }

    /// <summary>Runs the dead Torizo's main AI.</summary>
    internal void RunDeadTorizoMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunDeadTorizoMain(place, frame.Samus);
    }

    /// <summary>Starts corpse rotting on a power bomb without common damage handling.</summary>
    internal EnemyPowerBombReaction DeadTorizoPowerBombReaction => field ??= new()
    {
        ReplacesCommon = c => TriggerDeadTorizoPowerBomb(c.Enemy),
    };

    /// <summary>Marks projectile collision and starts rotting without impact animation or health loss.</summary>
    internal EnemyShotReaction DeadTorizoShotReaction => field ??= new()
    {
        // A shot is marked as having hit the corpse, deals no damage and makes no explosion, and starts the rot.
        BeforeSuperMissileQuake = (in EnemyShotContext c) =>
        {
            c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
            TriggerDeadTorizoRotting(c.Enemy);
            return true;
        },
    };

    /// <summary>Starts the corpse rotting when Samus touches it.</summary>
    internal static void ReactToDeadTorizoTouch(EnemyTouchContext context) =>
        // Touch and shot share one reaction. Contact does no damage: it makes the corpse ignore Samus and starts it
        // rotting at once.
        TriggerDeadTorizoRotting(context.Place);
}

using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
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
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new DeadTorizoEntity(this, place, common);

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

    /// <summary>The room's enemy places, among which each corpse's sand heap is found to draw.</summary>
    private readonly EnemyPlaces _places;

    /// <summary>Binds corpse rotting to room memory, frame timing, projectiles, presentation and the room's enemy places.</summary>
    internal DeadTorizoBehavior(EnemyRoomBinding room, EnemyFrameContext frameContext, EnemyProjectileEngine projectiles, EnemyPresentation presentation, EnemyAudioQueue audio, EnemyPlaces places)
    {
        _places = places;
        _audio = audio;
        _room = room;
        _frameContext = frameContext;
        _projectiles = projectiles;
        _presentation = presentation;
    }

    /// <summary>Initializes the dead Torizo, whose corpse keeps its own state on its place like the other corpses (#1808).</summary>
    internal static void InitializeDeadTorizo(RoomEnemyPlace place)
    {
        place.Contact = place.Contact.With(EnemyContact.SolidToSamus);
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        AnimationPlayback<DeadTorizoSegment> playback = EnemyAnimationPlayback.Start(place, DeadTorizoAnimations.StartingWith(Corpses.DeadTorizoSegment.Stationary));

        place.SetSpeciesState(new DeadTorizoEnemyState(new CorpseRotSchedule(DeadTorizoCorpseDefinitions.RowCount), DeadTorizoPhase.Wait)
        {
            Playback = playback,
            Velocity = new(default, SignedEightEight.FromParts(0, 8)),
        });
    }

    /// <summary>Runs the dead Torizo's main AI.</summary>
    internal void RunDeadTorizoMain(RoomEnemyPlace place, SamusState samus)
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
                state.PreRotDelayCounter++;
                if (state.PreRotDelayCounter >= 16)
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

    /// <summary>The rotting corpse's downward acceleration per update, one 256th of a pixel per update.</summary>
    private static readonly SignedEightEight DeadTorizoRotGravity = SignedEightEight.FromParts(0, 1);

    /// <summary>Advances the corpse's fall and runs one rot call of its schedule.</summary>
    private void RunDeadTorizoRotting(RoomEnemyPlace place, DeadTorizoEnemyState state)
    {
        state.Velocity = state.Velocity.WithY(state.Velocity.Y + DeadTorizoRotGravity);
        place.Position = place.Position.MovedBy(state.Velocity);
        state.ProcessCallCount++;
        bool stillRotting = state.Rot.Step(state.Rows.MoveRow, _ => FinishDeadTorizoCorpseRow());
        if (!stillRotting)
            state.Phase = DeadTorizoPhase.Finished;
    }

    /// <summary>Shot/touch tail.</summary>
    internal static void TriggerDeadTorizoRotting(RoomEnemyPlace place)
    {
        DeadTorizoEnemyState state = place.RequireSpeciesState<DeadTorizoEnemyState>();
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
        state.Phase = DeadTorizoPhase.Rotting;
    }

    /// <summary>Power-bomb entry.</summary>
    internal static void TriggerDeadTorizoPowerBomb(RoomEnemyPlace place)
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
            int verticalDistance;
            int verticalReach;
            if (CoordinateAxis.Difference(kinematics.Position.Y.Whole, place.Position.Y.Whole) >= 0)
            {
                verticalDistance = CoordinateAxis.Wrap(kinematics.Position.Y.Whole - place.Position.Y.Whole);
                verticalReach = Math.Abs(hitbox.Bottom);
            }
            else
            {
                verticalDistance = CoordinateAxis.Wrap(place.Position.Y.Whole - kinematics.Position.Y.Whole);
                verticalReach = Math.Abs(hitbox.Top);
            }

            int verticalOverlap = CoordinateAxis.Wrap(
                kinematics.Radius.Y + verticalReach - verticalDistance);
            if (CoordinateAxis.AsSigned(verticalOverlap) < 0)
                continue;

            int horizontalDistance;
            int horizontalReach;
            if (CoordinateAxis.Difference(kinematics.Position.X.Whole, place.Position.X.Whole) >= 0)
            {
                horizontalDistance = CoordinateAxis.Wrap(kinematics.Position.X.Whole - place.Position.X.Whole);
                horizontalReach = Math.Abs(hitbox.Right);
            }
            else
            {
                horizontalDistance = CoordinateAxis.Wrap(place.Position.X.Whole - kinematics.Position.X.Whole);
                horizontalReach = Math.Abs(hitbox.Left);
            }

            int overlap = CoordinateAxis.AsSigned(
                kinematics.Radius.X + horizontalReach - horizontalDistance);
            if (overlap < 0)
                continue;
            if (overlap < 4)
                overlap = 4;

            kinematics.ExtraDisplacement = FixedVector<RoomPixels>.FromWholePixels(new(overlap, 4));
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
            MiscDustAnimation.CorpseDustCloud);
        if (CorpseCadenceDefinitions.DustSoundEveryEighthEnemyPass.IsDue(_frameContext.RandomEnemyCounter))
            _audio.QueueEnemySound(EnemySound.LiquidDamage, priority: SoundRequestPriority.Normal);
    }

    /// <summary>The body rows the dead Torizo in <paramref name="place"/> shows: its own, as its rot calls have left them (#1539).</summary>
    internal static CorpseRotRows ShownDeadTorizoRows(RoomEnemyPlace place) => place.RequireSpeciesState<DeadTorizoEnemyState>().Rows.Shown;

    /// <summary>
    /// The sand rows <paramref name="state"/>'s sand heap shows (#1539): only the rows its heap has formed into, as the corpse's
    /// own rot calls form it. Every fifteenth rot call one more line of sand falls, from the heap's last row up, until
    /// fifteen have fallen; the intact corpse has none.
    /// </summary>
    internal static CorpseRotRows ShownSand(DeadTorizoEnemyState state)
    {
        int lines = Math.Min(DeadTorizoCorpseDefinitions.SandLines,
            state.ProcessCallCount / DeadTorizoCorpseDefinitions.RotCallsPerSandLine);
        return CorpseRotRows.Showing(DeadTorizoCorpseDefinitions.SandRowCount, row => row >= DeadTorizoCorpseDefinitions.SandRowCount - lines);
    }

    /// <summary>
    /// Draws each initialized corpse's sand heap after the enemy queue, in the order of the room's enemy places,
    /// publishing its frame as the encounter's own (#1392), with the rows that corpse's own rot has formed (#1539).
    /// </summary>
    internal void DrawDeadTorizoSandHeap(SpriteFrameBuffer sprites, Position16<RoomPixels> camera)
    {
        foreach (RoomEnemyPlace place in _places.InUpdateOrder)
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
    internal static void AdvanceDeadTorizoTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _)
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
        // The dead corpses and Shitroid receive the projectile prelude but never an impact or vulnerability.
        // Its private reaction runs after the ordinary collision prelude but before common impact/vulnerability
        // code. The shot is marked, not converted into an explosion, and the corpse never loses health.
        BeforeSuperMissileQuake = (in EnemyShotContext c) =>
        {
            c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile, c.MarksCollision);
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

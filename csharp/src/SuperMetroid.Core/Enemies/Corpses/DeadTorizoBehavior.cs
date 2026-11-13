using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
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
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new DeadTorizoEntity(this, slot, common);

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

    /// <summary>Binds corpse rotting to room memory, frame timing, projectiles, and presentation.</summary>
    internal DeadTorizoBehavior(EnemyRoomBinding room, EnemyFrameContext frameContext, EnemyProjectileEngine projectiles, EnemyPresentation presentation, EnemyAudioQueue audio)
    {
        _audio = audio;
        _room = room;
        _frameContext = frameContext;
        _projectiles = projectiles;
        _presentation = presentation;
    }

    /// <summary>Single corpse state owned by enemy slot zero in the loaded room.</summary>
    internal DeadTorizoEnemyState? DeadTorizo { get; private set; }
    /// <summary>Slot zero's typed corpse pose (#641, #1600), started before the corpse state exists.</summary>
    private AnimationPlayback<DeadTorizoSegment> playback;

    /// <summary>Initializes the dead Torizo.</summary>
    internal void InitializeDeadTorizo(RoomEnemySlot slot)
    {
        if (!slot.IsListedFirst())
        {
            throw new InvalidDataException(
                $"Dead Torizo requires enemy slot zero, not slot {slot.SlotIndex}.");
        }

        slot.Contact = slot.Contact.With(EnemyContact.SolidToSamus);
        slot.Processing = slot.Processing.With(EnemyProcessing.Animates);
        playback = EnemyAnimationPlayback.Start(slot, DeadTorizoAnimations.StartingWith(Corpses.DeadTorizoSegment.Stationary));

        DeadTorizo = new DeadTorizoEnemyState(slot, new CorpseRotSchedule(DeadTorizoCorpseDefinitions.RowCount))
        {
            Function = DeadTorizoFunction.Wait,
            Velocity = new(default, SignedEightEight.FromParts(0, 8)),
        };
        slot.SetSpeciesState(DeadTorizo);
    }

    /// <summary>Runs the dead Torizo's main AI.</summary>
    internal void RunDeadTorizoMain(RoomEnemySlot slot, SamusState? samus)
    {
        DeadTorizoEnemyState state = RequireDeadTorizoState(slot);

        // The custom detector remains enabled while the intact corpse is solid. Rotting
        // disables interaction separately, after publishing Samus's external displacement.
        if (!slot.Contact.HasAny(EnemyContact.IgnoresSamus) &&
            samus is not null &&
            DeadTorizoCustomHitboxOverlaps(slot, samus))
        {
            TriggerDeadTorizoRotting(slot);
        }

        switch (state.Function ?? throw new InvalidDataException("Dead Torizo has no corpse function."))
        {
            case DeadTorizoFunction.Wait:
                if (samus?.Kinematics.DidCollideWithSolidEnemy(slot) == true)
                    state.Function = DeadTorizoFunction.PreRot;
                break;

            case DeadTorizoFunction.PreRot:
                state.PreRotDelayCounter++;
                if (state.PreRotDelayCounter >= 16)
                {
                    slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
                    state.Function = DeadTorizoFunction.Rotting;
                    RunDeadTorizoRotting(slot, state);
                }
                break;

            case DeadTorizoFunction.Rotting:
                RunDeadTorizoRotting(slot, state);
                break;

            case DeadTorizoFunction.NoOperation:
                break;

            default:
                throw new InvalidDataException(
                    $"Dead Torizo function {state.Function} is not translated.");
        }
    }

    /// <summary>The rotting corpse's downward acceleration per update, one 256th of a pixel per update.</summary>
    private static readonly SignedEightEight DeadTorizoRotGravity = SignedEightEight.FromParts(0, 1);

    /// <summary>Advances the corpse's fall and runs one rot call of its schedule.</summary>
    private void RunDeadTorizoRotting(RoomEnemySlot slot, DeadTorizoEnemyState state)
    {
        state.Velocity = state.Velocity.WithY(state.Velocity.Y + DeadTorizoRotGravity);
        MoveWithEightEightVelocity(slot, state.Velocity);
        state.ProcessCallCount++;
        bool stillRotting = state.Rot.Step(state.Rows.MoveRow, _ => FinishDeadTorizoCorpseRow());
        if (!stillRotting)
            state.Function = DeadTorizoFunction.NoOperation;
    }

    /// <summary>Shot/touch tail.</summary>
    internal void TriggerDeadTorizoRotting(RoomEnemySlot slot)
    {
        DeadTorizoEnemyState state = RequireDeadTorizoState(slot);
        slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
        state.Function = DeadTorizoFunction.Rotting;
    }

    /// <summary>Power-bomb entry.</summary>
    internal void TriggerDeadTorizoPowerBomb(RoomEnemySlot slot)
    {
        RequireDeadTorizoState(slot);
        if (!slot.Contact.HasAny(EnemyContact.IgnoresSamus))
            TriggerDeadTorizoRotting(slot);
    }

    /// <summary>Checks asymmetric corpse touch boxes and publishes Samus's minimum separation displacement.</summary>
    private static bool DeadTorizoCustomHitboxOverlaps(RoomEnemySlot slot, SamusState samus)
    {
        SamusKinematicsState kinematics = samus.Kinematics;
        foreach (EnemyHitboxRectangle hitbox in DeadTorizoTouchDefinitions.TouchHitboxes)
        {
            // The game takes the magnitude of whichever signed edge faces Samus, so each edge
            // acts as a reach on its own side of the corpse origin.
            int verticalDistance;
            int verticalReach;
            if (CoordinateAxis.Difference(kinematics.Position.Y.Whole, slot.Position.Y.Whole) >= 0)
            {
                verticalDistance = CoordinateAxis.Wrap(kinematics.Position.Y.Whole - slot.Position.Y.Whole);
                verticalReach = Math.Abs(hitbox.Bottom);
            }
            else
            {
                verticalDistance = CoordinateAxis.Wrap(slot.Position.Y.Whole - kinematics.Position.Y.Whole);
                verticalReach = Math.Abs(hitbox.Top);
            }

            int verticalOverlap = CoordinateAxis.Wrap(
                kinematics.Radius.Y + verticalReach - verticalDistance);
            if (CoordinateAxis.AsSigned(verticalOverlap) < 0)
                continue;

            int horizontalDistance;
            int horizontalReach;
            if (CoordinateAxis.Difference(kinematics.Position.X.Whole, slot.Position.X.Whole) >= 0)
            {
                horizontalDistance = CoordinateAxis.Wrap(kinematics.Position.X.Whole - slot.Position.X.Whole);
                horizontalReach = Math.Abs(hitbox.Right);
            }
            else
            {
                horizontalDistance = CoordinateAxis.Wrap(slot.Position.X.Whole - kinematics.Position.X.Whole);
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
        _projectiles.SpawnRoomGraphicsDustExplosion(
            new(CorpseDustDefinitions.TorizoSpread.Of(random) + 272, 188),
            MiscDustAnimation.CorpseDustCloud);
        if (CorpseCadenceDefinitions.DustSoundEveryEighthEnemyPass.IsDue(_frameContext.RandomEnemyCounter))
            _audio.QueueEnemySound(EnemySound.LiquidDamage, priority: SoundRequestPriority.Normal);
    }

    /// <summary>
    /// Applies shared signed 8.8 enemy velocity pair. Dead Torizo and Shitroid
    /// both call the same shared helper; retaining one implementation also makes explicit that
    /// the step moves the subposition only in 256ths of a pixel.
    /// </summary>
    internal static void MoveWithEightEightVelocity(RoomEnemySlot slot, EightEightVector<RoomPixels> velocity)
    {
        FixedCoordinate x = slot.Position.X.MovedBy(velocity.X);
        slot.Position = slot.Position.WithFractionX(x.Fraction).WithWholeX(x.Whole);

        FixedCoordinate y = slot.Position.Y.MovedBy(velocity.Y);
        slot.Position = slot.Position.WithFractionY(y.Fraction).WithWholeY(y.Whole);
    }

    /// <summary>The body rows the dead Torizo in <paramref name="slot"/> shows: its own, as its rot calls have left them (#1539).</summary>
    internal CorpseRotRows ShownDeadTorizoRows(RoomEnemySlot slot) => RequireDeadTorizoState(slot).Rows.Shown;

    /// <summary>
    /// The sand rows <paramref name="state"/>'s sand heap shows (#1539): only the rows its heap has formed into, as the corpse's
    /// own rot calls form it. Every fifteenth rot call one more line of sand falls, from the heap's last row up, until
    /// fifteen have fallen; the intact corpse has none.
    /// </summary>
    internal static CorpseRotRows ShownSand(DeadTorizoEnemyState state)
    {
        int lines = Math.Min(DeadTorizoCorpseDefinitions.SandLines,
            checked((int)state.ProcessCallCount) / DeadTorizoCorpseDefinitions.RotCallsPerSandLine);
        return CorpseRotRows.Showing(DeadTorizoCorpseDefinitions.SandRowCount, row => row >= DeadTorizoCorpseDefinitions.SandRowCount - lines);
    }

    /// <summary>
    /// Draws the corpse's sand heap after the enemy queue, publishing its frame as the encounter's own (#1392), with the
    /// rows the corpse's own rot has formed (#1539).
    /// </summary>
    internal void DrawDeadTorizoSandHeap(SpriteFrameBuffer sprites, Position16<RoomPixels> camera)
    {
        if (DeadTorizo is not { } state)
            return;

        // The heap is published wherever it lies; the display clips it to the view (#1541).
        int screenY = CoordinateAxis.Wrap(187 - camera.Y);
        _presentation.PublishEncounterFrame(sprites, new SpriteSource(SpriteSourceKind.DeadTorizoSandHeap), DeadTorizoArtworkDefinitions.SandHeapFrame,
            new Position16<ScreenPixels>(CoordinateAxis.Wrap(296 - camera.X), screenY), PictureShading.Standard, rows: ShownSand(state));
    }

    /// <summary>Requires the initialized slot-zero corpse state and rejects unrelated enemy slots.</summary>
    private DeadTorizoEnemyState RequireDeadTorizoState(RoomEnemySlot slot)
    {
        DeadTorizoEnemyState state = DeadTorizo ??
            throw new InvalidDataException("Dead Torizo has no initialized extended state.");
        if (!ReferenceEquals(state.Slot, slot))
            throw new InvalidDataException("A non-Dead-Torizo slot entered its dispatcher.");
        return state;
    }

    /// <summary>Routes corpse bomb contact to rotting after the common no-op gate.</summary>
    internal void AddDeadTorizoBombReactions(EnemySharedReactions table)
    {
        // Specials consulted after the selected literal no-op gate.
        // Corpse shot reactions run directly after the collision mark. They do not consult bomb
        // vulnerability or health; they only hand the actor to its authored rotting state machine.
        table.AddBombSpecialAfterNoOp(c => c.Enemy.DefinitionId == EnemyDefinitionId.CorpseTorizo &&
                                           c.SelectedShotAi == EnemyReaction.DeadTorizoTouchAndShot,
            c => TriggerDeadTorizoRotting(c.Enemy));
    }

    /// <summary>Advances DeadTorizo's typed animation.</summary>
    internal void AdvanceDeadTorizoTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _)
    {
        if (!slot.AnimationClock.Expire())
            return;
        EnemyAnimationPlayback.Play(slot, DeadTorizoAnimations.Segments, ref playback,
            static cue => throw new InvalidOperationException($"A cue-free dead Torizo animation raised {cue}."));
    }

    /// <summary>Runs DeadTorizo's main AI.</summary>
    internal void RunDeadTorizoMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunDeadTorizoMain(slot, frame.Samus);
    }

    /// <summary>Starts corpse rotting on a power bomb without common damage handling.</summary>
    internal void AddDeadTorizoPowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.CorpseTorizo &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyReaction.DeadTorizoPowerBomb)
        {
            ReplacesCommon = (enemy, _) => TriggerDeadTorizoPowerBomb(enemy),
        });
    }

    /// <summary>Marks projectile collision and starts rotting without impact animation or health loss.</summary>
    internal void AddDeadTorizoShotReactions(EnemySharedReactions table)
    {
        // The dead corpses and Shitroid receive the projectile prelude but never an impact or vulnerability.
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.CorpseTorizo &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.DeadTorizoTouchAndShot)
        {
            // Its private reaction runs after the ordinary collision prelude but before common impact/vulnerability
            // code. The shot is marked, not converted into an explosion, and the corpse never loses health.
            BeforeSuperMissileQuake = (in EnemyShotContext c) =>
            {
                c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile, c.MarksCollision);
                TriggerDeadTorizoRotting(c.Enemy);
                return true;
            },
        });
    }

    /// <summary>Starts the same rotting tail on direct Samus contact.</summary>
    internal void AddDeadTorizoTouch(EnemySharedReactions table)
    {
        // Touch and shot share one reaction. Contact does not enter common damage; it makes the corpse solid
        // and starts the exact rotting table immediately.
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.CorpseTorizo &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.DeadTorizoTouchAndShot,
            context => TriggerDeadTorizoRotting(context.Slot));
    }
}

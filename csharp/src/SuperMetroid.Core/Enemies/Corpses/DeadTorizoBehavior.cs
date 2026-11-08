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

/// <summary>The dead Torizo species owner: its per-room state, AI routines and the interactions it registers.</summary>
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
    /// <summary>Draws the fixed corpse hook using installed spritemap parts.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Receives the species' end-of-update sound requests.</summary>
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
    /// <summary>Slot zero's typed corpse pose (#641), started before the corpse state exists.</summary>
    private AnimationProgramCursor<DeadTorizoProgram> program;

    /// <summary>Initializes the dead Torizo.</summary>
    internal void InitializeDeadTorizo(RoomEnemySlot slot)
    {
        if (!slot.IsListedFirst())
        {
            throw new InvalidDataException(
                $"Dead Torizo requires enemy slot zero, not slot {slot.SlotIndex}.");
        }

        slot.Properties = slot.Properties.With(
            EnemyProperties.SolidToSamus | EnemyProperties.RunsAnimationScript);
        program = AnimationProgramCursor<DeadTorizoProgram>.Start(Corpses.DeadTorizoProgram.Stationary);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);

        DeadTorizo = new DeadTorizoEnemyState(slot, new CorpseRotSchedule(DeadTorizoCorpseDefinitions.RowCount))
        {
            Function = DeadTorizoFunction.Wait,
            Velocity = new(default, new SignedEightEight(8)),
        };
        slot.SetSpeciesState(DeadTorizo, DeadTorizo.ClearWithRecord);
    }

    /// <summary>Runs the dead Torizo's main AI.</summary>
    internal void RunDeadTorizoMain(RoomEnemySlot slot, SamusState? samus)
    {
        DeadTorizoEnemyState state = RequireDeadTorizoState(slot);

        // The custom detector remains enabled while the intact corpse is solid. Rotting
        // disables interaction separately, after publishing Samus's external displacement.
        if (!slot.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) &&
            samus is not null &&
            DeadTorizoCustomHitboxOverlaps(slot, samus))
        {
            TriggerDeadTorizoRotting(slot);
        }

        switch (state.Function ?? throw new InvalidDataException("Dead Torizo has no corpse function."))
        {
            case DeadTorizoFunction.Wait:
                if (samus?.Kinematics.DidCollideWithSolidEnemy(slot.SlotIndex) == true)
                    state.Function = DeadTorizoFunction.PreRot;
                break;

            case DeadTorizoFunction.PreRot:
                state.PreRotDelayCounter++;
                if (state.PreRotDelayCounter >= 16)
                {
                    slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
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

        ShowNextRotHalf(state);
    }

    /// <summary>Advances the corpse's fall and runs one rot call of its schedule.</summary>
    private void RunDeadTorizoRotting(RoomEnemySlot slot, DeadTorizoEnemyState state)
    {
        state.Velocity = state.Velocity.WithY(SignedEightEight.Wrapped(state.Velocity.Y.RawValue + 1));
        MoveWithEightEightVelocity(slot, state.Velocity);
        state.ProcessCallCount++;
        bool stillRotting = state.Rot.Step((row, operation) => state.Rows.MoveRow(row, operation), _ => FinishDeadTorizoCorpseRow(state));
        if (!stillRotting)
            state.Function = DeadTorizoFunction.NoOperation;
    }

    /// <summary>Shot/touch tail.</summary>
    internal void TriggerDeadTorizoRotting(RoomEnemySlot slot)
    {
        DeadTorizoEnemyState state = RequireDeadTorizoState(slot);
        slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
        state.Function = DeadTorizoFunction.Rotting;
    }

    /// <summary>Power-bomb entry.</summary>
    internal void TriggerDeadTorizoPowerBomb(RoomEnemySlot slot)
    {
        RequireDeadTorizoState(slot);
        if (!slot.Properties.HasAny(EnemyProperties.IgnoreSamusCollision))
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

    /// <summary>Records a finished rot row, spawns dust from shared RNG, and optionally publishes sound.</summary>
    private void FinishDeadTorizoCorpseRow(DeadTorizoEnemyState state)
    {
        state.FinishedEntryCount++;
        int random = _room.Random.Read();
        _projectiles.SpawnRoomGraphicsDustExplosion(
            new(CorpseDustDefinitions.TorizoSpread.Of(random) + 272, 188),
            MiscDustAnimation.CorpseDustCloud);
        state.DustSpawnCount++;
        if ((_frameContext.RandomEnemyCounter & 7) == 0)
            _audio.RequestSoundAtUpdateEnd(EnemySoundSource.DeadTorizo, Library2Sound.LiquidDamage, priority: SoundRequestPriority.Normal);
    }

    /// <summary>
    /// Applies shared signed 8.8 enemy velocity pair. Dead Torizo and Shitroid
    /// both call the same shared helper; retaining one implementation also makes its
    /// unusual use of the high byte of the subposition explicit.
    /// </summary>
    internal static void MoveWithEightEightVelocity(RoomEnemySlot slot, EightEightVector<RoomPixels> velocity)
    {
        FixedCoordinate x = slot.Position.X.MovedBy(velocity.X);
        slot.Position = slot.Position.WithFractionX(x.Fraction).WithWholeX(x.Whole);

        FixedCoordinate y = slot.Position.Y.MovedBy(velocity.Y);
        slot.Position = slot.Position.WithFractionY(y.Fraction).WithWholeY(y.Whole);
    }

    /// <summary>
    /// Switches to the other half of the corpse's rows and shows that half as the rot calls the corpse has taken leave it
    /// (#1406, #1531): its body rows and its strip of sand. The game uploads one half each main-AI call, while the corpse is
    /// intact, rotting or finished, so the half shows those rows from this update on.
    /// </summary>
    internal static void ShowNextRotHalf(DeadTorizoEnemyState state)
    {
        state.ShowsOddHalf = !state.ShowsOddHalf;
        int step = checked((int)state.ProcessCallCount);
        if (state.ShowsOddHalf)
        {
            state.OddHalfStep = step;
            state.OddHalfRows = state.Rows.Shown;
        }
        else
        {
            state.EvenHalfStep = step;
            state.EvenHalfRows = state.Rows.Shown;
        }
    }

    /// <summary>The body rows the dead Torizo in <paramref name="slot"/> shows: each half's as it last uploaded it (#1531).</summary>
    internal CorpseRotRows ShownDeadTorizoRows(RoomEnemySlot slot) => ShownBody(RequireDeadTorizoState(slot));

    /// <summary>
    /// The body rows <paramref name="state"/>'s corpse shows: the even half's above the odd half's first row and the odd
    /// half's from it down, each as the half was last uploaded, or showing itself before its first upload (#1531).
    /// </summary>
    internal static CorpseRotRows ShownBody(DeadTorizoEnemyState state) =>
        CorpseRotRows.Joined(state.EvenHalfRows ?? UnrottedBody, state.OddHalfRows ?? UnrottedBody, DeadTorizoCorpseDefinitions.OddHalfFirstRow);

    /// <summary>
    /// The sand rows <paramref name="state"/>'s hook shows (#1531): each strip as its half was last uploaded, or showing
    /// itself before its first upload. Once uploaded, a strip shows only the rows its sand has fallen into: every fifteenth
    /// rot call one more line falls, from the last up, until fifteen have fallen.
    /// </summary>
    internal static CorpseRotRows ShownSand(DeadTorizoEnemyState state) =>
        CorpseRotRows.Showing(DeadTorizoCorpseDefinitions.SandRowCount, row => SandShows(row,
            row < DeadTorizoCorpseDefinitions.SandStripRows ? state.OddHalfStep : state.EvenHalfStep));

    /// <summary>Whether sand row <paramref name="row"/> shows, its strip last uploaded after <paramref name="step"/> rot calls, or never.</summary>
    private static bool SandShows(int row, int? step) => step is not int calls
        || row >= DeadTorizoCorpseDefinitions.SandRowCount
            - Math.Min(DeadTorizoCorpseDefinitions.SandLines, calls / DeadTorizoCorpseDefinitions.RotCallsPerSandLine);

    /// <summary>The corpse's body rows before any rot call.</summary>
    private static readonly CorpseRotRows UnrottedBody = CorpseRotRows.Unrotted(DeadTorizoCorpseDefinitions.RowCount);

    /// <summary>
    /// Runs the post-enemy ordinary-spritemap hook, publishing its frame as the encounter's own (#1392),
    /// over the corpse's rows as their rot shows them.
    /// </summary>
    internal void DrawDeadTorizoHook(SpriteFrameBuffer sprites, Position16<RoomPixels> camera)
    {
        if (DeadTorizo is not { } state)
            return;

        // The hook is published wherever it lies; the display clips it to the view (#1541).
        int screenY = CoordinateAxis.Wrap(187 - camera.Y);
        _presentation.PublishEncounterFrame(sprites, new SpriteSource(SpriteSourceKind.DeadTorizoHook), DeadTorizoArtworkDefinitions.HookFrame,
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
        // Corpse shot callbacks run directly after the collision mark. They do not consult bomb
        // vulnerability or health; they only hand the actor to its authored rotting state machine.
        table.AddBombSpecialAfterNoOp(c => c.Enemy.DefinitionId == EnemyDefinitionId.CorpseTorizo &&
                                           c.SelectedShotAi == EnemyReaction.DeadTorizoTouchAndShot,
            c => TriggerDeadTorizoRotting(c.Enemy));
    }

    /// <summary>Advances DeadTorizo's typed animation.</summary>
    internal void AdvanceDeadTorizoTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        EnemyProgramAnimation.Run(slot, DeadTorizoAnimations.Programs, ref program, DeadTorizoFrameDefinitions.ShownFrames.Of);
    }

    /// <summary>Ports DeadTorizo's main AI.</summary>
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
            // Its private callback runs after the ordinary collision prelude but before common impact/vulnerability
            // code. The shot is marked, not converted into an explosion, and the corpse never loses health.
            BeforeSuperMissileQuake = (in EnemyShotContext c) =>
            {
                c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
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

using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Enemies.Drawing;
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

    /// <summary>Supplies corpse scratch WRAM, installed tiles, and the shared random seed.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Supplies the enemy-frame counter used to cadence rotting sounds.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Spawns dust when individual corpse rows finish rotting.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Draws the fixed corpse hook using installed spritemap parts.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Binds corpse rotting to room memory, frame timing, projectiles, and presentation.</summary>
    internal DeadTorizoBehavior(EnemyRoomBinding room, EnemyFrameContext frameContext, EnemyProjectileEngine projectiles, EnemyPresentation presentation)
    {
        _room = room;
        _frameContext = frameContext;
        _projectiles = projectiles;
        _presentation = presentation;
    }

    /// <summary>Single corpse state owned by enemy slot zero in the loaded room.</summary>
    internal DeadTorizoEnemyState? DeadTorizo { get; private set; }
    /// <summary>Slot zero's typed corpse pose (#641), started before the corpse state exists.</summary>
    private AnimationProgramCursor<DeadTorizoProgram> program;

    /// <summary>The dust sound requested by a completed corpse row this frame, or null.</summary>
    public SoundEffectId? LastDeadTorizoSoundEffect { get; private set; }

    /// <summary>Ports <c>DeadTorizo_Init</c> at <c>$A9:D308</c>.</summary>
    internal void InitializeDeadTorizo(RoomEnemySlot slot)
    {
        if (slot.SlotIndex != 0)
        {
            throw new InvalidDataException(
                $"Dead Torizo requires native slot zero, not slot {slot.SlotIndex}.");
        }

        slot.Properties = slot.Properties.With(
            EnemyProperties.SolidToSamus | EnemyProperties.ProcessInstructions);
        program = AnimationProgramCursor<DeadTorizoProgram>.Start(Corpses.DeadTorizoProgram.Stationary);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        slot.PaletteIndex = EnemyPaletteBits.Palette1;

        DeadTorizoCorpseDefinition definition = DeadTorizoCorpseDefinitions.Corpse;
        int yLimit = definition.EntryCount - 1;
        DeadTorizo = new DeadTorizoEnemyState(slot,
            new CorpseRottingProgram { YLimit = yLimit, LateMoveEntryIndex = yLimit - 1, WrapOffset = definition.WrapOffset },
            _room.Staging.RotTable(definition.RotTableOffset, definition.EntryCount))
        {
            Function = DeadTorizoFunction.Wait,
            Velocity = new(default, new SnesSignedEightEight(8)),
        };
        slot.SetSpeciesState(DeadTorizo, DeadTorizo.ClearWithRecord);

        CorpseRottingTableProcessor.Initialize(DeadTorizo.RotTable);
    }

    /// <summary>Ports <c>DeadTorizo_Main</c> at <c>$A9:D368</c>.</summary>
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
                if (samus?.Kinematics.DidCollideWithSolidEnemy(slot.EnemyIndex) == true)
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

    /// <summary>Advances sand rows, corpse motion, and one step of the native rotting table.</summary>
    private void RunDeadTorizoRotting(RoomEnemySlot slot, DeadTorizoEnemyState state)
    {
        state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(state.Velocity.Y.RawValue + 1))));
        MoveBankA9EnemyWithVelocity(slot, state.Velocity);
        state.ProcessCallCount++;
        bool stillRotting = CorpseRottingTableProcessor.Step(
            state.RotTable,
            state.YLimit,
            state.LateMoveEntryIndex,
            _ => FinishDeadTorizoCorpseRow(state));
        if (!stillRotting)
            state.Function = DeadTorizoFunction.NoOperation;
    }

    /// <summary>Shot/touch tail at <c>$A9:D433</c>.</summary>
    internal void TriggerDeadTorizoRotting(RoomEnemySlot slot)
    {
        DeadTorizoEnemyState state = RequireDeadTorizoState(slot);
        slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
        state.Function = DeadTorizoFunction.Rotting;
    }

    /// <summary>Power-bomb entry at <c>$A9:D42A</c>.</summary>
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
            // The cartridge takes the magnitude of whichever signed edge faces Samus, so each edge
            // acts as a reach on its own side of the corpse origin.
            ushort verticalDistance;
            int verticalReach;
            if (unchecked((short)(kinematics.Position.Y.Whole - slot.Position.Y.Whole)) >= 0)
            {
                verticalDistance = unchecked((ushort)(kinematics.Position.Y.Whole - slot.Position.Y.Whole));
                verticalReach = Math.Abs((int)hitbox.Bottom);
            }
            else
            {
                verticalDistance = unchecked((ushort)(slot.Position.Y.Whole - kinematics.Position.Y.Whole));
                verticalReach = Math.Abs((int)hitbox.Top);
            }

            ushort verticalOverlap = unchecked((ushort)(
                kinematics.Radius.Y + verticalReach - verticalDistance));
            if (unchecked((short)verticalOverlap) < 0)
                continue;

            ushort horizontalDistance;
            int horizontalReach;
            if (unchecked((short)(kinematics.Position.X.Whole - slot.Position.X.Whole)) >= 0)
            {
                horizontalDistance = unchecked((ushort)(kinematics.Position.X.Whole - slot.Position.X.Whole));
                horizontalReach = Math.Abs((int)hitbox.Right);
            }
            else
            {
                horizontalDistance = unchecked((ushort)(slot.Position.X.Whole - kinematics.Position.X.Whole));
                horizontalReach = Math.Abs((int)hitbox.Left);
            }

            short overlap = unchecked((short)(
                kinematics.Radius.X + horizontalReach - horizontalDistance));
            if (overlap < 0)
                continue;
            if (overlap < 4)
                overlap = 4;

            kinematics.ExtraDisplacement = FixedVector<RoomPixels>.FromWholePixels(new(unchecked(overlap), 4));
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
            new(unchecked((ushort)((random & CorpseDustDefinitions.TorizoSpreadBits) + 272)), 188),
            MiscDustAnimation.CorpseDustCloud);
        state.DustSpawnCount++;
        if ((_frameContext.RandomEnemyCounter & 7) == 0)
            LastDeadTorizoSoundEffect = Library2Sound.LiquidDamage;
    }


    /// <summary>
    /// Applies bank-$A9's shared signed 8.8 enemy velocity pair. Dead Torizo and Shitroid
    /// both call the same cartridge helper; retaining one implementation also makes its
    /// unusual use of the high byte of the subposition explicit.
    /// </summary>
    internal static void MoveBankA9EnemyWithVelocity(RoomEnemySlot slot, EightEightVector<RoomPixels> velocity)
    {
        SnesFixedPosition x = new SnesFixedPosition(slot.Position.X.Whole, slot.Position.X.Fraction)
            .AddEightEightToHighFraction(velocity.X.RawValue);
        slot.Position = slot.Position.WithFractionX(x.Fraction).WithWholeX(x.Whole);

        SnesFixedPosition y = new SnesFixedPosition(slot.Position.Y.Whole, slot.Position.Y.Fraction)
            .AddEightEightToHighFraction(velocity.Y.RawValue);
        slot.Position = slot.Position.WithFractionY(y.Fraction).WithWholeY(y.Whole);
    }

    /// <summary>
    /// Switches to the other half of the corpse's rows and selects the rot steps the corpse has taken as the step that half
    /// shows (#1406). The cartridge uploads one half each main-AI call, while the corpse is intact, rotting or finished, so
    /// the half's pictures show the step from this update on.
    /// </summary>
    internal static void ShowNextRotHalf(DeadTorizoEnemyState state)
    {
        state.ShowsOddHalf = !state.ShowsOddHalf;
        int step = checked((int)state.ProcessCallCount);
        if (state.ShowsOddHalf)
            state.OddHalfStep = step;
        else
            state.EvenHalfStep = step;
    }

    /// <summary>The rot the dead Torizo in <paramref name="slot"/> shows: the step each half of its rows shows, even then odd (#1406).</summary>
    internal CorpseRotView ShownDeadTorizoRot(RoomEnemySlot slot) => ShownRot(RequireDeadTorizoState(slot));

    /// <summary>The rot <paramref name="state"/>'s corpse shows: the step each half of its rows shows, even then odd.</summary>
    private static CorpseRotView ShownRot(DeadTorizoEnemyState state) =>
        new(DeadTorizoCorpseDefinitions.Kind, [state.EvenHalfStep, state.OddHalfStep]);


    /// <summary>
    /// Runs the post-enemy ordinary-spritemap hook at <c>$A9:D39A</c>, publishing its frame as the encounter's own (#1392),
    /// over the corpse's rows as their rot shows them.
    /// </summary>
    internal void DrawDeadTorizoHook(SpriteFrameBuffer sprites, Position16<RoomPixels> camera)
    {
        if (DeadTorizo is not { } state)
            return;

        ushort screenY = unchecked((ushort)(187 - camera.Y));
        if (unchecked((short)screenY) < 0)
            return;
        _presentation.PublishEncounterFrame(sprites, new SpriteSource(SpriteSourceKind.DeadTorizoHook), DeadTorizoArtworkDefinitions.HookFrame,
            new Position16<ScreenPixels>(unchecked((ushort)(296 - camera.X)), screenY), paletteBits: 0, rot: ShownRot(state));
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



    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastDeadTorizoSoundEffect = null;
    }

    /// <summary>Routes corpse bomb contact to rotting after the common no-op gate.</summary>
    internal void RegisterDeadTorizoBombReactions(EnemySharedReactions table)
    {
        // Specials consulted after the selected literal no-op gate.
        // Corpse shot callbacks run directly after the bank-$A0 collision mark. They do not consult bomb
        // vulnerability or health; they only hand the actor to its authored rotting state machine.
        table.AddBombSpecialAfterNoOp(c => c.Enemy.DefinitionId == EnemyDefinitionId.CorpseTorizo &&
                                           c.SelectedShotAi == EnemyInteractionCallback.DeadTorizoTouchAndShot,
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
    internal void RegisterDeadTorizoPowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.CorpseTorizo &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.DeadTorizoPowerBomb)
        {
            ReplacesCommon = (enemy, _) => TriggerDeadTorizoPowerBomb(enemy),
        });
    }

    /// <summary>Marks projectile collision and starts rotting without impact animation or health loss.</summary>
    internal void RegisterDeadTorizoShotReactions(EnemySharedReactions table)
    {
        // The dead corpses and Shitroid receive the bank-$A0 projectile prelude but never an impact or vulnerability.
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.CorpseTorizo &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.DeadTorizoTouchAndShot)
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
    internal void RegisterDeadTorizoTouch(EnemySharedReactions table)
    {
        // `$A9:D433` is shared by touch and shot. Contact does not enter common damage; it makes the corpse solid
        // and starts the exact rotting table immediately.
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.CorpseTorizo &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.DeadTorizoTouchAndShot,
            context => TriggerDeadTorizoRotting(context.Slot));
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastDeadTorizoSoundEffect is SoundEffectId sound)
            audio.QueueEnemySound(sound, maximumQueued: SoundQueueLimit.Max6);
    }
}

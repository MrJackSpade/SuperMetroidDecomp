using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Powamp species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class PowampBehavior : IEnemySpecies
{
    private readonly EnemyTerrain _terrain;
    private readonly EnemyProjectileEngine _projectiles;
    private readonly EnemySlotTable _slotTable;
    private readonly EnemyCollision _collision;
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    internal PowampBehavior(EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemySlotTable slotTable, EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops)
    {
        _terrain = terrain;
        _projectiles = projectiles;
        _slotTable = slotTable;
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
    }

    private const ushort PowampUngrappledTravelDistance = 0x0040;
    private const ushort PowampRestFrames = 0x003c;
    private const ushort PowampTransitionFrames = 0x000a;
    private const ushort PowampDeathDelayFrames = 0x0020;
    private const ushort PowampWiggleFramesPerOffset = 0x0005;

    internal readonly PowampEnemyState?[] _powampStates =
        new PowampEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Powamp</c> at $A8:C1C9.</summary>
    internal void InitializePowamp(RoomEnemySlot slot)
    {
        var state = new PowampEnemyState(slot);
        _powampStates[slot.SlotIndex] = state;

        // The initializer forces animation processing for both halves and clears the
        // instruction interpreter's loop counter before choosing the half-specific list.
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.SpritemapPointer = 0x804d;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);

        if (!state.IsBalloon)
        {
            // Enemy[-1] is not a descriptive relationship in the original—it is a literal
            // $40-byte negative index. Reject malformed populations instead of accidentally
            // binding a body to an unrelated actor.
            RequirePowampBalloon(slot);
            state.FunctionTimer = NativeCountdown.Start(PowampRestFrames);
            state.Function = PowampEnemyFunction.DeflatedResting;
            SetPowampInstruction(slot, PowampInstructionProgramDefinitions.BodySlow);
            return;
        }

        state.BalloonSpawn = slot.Position.Whole;
        state.Function = PowampEnemyFunction.BalloonNoOp;
        SetPowampInstruction(slot, PowampInstructionProgramDefinitions.BalloonDeflated);
        state.BalloonGrappleTravelDistance = slot.Parameter2;
    }

    /// <summary>Ports <c>MainAI_Powamp</c> and every body function it dispatches.</summary>
    internal void RunPowampMain(RoomEnemySlot slot, PowampEnemyState state, RoomLevelData? level)
    {
        if (state.IsBalloon)
            return;

        RoomEnemySlot balloon = RequirePowampBalloon(slot);
        switch (state.Function)
        {
            case PowampEnemyFunction.DeflatedResting:
                RunPowampDeflatedResting(slot, state, balloon);
                return;
            case PowampEnemyFunction.Inflating:
                RunPowampInflating(slot, state, balloon);
                return;
            case PowampEnemyFunction.InflatedRiseToTargetHeight:
                RunPowampRise(slot, state, balloon, RequirePowampLevel(level), grappled: false);
                return;
            case PowampEnemyFunction.InflatedFinishWiggle:
                RunPowampFinishWiggle(slot, state, balloon, RequirePowampLevel(level), grappled: false);
                return;
            case PowampEnemyFunction.GrappledRiseToTargetHeight:
                RunPowampRise(slot, state, balloon, RequirePowampLevel(level), grappled: true);
                return;
            case PowampEnemyFunction.GrappledFinishWiggle:
                RunPowampFinishWiggle(slot, state, balloon, RequirePowampLevel(level), grappled: true);
                return;
            case PowampEnemyFunction.GrappledResting:
                RunPowampGrappledResting(slot, state, balloon);
                return;
            case PowampEnemyFunction.Deflating:
                RunPowampDeflating(slot, state, balloon);
                return;
            case PowampEnemyFunction.DeflatedSinking:
                RunPowampSinking(slot, state, balloon, RequirePowampLevel(level));
                return;
            case PowampEnemyFunction.FatalDamage:
                BeginPowampDeathSequence(slot, state, balloon);
                return;
            case PowampEnemyFunction.DeathSequence:
                RunPowampDeathSequence(slot, state, balloon);
                return;
            default:
                throw new InvalidDataException(
                    $"Powamp body function $A8:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private static void RunPowampDeflatedResting(
        RoomEnemySlot body,
        PowampEnemyState state,
        RoomEnemySlot balloon)
    {
        if (TickPowampTimer(state))
        {
            SetPowampInstruction(balloon, PowampInstructionProgramDefinitions.BalloonInflate0);
            state.Function = PowampEnemyFunction.Inflating;
            state.FunctionTimer = NativeCountdown.Start(PowampTransitionFrames);
        }
        AlignPowampBalloonY(body, balloon);
    }

    private static void RunPowampInflating(
        RoomEnemySlot body,
        PowampEnemyState state,
        RoomEnemySlot balloon)
    {
        if (TickPowampTimer(state))
        {
            state.Function = PowampEnemyFunction.InflatedRiseToTargetHeight;
            // $FFFF:$8000 is exactly -0.5 pixels/frame on the NTSC cartridge.
            state.YVelocity = 0xffff;
            state.YSubvelocity = 0x8000;
            SetPowampInstruction(body, PowampInstructionProgramDefinitions.BodyFast);
        }
        AlignPowampBalloonY(body, balloon);
    }

    private void RunPowampRise(
        RoomEnemySlot body,
        PowampEnemyState state,
        RoomEnemySlot balloon,
        RoomLevelData level,
        bool grappled)
    {
        // The ordinary rise changes dispatch and executes the grappled routine immediately
        // on the frame common grapple AI publishes bit one.
        bool grappleAiActive = (body.AiHandlerBits & 1) != 0;
        if (!grappled && grappleAiActive)
        {
            state.Function = PowampEnemyFunction.GrappledRiseToTargetHeight;
            RunPowampRise(body, state, balloon, level, grappled: true);
            return;
        }

        // Releasing a grapple during either grappled rising state consumes the frame after
        // changing the function. In particular it does not realign the balloon on this path.
        if (grappled && !grappleAiActive)
        {
            state.Function = PowampEnemyFunction.InflatedFinishWiggle;
            return;
        }

        AdvancePowampWiggle(body, state, balloon, stopWhenCentered: false);
        bool collided = _terrain.MoveEnemyVertically(level, body, ComposePowampVerticalDisplacement(state));
        int travelDistance = grappled
            ? RequirePowampState(balloon).BalloonGrappleTravelDistance
            : PowampUngrappledTravelDistance;
        ushort targetY = unchecked((ushort)(RequirePowampState(balloon).BalloonSpawn.Y - travelDistance));

        // CMP target,body followed by BMI means "continue while body is still below the
        // target". A room collision completes the rise regardless of its Y coordinate.
        bool reachedTarget = !EnemyMath.IsNegative16(targetY - body.Position.Y.Whole);
        if (collided || reachedTarget)
        {
            if (IsPowampCentered(state.WiggleIndex))
            {
                if (grappled)
                    state.Function = PowampEnemyFunction.GrappledResting;
                else
                    StartPowampDeflating(state, balloon);
            }
            else
            {
                state.Function = grappled
                    ? PowampEnemyFunction.GrappledFinishWiggle
                    : PowampEnemyFunction.InflatedFinishWiggle;
            }
        }
        AlignPowampBalloonY(body, balloon);
    }

    private void RunPowampFinishWiggle(
        RoomEnemySlot body,
        PowampEnemyState state,
        RoomEnemySlot balloon,
        RoomLevelData level,
        bool grappled)
    {
        bool grappleAiActive = (body.AiHandlerBits & 1) != 0;
        if (grappled && !grappleAiActive)
        {
            state.Function = PowampEnemyFunction.InflatedFinishWiggle;
            return;
        }

        bool centered = AdvancePowampWiggle(body, state, balloon, stopWhenCentered: true);
        if (centered)
        {
            if (grappled)
                state.Function = PowampEnemyFunction.GrappledResting;
            else
                StartPowampDeflating(state, balloon);
        }
        else
        {
            // Native finish-wiggle ignores collision carry; terrain clips the displacement
            // but does not select another function until the horizontal wiggle is centered.
            _terrain.MoveEnemyVertically(level, body, ComposePowampVerticalDisplacement(state));
        }
        AlignPowampBalloonY(body, balloon);
    }

    private static void RunPowampGrappledResting(
        RoomEnemySlot body,
        PowampEnemyState state,
        RoomEnemySlot balloon)
    {
        if ((body.AiHandlerBits & 1) == 0)
            StartPowampDeflating(state, balloon);
        AlignPowampBalloonY(body, balloon);
    }

    private static void RunPowampDeflating(
        RoomEnemySlot body,
        PowampEnemyState state,
        RoomEnemySlot balloon)
    {
        if (TickPowampTimer(state))
        {
            state.Function = PowampEnemyFunction.DeflatedSinking;
            // $0001:$0000 is exactly +1 pixel/frame.
            state.YVelocity = 1;
            state.YSubvelocity = 0;
        }
        AlignPowampBalloonY(body, balloon);
    }

    private void RunPowampSinking(
        RoomEnemySlot body,
        PowampEnemyState state,
        RoomEnemySlot balloon,
        RoomLevelData level)
    {
        _terrain.MoveEnemyVertically(level, body, ComposePowampVerticalDisplacement(state));
        ushort spawnY = RequirePowampState(balloon).BalloonSpawn.Y;
        if (!EnemyMath.IsNegative16(body.Position.Y.Whole - spawnY))
        {
            body.Position = body.Position.WithWholeY(spawnY);
            state.Function = PowampEnemyFunction.DeflatedResting;
            state.FunctionTimer = NativeCountdown.Start(PowampRestFrames);
            SetPowampInstruction(body, PowampInstructionProgramDefinitions.BodySlow);
        }
        AlignPowampBalloonY(body, balloon);
    }

    private static void BeginPowampDeathSequence(
        RoomEnemySlot body,
        PowampEnemyState state,
        RoomEnemySlot balloon)
    {
        ushort cursor = balloon.CurrentInstruction;
        if (cursor >= PowampInstructionProgramDefinitions.BalloonStartSinking)
        {
            ushort byteOffset = unchecked((ushort)(
                cursor - 4 - PowampInstructionProgramDefinitions.BalloonStartSinking));
            byteOffset >>= 1;
            if (byteOffset != 0)
            {
                // The native pointer table is indexed by the byte offset left in Y. Offset
                // two selects inflate stage one and offset four restarts stage zero; entry
                // zero is skipped.
                ushort replacement = byteOffset switch
                {
                    2 => PowampInstructionProgramDefinitions.BalloonInflate1,
                    4 => PowampInstructionProgramDefinitions.BalloonInflate0,
                    _ => throw new InvalidDataException(
                        $"Powamp death saw unsupported balloon cursor $A8:{cursor:X4}."),
                };
                SetPowampInstruction(balloon, replacement);
            }
        }

        state.Function = PowampEnemyFunction.DeathSequence;
        state.FunctionTimer = NativeCountdown.Start(PowampDeathDelayFrames);
        AlignPowampBalloonY(body, balloon);
    }

    private void RunPowampDeathSequence(
        RoomEnemySlot body,
        PowampEnemyState state,
        RoomEnemySlot balloon)
    {
        if (!TickPowampTimer(state))
        {
            AlignPowampBalloonY(body, balloon);
            return;
        }

        // `$A8:C5A6-$C5B5`: the balloon is deleted, the spikes fire, and the body goes
        // through EnemyDeath with whatever the spike loop left in A as its animation.
        balloon.Health = 0;
        balloon.Properties = balloon.Properties.With(EnemyProperties.Deleted);
        ushort accumulator = SpawnPowampSpikeBurst(body);
        _deathsAndDrops.StartGenericEnemyDeath(body, accumulator);
    }

    /// <summary>Ports the body-only private tail of <c>EnemyTouch_Powamp</c>.</summary>
    private void ResolvePowampTouch(RoomEnemySlot body, SamusState samus)
    {
        ushort originalPalette = body.PaletteIndex;
        _collision.ResolveNormalEnemyTouch(body, samus);
        if (body.Health != 0)
            return;

        RoomEnemySlot balloon = RequirePowampBalloon(body);
        balloon.Properties = balloon.Properties.With(EnemyProperties.Deleted);
        body.PaletteIndex = originalPalette;
        SpawnPowampSpikeBurst(body);
        body.PaletteIndex = EnemyPaletteBits.Palette5;
    }

    /// <summary>Copies common shot/freeze presentation from the body to its balloon.</summary>
    private void ResolvePowampShotAfterCommon(RoomEnemySlot body)
    {
        RoomEnemySlot balloon = RequirePowampBalloon(body);
        if ((body.AiHandlerBits & 4) != 0)
        {
            balloon.FrozenTimer = body.FrozenTimer;
            balloon.AiHandlerBits = unchecked((ushort)(balloon.AiHandlerBits | 4));
        }
        if ((body.AiHandlerBits & 2) != 0)
        {
            balloon.FlashTimer = body.FlashTimer;
            balloon.AiHandlerBits = unchecked((ushort)(balloon.AiHandlerBits | 2));
        }
        if (body.Health == 0)
        {
            RequirePowampState(body).Function = PowampEnemyFunction.FatalDamage;
            // init1 becomes the native one-word guard that rejects further touch/shot AI.
            body.Parameter2 = 1;
        }
    }

    /// <summary>Ports the paired state propagation after <c>PowerBombReaction_Powamp</c>.</summary>
    private void ResolvePowampPowerBombAfterCommon(RoomEnemySlot body)
    {
        RoomEnemySlot balloon = RequirePowampBalloon(body);
        if (body.Health == 0)
        {
            balloon.Properties = balloon.Properties.With(EnemyProperties.Deleted);
            return;
        }

        balloon.ShakeTimer = body.ShakeTimer;
        balloon.InvincibilityTimer = body.InvincibilityTimer;
        balloon.FlashTimer = body.FlashTimer;
        balloon.FrozenTimer = body.FrozenTimer;
        balloon.AiHandlerBits = body.AiHandlerBits;
    }

    private bool AdvancePowampWiggle(
        RoomEnemySlot body,
        PowampEnemyState state,
        RoomEnemySlot balloon,
        bool stopWhenCentered)
    {
        state.WiggleTimer = state.WiggleTimer.Decrement().Remaining;
        if (!state.WiggleTimer.IsZero && !EnemyMath.IsNegative16(state.WiggleTimer.RawValue))
            return false;

        state.WiggleTimer = NativeCountdown.Start(PowampWiggleFramesPerOffset);
        if (state.WiggleIndex >= PowampMotionDefinitions.WigglePhaseCount)
        {
            throw new InvalidDataException(
                $"Powamp wiggle index {state.WiggleIndex} exceeds the 12-word ROM table.");
        }

        ushort x = unchecked((ushort)(
            RequirePowampState(balloon).BalloonSpawn.X + PowampMotionDefinitions.WiggleOffset(state.WiggleIndex)));
        balloon.Position = balloon.Position.WithWholeX(x);
        body.Position = body.Position.WithWholeX(x);
        if (stopWhenCentered && IsPowampCentered(state.WiggleIndex))
            return true;

        state.WiggleIndex = unchecked((ushort)(state.WiggleIndex + 1));
        if (state.WiggleIndex >= PowampMotionDefinitions.WigglePhaseCount)
            state.WiggleIndex = 0;
        return false;
    }

    internal static void AlignPowampBalloonY(RoomEnemySlot body, RoomEnemySlot balloon)
    {
        ushort cursor = balloon.CurrentInstruction;
        bool sinking;
        ushort basePointer;
        if (cursor < PowampInstructionProgramDefinitions.BalloonStartSinking)
        {
            sinking = false;
            basePointer = PowampInstructionProgramDefinitions.BalloonInflate0;
        }
        else
        {
            sinking = true;
            basePointer = PowampInstructionProgramDefinitions.BalloonStartSinking;
        }

        // The 65C816 calculation produces a byte offset (0,2,4), not a logical element
        // index. Underflow and any value >=6 deliberately fall back to the first entry.
        ushort byteOffset = unchecked((ushort)(cursor - 4 - basePointer));
        byteOffset >>= 1;
        int index = byteOffset < 6 ? byteOffset / 2 : 0;
        balloon.Position = balloon.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + PowampMotionDefinitions.BalloonOffset(index, sinking))));
    }

    private static void StartPowampDeflating(PowampEnemyState state, RoomEnemySlot balloon)
    {
        state.Function = PowampEnemyFunction.Deflating;
        state.FunctionTimer = NativeCountdown.Start(PowampTransitionFrames);
        SetPowampInstruction(
            balloon,
            PowampInstructionProgramDefinitions.BalloonStartSinking);
    }

    private static bool TickPowampTimer(PowampEnemyState state)
    {
        state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
        return state.FunctionTimer.IsZero || EnemyMath.IsNegative16(state.FunctionTimer.RawValue);
    }

    private static int ComposePowampVerticalDisplacement(PowampEnemyState state) =>
        unchecked((SnesSixteenSixteen.FromWhole((short)state.YVelocity)) | state.YSubvelocity);

    private static bool IsPowampCentered(ushort wiggleIndex) => wiggleIndex is 0 or 6;

    private static void SetPowampInstruction(RoomEnemySlot slot, ushort instructionList)
    {
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    private static RoomLevelData RequirePowampLevel(RoomLevelData? level) =>
        level ?? throw new InvalidOperationException("Powamp movement requires room level data.");

    private RoomEnemySlot RequirePowampBalloon(RoomEnemySlot body)
    {
        if (body.SlotIndex == 0)
            throw new InvalidDataException("A Powamp body has no preceding balloon slot.");
        RoomEnemySlot balloon = _slotTable._slots[body.SlotIndex - 1];
        if (balloon.EnemyDefinitionPointer != EnemyDefinitionId.Powamp || balloon.Parameter1 == 0)
        {
            throw new InvalidDataException(
                $"Powamp body slot {body.SlotIndex} is not preceded by its balloon half.");
        }
        return balloon;
    }

    internal PowampEnemyState RequirePowampState(RoomEnemySlot slot) =>
        _powampStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Powamp state.");

    internal void RegisterPowampBombReactions(EnemyAiTable table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Powamp &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.PowampShot)
        {
            RunsGenericDeath = false,
            AfterCommon = c => ResolvePowampShotAfterCommon(c.Enemy),
        });
    }

    internal static void RegisterPowampProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunPowampSpikePreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.PowampSpike);
    }

    internal void RegisterPowampInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Powamp, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Powamp, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializePowamp(slot);
        });
    }

    internal void RegisterPowampMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Powamp, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Powamp, (slot, frame, nmiFrameCounter8) =>
        {
            RunPowampMain(slot, RequirePowampState(slot), frame.Level);
        });
    }

    internal void RegisterPowampPowerBombReactions(EnemyAiTable table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Powamp &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.PowampPowerBomb)
        {
            AfterCommon = (enemy, _) =>
            {
                if (enemy.Parameter1 == 0)
                    ResolvePowampPowerBombAfterCommon(enemy);
            },
        });
    }

    internal void RegisterPowampShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Powamp &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.PowampShot)
        {
            // Powamp writes one to body init1 as soon as a lethal shot starts its private 32-frame death. Later
            // overlaps return before common shot AI, so they must not consume or explode the projectile while the
            // body is still visibly deflating.
            SkipsEnemy = enemy => enemy.Parameter2 != 0,
            ExemptFromGenericDeath = true,
            AfterCommon = (c, _) => ResolvePowampShotAfterCommon(c.Enemy),
        });
    }

    internal void RegisterPowampTouch(EnemyAiTable table)
    {
        // init1 is zero only for the body before fatal shot damage. Balloons are normally absent from the
        // interactive list because population property $0400 excludes them, but retain the handler's literal guard.
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Powamp &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.PowampTouch,
            context =>
            {
                if (context.Slot.Parameter2 == 0)
                    ResolvePowampTouch(context.Slot, context.Samus);
            });
    }
    /// <summary>Ports <c>FirePowampSpikesIn8Directions</c> at $A8:C223.</summary>
    /// <returns>
    /// The accumulator <c>FirePowampSpikesIn8Directions</c> leaves: the last spike's native
    /// projectile index, or the body's graphics word when the pool was full. The death
    /// sequence passes it straight to EnemyDeath as the animation.
    /// </returns>
    internal ushort SpawnPowampSpikeBurst(RoomEnemySlot body)
    {
        ushort graphics = unchecked((ushort)(body.VramTilesIndex | body.PaletteIndex));
        ushort accumulator = 0;
        // Native Y counts from seven down to zero. Allocation independently searches the
        // eighteen-slot bank-$86 pool from native index $22 downward on every iteration.
        for (int direction = 7; direction >= 0; direction--)
        {
            RoomEnemyProjectileSlot? spike = _projectiles.AllocateEnemyProjectile();
            if (spike is null)
                return graphics;

            // SpawnEnemyProjectileY_ParameterA_XGraphics copies definition $D298 before
            // initializer $D23A zeros velocities and captures the owner position. Reading
            // the record through the shared helper preserves its list, map sentinel, radii,
            // damage, and all property bits instead of maintaining a second hand copy here.
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                spike,
                RoomEnemyProjectileKind.PowampSpike,
                unchecked((ushort)(body.VramTilesIndex | body.PaletteIndex)));
            spike.Position = FixedPosition<RoomPixels>.FromWhole(body.Position.Whole);
            spike.Velocity = new(new SnesSignedEightEight(0), new SnesSignedEightEight(0));
            spike.DirectionParameter = unchecked((ushort)direction);
            accumulator = unchecked((ushort)(spike.SlotIndex * 2));
        }
        return accumulator;
    }

    /// <summary>Ports <c>PreInstruction_EnemyProjectile_PowampSpike</c> at $86:D263.</summary>
    internal static void RunPowampSpikePreInstruction(
        RoomEnemyProjectileSlot spike,
        RoomLevelData level)
    {
        int direction = spike.DirectionParameter;
        if ((uint)direction >= PowampMotionDefinitions.SpikeDirectionCount)
        {
            throw new InvalidDataException(
                $"Powamp spike direction {direction} exceeds the eight-entry ROM table.");
        }

        spike.Velocity = spike.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(
            unchecked((short)spike.Velocity.X.RawValue) + PowampMotionDefinitions.SpikeXAcceleration(direction)))));
        if (EnemyProjectileEngine.MoveProjectileAxis(spike, level, horizontal: true))
        {
            BeginPowampSpikeDeletion(spike);
            return;
        }

        spike.Velocity = spike.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(
            unchecked((short)spike.Velocity.Y.RawValue) + PowampMotionDefinitions.SpikeYAcceleration(direction)))));
        if (EnemyProjectileEngine.MoveProjectileAxis(spike, level, horizontal: false))
            BeginPowampSpikeDeletion(spike);
    }

    internal static void BeginPowampSpikeDeletion(RoomEnemyProjectileSlot spike)
    {
        // The list contains the common delete opcode as its first word. Installing timer
        // one lets the shared instruction interpreter consume it later in this same frame.
        spike.InstructionPointer = PowampSpikeInstructionProgramDefinitions.Delete;
        spike.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        Array.Clear(_powampStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}

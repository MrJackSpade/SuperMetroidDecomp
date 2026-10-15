using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Powamp.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>The Powamp species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class PowampBehavior : IEnemySpecies
{
    /// <summary>Clips body rise and sink motion against room collision.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Spawns the eight radial spikes emitted at death.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Resolves each body's preceding balloon slot.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Applies shared shot and touch reactions before paired-state propagation.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Starts ordinary death processing after the delayed spike burst.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Connects Powamp's paired body and balloon AI to terrain, projectile, and collision services.</summary>
    internal PowampBehavior(EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemySlotTable slotTable, EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops)
    {
        _terrain = terrain;
        _projectiles = projectiles;
        _slotTable = slotTable;
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
    }

    /// <summary>Ungrappled body rise measured from its balloon's spawn Y position.</summary>
    private const ushort PowampUngrappledTravelDistance = 64;
    /// <summary>Frames the underground body rests before reinflating its balloon.</summary>
    private const ushort PowampRestFrames = 60;
    /// <summary>Inflation or deflation transition duration.</summary>
    private const ushort PowampTransitionFrames = 10;
    /// <summary>Delay between fatal damage and deleting the balloon with a spike burst.</summary>
    private const ushort PowampDeathDelayFrames = 32;
    /// <summary>Frames between successive shared body and balloon horizontal wiggle offsets.</summary>
    private const ushort PowampWiggleFramesPerOffset = 5;

    /// <summary>Per-slot body or balloon state, including native paired movement words.</summary>
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
        slot.SpritemapPointer = CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap;
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
            SetPowampProgram(slot, state, PowampProgram.BodySlow);
            return;
        }

        state.BalloonSpawn = slot.Position.Whole;
        state.Function = PowampEnemyFunction.BalloonNoOp;
        SetPowampProgram(slot, state, PowampProgram.BalloonDeflated);
        state.BalloonGrappleTravelDistance = slot.Parameter2;
    }

    /// <summary>Ports <c>MainAI_Powamp</c> and every body function it dispatches.</summary>
    internal void RunPowampMain(RoomEnemySlot slot, PowampEnemyState state, RoomLevelData? level)
    {
        if (state.IsBalloon)
            return;

        PowampBalloon balloon = RequirePowampBalloon(slot);
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

    /// <summary>Counts down the deflated rest and starts balloon inflation.</summary>
    private static void RunPowampDeflatedResting(
        RoomEnemySlot body,
        PowampEnemyState state,
        PowampBalloon balloon)
    {
        if (TickPowampTimer(state))
        {
            SetPowampProgram(balloon.Slot, balloon.State, PowampProgram.BalloonInflate0);
            state.Function = PowampEnemyFunction.Inflating;
            state.FunctionTimer = NativeCountdown.Start(PowampTransitionFrames);
        }
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Finishes inflation and seeds the half-pixel upward body velocity.</summary>
    private static void RunPowampInflating(
        RoomEnemySlot body,
        PowampEnemyState state,
        PowampBalloon balloon)
    {
        if (TickPowampTimer(state))
        {
            state.Function = PowampEnemyFunction.InflatedRiseToTargetHeight;
            // $FFFF:$8000 is exactly -0.5 pixels/frame on the NTSC cartridge.
            state.YVelocity = unchecked((ushort)-1);
            state.YSubvelocity = 32768;
            SetPowampProgram(body, state, PowampProgram.BodyFast);
        }
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Rises to the grappling-dependent height or terrain, preserving wiggle and grapple handoff order.</summary>
    private void RunPowampRise(
        RoomEnemySlot body,
        PowampEnemyState state,
        PowampBalloon balloon,
        RoomLevelData level,
        bool grappled)
    {
        // The ordinary rise changes dispatch and executes the grappled routine immediately
        // on the frame common grapple AI publishes bit one.
        bool grappleAiActive = (body.AiHandlerBits & EnemyAiHandlerMasks.Grappled) != 0;
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
            ? balloon.State.BalloonGrappleTravelDistance
            : PowampUngrappledTravelDistance;
        ushort targetY = unchecked((ushort)(balloon.State.BalloonSpawn.Y - travelDistance));

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

    /// <summary>Continues clipped vertical movement until horizontal wiggle reaches a centered offset.</summary>
    private void RunPowampFinishWiggle(
        RoomEnemySlot body,
        PowampEnemyState state,
        PowampBalloon balloon,
        RoomLevelData level,
        bool grappled)
    {
        bool grappleAiActive = (body.AiHandlerBits & EnemyAiHandlerMasks.Grappled) != 0;
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

    /// <summary>Holds the inflated pair while grapple AI is active, deflating on release.</summary>
    private static void RunPowampGrappledResting(
        RoomEnemySlot body,
        PowampEnemyState state,
        PowampBalloon balloon)
    {
        if ((body.AiHandlerBits & EnemyAiHandlerMasks.Grappled) == 0)
            StartPowampDeflating(state, balloon);
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Counts the balloon deflation interval before body sinking begins.</summary>
    private static void RunPowampDeflating(
        RoomEnemySlot body,
        PowampEnemyState state,
        PowampBalloon balloon)
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

    /// <summary>Sinks the body to its spawn Y and restarts the slow deflated cycle.</summary>
    private void RunPowampSinking(
        RoomEnemySlot body,
        PowampEnemyState state,
        PowampBalloon balloon,
        RoomLevelData level)
    {
        _terrain.MoveEnemyVertically(level, body, ComposePowampVerticalDisplacement(state));
        ushort spawnY = balloon.State.BalloonSpawn.Y;
        if (!EnemyMath.IsNegative16(body.Position.Y.Whole - spawnY))
        {
            body.Position = body.Position.WithWholeY(spawnY);
            state.Function = PowampEnemyFunction.DeflatedResting;
            state.FunctionTimer = NativeCountdown.Start(PowampRestFrames);
            SetPowampProgram(body, state, PowampProgram.BodySlow);
        }
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Selects a balloon inflation frame for fatal damage and primes the 32-frame death delay.</summary>
    private static void BeginPowampDeathSequence(
        RoomEnemySlot body,
        PowampEnemyState state,
        PowampBalloon balloon)
    {
        (bool sinking, int shownPoses) = PowampBalloonPhase(balloon.State.Program);
        if (sinking && shownPoses != 1)
        {
            // The native pointer table is indexed by the byte offset left in Y, twice the poses shown less one. Two
            // shown poses select inflate stage one and three restart stage zero; one pose (entry zero) is skipped.
            PowampProgram replacement = shownPoses switch
            {
                2 => PowampProgram.BalloonInflate1,
                3 => PowampProgram.BalloonInflate0,
                _ => throw new InvalidDataException(
                    $"Powamp death saw a sinking balloon with {shownPoses} poses shown."),
            };
            SetPowampProgram(balloon.Slot, balloon.State, replacement);
        }

        state.Function = PowampEnemyFunction.DeathSequence;
        state.FunctionTimer = NativeCountdown.Start(PowampDeathDelayFrames);
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Deletes the balloon after the death delay, emits spikes, then runs generic body death.</summary>
    private void RunPowampDeathSequence(
        RoomEnemySlot body,
        PowampEnemyState state,
        PowampBalloon balloon)
    {
        if (!TickPowampTimer(state))
        {
            AlignPowampBalloonY(body, balloon);
            return;
        }

        // `$A8:C5A6-$C5B5`: the balloon is deleted, the spikes fire, and the body goes
        // through EnemyDeath with whatever the spike loop left in A as its animation.
        balloon.Slot.Health = 0;
        balloon.Slot.Properties = balloon.Slot.Properties.With(EnemyProperties.Deleted);
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

        PowampBalloon balloon = RequirePowampBalloon(body);
        balloon.Slot.Properties = balloon.Slot.Properties.With(EnemyProperties.Deleted);
        body.PaletteIndex = originalPalette;
        SpawnPowampSpikeBurst(body);
        body.PaletteIndex = EnemyPaletteBits.Palette5;
    }

    /// <summary>Copies common shot/freeze presentation from the body to its balloon.</summary>
    private void ResolvePowampShotAfterCommon(RoomEnemySlot body)
    {
        PowampBalloon balloon = RequirePowampBalloon(body);
        if ((body.AiHandlerBits & EnemyAiHandlerMasks.Frozen) != 0)
        {
            balloon.Slot.FrozenTimer = body.FrozenTimer;
            balloon.Slot.AiHandlerBits = unchecked((ushort)(balloon.Slot.AiHandlerBits | EnemyAiHandlerMasks.Frozen));
        }
        if ((body.AiHandlerBits & EnemyAiHandlerMasks.Hurt) != 0)
        {
            balloon.Slot.FlashTimer = body.FlashTimer;
            balloon.Slot.AiHandlerBits = unchecked((ushort)(balloon.Slot.AiHandlerBits | EnemyAiHandlerMasks.Hurt));
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
        PowampBalloon balloon = RequirePowampBalloon(body);
        if (body.Health == 0)
        {
            balloon.Slot.Properties = balloon.Slot.Properties.With(EnemyProperties.Deleted);
            return;
        }

        balloon.Slot.ShakeTimer = body.ShakeTimer;
        balloon.Slot.InvincibilityTimer = body.InvincibilityTimer;
        balloon.Slot.FlashTimer = body.FlashTimer;
        balloon.Slot.FrozenTimer = body.FrozenTimer;
        balloon.Slot.AiHandlerBits = body.AiHandlerBits;
    }

    /// <summary>Advances both halves to the same authored X offset every five frames.</summary>
    private static bool AdvancePowampWiggle(
        RoomEnemySlot body,
        PowampEnemyState state,
        PowampBalloon balloon,
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
            balloon.State.BalloonSpawn.X + PowampMotionDefinitions.WiggleOffset(state.WiggleIndex)));
        balloon.Slot.Position = balloon.Slot.Position.WithWholeX(x);
        body.Position = body.Position.WithWholeX(x);
        if (stopWhenCentered && IsPowampCentered(state.WiggleIndex))
            return true;

        state.WiggleIndex = unchecked((ushort)(state.WiggleIndex + 1));
        if (state.WiggleIndex >= PowampMotionDefinitions.WigglePhaseCount)
            state.WiggleIndex = 0;
        return false;
    }

    /// <summary>Positions the balloon above the body using its current inflate or sink animation phase.</summary>
    internal static void AlignPowampBalloonY(RoomEnemySlot body, PowampBalloon balloon)
    {
        (bool sinking, int shownPoses) = PowampBalloonPhase(balloon.State.Program);
        // The 65C816 calculation halves the pointer's byte offset past the first pose, (2 * shown - 2), and indexes
        // words with it. Before any pose has shown the offset underflows past five and falls back to the first entry.
        int index = shownPoses == 0 ? 0 : shownPoses - 1;
        balloon.Slot.Position = balloon.Slot.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + PowampMotionDefinitions.BalloonOffset(index, sinking))));
    }

    /// <summary>
    /// Whether the balloon is in its sink sequence and how many of that sequence's poses it has shown. The native
    /// instruction pointer the AI reads is the sequence's first list plus four bytes per shown pose; the deflated and
    /// second inflate lists continue those sequences.
    /// </summary>
    internal static (bool Sinking, int ShownPoses) PowampBalloonPhase(AnimationProgramCursor<PowampProgram> cursor) => cursor.Program switch
    {
        PowampProgram.BalloonInflate0 => (false, cursor.Position),
        PowampProgram.BalloonInflate1 => (false, 1 + cursor.Position),
        PowampProgram.BalloonStartSinking => (true, cursor.Position),
        PowampProgram.BalloonDeflated => (true, 2 + cursor.Position),
        _ => throw new InvalidOperationException($"Powamp balloon runs body animation {cursor.Program}."),
    };

    /// <summary>Switches the balloon to its sinking art and begins the deflation timer.</summary>
    private static void StartPowampDeflating(PowampEnemyState state, PowampBalloon balloon)
    {
        state.Function = PowampEnemyFunction.Deflating;
        state.FunctionTimer = NativeCountdown.Start(PowampTransitionFrames);
        SetPowampProgram(balloon.Slot, balloon.State, PowampProgram.BalloonStartSinking);
    }

    /// <summary>Decrements the native timer, treating zero and signed underflow as elapsed.</summary>
    private static bool TickPowampTimer(PowampEnemyState state)
    {
        state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
        return state.FunctionTimer.IsZero || EnemyMath.IsNegative16(state.FunctionTimer.RawValue);
    }

    /// <summary>Combines native whole and fractional Y-velocity words for terrain movement.</summary>
    private static int ComposePowampVerticalDisplacement(PowampEnemyState state) =>
        unchecked((SnesSixteenSixteen.FromWhole((short)state.YVelocity)) | state.YSubvelocity);

    /// <summary>Recognizes the two zero-offset points in the twelve-phase wiggle table.</summary>
    private static bool IsPowampCentered(ushort wiggleIndex) => wiggleIndex is 0 or 6;

    /// <summary>Restarts a body or balloon animation on the next interpreter pass.</summary>
    private static void SetPowampProgram(RoomEnemySlot slot, PowampEnemyState state, PowampProgram program)
    {
        state.Program = AnimationProgramCursor<PowampProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Rejects a rise or sink dispatch without room collision data.</summary>
    private static RoomLevelData RequirePowampLevel(RoomLevelData? level) =>
        level ?? throw new InvalidOperationException("Powamp movement requires room level data.");

    /// <summary>Requires the literal preceding enemy slot to be this body's initialized balloon half.</summary>
    private PowampBalloon RequirePowampBalloon(RoomEnemySlot body)
    {
        if (body.SlotIndex == 0)
            throw new InvalidDataException("A Powamp body has no preceding balloon slot.");
        RoomEnemySlot balloon = _slotTable._slots[body.SlotIndex - 1];
        if (balloon.EnemyDefinitionPointer != EnemyDefinitionId.Powamp || balloon.Parameter1 == 0)
        {
            throw new InvalidDataException(
                $"Powamp body slot {body.SlotIndex} is not preceded by its balloon half.");
        }
        return new(balloon, RequirePowampState(balloon));
    }

    /// <summary>Requires the per-slot state installed by Powamp initialization.</summary>
    internal PowampEnemyState RequirePowampState(RoomEnemySlot slot) =>
        _powampStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Powamp state.");

    /// <summary>Routes bomb hits through common shot logic and mirrors damage state to the balloon.</summary>
    internal void RegisterPowampBombReactions(EnemyAiTable table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Powamp &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.PowampShot)
        {
            RunsGenericDeath = false,
            AfterCommon = c => ResolvePowampShotAfterCommon(c.Enemy),
        });
    }

    /// <summary>Connects radial spikes to their bank-$86 movement pre-instruction.</summary>
    internal static void RegisterPowampProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunPowampSpikePreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.PowampSpike);
    }

    /// <summary>Runs Powamp's body and balloon animations as typed programs (#641).</summary>
    internal void RegisterPowampTypedAnimations(EnemyAiTable table) =>
        table.AddTypedAnimation(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Powamp, (slot, _) =>
        {
            if (!EnemyProgramAnimation.Expire(slot))
                return;
            var state = RequirePowampState(slot);
            AnimationProgramCursor<PowampProgram> cursor = state.Program;
            EnemyProgramAnimation.Run(slot, PowampAnimations.Programs, ref cursor, PowampFrameDefinitions.Spritemap);
            state.Program = cursor;
        });


    /// <summary>Registers initialization for both halves of a Powamp pair.</summary>
    internal void RegisterPowampInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Powamp, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Powamp, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializePowamp(slot);
        });
    }

    /// <summary>Routes Powamp main AI through its paired-body phase dispatcher.</summary>
    internal void RegisterPowampMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Powamp, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Powamp, (slot, frame, nmiFrameCounter8) =>
        {
            RunPowampMain(slot, RequirePowampState(slot), frame.Level);
        });
    }

    /// <summary>Propagates power-bomb reaction state from the body to its balloon.</summary>
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

    /// <summary>Skips later shots once fatal damage begins and mirrors surviving body presentation to the balloon.</summary>
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

    /// <summary>Applies touch behavior only to the body before its fatal-shot state begins.</summary>
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

    /// <summary>Marks a spike for its deletion list after its movement phase ends.</summary>
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

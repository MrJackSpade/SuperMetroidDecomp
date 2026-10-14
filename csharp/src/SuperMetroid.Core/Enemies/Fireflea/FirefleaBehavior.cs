using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Fireflea.Artwork;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>The Fireflea species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class FirefleaBehavior : IEnemySpecies
{
    /// <summary>Runs common touch handling before Fireflea's unconditional native death call.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Starts explosion and drop processing, including the retail second-death path.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Holds the shared darkness level that advances when Firefleas die.</summary>
    private readonly EnemyRoomStatus _roomStatus;

    /// <summary>Connects Fireflea motion and collision callbacks to room death and darkness state.</summary>
    internal FirefleaBehavior(EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops, EnemyRoomStatus roomStatus)
    {
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
        _roomStatus = roomStatus;
    }

    /// <summary>Lower vertical travel bounds indexed by native enemy slot.</summary>
    private readonly ushort[] _firefleaMinimumYPositions =
        new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Upper vertical travel bounds indexed by native enemy slot.</summary>
    private readonly ushort[] _firefleaMaximumYPositions =
        new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Current signed speed-table entries, toggled when a vertical bound is crossed.</summary>
    private readonly ushort[] _firefleaSpeedTableIndexes =
        new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Per-slot circle or vertical-motion state reset on room load.</summary>
    internal readonly FirefleaEnemyState?[] _firefleaStates =
        new FirefleaEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Fireflea</c> at <c>$A3:8D2D</c>, decoding motion parameters and the retail initial-circle angle bug.</summary>
    internal void InitializeFireflea(RoomEnemySlot slot)
    {
        var state = new FirefleaEnemyState(
            slot,
            _firefleaMinimumYPositions,
            _firefleaMaximumYPositions,
            _firefleaSpeedTableIndexes);
        _firefleaStates[slot.SlotIndex] = state;
        state.Program = AnimationProgramCursor<FirefleaProgram>.Start(FirefleaProgram.Loop);

        // Parameter one is native `init0`: bit one selects circle versus vertical motion,
        // bit zero selects the sign half of the speed table, and its high byte is the initial
        // circle angle. Parameter two (`init1`) packs speed index low and radius index high.
        ushort speedTableIndex = unchecked((ushort)((SnesLittleEndian.LowByte(slot.Parameter2)) * 8));
        if ((slot.Parameter1 & 1) == 0)
            speedTableIndex = unchecked((ushort)(speedTableIndex + 4));
        state.SpeedTableIndex = speedTableIndex;
        (state.AngleDelta, state.SubAngleDelta) = EnemyMath.ReadLinearEnemySpeed(speedTableIndex);

        byte radiusIndex = SnesLittleEndian.HighByte(slot.Parameter2);
        state.Radius = FirefleaMovementDefinitions.Radius(radiusIndex);

        if (state.UsesCircularMovement)
        {
            state.Center = slot.Position.Whole;
            state.Angle = unchecked((ushort)(SnesLittleEndian.HighByteInPlace(slot.Parameter1)));

            // The initializer passes the complete 8.8 angle word to an eight-bit routine,
            // which reads its low byte. That is the retail missing-division bug; main AI
            // begins using the high byte on frame one and corrects the position immediately.
            slot.Position = slot.Position.WithWhole(state.Center + EnemyMath.CircleOffset(state.Angle, state.Radius));
            return;
        }

        state.MinimumYPosition = unchecked((ushort)(slot.Position.Y.Whole - state.Radius));
        state.MaximumYPosition = unchecked((ushort)(slot.Position.Y.Whole + state.Radius));
    }

    /// <summary>Ports <c>MainAI_Fireflea</c> at <c>$A3:8DEE</c>, advancing circular angle or wrapped vertical travel.</summary>
    private static void RunFirefleaMain(RoomEnemySlot slot, FirefleaEnemyState state)
    {
        if (state.UsesCircularMovement)
        {
            ushort angle = SnesAngle.FromRaw(state.Angle).TableIndex;
            slot.Position = slot.Position.WithWhole(state.Center + EnemyMath.CircleOffset(angle, state.Radius));

            // `$A3:8E2D` performs an unaligned word read at var0+1. Combining the high byte
            // of sub-angle with the low byte of whole-angle reproduces that exact signed 8.8
            // delta, including negative half-speeds such as `$FF80`.
            ushort packedDelta = unchecked((ushort)(
                SnesEightEight.FromWhole(state.AngleDelta) | SnesLittleEndian.HighByte(state.SubAngleDelta)));
            state.Angle = unchecked((ushort)(state.Angle + packedDelta));
            return;
        }

        (short wholeSpeed, ushort fractionalSpeed) =
            EnemyMath.ReadLinearEnemySpeed(state.SpeedTableIndex);
        uint fixedY = slot.Position.Y.RawValue;
        int displacement = (SnesSixteenSixteen.FromWhole(wholeSpeed)) | fractionalSpeed;
        fixedY = unchecked(fixedY + (uint)displacement);
        slot.Position = slot.Position.WithY(SnesFixedPosition.FromRaw(unchecked((uint)fixedY)));

        // Native CMP/BMI and CMP/BPL consume wrapped signed subtraction, not ordinary host
        // bounds comparisons. The actor is intentionally allowed to overshoot before XOR
        // bit two switches between the adjacent positive/negative table entries.
        bool crossedMinimum = unchecked((short)(
            slot.Position.Y.Whole - state.MinimumYPosition)) < 0;
        bool reachedMaximum = unchecked((short)(
            slot.Position.Y.Whole - state.MaximumYPosition)) >= 0;
        if (crossedMinimum || reachedMaximum)
            state.SpeedTableIndex ^= 4;
    }

    /// <summary>
    /// Ports Fireflea touch AI at $A3:8E6B, including its retail double-death bug. The
    /// private handler calls common touch AI and then calls <c>EnemyDeath</c> unconditionally:
    /// normal contact therefore kills an otherwise healthy Fireflea, while Screw Attack and
    /// similar contact attacks can increment the room kill count twice.
    /// </summary>
    internal void ResolveFirefleaTouch(
        RoomEnemySlot slot,
        SamusState samus)
    {
        _collision.ResolveNormalEnemyTouch(slot, samus);

        // With ordinary contact, native A still contains the surviving health ($A0:A480).
        // EnemyDeath clamps values above four to the small explosion. Queue that real
        // death program before clearing the actor so its artwork and sound can run.
        if (slot.EnemyDefinitionPointer != 0)
            _deathsAndDrops.StartGenericEnemyDeath(slot, deathAnimation: slot.Health);
        else
        {
            // Common attacking contact already ran EnemyDeath ($A0:A48F), which returns A = 0
            // (or $00A3 after installing the respawn placeholder); $A3:8E6F then runs EnemyDeath
            // again on the cleared slot. Both values select the small explosion. It spawns at
            // (0,0) with the slot's current header (zero, or the placeholder), clears any
            // placeholder, and counts a second kill.
            _deathsAndDrops.StartGenericEnemyDeath(slot, deathAnimation: 0);
        }
        AdvanceFirefleaDarknessLevel();
    }

    /// <summary>
    /// Advances global <c>$177E</c> after one Fireflea death. Retail rooms contain at most
    /// five, so valid progression is 0,2,4,6,8,10; the original comparison nevertheless
    /// permits 12 and rejects 14, and that exact boundary remains visible here.
    /// </summary>
    private void AdvanceFirefleaDarknessLevel()
    {
        ushort next = unchecked((ushort)(_roomStatus.FirefleaDarknessLevel + 2));
        if (unchecked((short)(next - 0x000e)) < 0)
            _roomStatus.FirefleaDarknessLevel = next;
    }

    /// <summary>Rejects main-AI dispatch for a slot that has not run Fireflea initialization.</summary>
    private FirefleaEnemyState RequireFirefleaState(RoomEnemySlot slot) =>
        _firefleaStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Fireflea state.");

    /// <summary>Advances darkness after common bomb damage kills a Fireflea.</summary>
    internal void RegisterFirefleaBombReactions(EnemyAiTable table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Fireflea &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.FirefleaShot)
        {
            AfterCommon = c =>
            {
                if (c.Enemy.Health == 0)
                    AdvanceFirefleaDarknessLevel();
            },
        });
    }

    /// <summary>Runs Fireflea's glow loop as a typed program (#641).</summary>
    internal void RegisterFirefleaTypedAnimations(EnemyAiTable table) =>
        table.AddTypedAnimation(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Fireflea, (slot, _) =>
        {
            if (!EnemyProgramAnimation.Expire(slot))
                return;
            var state = RequireFirefleaState(slot);
            AnimationProgramCursor<FirefleaProgram> cursor = state.Program;
            EnemyProgramAnimation.Run(slot, FirefleaAnimations.Programs, ref cursor, FirefleaFrameDefinitions.Spritemap);
            state.Program = cursor;
        });

    /// <summary>Routes Fireflea definition slots to their parameter-decoding initializer.</summary>
    internal void RegisterFirefleaInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Fireflea, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Fireflea, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeFireflea(slot);
        });
    }

    /// <summary>Routes Fireflea definition slots to the circular or vertical motion update.</summary>
    internal void RegisterFirefleaMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Fireflea, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Fireflea, (slot, frame, nmiFrameCounter8) =>
        {
            RunFirefleaMain(slot, RequireFirefleaState(slot));
        });
    }

    /// <summary>Advances darkness before a lethal common power-bomb death is finalized.</summary>
    internal void RegisterFirefleaPowerBombReactions(EnemyAiTable table)
    {
        // Reactions around common power-bomb damage.
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Fireflea &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.FirefleaPowerBomb)
        {
            BeforeGenericDeath = (_, _) => AdvanceFirefleaDarknessLevel(),
        });
    }

    /// <summary>Advances darkness before a lethal common projectile death is finalized.</summary>
    internal void RegisterFirefleaShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Fireflea &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.FirefleaShot)
        {
            BeforeGenericDeath = _ => AdvanceFirefleaDarknessLevel(),
        });
    }

    /// <summary>Registers Fireflea's touch callback, which runs the retail unconditional-death sequence.</summary>
    internal void RegisterFirefleaTouch(EnemyAiTable table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Fireflea &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.FirefleaTouch,
            context => ResolveFirefleaTouch(context.Slot, context.Samus));
    }

    /// <summary>Discards slot-indexed motion state before a new room population initializes.</summary>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        Array.Clear(_firefleaStates);
    }

    /// <summary>Fireflea has no species-wide frame action beyond each slot's registered main AI.</summary>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}

using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Alcoon species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class AlcoonBehavior : IEnemySpecies
{
    /// <summary>Stores room.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Stores terrain.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Stores projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Initializes a new alcoon behavior instance.</summary>
    internal AlcoonBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyProjectileEngine projectiles)
    {
        _room = room;
        _terrain = terrain;
        _projectiles = projectiles;
    }

    /// <summary>Executes Alcoon's private animation instructions.</summary>
    private bool TryProcessAlcoonInstruction(RoomEnemySlot slot, RoomLevelData? level, ushort word, ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Alcoon ||
            !Enum.IsDefined((AlcoonInstruction)word))
            return false;

        switch ((AlcoonInstruction)word)
        {
            case AlcoonInstruction.SpawnFireballUpward:
                SpawnAlcoonFireball(slot, yVelocityTableByteOffset: 0);
                LastAlcoonSoundEffect = AlcoonFireSound;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case AlcoonInstruction.SpawnFireballHorizontally:
                SpawnAlcoonFireball(slot, yVelocityTableByteOffset: 2);
                LastAlcoonSoundEffect = AlcoonFireSound;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case AlcoonInstruction.SpawnFireballDownward:
                SpawnAlcoonFireball(slot, yVelocityTableByteOffset: 4);
                LastAlcoonSoundEffect = AlcoonFireSound;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case AlcoonInstruction.StartWalking:
                cursor = StartAlcoonWalking(RequireAlcoonState(slot));
                return true;
            case AlcoonInstruction.DecrementStepCounterMoveHorizontally:
                cursor = MoveAlcoonHorizontally(
                    slot,
                    RequireAlcoonState(slot),
                    level,
                    unchecked((ushort)(cursor + 2)),
                    decrementStepCounter: true);
                return true;
            case AlcoonInstruction.MoveHorizontallyTurnIfWallCollision:
                cursor = MoveAlcoonHorizontally(
                    slot,
                    RequireAlcoonState(slot),
                    level,
                    unchecked((ushort)(cursor + 2)),
                    decrementStepCounter: false);
                return true;
            default:
                throw new InvalidOperationException(
                    $"Alcoon does not own instruction ${word:X4}.");
        }
    }


    /// <summary>Stores alcoon emerge xdistance.</summary>
    private const ushort AlcoonEmergeXDistance = 0x0050;
    /// <summary>Stores alcoon hide xdistance.</summary>
    private const ushort AlcoonHideXDistance = 0x0070;
    /// <summary>Stores alcoon emerge sound.</summary>
    private const ushort AlcoonEmergeSound = 0x005e;
    /// <summary>Stores alcoon fire sound.</summary>
    private const ushort AlcoonFireSound = 0x003f;
    /// <summary>Stores alcoon jump simulation limit.</summary>
    private const int AlcoonJumpSimulationLimit = 4096;

    /// <summary>Stores alcoon yaccelerations.</summary>
    internal readonly ushort[] _alcoonYAccelerations = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores alcoon ysubaccelerations.</summary>
    internal readonly ushort[] _alcoonYSubaccelerations = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores alcoon spawn xpositions.</summary>
    internal readonly ushort[] _alcoonSpawnXPositions = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores alcoon landing ypositions.</summary>
    internal readonly ushort[] _alcoonLandingYPositions = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores alcoon step counters.</summary>
    internal readonly ushort[] _alcoonStepCounters = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores alcoon states.</summary>
    internal readonly AlcoonEnemyState?[] _alcoonStates =
        new AlcoonEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Alcoon</c> at <c>$A8:DCCD</c>, including its floor search.</summary>
    private void InitializeAlcoon(RoomEnemySlot slot, RoomLevelData? level)
    {
        if (level is null)
        {
            throw new InvalidOperationException(
                "Alcoon initialization requires the room's collision layer to simulate its landing.");
        }

        var state = new AlcoonEnemyState(
            slot,
            _alcoonYAccelerations,
            _alcoonYSubaccelerations,
            _alcoonSpawnXPositions,
            _alcoonLandingYPositions,
            _alcoonStepCounters);
        _alcoonStates[slot.SlotIndex] = state;

        state.StepCounter = 0;
        state.SpawnPosition = slot.Position.Whole;
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.CurrentInstruction = AlcoonInstructionProgramDefinitions.WalkingLeft;
        state.Function = AlcoonEnemyFunction.WaitingForSamus;
        SetupAlcoonJumpMovement(state);

        // The original initializer really executes every half-pixel-accelerated ascent
        // step, then every collision-aware descent step. It ultimately restores only the
        // whole Y coordinate; the collision-produced subposition deliberately survives.
        int iterations = 0;
        do
        {
            AddAlcoonYVelocity(slot, state);
            AccelerateAlcoonY(state);
            GuardAlcoonJumpSimulation(++iterations, slot);
        }
        while (state.Velocity.Y.WholePixels < 0);

        while (true)
        {
            int displacement = state.Velocity.Y.RawValue;
            if (_terrain.MoveEnemyVertically(level, slot, displacement))
                break;
            AccelerateAlcoonY(state);
            GuardAlcoonJumpSimulation(++iterations, slot);
        }

        state.LandingYPosition = slot.Position.Y.Whole;
        slot.Position = slot.Position.WithWholeY(state.SpawnPosition.Y);
    }

    /// <summary>Ports <c>MainAI_Alcoon</c> and all seven indirect targets.</summary>
    private void RunAlcoonMain(
        RoomEnemySlot slot,
        AlcoonEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        switch (state.Function)
        {
            case AlcoonEnemyFunction.WaitingForSamus:
                WaitForAlcoonActivation(slot, state, samus);
                return;
            case AlcoonEnemyFunction.EmergingRising:
                RunAlcoonEmergingRise(slot, state);
                return;
            case AlcoonEnemyFunction.EmergingFalling:
                RunAlcoonEmergingFall(slot, state, samus, level);
                return;
            case AlcoonEnemyFunction.WalkingAndFiring:
                RunEmergedAlcoon(slot, state, samus, level);
                return;
            case AlcoonEnemyFunction.WaitingForFireAnimation:
                return;
            case AlcoonEnemyFunction.HidingRising:
                RunAlcoonHidingRise(slot, state);
                return;
            case AlcoonEnemyFunction.HidingFalling:
                RunAlcoonHidingFall(slot, state);
                return;
            default:
                throw new InvalidDataException(
                    $"Alcoon function $A8:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Wait for alcoon activation.</summary>
    private void WaitForAlcoonActivation(
        RoomEnemySlot slot,
        AlcoonEnemyState state,
        SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Alcoon proximity AI requires the active Samus actor.");

        if (EnemyMath.WrappedMagnitude(unchecked((ushort)(state.LandingYPosition - samus.Position.Y.Whole))) >= 0x20)
            return;

        ushort signedXDistance = unchecked((ushort)(samus.Position.X.Whole - slot.Position.X.Whole));
        if (EnemyMath.WrappedMagnitude(signedXDistance) >= AlcoonEmergeXDistance)
            return;

        SetupAlcoonJumpMovement(state);
        if (unchecked((short)signedXDistance) < 0)
        {
            state.Velocity = state.Velocity.WithWholeX(-2);
            InstallAlcoonInstruction(slot, AlcoonInstructionProgramDefinitions.AirborneLeftLookingUp);
        }
        else
        {
            state.Velocity = state.Velocity.WithWholeX(2);
            InstallAlcoonInstruction(slot, AlcoonInstructionProgramDefinitions.AirborneRightLookingUp);
        }
        state.Function = AlcoonEnemyFunction.EmergingRising;
        LastAlcoonSoundEffect = AlcoonEmergeSound;
    }

    /// <summary>Runs alcoon emerging rise.</summary>
    private static void RunAlcoonEmergingRise(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        AddAlcoonYVelocity(slot, state);
        AccelerateAlcoonY(state);
        if (state.Velocity.Y.WholePixels < 0)
            return;

        state.Function = AlcoonEnemyFunction.EmergingFalling;
        InstallAlcoonInstruction(
            slot,
            state.Velocity.X.WholePixels < 0
                ? AlcoonInstructionProgramDefinitions.AirborneLeftLookingForward
                : AlcoonInstructionProgramDefinitions.AirborneRightLookingForward);
    }

    /// <summary>Runs alcoon emerging fall.</summary>
    private void RunAlcoonEmergingFall(
        RoomEnemySlot slot,
        AlcoonEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Alcoon landing direction requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("Alcoon falling AI requires room collision data.");

        int displacement = state.Velocity.Y.RawValue;
        if (!_terrain.MoveEnemyVertically(level, slot, displacement))
        {
            AccelerateAlcoonY(state);
            return;
        }

        ushort signedXDistance = unchecked((ushort)(samus.Position.X.Whole - slot.Position.X.Whole));
        if (unchecked((short)signedXDistance) < 0)
        {
            state.Velocity = state.Velocity.WithWholeX(-2);
            InstallAlcoonInstruction(slot, AlcoonInstructionProgramDefinitions.WalkingLeft);
        }
        else
        {
            state.Velocity = state.Velocity.WithWholeX(2);
            InstallAlcoonInstruction(slot, AlcoonInstructionProgramDefinitions.WalkingRight);
        }
        state.Function = AlcoonEnemyFunction.WalkingAndFiring;
        state.StepCounter = 1;
    }

    /// <summary>Runs emerged alcoon.</summary>
    private void RunEmergedAlcoon(
        RoomEnemySlot slot,
        AlcoonEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Alcoon firing AI requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("Alcoon walking AI requires room collision data.");

        // This is a real two-pixel downward move, not merely a floor probe. On ordinary
        // floors it collides and keeps Alcoon aligned; if the floor vanishes it lets the
        // walking actor fall by the same amount before the remainder of main AI runs.
        _terrain.MoveEnemyVertically(level, slot, 2 << 16);

        ushort spawnDistance = unchecked((ushort)(state.SpawnPosition.X - slot.Position.X.Whole));
        if (EnemyMath.WrappedMagnitude(spawnDistance) >= AlcoonHideXDistance)
        {
            state.Function = AlcoonEnemyFunction.HidingRising;
            state.Velocity = state.Velocity.WithY(SnesSignedSixteenSixteen.FromParts(-4, 0));
            InstallAlcoonInstruction(
                slot,
                state.Velocity.X.WholePixels < 0
                    ? AlcoonInstructionProgramDefinitions.AirborneLeftLookingForward
                    : AlcoonInstructionProgramDefinitions.AirborneRightLookingForward);
            return;
        }

        if (state.StepCounter != 0)
            return;

        bool samusIsLeft = unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0;
        bool movingLeft = state.Velocity.X.WholePixels < 0;
        if (samusIsLeft != movingLeft)
            return;

        InstallAlcoonInstruction(
            slot,
            movingLeft
                ? AlcoonInstructionProgramDefinitions.FireLeft
                : AlcoonInstructionProgramDefinitions.FireRight);
        state.Function = AlcoonEnemyFunction.WaitingForFireAnimation;
    }

    /// <summary>Runs alcoon hiding rise.</summary>
    private static void RunAlcoonHidingRise(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        AddAlcoonYVelocity(slot, state);
        AccelerateAlcoonY(state);
        if (state.Velocity.Y.WholePixels >= 0)
            state.Function = AlcoonEnemyFunction.HidingFalling;
    }

    /// <summary>Runs alcoon hiding fall.</summary>
    private static void RunAlcoonHidingFall(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        AddAlcoonYVelocity(slot, state);
        if (unchecked((short)(slot.Position.Y.Whole - state.SpawnPosition.Y)) < 0)
        {
            AccelerateAlcoonY(state);
            return;
        }

        // Like the original, the reset snaps only whole coordinates. Both subpositions and
        // the last animation map remain live while the actor waits underground.
        slot.Position = slot.Position.WithWhole(state.SpawnPosition);
        state.Function = AlcoonEnemyFunction.WaitingForSamus;
    }

    /// <summary>Setup alcoon jump movement.</summary>
    private static void SetupAlcoonJumpMovement(AlcoonEnemyState state)
    {
        state.Velocity = new FixedVector<RoomPixels>(
            SnesSignedSixteenSixteen.Zero, SnesSignedSixteenSixteen.FromParts(-12, 0));
        state.ClearedVariable = 0;
        state.YAcceleration = 0;
        state.YSubacceleration = 0x8000;
    }

    /// <summary>Accelerate alcoon y.</summary>
    private static void AccelerateAlcoonY(AlcoonEnemyState state)
    {
        SnesSignedSixteenSixteen acceleration = SnesSignedSixteenSixteen.FromParts(
            unchecked((short)state.YAcceleration), state.YSubacceleration);
        state.Velocity = state.Velocity.WithY(state.Velocity.Y + acceleration);
    }

    /// <summary>Adds alcoon yvelocity.</summary>
    private static void AddAlcoonYVelocity(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        slot.Position = slot.Position.AdvanceY(state.Velocity.Y);
    }

    /// <summary>Guard alcoon jump simulation.</summary>
    private static void GuardAlcoonJumpSimulation(int iterations, RoomEnemySlot slot)
    {
        if (iterations > AlcoonJumpSimulationLimit)
        {
            throw new InvalidDataException(
                $"Alcoon slot {slot.SlotIndex} did not find a landing floor within " +
                $"{AlcoonJumpSimulationLimit} initialization steps.");
        }
    }

    /// <summary>Installs alcoon instruction.</summary>
    private static void InstallAlcoonInstruction(RoomEnemySlot slot, ushort instructionList)
    {
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = 1;
    }

    /// <summary>Implements Alcoon instruction <c>$A8:DF3F</c> and returns native Y.</summary>
    private ushort StartAlcoonWalking(AlcoonEnemyState state)
    {
        state.Function = AlcoonEnemyFunction.WalkingAndFiring;
        Func<ushort> readRandomNumber = _room.Random.Read;
        ushort count = unchecked((ushort)(readRandomNumber() & 3));
        state.StepCounter = count == 0 ? (ushort)2 : count;
        return state.Velocity.X.WholePixels < 0
            ? AlcoonInstructionProgramDefinitions.WalkingLeft
            : AlcoonInstructionProgramDefinitions.WalkingRight;
    }

    /// <summary>
    /// Implements both movement bytecodes at <c>$A8:DF63/$DF71</c>. The caller passes the
    /// interpreter cursor already advanced beyond the opcode, matching the native Y register.
    /// </summary>
    private ushort MoveAlcoonHorizontally(
        RoomEnemySlot slot,
        AlcoonEnemyState state,
        RoomLevelData? level,
        ushort nextCursor,
        bool decrementStepCounter)
    {
        if (level is null)
            throw new InvalidOperationException("Alcoon movement instruction requires room collision data.");

        if (decrementStepCounter && state.StepCounter != 0)
            state.StepCounter = unchecked((ushort)(state.StepCounter - 1));

        int displacement = state.Velocity.X.RawValue;
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
        {
            EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, slot);
            return nextCursor;
        }

        short oldVelocity = state.Velocity.X.WholePixels;
        state.Velocity = state.Velocity.WithWholeX(unchecked((short)-oldVelocity));
        return oldVelocity >= 0
            ? AlcoonInstructionProgramDefinitions.WalkingLeftFirstFrame
            : AlcoonInstructionProgramDefinitions.WalkingRightFirstFrame;
    }

    /// <summary>Requires alcoon state.</summary>
    private AlcoonEnemyState RequireAlcoonState(RoomEnemySlot slot) =>
        _alcoonStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Alcoon state.");

    /// <summary>Registers alcoon projectiles.</summary>
    internal static void RegisterAlcoonProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunAlcoonFireballPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.AlcoonFireball);
    }

    /// <summary>Registers alcoon initialization ai.</summary>
    internal void RegisterAlcoonInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Alcoon, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Alcoon, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeAlcoon(slot, level);
        });
    }

    /// <summary>Registers alcoon main ai.</summary>
    internal void RegisterAlcoonMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Alcoon, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Alcoon, (slot, frame, nmiFrameCounter8) =>
        {
            RunAlcoonMain(slot, RequireAlcoonState(slot), frame.Samus, frame.Level);
        });
    }

    /// <summary>Registers alcoon owned instructions.</summary>
    internal void RegisterAlcoonOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessAlcoonInstruction(slot, context.Level, word, ref cursor);
        });
    }
    /// <summary>Stores alcoon fireball graphics horizontal speed.</summary>
    private const ushort AlcoonFireballGraphicsHorizontalSpeed = 0x0400;
    /// <summary>Stores alcoon fireball terminal horizontal speed.</summary>
    private const ushort AlcoonFireballTerminalHorizontalSpeed = 0x0200;
    /// <summary>Stores alcoon fireball horizontal deceleration.</summary>
    private const ushort AlcoonFireballHorizontalDeceleration = 0x0040;

    /// <summary>
    /// Ports <c>InitAI_EnemyProjectile_AlcoonFireball</c> at <c>$86:9EB2</c>. The parameter
    /// is a byte offset (0, 2, or 4), exactly as passed in A by Alcoon's three instructions.
    /// </summary>
    internal void SpawnAlcoonFireball(RoomEnemySlot alcoon, ushort yVelocityTableByteOffset)
    {
        if (yVelocityTableByteOffset is not (0 or 2 or 4))
        {
            throw new InvalidDataException(
                $"Alcoon fireball Y-velocity byte offset {yVelocityTableByteOffset} is invalid.");
        }

        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        AlcoonEnemyState state = RequireAlcoonState(alcoon);
        bool movingLeft = state.Velocity.X.WholePixels < 0;

        // SpawnEnemyProjectileY_ParameterA_XGraphics copies the complete seven-word $9E90
        // definition before initializer $9EB2 touches position, subposition, and velocity.
        // Keeping that division exact makes radii, damage, persistence, shot blocking,
        // animation, and pre-instruction cartridge-owned instead of duplicating them here.
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.AlcoonFireball,
            unchecked((ushort)(alcoon.VramTilesIndex | alcoon.PaletteIndex)));
        projectile.Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(alcoon.Position.X.Whole + (movingLeft ? -16 : 16))), unchecked((ushort)(alcoon.Position.Y.Whole - 12))));
        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(movingLeft
            ? unchecked((ushort)-AlcoonFireballGraphicsHorizontalSpeed)
            : AlcoonFireballGraphicsHorizontalSpeed));
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(EnemyFireballLaunchDefinitions.AlcoonYVelocity(yVelocityTableByteOffset)));
    }

    /// <summary>Ports Alcoon fireball pre-instruction <c>$86:9EFF-$9F40</c>.</summary>
    internal static void RunAlcoonFireballPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        // Unlike Ridley's fireball, this definition checks vertical collision first. If
        // either axis collides, the native routine clears the projectile ID immediately and
        // never applies horizontal deceleration on that frame.
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: false) ||
            EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.Clear();
            return;
        }

        short velocity = unchecked((short)projectile.Velocity.X.RawValue);
        if (velocity < 0)
        {
            velocity = unchecked((short)(velocity + AlcoonFireballHorizontalDeceleration));
            if (velocity >= unchecked((short)-AlcoonFireballTerminalHorizontalSpeed))
                velocity = unchecked((short)-AlcoonFireballTerminalHorizontalSpeed);
        }
        else
        {
            velocity = unchecked((short)(velocity - AlcoonFireballHorizontalDeceleration));
            if (velocity < AlcoonFireballTerminalHorizontalSpeed)
                velocity = unchecked((short)AlcoonFireballTerminalHorizontalSpeed);
        }
        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)velocity)));
    }

    /// <summary>Last library-two Alcoon emerge or volley sound this enemy pass, using the Max6 queue entry; null means no current-pass request.</summary>
    public ushort? LastAlcoonSoundEffect { get; private set; }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        LastAlcoonSoundEffect = null;
        Array.Clear(_alcoonStates);
        Array.Clear(_alcoonYAccelerations);
        Array.Clear(_alcoonYSubaccelerations);
        Array.Clear(_alcoonSpawnXPositions);
        Array.Clear(_alcoonLandingYPositions);
        Array.Clear(_alcoonStepCounters);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastAlcoonSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastAlcoonSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }
}

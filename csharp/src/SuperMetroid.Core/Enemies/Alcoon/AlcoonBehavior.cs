using SuperMetroid.Core.Enemies.Alcoon.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>The Alcoon species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class AlcoonBehavior : IEnemySpecies
{
    /// <summary>Provides RNG for the randomized post-volley walk count.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves Alcoon's jump landing and its walking collisions.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Spawns the three directed fireball variants.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Connects Alcoon's per-room AI to collision, RNG, and projectile services.</summary>
    internal AlcoonBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyProjectileEngine projectiles)
    {
        _room = room;
        _terrain = terrain;
        _projectiles = projectiles;
    }

    /// <summary>
    /// Carries out one cue of the Alcoon's animations: spitting a fireball, starting to walk, or stepping. Starting to walk
    /// always continues at the walk for the Alcoon's facing, and a step that meets a wall turns it round to the other walk.
    /// </summary>
    private AlcoonProgram? ApplyAlcoonCue(RoomEnemySlot slot, AlcoonEnemyState state, RoomLevelData? level, AlcoonCue cue)
    {
        switch (cue.Kind)
        {
            case AlcoonCueKind.FireUpward:
                SpawnAlcoonFireball(slot, yVelocityTableByteOffset: 0);
                LastAlcoonSoundEffect = AlcoonFireSound;
                return null;
            case AlcoonCueKind.FireHorizontally:
                SpawnAlcoonFireball(slot, yVelocityTableByteOffset: 2);
                LastAlcoonSoundEffect = AlcoonFireSound;
                return null;
            case AlcoonCueKind.FireDownward:
                SpawnAlcoonFireball(slot, yVelocityTableByteOffset: 4);
                LastAlcoonSoundEffect = AlcoonFireSound;
                return null;
            case AlcoonCueKind.StartWalking:
                return StartAlcoonWalking(state);
            case AlcoonCueKind.Step:
                return MoveAlcoonHorizontally(slot, state, level, decrementStepCounter: true);
            case AlcoonCueKind.Move:
                return MoveAlcoonHorizontally(slot, state, level, decrementStepCounter: false);
            default:
                throw new InvalidOperationException($"Undefined {nameof(AlcoonCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Maximum horizontal distance for activation, after the landing-height check.</summary>
    private const ushort AlcoonEmergeXDistance = 0x0050;
    /// <summary>Distance from spawn that causes an emerged Alcoon to jump back underground.</summary>
    private const ushort AlcoonHideXDistance = 0x0070;
    /// <summary>Library-two sound queued when Alcoon emerges near Samus.</summary>
    private const ushort AlcoonEmergeSound = 0x005e;
    /// <summary>Library-two sound queued by each fireball instruction.</summary>
    private const ushort AlcoonFireSound = 0x003f;
    /// <summary>Guards the initializer's simulated ascent and floor search against invalid room geometry.</summary>
    private const int AlcoonJumpSimulationLimit = 4096;

    /// <summary>Whole halves of per-slot vertical acceleration for native WRAM compatibility.</summary>
    internal readonly ushort[] _alcoonYAccelerations = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Fractional halves of per-slot vertical acceleration.</summary>
    internal readonly ushort[] _alcoonYSubaccelerations = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Spawn X coordinates used to decide when Alcoon hides again.</summary>
    internal readonly ushort[] _alcoonSpawnXPositions = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Predicted floor Y coordinates used for the activation-height check.</summary>
    internal readonly ushort[] _alcoonLandingYPositions = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Walking steps remaining before Alcoon may fire at Samus.</summary>
    internal readonly ushort[] _alcoonStepCounters = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Active Alcoon state indexed by enemy slot.</summary>
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

        state.StepCounter = NativeCountdown.Start(0);
        state.SpawnPosition = slot.Position.Whole;
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        state.Program = AnimationProgramCursor<AlcoonProgram>.Start(AlcoonProgram.WalkingLeft);
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

    /// <summary>Starts Alcoon's jump when Samus nears its predicted landing point.</summary>
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
            InstallAlcoonInstruction(slot, state, AlcoonProgram.AirborneLeftLookingUp);
        }
        else
        {
            state.Velocity = state.Velocity.WithWholeX(2);
            InstallAlcoonInstruction(slot, state, AlcoonProgram.AirborneRightLookingUp);
        }
        state.Function = AlcoonEnemyFunction.EmergingRising;
        LastAlcoonSoundEffect = AlcoonEmergeSound;
    }

    /// <summary>Integrates the emerging ascent until vertical velocity ceases to be negative.</summary>
    private static void RunAlcoonEmergingRise(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        AddAlcoonYVelocity(slot, state);
        AccelerateAlcoonY(state);
        if (state.Velocity.Y.WholePixels < 0)
            return;

        state.Function = AlcoonEnemyFunction.EmergingFalling;
        InstallAlcoonInstruction(
            slot,
            state,
            state.Velocity.X.WholePixels < 0
                ? AlcoonProgram.AirborneLeftLookingForward
                : AlcoonProgram.AirborneRightLookingForward);
    }

    /// <summary>Applies collision-aware descent, then chooses the initial walking direction toward Samus.</summary>
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
            InstallAlcoonInstruction(slot, state, AlcoonProgram.WalkingLeft);
        }
        else
        {
            state.Velocity = state.Velocity.WithWholeX(2);
            InstallAlcoonInstruction(slot, state, AlcoonProgram.WalkingRight);
        }
        state.Function = AlcoonEnemyFunction.WalkingAndFiring;
        state.StepCounter = NativeCountdown.Start(1);
    }

    /// <summary>Maintains floor contact, starts a retreat at the spawn-distance limit, or begins a forward volley.</summary>
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
                state,
                state.Velocity.X.WholePixels < 0
                    ? AlcoonProgram.AirborneLeftLookingForward
                    : AlcoonProgram.AirborneRightLookingForward);
            return;
        }

        if (!state.StepCounter.IsZero)
            return;

        bool samusIsLeft = unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0;
        bool movingLeft = state.Velocity.X.WholePixels < 0;
        if (samusIsLeft != movingLeft)
            return;

        InstallAlcoonInstruction(
            slot,
            state,
            movingLeft
                ? AlcoonProgram.FireLeft
                : AlcoonProgram.FireRight);
        state.Function = AlcoonEnemyFunction.WaitingForFireAnimation;
    }

    /// <summary>Starts the retreat arc upward until gravity reverses its Y velocity.</summary>
    private static void RunAlcoonHidingRise(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        AddAlcoonYVelocity(slot, state);
        AccelerateAlcoonY(state);
        if (state.Velocity.Y.WholePixels >= 0)
            state.Function = AlcoonEnemyFunction.HidingFalling;
    }

    /// <summary>Returns the retreating actor to its whole-pixel spawn position while retaining subpositions.</summary>
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

    /// <summary>Resets the jump to a twelve-pixel upward velocity and half-pixel acceleration.</summary>
    private static void SetupAlcoonJumpMovement(AlcoonEnemyState state)
    {
        state.Velocity = new FixedVector<RoomPixels>(
            SnesSignedSixteenSixteen.Zero, SnesSignedSixteenSixteen.FromParts(-12, 0));
        state.ClearedVariable = 0;
        state.YAcceleration = 0;
        state.YSubacceleration = 0x8000;
    }

    /// <summary>Adds the per-slot 16.16 acceleration to Alcoon's vertical velocity.</summary>
    private static void AccelerateAlcoonY(AlcoonEnemyState state)
    {
        SnesSignedSixteenSixteen acceleration = SnesSignedSixteenSixteen.FromParts(
            unchecked((short)state.YAcceleration), state.YSubacceleration);
        state.Velocity = state.Velocity.WithY(state.Velocity.Y + acceleration);
    }

    /// <summary>Integrates vertical velocity without collision during airborne ascent or retreat.</summary>
    private static void AddAlcoonYVelocity(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        slot.Position = slot.Position.AdvanceY(state.Velocity.Y);
    }

    /// <summary>Rejects an initializer that cannot find a landing floor within the simulation bound.</summary>
    private static void GuardAlcoonJumpSimulation(int iterations, RoomEnemySlot slot)
    {
        if (iterations > AlcoonJumpSimulationLimit)
        {
            throw new InvalidDataException(
                $"Alcoon slot {slot.SlotIndex} did not find a landing floor within " +
                $"{AlcoonJumpSimulationLimit} initialization steps.");
        }
    }

    /// <summary>Switches Alcoon's animation so its first step runs on the next update.</summary>
    private static void InstallAlcoonInstruction(RoomEnemySlot slot, AlcoonEnemyState state, AlcoonProgram program)
    {
        state.Program = AnimationProgramCursor<AlcoonProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Implements Alcoon instruction <c>$A8:DF3F</c> and returns the walk the animation continues at.</summary>
    private AlcoonProgram StartAlcoonWalking(AlcoonEnemyState state)
    {
        state.Function = AlcoonEnemyFunction.WalkingAndFiring;
        Func<ushort> readRandomNumber = _room.Random.Read;
        ushort count = unchecked((ushort)(readRandomNumber() & 3));
        state.StepCounter = NativeCountdown.FromRaw(count == 0 ? (ushort)2 : count);
        return state.Velocity.X.WholePixels < 0
            ? AlcoonProgram.WalkingLeft
            : AlcoonProgram.WalkingRight;
    }

    /// <summary>
    /// Implements both movement bytecodes at <c>$A8:DF63/$DF71</c>: the animation runs on unless a wall turns the Alcoon
    /// round, when it continues at the other walk's first pose.
    /// </summary>
    private AlcoonProgram? MoveAlcoonHorizontally(
        RoomEnemySlot slot,
        AlcoonEnemyState state,
        RoomLevelData? level,
        bool decrementStepCounter)
    {
        if (level is null)
            throw new InvalidOperationException("Alcoon movement instruction requires room collision data.");

        if (decrementStepCounter && !state.StepCounter.IsZero)
            state.StepCounter = state.StepCounter.Decrement().Remaining;

        int displacement = state.Velocity.X.RawValue;
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
        {
            EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, slot);
            return null;
        }

        short oldVelocity = state.Velocity.X.WholePixels;
        state.Velocity = state.Velocity.WithWholeX(unchecked((short)-oldVelocity));
        return oldVelocity >= 0
            ? AlcoonProgram.WalkingLeftLoop
            : AlcoonProgram.WalkingRightLoop;
    }

    /// <summary>Requires per-slot state created by the Alcoon initializer.</summary>
    private AlcoonEnemyState RequireAlcoonState(RoomEnemySlot slot) =>
        _alcoonStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Alcoon state.");

    /// <summary>Connects Alcoon fireballs to their bank-$86 movement pre-instruction.</summary>
    internal static void RegisterAlcoonProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunAlcoonFireballPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.AlcoonFireball);
    }

    /// <summary>Registers initialization only for the Alcoon enemy header.</summary>
    internal void RegisterAlcoonInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Alcoon, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Alcoon, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeAlcoon(slot, level);
        });
    }

    /// <summary>Registers Alcoon's seven-phase main dispatcher.</summary>
    internal void RegisterAlcoonMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Alcoon, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Alcoon, (slot, frame, nmiFrameCounter8) =>
        {
            RunAlcoonMain(slot, RequireAlcoonState(slot), frame.Samus, frame.Level);
        });
    }

    /// <summary>Runs the Alcoon's walking, fireball and airborne animations as typed programs (#641); its steps need the room's collision data.</summary>
    internal void RegisterAlcoonTypedAnimations(EnemyAiTable table) =>
        table.AddTypedAnimation(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Alcoon, (slot, context) =>
        {
            if (!EnemyProgramAnimation.Expire(slot))
                return;
            var state = RequireAlcoonState(slot);
            AnimationProgramCursor<AlcoonProgram> cursor = state.Program;
            EnemyProgramAnimation.Run(slot, AlcoonAnimations.Programs, ref cursor, AlcoonFrameDefinitions.Spritemap,
                    (cue, _) => ApplyAlcoonCue(slot, state, context.Level, cue));
            state.Program = cursor;
        });
    /// <summary>Initial horizontal fireball speed in native 8.8 units.</summary>
    private const ushort AlcoonFireballGraphicsHorizontalSpeed = 0x0400;
    /// <summary>Horizontal speed at which fireball deceleration stops.</summary>
    private const ushort AlcoonFireballTerminalHorizontalSpeed = 0x0200;
    /// <summary>Native 8.8 horizontal speed reduction applied per fireball update.</summary>
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

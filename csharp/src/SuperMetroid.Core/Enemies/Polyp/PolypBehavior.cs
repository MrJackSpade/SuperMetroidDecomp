using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Polyp.Artwork;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>The Polyp species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class PolypBehavior : IEnemySpecies
{
    /// <summary>Provides the shared room RNG and enemy state used by all Polyp actors.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates rock projectile slots when a Polyp launches.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Binds the Polyp species to one room's enemies and projectile pool.</summary>
    internal PolypBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles)
    {
        _room = room;
        _projectiles = projectiles;
    }

    /// <summary>Samus must be within 64 pixels on each axis to trigger a launch.</summary>
    private const ushort PolypProximity = 64;
    /// <summary>Each Polyp initializer resets the shared enemy random seed to $0011.</summary>
    private const ushort PolypRandomSeed = 17;

    /// <summary>Slot-indexed function and cooldown state for the active room's Polyp actors.</summary>
    internal readonly PolypEnemyState?[] _polypStates =
        new PolypEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Polyp</c> at $A2:B570.</summary>
    internal void InitializePolyp(RoomEnemySlot slot)
    {
        Action<ushort> setRandomNumber = _room.Random.Seed;

        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        _polypStates[slot.SlotIndex] = new PolypEnemyState(slot)
        {
            Program = AnimationProgramCursor<PolypProgram>.Start(PolypProgram.Stationary),
            Function = PolypEnemyFunction.WaitingForSamus,
            CooldownTimer = NativeCountdown.Start(0),
        };

        // Every Polyp initializer writes $0011 to the shared seed. In Volcano the fourth
        // actor therefore leaves this exact value for the first gameplay frame; treating it
        // as actor-local randomness would produce a different launch sequence.
        setRandomNumber(PolypRandomSeed);
    }

    /// <summary>Ports <c>MainAI_Polyp</c> and its three indirect functions at $A2:B58F.</summary>
    private void RunPolypMain(
        RoomEnemySlot slot,
        PolypEnemyState state,
        SamusState? samus)
    {
        switch (state.Function)
        {
            case PolypEnemyFunction.WaitingForSamus:
                if (samus is null)
                {
                    throw new InvalidOperationException(
                        "Polyp proximity AI requires the active Samus actor.");
                }

                if (IsWithinPolypAxis(samus.Position.X.Whole, slot.Position.X.Whole) &&
                    IsWithinPolypAxis(samus.Position.Y.Whole, slot.Position.Y.Whole))
                {
                    // Native AI only installs the shoot function here. The rock is emitted
                    // on the next enemy frame, a one-frame tell preserved deliberately.
                    state.Function = PolypEnemyFunction.ShootingRock;
                }
                return;

            case PolypEnemyFunction.ShootingRock:
                ShootPolypRock(slot, state);
                return;

            case PolypEnemyFunction.Cooldown:
                NativeWordCounterStep timer = state.CooldownTimer.Decrement();
                state.CooldownTimer = timer.Remaining;
                if (timer.IsNegative)
                    state.Function = PolypEnemyFunction.WaitingForSamus;
                return;

            default:
                throw new InvalidDataException(
                    $"Polyp function $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports the three independent random-table selections at $A2:B5B2.</summary>
    private void ShootPolypRock(RoomEnemySlot slot, PolypEnemyState state)
    {
        ushort xVelocity = PolypLaunchDefinitions.XVelocity(_room.Random.Generate());

        ushort initialYSpeed = PolypLaunchDefinitions.InitialYIndex(_room.Random.Generate());
        SpawnPolypRock(slot, initialYSpeed, xVelocity);

        state.Function = PolypEnemyFunction.Cooldown;
        state.CooldownTimer = NativeCountdown.FromRaw(PolypLaunchDefinitions.Cooldown(_room.Random.Generate()));
    }

    /// <summary>
    /// Recreates <c>GetSignedYMinusX</c> followed by the signed threshold comparison used by
    /// both bank-$A0 proximity helpers. Equality at 64 pixels is outside; the modular $8000
    /// difference also remains outside without passing through host <c>Math.Abs</c> overflow.
    /// </summary>
    private static bool IsWithinPolypAxis(ushort samusCoordinate, ushort enemyCoordinate)
    {
        ushort difference = unchecked((ushort)(samusCoordinate - enemyCoordinate));
        ushort magnitude = EnemyMath.IsNegative16(difference)
            ? unchecked((ushort)-difference)
            : difference;
        return unchecked((short)(magnitude - PolypProximity)) < 0;
    }

    /// <summary>Rejects a main-AI dispatch before its slot has received Polyp initialization.</summary>
    private PolypEnemyState RequirePolypState(RoomEnemySlot slot) =>
        _polypStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Polyp state.");

    /// <summary>Routes Polyp rock projectiles through their rise, fall, and viewport-culling routine.</summary>
    internal static void RegisterPolypProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            // Polyp rock: quadratic rise/fall and viewport cull.
            RunPolypRockPreInstruction(context.Projectile, context.Camera);
        }, EnemyProjectilePreInstruction.PolypRock);
    }

    /// <summary>Runs Polyp's stationary pose as a typed program (#641).</summary>
    internal void RegisterPolypTypedAnimations(EnemyAiTable table) =>
        table.AddTypedAnimation(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.LavaRocks, (slot, _) =>
        {
            if (!EnemyProgramAnimation.Expire(slot))
                return;
            var state = RequirePolypState(slot);
            AnimationProgramCursor<PolypProgram> cursor = state.Program;
            EnemyProgramAnimation.Run(slot, PolypAnimations.Programs, ref cursor, PolypFrameDefinitions.Spritemap);
            state.Program = cursor;
        });

    /// <summary>Associates the lava-rock enemy definition with Polyp slot initialization.</summary>
    internal void RegisterPolypInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Polyp, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.LavaRocks, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializePolyp(slot);
        });
    }

    /// <summary>Dispatches lava-rock enemy frames to the slot's current Polyp function.</summary>
    internal void RegisterPolypMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Polyp, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.LavaRocks, (slot, frame, nmiFrameCounter8) =>
        {
            RunPolypMain(slot, RequirePolypState(slot), frame.Samus);
        });
    }
    /// <summary>Native projectile pre-instruction installed on each launched rock.</summary>
    private const ushort PolypRockPreInstruction =
        (ushort)EnemyProjectilePreInstruction.PolypRock;
    /// <summary>Each rock frame advances the quadratic speed-table index by two records.</summary>
    private const ushort PolypRockGravityStep = 2;
    /// <summary>Falling rocks stop accelerating at the native $0040 speed index.</summary>
    private const ushort PolypRockTerminalSpeedIndex = 64;

    /// <summary>
    /// Ports <c>SpawnEnemyProjectileY_ParameterA_XGraphics</c> plus initializer $86:BBDB.
    /// The actor's palette/tile word and all four position halves are copied at allocation.
    /// </summary>
    internal void SpawnPolypRock(
        RoomEnemySlot source,
        ushort initialYSpeed,
        ushort xVelocity)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.PolypRock,
            unchecked((ushort)(source.PaletteIndex | source.VramTilesIndex)));
        projectile.InstallAnimation(new PolypRockAnimation(PolypRockProgram.Initial));
        projectile.PreInstruction = PolypRockPreInstruction;
        projectile.Variable0 = (ushort)PolypRockFunction.Rising;
        projectile.Velocity = new(new SnesSignedEightEight(xVelocity), new SnesSignedEightEight(initialYSpeed));
        projectile.Position = source.Position;
    }

    /// <summary>Ports pre-instruction $86:BC0F and its rising/falling dispatcher.</summary>
    internal static void RunPolypRockPreInstruction(
        RoomEnemyProjectileSlot projectile,
        Position16<RoomPixels> camera)
    {
        switch (ClosedNativeWords.Decode<PolypRockFunction>(projectile.Variable0, "Polyp rock function"))
        {
            case PolypRockFunction.Rising:
                StepRisingPolypRock(projectile);
                break;
            case PolypRockFunction.Falling:
                StepFallingPolypRock(projectile);
                break;
            default:
                throw new InvalidDataException(
                    $"Polyp rock function $86:{projectile.Variable0:X4} is not translated.");
        }

        CacatacBehavior.DeleteEnemyProjectileIfOutsideInclusiveViewport(projectile, camera);
    }

    /// <summary>Integrates two upward quadratic samples unless the apex consumes this frame.</summary>
    private static void StepRisingPolypRock(RoomEnemyProjectileSlot projectile)
    {
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(
            projectile.Velocity.Y.RawValue - PolypRockGravityStep))));
        if (unchecked((short)projectile.Velocity.Y.RawValue) < 0)
        {
            // Crossing below zero consumes an entire apex frame: retail installs falling
            // state and returns before either vertical or horizontal movement.
            projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(0));
            projectile.Variable0 = (ushort)PolypRockFunction.Falling;
            return;
        }

        // The native routine integrates two adjacent quadratic samples per frame, first
        // index velocity+1 and then velocity. This is why the table index changes by two
        // while the visual arc still advances at gameplay-frame rate.
        AddPolypRockQuadraticStep(
            projectile,
            unchecked((ushort)(projectile.Velocity.Y.RawValue + 1)),
            negative: true);
        AddPolypRockQuadraticStep(projectile, projectile.Velocity.Y.RawValue, negative: true);
        MovePolypRockHorizontally(projectile);
    }

    /// <summary>Clamps downward speed and integrates two positive quadratic samples per frame.</summary>
    private static void StepFallingPolypRock(RoomEnemyProjectileSlot projectile)
    {
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(
            projectile.Velocity.Y.RawValue + PolypRockGravityStep))));
        if (unchecked((short)(projectile.Velocity.Y.RawValue - PolypRockTerminalSpeedIndex)) >= 0)
            projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(PolypRockTerminalSpeedIndex));

        AddPolypRockQuadraticStep(
            projectile,
            unchecked((ushort)(projectile.Velocity.Y.RawValue - 1)),
            negative: false);
        AddPolypRockQuadraticStep(projectile, projectile.Velocity.Y.RawValue, negative: false);
        MovePolypRockHorizontally(projectile);
    }

    /// <summary>
    /// Adds one positive or cartridge-stored negative 16.16 record from $A0:CBC7. Variable
    /// one is real scratch WRAM: after each sample it contains the whole word because the
    /// assembly overwrites its earlier fractional copy before returning.
    /// </summary>
    internal static void AddPolypRockQuadraticStep(
        RoomEnemyProjectileSlot projectile,
        ushort tableIndex,
        bool negative)
    {
        if (tableIndex >= EnemyQuadraticSpeedDefinitions.RecordCount)
        {
            throw new InvalidDataException(
                $"Polyp rock quadratic speed index ${tableIndex:X4} exceeds $005E.");
        }

        int record = tableIndex * EnemyQuadraticSpeedDefinitions.RecordSize +
            (negative ? 4 : 0);
        ushort fraction = EnemyQuadraticSpeedDefinitions.ReadWord(record);
        projectile.Variable1 = fraction;
        uint fractionalSum = (uint)projectile.Position.Y.Fraction + fraction;
        projectile.Position = projectile.Position.WithFractionY(unchecked((ushort)fractionalSum));
        if (fractionalSum > ushort.MaxValue)
            projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(projectile.Position.Y.Whole + 1)));

        ushort whole = EnemyQuadraticSpeedDefinitions.ReadWord(record + 2);
        projectile.Variable1 = whole;
        projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(projectile.Position.Y.Whole + whole)));
    }

    /// <summary>Ports $86:BCF4's split signed-8.8 horizontal addition.</summary>
    private static void MovePolypRockHorizontally(RoomEnemyProjectileSlot projectile)
    {
        projectile.Position = projectile.Position.AdvanceX(projectile.Velocity.X.ToSixteenSixteenDelta());
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        Array.Clear(_polypStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}

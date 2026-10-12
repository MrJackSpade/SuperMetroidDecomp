namespace SuperMetroid.Core.Game;

/// <summary>The Polyp species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class PolypBehavior : IEnemySpecies
{
    private readonly EnemyRoomBinding _room;
    private readonly EnemyProjectileEngine _projectiles;

    internal PolypBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles)
    {
        _room = room;
        _projectiles = projectiles;
    }

    private const ushort PolypProximity = 0x0040;
    private const ushort PolypRandomSeed = 0x0011;

    internal readonly PolypEnemyState?[] _polypStates =
        new PolypEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Polyp</c> at $A2:B570.</summary>
    internal void InitializePolyp(RoomEnemySlot slot)
    {
        Action<ushort> setRandomNumber = _room.Random.Seed;

        slot.CurrentInstruction = PolypInstructionProgramDefinitions.Stationary;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        _polypStates[slot.SlotIndex] = new PolypEnemyState(slot)
        {
            Function = PolypEnemyFunction.WaitingForSamus,
            CooldownTimer = 0,
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
                NativeWordCounterStep timer = NativeWordCounter.Decrement(state.CooldownTimer);
                state.CooldownTimer = timer.Value;
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
        state.CooldownTimer = PolypLaunchDefinitions.Cooldown(_room.Random.Generate());
    }

    /// <summary>
    /// Recreates <c>GetSignedYMinusX</c> followed by the signed threshold comparison used by
    /// both bank-$A0 proximity helpers. Equality at 64 pixels is outside; the modular $8000
    /// difference also remains outside without passing through host <c>Math.Abs</c> overflow.
    /// </summary>
    private static bool IsWithinPolypAxis(ushort samusCoordinate, ushort enemyCoordinate)
    {
        ushort difference = unchecked((ushort)(samusCoordinate - enemyCoordinate));
        ushort magnitude = (difference & 0x8000) != 0
            ? unchecked((ushort)-difference)
            : difference;
        return unchecked((short)(magnitude - PolypProximity)) < 0;
    }

    private PolypEnemyState RequirePolypState(RoomEnemySlot slot) =>
        _polypStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Polyp state.");

    internal static void RegisterPolypProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            // Polyp rock: quadratic rise/fall and viewport cull.
            RunPolypRockPreInstruction(context.Projectile, context.Camera);
        }, EnemyProjectilePreInstruction.PolypRock);
    }

    internal void RegisterPolypInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Polyp, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.LavaRocks, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializePolyp(slot);
        });
    }

    internal void RegisterPolypMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Polyp, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.LavaRocks, (slot, frame, nmiFrameCounter8) =>
        {
            RunPolypMain(slot, RequirePolypState(slot), frame.Samus);
        });
    }
    private const ushort PolypRockPreInstruction =
        (ushort)EnemyProjectilePreInstruction.PolypRock;
    private const ushort PolypRockGravityStep = 2;
    private const ushort PolypRockTerminalSpeedIndex = 0x0040;

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
        projectile.InstructionPointer = PolypRockInstructionProgramDefinitions.Initial;
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

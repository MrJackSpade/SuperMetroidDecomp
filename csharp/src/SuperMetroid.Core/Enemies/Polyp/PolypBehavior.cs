using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Polyp.Artwork;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>The Polyp species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class PolypBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>The phase a rock keeps in its variable zero.</summary>
    private static readonly ParameterVariants<PolypRockFunction> RockFunctions = new(new Dictionary<int, PolypRockFunction>
    {
        [1] = PolypRockFunction.Rising,
        [2] = PolypRockFunction.Falling,
    });

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.LavaRocks];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new PolypEntity(this, slot, common);

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

    /// <summary>Ports <c>InitAI_Polyp</c> at $A2:B570.</summary>
    internal void InitializePolyp(RoomEnemySlot slot)
    {
        Action<int> setRandomNumber = _room.Random.Seed;

        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        var state = new PolypEnemyState
        {
            Program = AnimationProgramCursor<PolypProgram>.Start(PolypProgram.Stationary),
            Function = PolypEnemyFunction.WaitingForSamus,
            CooldownTimer = WrappingCountdown.Start(0),
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);

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
                CountdownStep timer = state.CooldownTimer.Decrement();
                state.CooldownTimer = timer.Remaining;
                if (timer.IsNegative)
                    state.Function = PolypEnemyFunction.WaitingForSamus;
                return;

            default:
                throw new InvalidDataException(
                    $"Polyp function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Ports the three independent random-table selections at $A2:B5B2.</summary>
    private void ShootPolypRock(RoomEnemySlot slot, PolypEnemyState state)
    {
        int xVelocity = PolypLaunchDefinitions.XVelocity(_room.Random.Generate());

        int initialYSpeed = PolypLaunchDefinitions.InitialYIndex(_room.Random.Generate());
        SpawnPolypRock(slot, initialYSpeed, xVelocity);

        state.Function = PolypEnemyFunction.Cooldown;
        state.CooldownTimer = WrappingCountdown.FromRaw(PolypLaunchDefinitions.Cooldown(_room.Random.Generate()));
    }

    /// <summary>
    /// Recreates <c>GetSignedYMinusX</c> followed by the signed threshold comparison used by
    /// both bank-$A0 proximity helpers. Equality at 64 pixels is outside; the modular $8000
    /// difference also remains outside without passing through host <c>Math.Abs</c> overflow.
    /// </summary>
    private static bool IsWithinPolypAxis(int samusCoordinate, int enemyCoordinate)
    {
        int difference = unchecked((ushort)(samusCoordinate - enemyCoordinate));
        int magnitude = EnemyMath.IsNegative16(difference)
            ? unchecked((ushort)-difference)
            : difference;
        return unchecked((short)(magnitude - PolypProximity)) < 0;
    }

    /// <summary>Routes Polyp rock projectiles through their rise, fall, and viewport-culling routine.</summary>
    internal static void AddPolypProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddPreInstruction(context =>
        {
            // Polyp rock: quadratic rise/fall and viewport cull.
            RunPolypRockPreInstruction(context.Projectile, context.Camera);
        }, EnemyProjectilePreInstruction.PolypRock);
    }

    /// <summary>Advances Polyp's typed animation.</summary>
    internal static void AdvancePolypTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, PolypAnimations.Programs, PolypFrameDefinitions.ShownFrames.Of,
            static (PolypEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor);

    /// <summary>Ports Polyp's main AI.</summary>
    internal void RunPolypMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunPolypMain(slot, slot.RequireSpeciesState<PolypEnemyState>(), frame.Samus);
    }

    /// <summary>Native projectile pre-instruction installed on each launched rock.</summary>
    private const EnemyProjectilePreInstruction PolypRockPreInstruction =
        EnemyProjectilePreInstruction.PolypRock;
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
        int initialYSpeed,
        int xVelocity)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.PolypRock,
            unchecked((ushort)(source.PaletteIndex | source.TileIndexBase)));
        projectile.InstallAnimation(new PolypRockAnimation(PolypRockProgram.Initial));
        projectile.PreInstruction = PolypRockPreInstruction;
        projectile.Variable0 = RockFunctions.Parameter(PolypRockFunction.Rising);
        projectile.Velocity = new(new SignedEightEight(checked((ushort)xVelocity)), new SignedEightEight(checked((ushort)initialYSpeed)));
        projectile.Position = source.Position;
    }

    /// <summary>Ports pre-instruction $86:BC0F and its rising/falling dispatcher.</summary>
    internal static void RunPolypRockPreInstruction(
        RoomEnemyProjectileSlot projectile,
        Position16<RoomPixels> camera)
    {
        switch (RockFunctions.Decode(projectile.Variable0))
        {
            case PolypRockFunction.Rising:
                StepRisingPolypRock(projectile);
                break;
            case PolypRockFunction.Falling:
                StepFallingPolypRock(projectile);
                break;
            default:
                throw new InvalidDataException(
                    $"Polyp rock function {projectile.Variable0} is not translated.");
        }

        CacatacBehavior.DeleteEnemyProjectileIfOutsideInclusiveViewport(projectile, camera);
    }

    /// <summary>Integrates two upward quadratic samples unless the apex consumes this frame.</summary>
    private static void StepRisingPolypRock(RoomEnemyProjectileSlot projectile)
    {
        projectile.Velocity = projectile.Velocity.WithY(new SignedEightEight(unchecked((ushort)(
            projectile.Velocity.Y.RawValue - PolypRockGravityStep))));
        if (unchecked((short)projectile.Velocity.Y.RawValue) < 0)
        {
            // Crossing below zero consumes an entire apex frame: retail installs falling
            // state and returns before either vertical or horizontal movement.
            projectile.Velocity = projectile.Velocity.WithY(new SignedEightEight(0));
            projectile.Variable0 = RockFunctions.Parameter(PolypRockFunction.Falling);
            return;
        }

        // The native routine integrates two adjacent quadratic samples per frame, first
        // index velocity+1 and then velocity. This is why the table index changes by two
        // while the visual arc still advances at gameplay-frame rate.
        AddPolypRockQuadraticStep(
            projectile,
            unchecked((ushort)(projectile.Velocity.Y.RawValue + 1)),
            half: QuadraticSpeedHalf.Negative);
        AddPolypRockQuadraticStep(projectile, projectile.Velocity.Y.RawValue, half: QuadraticSpeedHalf.Negative);
        MovePolypRockHorizontally(projectile);
    }

    /// <summary>Clamps downward speed and integrates two positive quadratic samples per frame.</summary>
    private static void StepFallingPolypRock(RoomEnemyProjectileSlot projectile)
    {
        projectile.Velocity = projectile.Velocity.WithY(new SignedEightEight(unchecked((ushort)(
            projectile.Velocity.Y.RawValue + PolypRockGravityStep))));
        if (unchecked((short)(projectile.Velocity.Y.RawValue - PolypRockTerminalSpeedIndex)) >= 0)
            projectile.Velocity = projectile.Velocity.WithY(new SignedEightEight(PolypRockTerminalSpeedIndex));

        AddPolypRockQuadraticStep(
            projectile,
            unchecked((ushort)(projectile.Velocity.Y.RawValue - 1)),
            half: QuadraticSpeedHalf.Positive);
        AddPolypRockQuadraticStep(projectile, projectile.Velocity.Y.RawValue, half: QuadraticSpeedHalf.Positive);
        MovePolypRockHorizontally(projectile);
    }

    /// <summary>
    /// Adds one positive or cartridge-stored negative 16.16 record from $A0:CBC7. Variable
    /// one is real scratch WRAM: after each sample it contains the whole word because the
    /// assembly overwrites its earlier fractional copy before returning.
    /// </summary>
    internal static void AddPolypRockQuadraticStep(
        RoomEnemyProjectileSlot projectile,
        int tableIndex,
        QuadraticSpeedHalf half)
    {
        if (tableIndex >= EnemyQuadraticSpeedDefinitions.RecordCount)
        {
            throw new InvalidDataException(
                $"Polyp rock quadratic speed index ${tableIndex:X4} exceeds $005E.");
        }

        int record = tableIndex * EnemyQuadraticSpeedDefinitions.RecordSize +
            (half == QuadraticSpeedHalf.Negative ? 4 : 0);
        int fraction = EnemyQuadraticSpeedDefinitions.ReadWord(record);
        projectile.Variable1 = fraction;
        // Both fractions are words, so their sum fits an int; a sum past the word carries into the whole pixel.
        int fractionalSum = projectile.Position.Y.Fraction + fraction;
        projectile.Position = projectile.Position.WithFractionY(unchecked((ushort)fractionalSum));
        if (fractionalSum > ushort.MaxValue)
            projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(projectile.Position.Y.Whole + 1)));

        int whole = EnemyQuadraticSpeedDefinitions.ReadWord(record + 2);
        projectile.Variable1 = whole;
        projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(projectile.Position.Y.Whole + whole)));
    }

    /// <summary>Ports $86:BCF4's split signed-8.8 horizontal addition.</summary>
    private static void MovePolypRockHorizontally(RoomEnemyProjectileSlot projectile)
    {
        projectile.Position = projectile.Position.AdvanceX(projectile.Velocity.X.ToSixteenSixteenDelta());
    }
}

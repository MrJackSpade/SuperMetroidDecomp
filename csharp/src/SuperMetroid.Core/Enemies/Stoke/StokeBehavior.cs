using SuperMetroid.Core.Enemies.Stoke.Artwork;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>The Stoke species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class StokeBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Stoke];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new StokeEntity(this, slot, common);

    /// <summary>Supplies the shared room RNG for Stoke's fireball attack branch.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Probes the floor and resolves Stoke's horizontal collision and turn.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Allocates Stoke's fireball into the enemy projectile subsystem.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Binds Stoke movement and fireball behavior to the current enemy room.</summary>
    internal StokeBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyProjectileEngine projectiles)
    {
        _room = room;
        _terrain = terrain;
        _projectiles = projectiles;
    }

    /// <summary>Carries out one cue of Stoke's animations.</summary>
    private StokeProgram? ApplyStokeCue(RoomEnemySlot slot, StokeEnemyState state, StokeCue cue)
    {
        switch (cue.Kind)
        {
            case StokeCueKind.SpawnFireball:
                SpawnStokeProjectile(slot, cue.FireballDirection);
                break;
            case StokeCueKind.SetMovingLeft:
                SetStokeMoving(state, StokeDirection.Left);
                break;
            case StokeCueKind.SetMovingRight:
                SetStokeMoving(state, StokeDirection.Right);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(StokeCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>Two-pixel downward probe used to test for a solid floor without moving Stoke.</summary>
    private const int StokeFloorProbeDisplacement = 2 << 16;

    /// <summary>Ports <c>InitAI_Stoke</c> at <c>$A2:89AD</c>.</summary>
    internal static void InitializeStoke(RoomEnemySlot slot)
    {
        if (slot.Spawn.Population.Parameter2 >= EnemyLinearSpeedDefinitions.RecordCount)
        {
            throw new InvalidDataException(
                $"Stoke speed parameter ${slot.Spawn.Population.Parameter2:X4} exceeds the NTSC common " +
                $"speed table's ${EnemyLinearSpeedDefinitions.RecordCount - 1:X2} maximum index.");
        }

        // The parameter is an entry number. ASL three times converts it to the byte offset
        // of {positive whole, positive fraction, negative whole, negative fraction}.
        int speedOffset = slot.Spawn.Population.Parameter2 * EnemyLinearSpeedDefinitions.RecordSize;
        var right = EnemyLinearSpeedDefinitions.Read(speedOffset);
        var left = EnemyLinearSpeedDefinitions.Read(speedOffset + 4);
        var state = new StokeEnemyState
        {
            RightVelocity = right,
            LeftVelocity = left,
            Function = StokeAiFunction.Moving,
            Direction = (StokeDirection)slot.Spawn.Population.Parameter1,
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);

        // Initialization writes the common empty map, then picks the left list by default.
        // A nonzero init0 takes the native right-facing branch. The room-loader tail later
        // restores the transient empty map, and the first instruction tick supplies art.
        slot.Frame = EnemyFrameId.Empty;
        SetStokeProgram(slot, state, StokeProgram.MovingLeft);
        if (slot.Spawn.Population.Parameter1 != 0)
        {
            SetStokeProgram(slot, state, StokeProgram.MovingRight);
        }
    }

    /// <summary>Ports <c>MainAI_Stoke</c> at <c>$A2:89F0</c>.</summary>
    private void RunStokeMain(RoomEnemySlot slot, StokeEnemyState state, RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Stoke walking AI requires the active room level.");

        switch (state.Function)
        {
            case StokeAiFunction.IdleDuringAttack:
                return;

            case StokeAiFunction.Moving:
                // A nonzero spawn direction other than one takes the native right-facing branch, so only the left
                // word walks left.
                bool left = state.Direction == StokeDirection.Left;
                RunMovingStoke(
                    slot,
                    state,
                    level,
                    left ? state.LeftVelocity.RawValue : state.RightVelocity.RawValue,
                    left ? StokeProgram.AttackingLeft : StokeProgram.AttackingRight);
                return;

            default:
                throw new InvalidDataException(
                    $"Stoke main-AI function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Ports the shared body of moving-left <c>$8A43</c> and moving-right <c>$8A5C</c>.</summary>
    private void RunMovingStoke(
        RoomEnemySlot slot,
        StokeEnemyState state,
        RoomLevelData level,
        int horizontalDisplacement,
        StokeProgram attackingProgram)
    {
        MoveStokeAndTurnAtTerrain(slot, state, level, horizontalDisplacement);

        // GenerateRandomNumber advances the global seed before the comparison. Only low
        // byte sums zero and one attack; byte wrapping is intentional and observable.
        int random = _room.Random.Generate();
        byte attackRoll = unchecked((byte)((byte)slot.FrameCounter + (byte)random));
        if (attackRoll >= 2)
            return;

        // The attack list owns Stoke until its trailing goto reaches $8932/$8958 and that
        // wrapper's command installs the corresponding moving function again.
        state.Function = StokeAiFunction.IdleDuringAttack;
        SetStokeProgram(slot, state, attackingProgram);
    }

    /// <summary>Ports <c>StokeMovement</c> at <c>$A2:8A76</c>.</summary>
    private void MoveStokeAndTurnAtTerrain(
        RoomEnemySlot slot,
        StokeEnemyState state,
        RoomLevelData level,
        int horizontalDisplacement)
    {
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, horizontalDisplacement))
        {
            TurnStokeAround(slot, state);
            return;
        }

        // $A0:BC76 is a probe, not MoveEnemyDown: it computes a candidate Y coordinate and
        // scans for any level word with bit 15 set without changing Stoke's Y/subposition.
        // Conflating it with vertical movement makes the actor sink two pixels every frame.
        if (!StokeHasSolidFloorTwoPixelsBelow(level, slot))
            TurnStokeAround(slot, state);
    }

    /// <summary>Ports <c>CheckForVerticalSolidBlockCollision</c>'s Stoke use at $A0:BC76.</summary>
    private static bool StokeHasSolidFloorTwoPixelsBelow(RoomLevelData level, RoomEnemySlot slot)
        => EnemyTerrain.EnemyHasSolidHighBitVerticallyAhead(level, slot, StokeFloorProbeDisplacement);

    /// <summary>Ports <c>TurnStokeAround</c> at <c>$A2:8A95</c>.</summary>
    private static void TurnStokeAround(RoomEnemySlot slot, StokeEnemyState state)
    {
        // The routine always selects the left wrapper first, then replaces it with right
        // only when the old direction was not one. It does not update direction/function;
        // the wrapper command executes later in this same enemy instruction phase.
        SetStokeProgram(slot, state, StokeProgram.MovingLeft);
        if (state.Direction != StokeDirection.Right)
            SetStokeProgram(slot, state, StokeProgram.MovingRight);
    }

    /// <summary>Switches the active animation so its first step runs on the next instruction tick.</summary>
    private static void SetStokeProgram(RoomEnemySlot slot, StokeEnemyState state, StokeProgram program)
    {
        state.Program = AnimationProgramCursor<StokeProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Instructions <c>$A2:8990</c> (left) and <c>$A2:899D</c> (right): resume movement in <paramref name="direction"/>.</summary>
    private static void SetStokeMoving(StokeEnemyState state, StokeDirection direction)
    {
        state.Function = StokeAiFunction.Moving;
        state.Direction = direction;
    }

    /// <summary>Connects the fireball's horizontal motion and viewport cull pre-instruction.</summary>
    internal static void RegisterStokeProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            // Stoke shot: horizontal motion and viewport cull.
            RunStokeProjectilePreInstruction(context.Projectile, context.Camera);
        }, EnemyProjectilePreInstruction.StokeFireball);
    }

    /// <summary>Ports Stoke's main AI.</summary>
    internal void RunStokeMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunStokeMain(slot, slot.RequireSpeciesState<StokeEnemyState>(), frame.Level);
    }

    /// <summary>Advances Stoke's typed animation.</summary>
    internal void AdvanceStokeTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, StokeAnimations.Programs, StokeFrameDefinitions.ShownFrames.Of,
            static (StokeEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor,
            (state, cue, _) => ApplyStokeCue(slot, state, cue));

    /// <summary>
    /// Ports <c>SpawnEnemyProjectileY_ParameterA_XGraphics</c> followed by Stoke projectile
    /// initializer <c>$86:DB18</c>. Direction zero selects the intentionally odd left branch,
    /// which reads the signed speed from Y velocity while still moving on the X axis.
    /// </summary>
    internal void SpawnStokeProjectile(RoomEnemySlot stoke, StokeDirection direction)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.StokeProjectile,
            unchecked((ushort)(stoke.PaletteIndex | stoke.VramTilesIndex)));
        projectile.DirectionParameter = (ushort)direction;
        projectile.Variable0 = direction == StokeDirection.Left
            ? (ushort)StokeProjectileFunction.MoveLeft
            : (ushort)StokeProjectileFunction.MoveRight;
        projectile.Position = new(new(stoke.Position.X.Whole, stoke.Position.X.Fraction), new(unchecked((ushort)(stoke.Position.Y.Whole + 2)), stoke.Position.Y.Fraction));
        projectile.Velocity = new(SnesSignedEightEight.FromSignedRaw(256), SnesSignedEightEight.FromSignedRaw(-256));
    }

    /// <summary>Ports pre-instruction <c>$86:DB5B</c> and its two indirect movers.</summary>
    internal static void RunStokeProjectilePreInstruction(
        RoomEnemyProjectileSlot projectile,
        Position16<RoomPixels> camera)
    {
        ushort horizontalVelocity = ClosedNativeWords.Decode<StokeProjectileFunction>(checked((ushort)projectile.Variable0), "Stoke projectile movement function") switch
        {
            // This is not a typo: $DB62 reads EnemyProjectile_YVelocity, producing -1 px.
            StokeProjectileFunction.MoveLeft => projectile.Velocity.Y.RawValue,
            StokeProjectileFunction.MoveRight => projectile.Velocity.X.RawValue,
            _ => throw new InvalidDataException(
                $"Stoke projectile movement pointer $86:{projectile.Variable0:X4} is not translated."),
        };

        projectile.Position = projectile.Position.AdvanceX(new SnesSignedEightEight(horizontalVelocity).ToSixteenSixteenDelta());

        // Stoke's $DBB6 helper is another byte-for-byte clone of Cacatac's $DAC2 cull.
        CacatacBehavior.DeleteEnemyProjectileIfOutsideInclusiveViewport(projectile, camera);
    }
}

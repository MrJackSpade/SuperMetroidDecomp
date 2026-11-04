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

    /// <summary>A Stoke projectile's sideways speed, one pixel per update, in signed 8.8.</summary>
    private const int StokeProjectileSpeed = 256;

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

    /// <summary>Initializes the Stoke.</summary>
    internal static void InitializeStoke(RoomEnemySlot slot)
    {
        StokeSpawnSettings settings = slot.SpawnSettings<StokeSpawnSettings>();
        if (settings.SpeedIndex >= EnemyLinearSpeedDefinitions.RowCount)
        {
            throw new InvalidDataException(
                $"Stoke speed index {settings.SpeedIndex} exceeds the NTSC common speed table's " +
                $"{EnemyLinearSpeedDefinitions.RowCount} rows.");
        }

        // The spawn settings name the row of the shared linear speed table Stoke patrols at.
        EnemyLinearSpeedRow speeds = EnemyLinearSpeeds.Row(settings.SpeedIndex);
        var state = new StokeEnemyState
        {
            RightVelocity = speeds.Positive,
            LeftVelocity = speeds.Negative,
            Function = StokeAiFunction.Moving,
            Direction = settings.Direction,
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);

        // Initialization writes the common empty map, then picks the left list by default.
        // A right-moving Stoke takes the right-facing branch. The room-loader tail later
        // restores the transient empty map, and the first instruction tick supplies art.
        slot.Frame = EnemyFrameId.Empty;
        SetStokeProgram(slot, state, StokeProgram.MovingLeft);
        if (settings.Direction == StokeDirection.Right)
        {
            SetStokeProgram(slot, state, StokeProgram.MovingRight);
        }
    }

    /// <summary>Runs the Stoke's main AI.</summary>
    private void RunStokeMain(RoomEnemySlot slot, StokeEnemyState state, RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Stoke walking AI requires the active room level.");

        switch (state.Function)
        {
            case StokeAiFunction.IdleDuringAttack:
                return;

            case StokeAiFunction.Moving:
                // A nonzero spawn direction other than one takes the right-facing branch, so only the left
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

    /// <summary>Runs the shared body of moving left and moving right.</summary>
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
        int attackRoll = WordWrap.LowByte(WordWrap.LowByte(slot.FrameCounter) + SharedRandomState.LowerByte(random));
        if (attackRoll >= 2)
            return;

        // The attack animation owns Stoke until its trailing branch reaches the resume-moving cue and that
        // wrapper's command installs the corresponding moving function again.
        state.Function = StokeAiFunction.IdleDuringAttack;
        SetStokeProgram(slot, state, attackingProgram);
    }

    /// <summary>Moves Stoke and turns it at terrain.</summary>
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

        // The attachment probe is not MoveEnemyDown: it computes a candidate Y coordinate and
        // scans for any level word with bit 15 set without changing Stoke's Y/subposition.
        // Conflating it with vertical movement makes the actor sink two pixels every frame.
        if (!StokeHasSolidFloorTwoPixelsBelow(level, slot))
            TurnStokeAround(slot, state);
    }

    /// <summary>Stoke's use of the vertical solid-block collision test.</summary>
    private static bool StokeHasSolidFloorTwoPixelsBelow(RoomLevelData level, RoomEnemySlot slot)
        => EnemyTerrain.EnemyMeetsAttachmentBlockVerticallyAhead(level, slot, StokeFloorProbeDisplacement);

    /// <summary>Turns Stoke around.</summary>
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
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Animation cues for left and right: resume movement in <paramref name="direction"/>.</summary>
    private static void SetStokeMoving(StokeEnemyState state, StokeDirection direction)
    {
        state.Function = StokeAiFunction.Moving;
        state.Direction = direction;
    }

    /// <summary>Connects the fireball's horizontal motion and viewport cull pre-instruction.</summary>
    internal static void AddStokeProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddFrameBehavior(context =>
        {
            // Stoke shot: horizontal motion and viewport cull.
            RunStokeProjectileFrameBehavior(context.Projectile, context.Camera);
        }, EnemyProjectileFrameBehavior.StokeFireballFlight);
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
    /// Spawns a Stoke projectile and initializes it:
    /// the projectile flies one pixel per update in Stoke's facing.
    /// </summary>
    internal void SpawnStokeProjectile(RoomEnemySlot stoke, StokeDirection direction)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.StokeProjectile,
            SpriteGraphics.SpawnedBy(stoke));
        projectile.Position = new(new(stoke.Position.X.Whole, stoke.Position.X.Fraction), new(CoordinateAxis.Wrap(stoke.Position.Y.Whole + 2), stoke.Position.Y.Fraction));
        projectile.Velocity = new(
            SignedEightEight.FromSignedRaw(direction == StokeDirection.Left ? -StokeProjectileSpeed : StokeProjectileSpeed),
            new SignedEightEight(0));
    }

    /// <summary>Runs the projectile's per-frame behaviour: the projectile flies sideways at its speed until it leaves the view.</summary>
    internal static void RunStokeProjectileFrameBehavior(
        RoomEnemyProjectileSlot projectile,
        Position16<RoomPixels> camera)
    {
        projectile.Position = projectile.Position.AdvanceX(projectile.Velocity.X.ToSixteenSixteenDelta());

        // Stoke's projectile cull is a byte-for-byte clone of Cacatac's.
        CacatacBehavior.DeleteEnemyProjectileIfOutsideInclusiveViewport(projectile, camera);
    }
}

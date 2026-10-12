using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Stoke species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class StokeBehavior : IEnemySpecies
{
    /// <summary>Stores room.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Stores terrain.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Stores projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Initializes a new stoke behavior instance.</summary>
    internal StokeBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyProjectileEngine projectiles)
    {
        _room = room;
        _terrain = terrain;
        _projectiles = projectiles;
    }

    /// <summary>Executes Stoke's private animation instructions.</summary>
    private bool TryProcessStokeInstruction(RoomEnemySlot slot, ushort word, ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Stoke ||
            !Enum.IsDefined((StokeInstruction)word))
            return false;

        switch ((StokeInstruction)word)
        {
            case StokeInstruction.SpawnFireball:
                SpawnStokeProjectile(slot, EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(slot, unchecked((ushort)(cursor + 2))));
                cursor = unchecked((ushort)(cursor + 4));
                return true;
            case StokeInstruction.SetMovingLeft:
                SetStokeMovingLeft(RequireStokeState(slot));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case StokeInstruction.SetMovingRight:
                SetStokeMovingRight(RequireStokeState(slot));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            default:
                throw new InvalidOperationException(
                    $"Stoke does not own instruction ${word:X4}.");
        }
    }


    /// <summary>Defines stoke floor probe displacement.</summary>
    private const int StokeFloorProbeDisplacement = 2 << 16;

    /// <summary>Stores stoke states.</summary>
    internal readonly StokeEnemyState?[] _stokeStates =
        new StokeEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Stoke</c> at <c>$A2:89AD</c>.</summary>
    internal void InitializeStoke(RoomEnemySlot slot)
    {
        if (slot.Parameter2 >= EnemyLinearSpeedDefinitions.RecordCount)
        {
            throw new InvalidDataException(
                $"Stoke speed parameter ${slot.Parameter2:X4} exceeds the NTSC common " +
                $"speed table's ${EnemyLinearSpeedDefinitions.RecordCount - 1:X2} maximum index.");
        }

        // The parameter is an entry number. ASL three times converts it to the byte offset
        // of {positive whole, positive fraction, negative whole, negative fraction}.
        int speedOffset = slot.Parameter2 * EnemyLinearSpeedDefinitions.RecordSize;
        var right = EnemyLinearSpeedDefinitions.Read(speedOffset);
        var left = EnemyLinearSpeedDefinitions.Read(speedOffset + 4);
        var state = new StokeEnemyState(slot)
        {
            RightVelocity = unchecked((ushort)right.Whole),
            RightSubvelocity = right.Fraction,
            LeftVelocity = unchecked((ushort)left.Whole),
            LeftSubvelocity = left.Fraction,
            Function = StokeAiFunction.MovingLeft,
            Direction = (StokeDirection)slot.Parameter1,
        };
        _stokeStates[slot.SlotIndex] = state;

        // Initialization writes the common empty map, then picks the left list by default.
        // A nonzero init0 takes the native right-facing branch. The room-loader tail later
        // restores the transient empty map, and the first instruction tick supplies art.
        slot.SpritemapPointer = 0x804d;
        SetStokeInstructionList(slot, StokeInstructionProgramDefinitions.MovingLeft);
        if (slot.Parameter1 != 0)
        {
            SetStokeInstructionList(slot, StokeInstructionProgramDefinitions.MovingRight);
            state.Function = StokeAiFunction.MovingRight;
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

            case StokeAiFunction.MovingLeft:
                RunMovingStoke(
                    slot,
                    state,
                    level,
                    (SnesSixteenSixteen.FromWhole((int)(short)state.LeftVelocity)) | state.LeftSubvelocity,
                    StokeInstructionProgramDefinitions.AttackingLeft);
                return;

            case StokeAiFunction.MovingRight:
                RunMovingStoke(
                    slot,
                    state,
                    level,
                    (SnesSixteenSixteen.FromWhole((int)(short)state.RightVelocity)) | state.RightSubvelocity,
                    StokeInstructionProgramDefinitions.AttackingRight);
                return;

            default:
                throw new InvalidDataException(
                    $"Stoke main-AI pointer $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports the shared body of moving-left <c>$8A43</c> and moving-right <c>$8A5C</c>.</summary>
    private void RunMovingStoke(
        RoomEnemySlot slot,
        StokeEnemyState state,
        RoomLevelData level,
        int horizontalDisplacement,
        ushort attackingInstructionList)
    {
        MoveStokeAndTurnAtTerrain(slot, state, level, horizontalDisplacement);

        // GenerateRandomNumber advances the global seed before the comparison. Only low
        // byte sums zero and one attack; byte wrapping is intentional and observable.
        ushort random = _room.Random.Generate();
        byte attackRoll = unchecked((byte)((byte)slot.FrameCounter + (byte)random));
        if (attackRoll >= 2)
            return;

        // The attack list owns Stoke until its trailing goto reaches $8932/$8958 and that
        // wrapper's command installs the corresponding moving function again.
        state.Function = StokeAiFunction.IdleDuringAttack;
        SetStokeInstructionList(slot, attackingInstructionList);
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
        SetStokeInstructionList(slot, StokeInstructionProgramDefinitions.MovingLeft);
        if (state.Direction != StokeDirection.Right)
            SetStokeInstructionList(slot, StokeInstructionProgramDefinitions.MovingRight);
    }

    /// <summary>Sets stoke instruction list.</summary>
    private static void SetStokeInstructionList(RoomEnemySlot slot, ushort instructionList)
    {
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    /// <summary>Requires stoke state.</summary>
    private StokeEnemyState RequireStokeState(RoomEnemySlot slot) =>
        _stokeStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Stoke state.");

    /// <summary>Instruction <c>$A2:8990</c>: resume leftward movement.</summary>
    private static void SetStokeMovingLeft(StokeEnemyState state)
    {
        state.Function = StokeAiFunction.MovingLeft;
        state.Direction = StokeDirection.Left;
    }

    /// <summary>Instruction <c>$A2:899D</c>: resume rightward movement.</summary>
    private static void SetStokeMovingRight(StokeEnemyState state)
    {
        state.Function = StokeAiFunction.MovingRight;
        state.Direction = StokeDirection.Right;
    }

    /// <summary>Register stoke projectiles.</summary>
    internal static void RegisterStokeProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            // Stoke shot: horizontal motion and viewport cull.
            RunStokeProjectilePreInstruction(context.Projectile, context.Camera);
        }, EnemyProjectilePreInstruction.StokeFireball);
    }

    /// <summary>Register stoke initialization ai.</summary>
    internal void RegisterStokeInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Stoke, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Stoke, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeStoke(slot);
        });
    }

    /// <summary>Register stoke main ai.</summary>
    internal void RegisterStokeMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Stoke, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Stoke, (slot, frame, nmiFrameCounter8) =>
        {
            RunStokeMain(slot, RequireStokeState(slot), frame.Level);
        });
    }

    /// <summary>Register stoke owned instructions.</summary>
    internal void RegisterStokeOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessStokeInstruction(slot, word, ref cursor);
        });
    }
    /// <summary>
    /// Ports <c>SpawnEnemyProjectileY_ParameterA_XGraphics</c> followed by Stoke projectile
    /// initializer <c>$86:DB18</c>. Direction zero selects the intentionally odd left branch,
    /// which reads the signed speed from Y velocity while still moving on the X axis.
    /// </summary>
    internal void SpawnStokeProjectile(RoomEnemySlot stoke, ushort direction)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.StokeProjectile,
            unchecked((ushort)(stoke.PaletteIndex | stoke.VramTilesIndex)));
        projectile.DirectionParameter = direction;
        projectile.Variable0 = direction == 0
            ? (ushort)StokeProjectileFunction.MoveLeft
            : (ushort)StokeProjectileFunction.MoveRight;
        projectile.Position = new(new(stoke.Position.X.Whole, stoke.Position.X.Fraction), new(unchecked((ushort)(stoke.Position.Y.Whole + 2)), stoke.Position.Y.Fraction));
        projectile.Velocity = new(new SnesSignedEightEight(0x0100), new SnesSignedEightEight(0xff00));
    }

    /// <summary>Ports pre-instruction <c>$86:DB5B</c> and its two indirect movers.</summary>
    internal static void RunStokeProjectilePreInstruction(
        RoomEnemyProjectileSlot projectile,
        Position16<RoomPixels> camera)
    {
        ushort horizontalVelocity = ClosedNativeWords.Decode<StokeProjectileFunction>(projectile.Variable0, "Stoke projectile movement function") switch
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

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        Array.Clear(_stokeStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}

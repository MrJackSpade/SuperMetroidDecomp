using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The walking space pirate species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class WalkingSpacePirateBehavior : IEnemySpecies
{
    /// <summary>Stores terrain.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Stores projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Initializes a new walking space pirate behavior instance.</summary>
    internal WalkingSpacePirateBehavior(EnemyTerrain terrain, EnemyProjectileEngine projectiles)
    {
        _terrain = terrain;
        _projectiles = projectiles;
    }

    /// <summary>Executes the walking Space Pirates' private animation instructions.</summary>
    private bool TryProcessWalkingSpacePirateInstruction(RoomEnemySlot slot, SamusState? samus, ushort word, ref ushort cursor)
    {
        if (!IsWalkingSpacePirateDefinition(slot.EnemyDefinitionPointer) ||
            (SpacePirateInstruction)word is not (
                SpacePirateInstruction.PirateWalking_FunctionInY or
                SpacePirateInstruction.PirateWalking_FireLaserLeftWithYOffsetInY or
                SpacePirateInstruction.PirateWalking_FireLaserRightWithYOffsetInY or
                SpacePirateInstruction.PirateWalking_ChooseAMovement))
            return false;

        switch ((SpacePirateInstruction)word)
        {
            case SpacePirateInstruction.PirateWalking_FunctionInY:
                // Pirate bytecode does not jump to the operand. It stores that bank-$B2
                // function address in native variable A for main AI to dispatch next
                // frame, then resumes immediately after the two-byte operand.
                RequireWalkingSpacePirateState(slot).Function =
                    (WalkingSpacePirateFunction)EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(slot, unchecked((ushort)(cursor + 2)));
                cursor = unchecked((ushort)(cursor + 4));
                return true;
            case SpacePirateInstruction.PirateWalking_FireLaserLeftWithYOffsetInY:
                SpawnWalkingSpacePirateLaser(
                    slot,
                    RequireWalkingSpacePirateState(slot),
                    movingRight: false,
                    EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(slot, unchecked((ushort)(cursor + 2))));
                cursor = unchecked((ushort)(cursor + 4));
                return true;
            case SpacePirateInstruction.PirateWalking_FireLaserRightWithYOffsetInY:
                SpawnWalkingSpacePirateLaser(
                    slot,
                    RequireWalkingSpacePirateState(slot),
                    movingRight: true,
                    EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(slot, unchecked((ushort)(cursor + 2))));
                cursor = unchecked((ushort)(cursor + 4));
                return true;
            case SpacePirateInstruction.PirateWalking_ChooseAMovement:
                // Unlike common goto, this opcode returns a direct instruction-list
                // pointer chosen from Samus's current side and vertical proximity.
                cursor = SelectWalkingSpacePirateMovement(slot, samus);
                return true;
            default:
                throw new InvalidOperationException(
                    $"Walking Space Pirate does not own instruction ${word:X4}.");
        }
    }


    /// <summary>Defines pirate mother brain laser sound.</summary>
    internal const ushort PirateMotherBrainLaserSound = 0x0067;
    /// <summary>Defines walking pirate one pixel down.</summary>
    private const int WalkingPirateOnePixelDown = 1 << 16;
    /// <summary>Defines walking pirate left ledge probe pixels.</summary>
    private const int WalkingPirateLeftLedgeProbePixels = 17;
    /// <summary>Defines walking pirate right ledge probe pixels.</summary>
    private const int WalkingPirateRightLedgeProbePixels = 16;
    /// <summary>Defines walking pirate left step displacement.</summary>
    private const int WalkingPirateLeftStepDisplacement = -0x3801;
    /// <summary>Defines walking pirate right step displacement.</summary>
    private const int WalkingPirateRightStepDisplacement = 0x3800;
    /// <summary>Defines walking pirate firing band pixels.</summary>
    private const int WalkingPirateFiringBandPixels = 16;
    /// <summary>Defines walking pirate flinch box pixels.</summary>
    private const int WalkingPirateFlinchBoxPixels = 32;
    /// <summary>Defines walking pirate laser muzzle xoffset.</summary>
    private const int WalkingPirateLaserMuzzleXOffset = 24;
    /// <summary>Defines walking pirate slow laser pixels per frame.</summary>
    private const int WalkingPirateSlowLaserPixelsPerFrame = 2;
    /// <summary>Defines walking pirate fast laser pixels per frame.</summary>
    private const int WalkingPirateFastLaserPixelsPerFrame = 4;

    // The six palette/health tiers share the same initialization, main AI, collision AI,
    // dimensions, animation lists, and projectile. Definition identity must still remain
    // explicit because damage, health, vulnerability, drops, tiles, and palette are ROM data.
    /// <summary>Stores walking space pirate states.</summary>
    internal readonly WalkingSpacePirateEnemyState?[] _walkingSpacePirateStates =
        new WalkingSpacePirateEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>
    /// Last library-two sound requested by the shared Pirate/Mother-Brain laser initializer
    /// or a Space Pirate instruction during this frame.
    /// </summary>
    public ushort? LastSpacePirateSoundEffect { get; internal set; }

    /// <summary>Checks whether walking space pirate definition.</summary>
    internal static bool IsWalkingSpacePirateDefinition(EnemyDefinitionId definition) => definition is
        EnemyDefinitionId.PirateGreyWalking or
        EnemyDefinitionId.PirateGreenWalking or
        EnemyDefinitionId.PirateRedWalking or
        EnemyDefinitionId.PirateGoldWalking or
        EnemyDefinitionId.PirateMagentaWalking or
        EnemyDefinitionId.PirateSilverWalking;

    /// <summary>Ports <c>InitAI_PirateWalking</c> at <c>$B2:FD02</c>.</summary>
    internal void InitializeWalkingSpacePirate(RoomEnemySlot slot)
    {
        var state = new WalkingSpacePirateEnemyState(slot);
        _walkingSpacePirateStates[slot.SlotIndex] = state;

        // Parameter-one bit zero is the only initial-facing selector. Bit fifteen is an
        // independent projectile-flinch option and must not accidentally select right art.
        var parameterFlags = (WalkingSpacePirateParameterFlags)slot.Parameter1;
        slot.CurrentInstruction =
            (parameterFlags & WalkingSpacePirateParameterFlags.StartsFacingRight) != 0
            ? WalkingSpacePirateInstructionProgramDefinitions.WalkingRight
            : WalkingSpacePirateInstructionProgramDefinitions.WalkingLeft;
        state.Function = WalkingSpacePirateFunction.NoOperation;

        // Native ADC/SBC wrap at 16 bits. A modded patrol radius can therefore straddle the
        // world-coordinate seam; keeping ushort arithmetic preserves its signed CMP behavior.
        state.RightPostXPosition = unchecked((ushort)(slot.Position.X.Whole + slot.Parameter2));
        state.LeftPostXPosition = unchecked((ushort)(slot.Position.X.Whole - slot.Parameter2));
    }

    /// <summary>Ports <c>MainAI_PirateWalking</c> at <c>$B2:FD32</c>.</summary>
    private void RunWalkingSpacePirateMain(
        RoomEnemySlot slot,
        WalkingSpacePirateEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        SamusProjectileSystem? samusProjectiles)
    {
        switch (state.Function)
        {
            case WalkingSpacePirateFunction.NoOperation:
            case WalkingSpacePirateFunction.AnimationOwnedNoOperation:
                break;

            case WalkingSpacePirateFunction.WalkingLeft:
                RunWalkingSpacePirateLeft(slot, state, RequireSamus(), RequireLevel());
                break;

            case WalkingSpacePirateFunction.WalkingRight:
                RunWalkingSpacePirateRight(slot, state, RequireSamus(), RequireLevel());
                break;

            default:
                throw new InvalidDataException(
                    $"Walking Space Pirate function $B2:{(ushort)state.Function:X4} is not translated.");
        }

        // Parameter-one bit fifteen enables the retail flinch detector after normal AI.
        // The detector is intentionally allowed to replace a list selected just above.
        if ((((WalkingSpacePirateParameterFlags)slot.Parameter1 &
                WalkingSpacePirateParameterFlags.SlowLaserAndProjectileFlinch) != 0) &&
            samus is not null)
            TryTriggerWalkingSpacePirateFlinch(slot, samus, samusProjectiles);

        SamusState RequireSamus() => samus ?? throw new InvalidOperationException(
            "Walking Space Pirate movement requires the active Samus position.");
        RoomLevelData RequireLevel() => level ?? throw new InvalidOperationException(
            "Walking Space Pirate movement requires room collision data.");
    }

    /// <summary>Ports the asymmetric left-walk function at <c>$B2:FD44</c>.</summary>
    private void RunWalkingSpacePirateLeft(
        RoomEnemySlot slot,
        WalkingSpacePirateEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (WalkingSpacePirateIsVerticallyClose(slot, samus))
        {
            InstallWalkingSpacePirateAttack(slot, samus);
            return;
        }

        // The first one-pixel downward move doubles as the grounded test. Carry clear in
        // the ROM means there was no floor, and the move itself is retained as falling.
        if (!_terrain.MoveEnemyVertically(level, slot, WalkingPirateOnePixelDown))
            return;

        // Left and right are not mirrors in the cartridge. Left probes a point 17 pixels
        // away, then walks by -$3801 subpixels; right probes +16 and walks by +$3800.
        // Preserve that one-subpixel/one-pixel asymmetry instead of normalizing the speed.
        ushort originalX = slot.Position.X.Whole;
        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(
            slot.Position.X.Whole - WalkingPirateLeftLedgeProbePixels)));
        bool floorAhead = _terrain.MoveEnemyVertically(level, slot, WalkingPirateOnePixelDown);
        slot.Position = slot.Position.WithWholeX(originalX);

        if (floorAhead)
        {
            // The ignored `$A0:BBBF` probe checks raw level-word bit 15 at -9 pixels. Its
            // only mutable result is the DP displacement pair, which the following move
            // immediately overwrites. It therefore has no persistent actor-side effect.
            bool hitWall = _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                displacement: WalkingPirateLeftStepDisplacement);
            if (!hitWall &&
                unchecked((short)(slot.Position.X.Whole - state.LeftPostXPosition)) >= 0)
            {
                return;
            }
        }

        InstallWalkingSpacePirateInstruction(
            slot,
            WalkingSpacePirateInstructionProgramDefinitions.LookingFacingLeft);
    }

    /// <summary>Ports the right-walk function at <c>$B2:FDCE</c>.</summary>
    private void RunWalkingSpacePirateRight(
        RoomEnemySlot slot,
        WalkingSpacePirateEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (WalkingSpacePirateIsVerticallyClose(slot, samus))
        {
            InstallWalkingSpacePirateAttack(slot, samus);
            return;
        }

        if (!_terrain.MoveEnemyVertically(level, slot, WalkingPirateOnePixelDown))
            return;

        ushort originalX = slot.Position.X.Whole;
        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(
            slot.Position.X.Whole + WalkingPirateRightLedgeProbePixels)));
        bool floorAhead = _terrain.MoveEnemyVertically(level, slot, WalkingPirateOnePixelDown);
        slot.Position = slot.Position.WithWholeX(originalX);

        if (floorAhead &&
            !_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                displacement: WalkingPirateRightStepDisplacement) &&
            unchecked((short)(slot.Position.X.Whole - state.RightPostXPosition)) < 0)
        {
            return;
        }

        InstallWalkingSpacePirateInstruction(
            slot,
            WalkingSpacePirateInstructionProgramDefinitions.LookingFacingRight);
    }

    /// <summary>Ports <c>PirateWalking_FlinchTrigger</c> at <c>$B2:FE4B</c>.</summary>
    private static void TryTriggerWalkingSpacePirateFlinch(
        RoomEnemySlot slot,
        SamusState samus,
        SamusProjectileSystem? projectiles)
    {
        if (projectiles is null)
            return;

        SamusProjectileSlot? firstAllocated = null;
        for (int index = SamusProjectileSystem.SlotCount - 1; index >= 0; index--)
        {
            // Native code tests projectile_type, not the instruction pointer. More
            // importantly, it stops at the first nonzero high-index slot even when that
            // shot is far away; it never searches lower slots for a closer projectile.
            if (projectiles.Slots[index].Type != 0)
            {
                firstAllocated = projectiles.Slots[index];
                break;
            }
        }
        if (firstAllocated is null ||
            Math.Abs(unchecked((short)(firstAllocated.Position.X.Whole - slot.Position.X.Whole))) >=
                WalkingPirateFlinchBoxPixels ||
            Math.Abs(unchecked((short)(firstAllocated.Position.Y.Whole - slot.Position.Y.Whole))) >=
                WalkingPirateFlinchBoxPixels)
        {
            return;
        }

        ushort list = unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)) < 0
            ? WalkingSpacePirateInstructionProgramDefinitions.FlinchFacingRight
            : WalkingSpacePirateInstructionProgramDefinitions.FlinchFacingLeft;
        InstallWalkingSpacePirateInstruction(slot, list);
    }

    /// <summary>Ports instruction <c>$B2:FCC8</c>'s direct-list selection.</summary>
    private static ushort SelectWalkingSpacePirateMovement(
        RoomEnemySlot slot,
        SamusState? samus)
    {
        if (samus is null)
        {
            throw new InvalidOperationException(
                "Walking Space Pirate animation selection requires the active Samus position.");
        }

        bool samusIsRightOrAligned =
            unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) >= 0;
        if (WalkingSpacePirateIsVerticallyClose(slot, samus))
        {
            return samusIsRightOrAligned
                ? WalkingSpacePirateInstructionProgramDefinitions.FireLasersRight
                : WalkingSpacePirateInstructionProgramDefinitions.FireLasersLeft;
        }

        // When Samus is not in the firing band, the Pirate walks toward the opposite side:
        // Samus right selects leftward walking, Samus left selects rightward walking.
        return samusIsRightOrAligned
            ? WalkingSpacePirateInstructionProgramDefinitions.WalkingLeft
            : WalkingSpacePirateInstructionProgramDefinitions.WalkingRight;
    }

    /// <summary>Walking space pirate is vertically close.</summary>
    private static bool WalkingSpacePirateIsVerticallyClose(
        RoomEnemySlot slot,
        SamusState samus) =>
        Math.Abs(unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole))) <
            WalkingPirateFiringBandPixels;

    /// <summary>Install walking space pirate attack.</summary>
    private static void InstallWalkingSpacePirateAttack(
        RoomEnemySlot slot,
        SamusState samus) =>
        InstallWalkingSpacePirateInstruction(
            slot,
            unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) >= 0
                ? WalkingSpacePirateInstructionProgramDefinitions.FireLasersRight
                : WalkingSpacePirateInstructionProgramDefinitions.FireLasersLeft);

    /// <summary>Install walking space pirate instruction.</summary>
    private static void InstallWalkingSpacePirateInstruction(
        RoomEnemySlot slot,
        ushort instructionPointer)
    {
        slot.CurrentInstruction = instructionPointer;
        slot.InstructionTimer = 1;
    }

    /// <summary>Ports firing opcodes <c>$B2:FC68/$FC90</c> and initializer $86:A009.</summary>
    private void SpawnWalkingSpacePirateLaser(
        RoomEnemySlot source,
        WalkingSpacePirateEnemyState state,
        bool movingRight,
        ushort yOffset)
    {
        if (!SpawnSpacePirateLaser(source, movingRight, yOffset))
            return;

        state.SpawnedLaserCount++;
    }

    /// <summary>
    /// Ports the common projectile initializer <c>$86:A009</c>. Wall and walking Pirates
    /// use different actor opcodes and muzzle offsets, but the allocated projectile is the
    /// same retail definition and therefore belongs in one shared implementation.
    /// </summary>
    internal bool SpawnSpacePirateLaser(
        RoomEnemySlot source,
        bool movingRight,
        ushort yOffset)
    {
        RoomEnemyProjectileSlot? projectile = SpawnPirateMotherBrainLaser(
            source,
            new(
                unchecked((ushort)(
                    source.Position.X.Whole +
                    (movingRight ? WalkingPirateLaserMuzzleXOffset : -WalkingPirateLaserMuzzleXOffset))),
                unchecked((ushort)(source.Position.Y.Whole - yOffset))),
            movingRight);
        if (projectile is null)
            return false;

        LastSpacePirateSoundEffect = PirateMotherBrainLaserSound;
        return true;
    }

    /// <summary>
    /// Ports the common projectile initializer <c>$86:A009</c> after the caller has chosen
    /// its muzzle coordinate. Walking Pirates, wall Pirates, and Mother Brain enter through
    /// different actor opcodes, but the cartridge deliberately funnels all of them through
    /// this definition so animation, speed flags, collision damage, and disposal agree.
    /// </summary>
    internal RoomEnemyProjectileSlot? SpawnPirateMotherBrainLaser(
        RoomEnemySlot source,
        Position16<RoomPixels> muzzle,
        bool movingRight)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return null;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.PirateMotherBrainLaser,
            graphicsIndex: 0);

        projectile.Position = FixedPosition<RoomPixels>.FromWhole(muzzle);
        projectile.InstructionPointer = movingRight
            ? SpacePirateProjectileInstructionProgramDefinitions.LaserRight
            : SpacePirateProjectileInstructionProgramDefinitions.LaserLeft;
        projectile.InstructionTimer = 1;

        // Although the definition names A05C as its pre-instruction, initializer A009
        // deliberately replaces it with A05B. The three two-frame muzzle maps are thus
        // stationary until list opcode A050 installs and immediately executes movement.
        projectile.PreInstruction = (ushort)EnemyProjectilePreInstruction.RTS_86A05B;
        projectile.Variable0 = source.Parameter1;

        // A009 replaces the projectile definition's property word with enemy damage OR
        // $1000. The source palette/VRAM are *not* copied: SpawnEprojWithRoomGfx passes a
        // graphics index of zero so the laser uses the room projectile tiles/palette.
        projectile.Damage = source.Definition.Damage;
        projectile.CanDamageSamus = true;
        projectile.PersistsOnSamusContact = false;
        projectile.BlocksSamusProjectiles = false;
        projectile.DirectionParameter = movingRight ? (ushort)1 : (ushort)0;
        return projectile;
    }

    /// <summary>The two bank-$86 Space Pirate/Mother Brain laser movement callbacks.</summary>
    private enum PirateLaserPreInstruction : ushort
    {
        /// <summary>$86:A05C: move left, then camera cull.</summary>
        Left = (ushort)EnemyProjectilePreInstruction.Pirate_MotherBrain_Laser_Left,
        /// <summary>$86:A07A: move right, then camera cull.</summary>
        Right = (ushort)EnemyProjectilePreInstruction.Pirate_MotherBrain_Laser_Right,
    }

    /// <summary>Runs pre-instructions $86:A05C/$A07A and exact 256x256 camera deletion.</summary>
    internal static void RunPirateMotherBrainLaserPreInstruction(
        RoomEnemyProjectileSlot projectile,
        Position16<RoomPixels> camera)
    {
        int pixels = (((WalkingSpacePirateParameterFlags)projectile.Variable0 &
                WalkingSpacePirateParameterFlags.SlowLaserAndProjectileFlinch) == 0)
            ? WalkingPirateFastLaserPixelsPerFrame
            : WalkingPirateSlowLaserPixelsPerFrame;
        PirateLaserPreInstruction direction = ClosedNativeWords.Decode<PirateLaserPreInstruction>(
            projectile.PreInstruction, $"{projectile.Kind} laser pre-instruction");
        projectile.Position = projectile.Position.WithWholeX(direction switch
        {
            PirateLaserPreInstruction.Left => unchecked((ushort)(projectile.Position.X.Whole - pixels)),
            PirateLaserPreInstruction.Right => unchecked((ushort)(projectile.Position.X.Whole + pixels)),
            _ => throw new InvalidOperationException($"Undefined laser pre-instruction {direction}."),
        });

        ushort right = unchecked((ushort)(camera.X + 256));
        ushort bottom = unchecked((ushort)(camera.Y + 256));
        bool offScreen = unchecked((short)(projectile.Position.X.Whole - camera.X)) < 0 ||
            unchecked((short)(projectile.Position.X.Whole - right)) >= 0 ||
            unchecked((short)(projectile.Position.Y.Whole - camera.Y)) < 0 ||
            unchecked((short)(projectile.Position.Y.Whole - bottom)) >= 0;
        if (offScreen)
            projectile.Clear();
    }

    /// <summary>Requires walking space pirate state.</summary>
    private WalkingSpacePirateEnemyState RequireWalkingSpacePirateState(
        RoomEnemySlot slot) =>
        _walkingSpacePirateStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized walking Space Pirate state.");

    /// <summary>Register walking space pirate projectiles.</summary>
    internal static void RegisterWalkingSpacePirateProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunPirateMotherBrainLaserPreInstruction(
                context.Projectile,
                context.Camera);
        }, EnemyProjectilePreInstruction.Pirate_MotherBrain_Laser_Left, EnemyProjectilePreInstruction.Pirate_MotherBrain_Laser_Right);

        table.RegisterInstruction(EnemyProjectileInstruction.PreInstructionInY_ExecuteY, null,
            (RoomEnemyProjectileSlot projectile, SamusState? samus, Position16<RoomPixels> camera, ushort word, ref ushort cursor) =>
            {
                projectile.PreInstruction = EnemyProjectileEngine.ReadEnemyProjectileInstructionMechanicsWord(
                    projectile,
                    unchecked((ushort)(cursor + 2)));

                // A050 returns the argument cursor unchanged. The common interpreter
                // consequently sees A05C/A07A as the next instruction, dispatches that
                // movement routine once immediately, then resumes after the operand.
                // Merely installing the pointer would leave every laser four/two pixels
                // behind the cartridge for its entire lifetime.
                RunPirateMotherBrainLaserPreInstruction(projectile, camera);
                if (!projectile.IsActive)
                    return EnemyProjectileInstructionOutcome.Stop;
                cursor = unchecked((ushort)(cursor + 4));
                return EnemyProjectileInstructionOutcome.Continue;
            });
    }

    /// <summary>Register walking space pirate initialization ai.</summary>
    internal void RegisterWalkingSpacePirateInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_PirateWalking, slot => IsWalkingSpacePirateDefinition(slot.EnemyDefinitionPointer), (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeWalkingSpacePirate(slot);
        });
    }

    /// <summary>Register walking space pirate main ai.</summary>
    internal void RegisterWalkingSpacePirateMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_PirateWalking, slot => IsWalkingSpacePirateDefinition(slot.EnemyDefinitionPointer), (slot, frame, nmiFrameCounter8) =>
        {
            RunWalkingSpacePirateMain(
                slot,
                RequireWalkingSpacePirateState(slot),
                frame.Samus,
                frame.Level,
                frame.SamusProjectiles);
        });
    }

    /// <summary>Register walking space pirate owned instructions.</summary>
    internal void RegisterWalkingSpacePirateOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessWalkingSpacePirateInstruction(slot, context.Samus, word, ref cursor);
        });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        Array.Clear(_walkingSpacePirateStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastSpacePirateSoundEffect = null;
    }

    /// <summary>Resolves space pirate hitbox shot.</summary>
    internal bool ResolveSpacePirateHitboxShot(in EnemyShotContext c)
    {
        if (!c.UsesExtendedHitboxes)
            return false;
        EnemyCollision.PirateHitboxShotAction pirateAction = EnemyCollision.SelectPirateHitboxShotAction(c.Enemy, c.Projectile, c.HitboxShotAiPointer);
        if (pirateAction != EnemyCollision.PirateHitboxShotAction.Normal)
        {
            // `$A0:9CD1-$9CD6` marks a non-plasma projectile as having entered an extended hitbox before the
            // selected callback runs. Normal hits get the equivalent lifecycle transition from TryStartEnemyImpact;
            // rejected and reflected hits return before that shared host path. Enemy property bit $1000 also forces
            // the mark in native code, but its broader meaning is not yet proven, so it deliberately remains a
            // documented raw bit rather than a prematurely named enum member.
            c.Projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
        }
        if (pirateAction == EnemyCollision.PirateHitboxShotAction.Ignore)
        {
            // `$B2:883E` deliberately returns for a just-fired Super Missile whose link variable is still zero.
            // The multibox collision walk nevertheless exits after invoking that callback.
            return true;
        }
        if (pirateAction == EnemyCollision.PirateHitboxShotAction.Reflect)
        {
            c.Enemy.InvincibilityTimer = 10;
            c.Projectile.Direction = (c.Projectile.Direction & 0x000f) switch
            {
                (ushort)SamusProjectileDirection.Left => (ushort)SamusProjectileDirection.UpRight,
                (ushort)SamusProjectileDirection.Right => (ushort)SamusProjectileDirection.UpLeft,
                _ => (ushort)SamusProjectileDirection.DownFacingLeft,
            };
            c.Projectiles.EnemyImpacts.ReflectFromEnemy(c.Bus, c.Projectile.SlotIndex);
            LastSpacePirateSoundEffect = 0x0066;
            return true;
        }
        return false;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastSpacePirateSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }
}

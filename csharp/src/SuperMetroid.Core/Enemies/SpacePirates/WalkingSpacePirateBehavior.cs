using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The walking space pirate species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class WalkingSpacePirateBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.PirateGoldWalking, EnemyDefinitionId.PirateGreenWalking, EnemyDefinitionId.PirateGreyWalking, EnemyDefinitionId.PirateMagentaWalking, EnemyDefinitionId.PirateRedWalking, EnemyDefinitionId.PirateSilverWalking];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new WalkingSpacePirateEntity(this, slot, common);

    /// <summary>Supplies solid-ground and ledge probes for the patrol AI.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Allocates the shared Pirate/Mother Brain laser projectile.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Binds the patrol collision owner and laser pool.</summary>
    internal WalkingSpacePirateBehavior(EnemyTerrain terrain, EnemyProjectileEngine projectiles)
    {
        _terrain = terrain;
        _projectiles = projectiles;
    }

    /// <summary>
    /// Carries out one cue of the walking Space Pirates' animations. Choosing a movement continues at the walk or laser
    /// animation it selects, as the native opcode returns that list; every other cue runs on.
    /// </summary>
    private WalkingSpacePirateProgram? ApplyWalkingSpacePirateCue(RoomEnemySlot slot, WalkingSpacePirateCue cue, SamusState? samus)
    {
        switch (cue.Kind)
        {
            case WalkingSpacePirateCueKind.SetFunction:
                // Pirate bytecode does not jump to the operand. It stores that bank-$B2 function
                // for main AI to dispatch next frame, then runs on.
                RequireWalkingSpacePirateState(slot).Function = cue.Function;
                return null;
            case WalkingSpacePirateCueKind.FireLaserLeft:
                SpawnWalkingSpacePirateLaser(slot, RequireWalkingSpacePirateState(slot), movingRight: false, cue.Value);
                return null;
            case WalkingSpacePirateCueKind.FireLaserRight:
                SpawnWalkingSpacePirateLaser(slot, RequireWalkingSpacePirateState(slot), movingRight: true, cue.Value);
                return null;
            case WalkingSpacePirateCueKind.ChooseMovement:
                return SelectWalkingSpacePirateMovement(slot, samus);
            default:
                throw new InvalidOperationException($"The walking Space Pirates' animations have no {cue.Kind} cue.");
        }
    }

    /// <summary>Fixed-point downward displacement used for the ground probe.</summary>
    private const int WalkingPirateOnePixelDown = 1 << 16;
    /// <summary>Left patrol edge is probed one pixel farther than the right edge.</summary>
    private const int WalkingPirateLeftLedgeProbePixels = 17;
    /// <summary>Right patrol ledge probe distance in room pixels.</summary>
    private const int WalkingPirateRightLedgeProbePixels = 16;
    /// <summary>Native signed fixed-point displacement for a left patrol step.</summary>
    private const int WalkingPirateLeftStepDisplacement = -14337;
    /// <summary>Native signed fixed-point displacement for a right patrol step.</summary>
    private const int WalkingPirateRightStepDisplacement = 14336;
    /// <summary>Maximum vertical separation for choosing the laser attack.</summary>
    private const int WalkingPirateFiringBandPixels = 16;
    /// <summary>Horizontal collision window used by the pirate's projectile flinch path.</summary>
    private const int WalkingPirateFlinchBoxPixels = 32;
    /// <summary>Horizontal distance from the enemy center to the laser muzzle.</summary>
    private const int WalkingPirateLaserMuzzleXOffset = 24;
    /// <summary>Slow laser's native whole-pixel movement per frame.</summary>
    private const int WalkingPirateSlowLaserPixelsPerFrame = 2;
    /// <summary>Fast laser's native whole-pixel movement per frame.</summary>
    private const int WalkingPirateFastLaserPixelsPerFrame = 4;

    /// <summary>
    /// Last library-two sound requested by the shared Pirate/Mother-Brain laser initializer
    /// or a Space Pirate instruction during this frame.
    /// </summary>
    public ushort? LastSpacePirateSoundEffect { get; internal set; }

    /// <summary>Recognizes the six walking variants that share patrol AI and laser behavior.</summary>
    internal static bool IsWalkingSpacePirateDefinition(EnemyDefinitionId definition) => definition is
        EnemyDefinitionId.PirateGreyWalking or
        EnemyDefinitionId.PirateGreenWalking or
        EnemyDefinitionId.PirateRedWalking or
        EnemyDefinitionId.PirateGoldWalking or
        EnemyDefinitionId.PirateMagentaWalking or
        EnemyDefinitionId.PirateSilverWalking;

    /// <summary>Ports <c>InitAI_PirateWalking</c> at <c>$B2:FD02</c>.</summary>
    internal static void InitializeWalkingSpacePirate(RoomEnemySlot slot)
    {
        var state = new WalkingSpacePirateEnemyState(slot);
        slot.SetSpeciesState(state);

        // Parameter-one bit zero is the only initial-facing selector. Bit fifteen is an
        // independent projectile-flinch option and must not accidentally select right art.
        var parameterFlags = (WalkingSpacePirateParameterFlags)slot.Parameter1;
        state.Program = AnimationProgramCursor<WalkingSpacePirateProgram>.Start(
            (parameterFlags & WalkingSpacePirateParameterFlags.StartsFacingRight) != 0
            ? WalkingSpacePirateProgram.WalkingRight
            : WalkingSpacePirateProgram.WalkingLeft);
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
            TryTriggerWalkingSpacePirateFlinch(slot, state, samus, samusProjectiles);

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
            InstallWalkingSpacePirateAttack(slot, state, samus);
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
            state,
            WalkingSpacePirateProgram.LookingFacingLeft);
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
            InstallWalkingSpacePirateAttack(slot, state, samus);
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
            state,
            WalkingSpacePirateProgram.LookingFacingRight);
    }

    /// <summary>Ports <c>PirateWalking_FlinchTrigger</c> at <c>$B2:FE4B</c>.</summary>
    private static void TryTriggerWalkingSpacePirateFlinch(
        RoomEnemySlot slot,
        WalkingSpacePirateEnemyState state,
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

        WalkingSpacePirateProgram list = unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)) < 0
            ? WalkingSpacePirateProgram.FlinchFacingRight
            : WalkingSpacePirateProgram.FlinchFacingLeft;
        InstallWalkingSpacePirateInstruction(slot, state, list);
    }

    /// <summary>Ports instruction <c>$B2:FCC8</c>'s direct-list selection.</summary>
    private static WalkingSpacePirateProgram SelectWalkingSpacePirateMovement(
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
                ? WalkingSpacePirateProgram.FireLasersRight
                : WalkingSpacePirateProgram.FireLasersLeft;
        }

        // When Samus is not in the firing band, the Pirate walks toward the opposite side:
        // Samus right selects leftward walking, Samus left selects rightward walking.
        return samusIsRightOrAligned
            ? WalkingSpacePirateProgram.WalkingLeft
            : WalkingSpacePirateProgram.WalkingRight;
    }

    /// <summary>Tests signed vertical separation against the native sixteen-pixel firing band.</summary>
    private static bool WalkingSpacePirateIsVerticallyClose(
        RoomEnemySlot slot,
        SamusState samus) =>
        Math.Abs(unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole))) <
            WalkingPirateFiringBandPixels;

    /// <summary>Chooses a left or right laser instruction list from Samus's current side.</summary>
    private static void InstallWalkingSpacePirateAttack(
        RoomEnemySlot slot,
        WalkingSpacePirateEnemyState state,
        SamusState samus) =>
        InstallWalkingSpacePirateInstruction(
            slot,
            state,
            unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) >= 0
                ? WalkingSpacePirateProgram.FireLasersRight
                : WalkingSpacePirateProgram.FireLasersLeft);

    /// <summary>Arms a newly selected instruction list for processing on the next tick.</summary>
    private static void InstallWalkingSpacePirateInstruction(
        RoomEnemySlot slot,
        WalkingSpacePirateEnemyState state,
        WalkingSpacePirateProgram program)
    {
        state.Program = AnimationProgramCursor<WalkingSpacePirateProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
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

        LastSpacePirateSoundEffect = EnemySoundEffectDefinitions.PirateMotherBrainLaserSound;
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
        projectile.InstructionTimer = NativeCountdown.Start(1);

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

    /// <summary>Requires the physical slot to have been initialized as a walking pirate.</summary>
    internal static WalkingSpacePirateEnemyState RequireWalkingSpacePirateState(
        RoomEnemySlot slot) =>
        slot.SpeciesState<WalkingSpacePirateEnemyState>() ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized walking Space Pirate state.");

    /// <summary>Registers laser movement and the opcode that immediately runs its installed pre-instruction.</summary>
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

    /// <summary>Ports WalkingSpacePirate's initialization AI.</summary>
    internal static void RunWalkingSpacePirateInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeWalkingSpacePirate(slot);
    }

    /// <summary>Ports WalkingSpacePirate's main AI.</summary>
    internal void RunWalkingSpacePirateMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunWalkingSpacePirateMain(
            slot,
            RequireWalkingSpacePirateState(slot),
            frame.Samus,
            frame.Level,
            frame.SamusProjectiles);
    }

    /// <summary>Advances WalkingSpacePirate's typed animation.</summary>
    internal void AdvanceWalkingSpacePirateTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        var state = RequireWalkingSpacePirateState(slot);
        AnimationProgramCursor<WalkingSpacePirateProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, WalkingSpacePirateAnimations.Programs, ref cursor, WalkingSpacePirateFrameDefinitions.Spritemap,
                (cue, _) => ApplyWalkingSpacePirateCue(slot, cue, context.Samus));
        state.Program = cursor;
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastSpacePirateSoundEffect = null;
    }

    /// <summary>Runs the extended-hitbox shot callback, including native collision marking before rejection or reflection.</summary>
    internal bool ResolveSpacePirateHitboxShot(in EnemyShotContext c)
    {
        if (!c.UsesExtendedHitboxes)
            return false;
        PirateHitboxShotActions.PirateHitboxShotAction pirateAction = PirateHitboxShotActions.SelectPirateHitboxShotAction(c.Enemy, c.Projectile, c.HitboxShotAiPointer);
        if (pirateAction != PirateHitboxShotActions.PirateHitboxShotAction.Normal)
        {
            // `$A0:9CD1-$9CD6` marks a non-plasma projectile as having entered an extended hitbox before the
            // selected callback runs. Normal hits get the equivalent lifecycle transition from TryStartEnemyImpact;
            // rejected and reflected hits return before that shared host path. Enemy property bit $1000 also forces
            // the mark in native code, but its broader meaning is not yet proven, so it deliberately remains a
            // documented raw bit rather than a prematurely named enum member.
            c.Projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
        }
        if (pirateAction == PirateHitboxShotActions.PirateHitboxShotAction.Ignore)
        {
            // `$B2:883E` deliberately returns for a just-fired Super Missile whose link variable is still zero.
            // The multibox collision walk nevertheless exits after invoking that callback.
            return true;
        }
        if (pirateAction == PirateHitboxShotActions.PirateHitboxShotAction.Reflect)
        {
            c.Enemy.InvincibilityTimer = NativeCountdown.Start(10);
            c.Projectile.Direction = (c.Projectile.Direction & SamusProjectileDirectionMasks.Direction) switch
            {
                (ushort)SamusProjectileDirection.Left => (ushort)SamusProjectileDirection.UpRight,
                (ushort)SamusProjectileDirection.Right => (ushort)SamusProjectileDirection.UpLeft,
                _ => (ushort)SamusProjectileDirection.DownFacingLeft,
            };
            c.Projectiles.EnemyImpacts.ReflectFromEnemy(c.Bus,
                c.Samus ?? throw new InvalidOperationException("A reflected beam reinherits Samus's movement and requires her."),
                c.Projectile.SlotIndex);
            LastSpacePirateSoundEffect = EnemySoundEffectDefinitions.WalkingPirateReflectSound;
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

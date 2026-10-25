using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Evir.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>The Evir species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class EvirBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. EvirPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new EvirEntity(this, slot, common);

    /// <summary>Physical body, arms, and projectile slots used by Evir's relative aliases.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Runs normal touch handling before Evir's shared combat tail.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Binds slot and collision owners for the three-record Evir actor.</summary>
    internal EvirBehavior(EnemySlotTable slotTable, EnemyCollision collision)
    {
        _slotTable = slotTable;
        _collision = collision;
    }

    /// <summary>Carries out one cue of the Evir projectile's animations: the spit sound and the regeneration steps.</summary>
    private EvirProgram? ApplyEvirCue(RoomEnemySlot slot, EvirEnemyState state, EvirCue cue)
    {
        switch (cue.Kind)
        {
            case EvirCueKind.PlaySpitSound:
                QueueEvirSpitSound();
                break;
            case EvirCueKind.SetInitialRegenerationOffset:
                SetInitialEvirRegenerationOffset(slot, state);
                break;
            case EvirCueKind.AdvanceRegenerationOffset:
                AdvanceEvirRegenerationOffset(slot, state);
                break;
            case EvirCueKind.FinishRegeneration:
                FinishEvirRegeneration(state);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EvirCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>Strict horizontal distance required before aiming the projectile.</summary>
    private const ushort EvirActivationDistance = 128;
    /// <summary>Whole-pixel magnitude passed through the native trig multipliers.</summary>
    private const ushort EvirProjectileSpeed = 4;
    /// <summary>Margin beyond each camera edge that starts projectile regeneration.</summary>
    private const ushort EvirFarOffscreenDistance = 256;


    /// <summary>The library-two sound the spit plays when it is launched or regenerates.</summary>
    private static readonly SoundEffectId EvirSpitSound =
        EnemySoundEffectDefinitions.EvirSpitSound;

    /// <summary>The horizontal distance, in room pixels, from the mouth at which the regenerating spit starts.</summary>
    private const short RegenerationStartDistance = 8;

    /// <summary>Most recent spit sound requested during this enemy frame.</summary>
    public SoundEffectId? LastEvirSoundEffect { get; private set; }

    /// <summary>Ports <c>InitAI_Evir</c> at $A8:87E0.</summary>
    internal void InitializeEvir(RoomEnemySlot slot, SamusState? samus)
    {
        var state = new EvirEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);

        if (slot.Spawn.Population.Parameter1 != 0)
        {
            // The second record is not a second creature. Its nonzero parameter selects the
            // arms role, which always reads facing and position from the immediately prior
            // body record. Retail gives it layer four so the two maps compose correctly.
            PositionEvirArms(slot, state, update: EvirArmsUpdate.Initialization);
            slot.Layer = 4;
        }
        else
        {
            SetEvirBodyFacing(slot, state, samus);

            // Parameter two's low byte selects the body's record in the shared linear-speed
            // table, whose positive half is the downward velocity and negative half the upward
            // one. The high byte is the steady-state number of frames between direction changes.
            int speedRecord = SnesLittleEndian.LowByte(slot.Spawn.Population.Parameter2);
            ushort speedOffset = checked((ushort)(speedRecord * EnemyLinearSpeedDefinitions.RecordSize));
            SnesSignedSixteenSixteen down = EnemyMath.ReadLinearEnemySpeed(speedOffset);
            state.DownVelocity = down;
            SnesSignedSixteenSixteen up = EnemyMath.ReadLinearEnemySpeed(
                checked((ushort)(speedOffset + EnemyLinearSpeedDefinitions.RecordSize / 2)));
            state.UpVelocity = up;
            state.MovementTimer = ReadFirstHalfCycle(speedRecord);
        }

        state.MovingDown = false;
        state.InstalledProgram = null;
        state.Function = EvirAiFunction.HandleBodyOrArms;
    }

    /// <summary>
    /// The body's first bobbing half-cycle. Instead of half its own direction-change period, the body takes half the
    /// upper eight bits of another enemy's value, fixed by its speed record
    /// (<see cref="EvirInitDefinitions.FirstHalfCycleSources"/>). The enemy is read as it stands while the population
    /// loads: for the second Evir of either retail room the third enemy is the first Evir's spit, already carrying its
    /// body's palette, so the first half-cycle is not zero.
    /// </summary>
    private WrappingCountdown ReadFirstHalfCycle(int speedRecord)
    {
        foreach (EvirFirstHalfCycleSource source in EvirInitDefinitions.FirstHalfCycleSources)
        {
            if (source.SpeedRecord != speedRecord)
                continue;
            RoomEnemySlot enemy = _slotTable.Slots[source.SlotIndex];
            ushort value = source.Field switch
            {
                EvirHalfCycleSourceField.PopulationParameter2 => checked((ushort)enemy.Spawn.Population.Parameter2),
                EvirHalfCycleSourceField.Palette => checked((ushort)enemy.PaletteIndex),
                _ => throw new InvalidOperationException($"Undefined {nameof(EvirHalfCycleSourceField)} {source.Field}."),
            };
            return WrappingCountdown.Start(SnesLittleEndian.HighByte(value) / 2);
        }
        throw new InvalidDataException(
            $"Evir speed record {speedRecord} takes its first half-cycle from a value no retail Evir reads.");
    }

    /// <summary>Ports <c>InitAI_EvirProjectile</c> at $A8:88B0.</summary>
    internal void InitializeEvirProjectile(RoomEnemySlot slot)
    {
        var state = new EvirEnemyState
        {
            RequestedProgram = EvirProgram.ProjectileNormal,
            InstalledProgram = null,
            Regenerating = false,
            InFlight = false,
            Function = EvirAiFunction.ProjectileIdle,
            RegenerationOffset = Offset16<RoomPixels>.Zero,
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);
        InstallEvirInstruction(slot, state);

        RoomEnemySlot body = RequireEvirRelativeSlot(slot, -2, EnemyDefinitionId.Evir, "projectile body", initializing: true);
        // The projectile definition has no graphics-set entry of its own. Native copies the
        // body's resolved palette/tile indexes so the projectile map addresses the Evir art.
        slot.PaletteIndex = body.PaletteIndex;
        slot.VramTilesIndex = body.VramTilesIndex;
        ResetEvirProjectilePosition(slot, state);

        // Native clears installed-list after having installed the normal list. That oddity
        // forces the first main-AI request to refresh the bytecode timer exactly once.
        state.InstalledProgram = null;
    }

    /// <summary>Tracks Samus while the spit is present and alternates the body's timed vertical travel.</summary>
    private void RunEvirMain(RoomEnemySlot slot, EvirEnemyState state, SamusState? samus)
    {
        if (state.Function != EvirAiFunction.HandleBodyOrArms)
        {
            throw new InvalidDataException(
                $"Evir body/arms function {state.Function} is not translated.");
        }

        if (slot.Spawn.Population.Parameter1 != 0)
        {
            PositionEvirArms(slot, state);
            return;
        }

        RoomEnemySlot projectile = RequireEvirRelativeSlot(
            slot,
            2,
            EnemyDefinitionId.EvirProjectile,
            "body projectile");
        EvirEnemyState projectileState = projectile.RequireSpeciesState<EvirEnemyState>();

        // Direction is allowed to follow Samus only while the projectile is fully present.
        // During regeneration the mouth, arms, and projectile retain their launch facing.
        if (!projectileState.Regenerating)
            SetEvirBodyFacing(slot, state, samus);

        slot.Position = slot.Position.AdvanceY(state.MovingDown ? state.DownVelocity : state.UpVelocity);

        // DEC/BPL means zero expires immediately into $FFFF. Every later reset uses the
        // unshifted high parameter byte, despite initialization's unrelated aliasing bug.
        CountdownStep step = state.MovementTimer.Decrement();
        state.MovementTimer = step.Remaining;
        if (step.IsNegative)
        {
            state.MovementTimer = WrappingCountdown.Start(SnesLittleEndian.HighByte(slot.Spawn.Population.Parameter2));
            state.MovingDown = !state.MovingDown;
        }
    }

    /// <summary>Dispatches idle attachment, aimed flight, or regeneration after selecting its animation list.</summary>
    private void RunEvirProjectileMain(
        RoomEnemySlot slot,
        EvirEnemyState state,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        if (slot.FrozenTimer.IsZero)
        {
            if (state.InFlight)
            {
                RequestEvirInstruction(
                    slot,
                    state,
                    EvirProgram.ProjectileNormal);
            }
            else if (state.Regenerating)
            {
                RequestEvirInstruction(
                    slot,
                    state,
                    EvirProgram.ProjectileRegenerating);
            }
            else
            {
                TryLaunchEvirProjectile(slot, state, samus);
            }
        }

        switch (state.Function)
        {
            case EvirAiFunction.ProjectileIdle:
                ResetEvirProjectilePosition(slot, state);
                break;
            case EvirAiFunction.ProjectileMoving:
                RunMovingEvirProjectile(slot, state, camera);
                break;
            case EvirAiFunction.ProjectileRegenerating:
                RunRegeneratingEvirProjectile(slot, state);
                break;
            case EvirAiFunction.HandleBodyOrArms:
            default:
                throw new InvalidDataException(
                    $"Evir projectile function {state.Function} is not translated.");
        }
    }

    /// <summary>Selects left or right body artwork from Samus's signed horizontal side.</summary>
    private static void SetEvirBodyFacing(
        RoomEnemySlot body,
        EvirEnemyState state,
        SamusState? samus)
    {
        SamusState activeSamus = samus ?? throw new InvalidOperationException(
            "Evir facing AI requires the active Samus actor.");
        state.Facing = unchecked((short)(activeSamus.Position.X.Whole - body.Position.X.Whole)) < 0
            ? EvirFacing.Left
            : EvirFacing.Right;
        RequestEvirInstruction(
            body,
            state,
            state.Facing == EvirFacing.Left
                ? EvirProgram.BodyFacingLeft
                : EvirProgram.BodyFacingRight);
    }

    /// <summary>Places the arms at a facing-dependent offset from the preceding body record.</summary>
    private void PositionEvirArms(RoomEnemySlot arms, EvirEnemyState state, EvirArmsUpdate update = EvirArmsUpdate.Main)
    {
        RoomEnemySlot body = RequireEvirRelativeSlot(arms, -1, EnemyDefinitionId.Evir, "arms body", update == EvirArmsUpdate.Initialization);
        EvirEnemyState bodyState = body.RequireSpeciesState<EvirEnemyState>();
        state.Facing = bodyState.Facing;
        arms.Position = arms.Position.WithWhole(new(unchecked((ushort)(
            body.Position.X.Whole + (state.Facing == EvirFacing.Left ? -4 : 4))), unchecked((ushort)(body.Position.Y.Whole + 10))));
        RequestEvirInstruction(
            arms,
            state,
            state.Facing == EvirFacing.Left
                ? EvirProgram.ArmsFacingLeft
                : EvirProgram.ArmsFacingRight);
    }

    /// <summary>Reattaches idle spit beneath the body at the current facing offset.</summary>
    private void ResetEvirProjectilePosition(RoomEnemySlot projectile, EvirEnemyState state)
    {
        RoomEnemySlot body = RequireEvirRelativeSlot(
            projectile,
            -2,
            EnemyDefinitionId.Evir,
            "projectile body");
        EvirEnemyState bodyState = body.RequireSpeciesState<EvirEnemyState>();
        state.Facing = bodyState.Facing;
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(
            body.Position.X.Whole + (state.Facing == EvirFacing.Left ? -4 : 4))), unchecked((ushort)(body.Position.Y.Whole + 18))));
    }

    /// <summary>Aims at nearby Samus using the native angle transform and signed 16.16 speed tables.</summary>
    private void TryLaunchEvirProjectile(
        RoomEnemySlot projectile,
        EvirEnemyState state,
        SamusState? samus)
    {
        SamusState activeSamus = samus ?? throw new InvalidOperationException(
            "Evir projectile aiming requires the active Samus actor.");
        RoomEnemySlot body = RequireEvirRelativeSlot(
            projectile,
            -2,
            EnemyDefinitionId.Evir,
            "projectile body");
        if (!EnemyMath.IsWithinStrictModularDistance(
                activeSamus.Position.X.Whole,
                body.Position.X.Whole,
                EvirActivationDistance))
        {
            return;
        }

        // CalculateAngleOfSamusFromEnemy returns zero up/clockwise. The two native angle
        // transforms below feed the buggy table multipliers and produce signed 16.16 speed.
        SnesByteAngle transformedAngle = EnemyMath.CalculateCartridgeAngle(
            new(unchecked((short)(activeSamus.Position.X.Whole - projectile.Position.X.Whole)), unchecked((short)(activeSamus.Position.Y.Whole - projectile.Position.Y.Whole)))).ToTableAngle();
        SnesSignedSixteenSixteen x = EnemyMath.ReadEightBitCosineFixedProduct(
            transformedAngle,
            EvirProjectileSpeed);
        SnesSignedSixteenSixteen y = EnemyMath.ReadEightBitNegativeSineFixedProduct(
            transformedAngle,
            EvirProjectileSpeed);
        state.ProjectileVelocity = new FixedVector<RoomPixels>(
            x,
            y);
        RequestEvirInstruction(
            projectile,
            state,
            EvirProgram.ProjectileNormal);
        state.InFlight = true;
        state.Function = EvirAiFunction.ProjectileMoving;
    }

    /// <summary>Checks the far-offscreen regeneration boundary before advancing the spit.</summary>
    private void RunMovingEvirProjectile(
        RoomEnemySlot projectile,
        EvirEnemyState state,
        Position16<RoomPixels> camera)
    {
        StartEvirRegenerationIfFarOffscreen(projectile, state, camera);
        projectile.Position = projectile.Position.Advance(state.ProjectileVelocity);
    }

    /// <summary>Starts return animation beyond the camera margin unless the body is frozen.</summary>
    private void StartEvirRegenerationIfFarOffscreen(
        RoomEnemySlot projectile,
        EvirEnemyState state,
        Position16<RoomPixels> camera)
    {
        bool farOffscreen =
            EnemyMath.IsNegative16(projectile.Position.X.Whole + EvirFarOffscreenDistance - camera.X) ||
            EnemyMath.IsNegative16(camera.X + 256 + EvirFarOffscreenDistance - projectile.Position.X.Whole) ||
            EnemyMath.IsNegative16(projectile.Position.Y.Whole + EvirFarOffscreenDistance - camera.Y) ||
            EnemyMath.IsNegative16(camera.Y + 256 + EvirFarOffscreenDistance - projectile.Position.Y.Whole);
        if (!farOffscreen)
            return;

        RoomEnemySlot body = RequireEvirRelativeSlot(
            projectile,
            -2,
            EnemyDefinitionId.Evir,
            "projectile body");
        if (!body.FrozenTimer.IsZero)
            return;

        state.InFlight = false;
        state.Regenerating = true;
        state.Function = EvirAiFunction.ProjectileRegenerating;
        RequestEvirInstruction(
            projectile,
            state,
            EvirProgram.ProjectileRegenerating);
    }

    /// <summary>Returns the spit toward its body and waits for the animation's completion flag.</summary>
    private void RunRegeneratingEvirProjectile(RoomEnemySlot projectile, EvirEnemyState state)
    {
        RoomEnemySlot body = RequireEvirRelativeSlot(
            projectile,
            -2,
            EnemyDefinitionId.Evir,
            "projectile body");
        if (!body.FrozenTimer.IsZero)
            return;

        if (!state.Regenerating)
        {
            RequestEvirInstruction(
                projectile,
                state,
                EvirProgram.ProjectileNormal);
            state.InFlight = false;
            state.Function = EvirAiFunction.ProjectileIdle;
            return;
        }

        ResetEvirProjectilePosition(projectile, state);
        projectile.Position += state.RegenerationOffset;
    }

    /// <summary>Instruction $A8:878F.</summary>
    private void QueueEvirSpitSound() => LastEvirSoundEffect = EvirSpitSound;

    /// <summary>Instruction $A8:879B.</summary>
    private void SetInitialEvirRegenerationOffset(RoomEnemySlot slot, EvirEnemyState state) =>
        state.RegenerationOffset = new(EvirBodyFacesRight(slot) ? (short)-RegenerationStartDistance : RegenerationStartDistance, 0);

    /// <summary>Instruction $A8:87B6.</summary>
    private void AdvanceEvirRegenerationOffset(RoomEnemySlot slot, EvirEnemyState state) =>
        state.RegenerationOffset += new Offset16<RoomPixels>(EvirBodyFacesRight(slot) ? (short)1 : (short)-1, 0);

    /// <summary>
    /// Both regeneration instructions test <c>Evir.instList-$80,X</c>: the installed list
    /// of the body two slots before the projectile, not any facing word of its own.
    /// </summary>
    private bool EvirBodyFacesRight(RoomEnemySlot projectile)
    {
        EvirEnemyState body = _slotTable.Slots[projectile.SlotIndex - 2].SpeciesState<EvirEnemyState>() ?? throw new InvalidDataException(
            $"Evir projectile slot {projectile.SlotIndex} has no body two slots before it.");
        return body.InstalledProgram == EvirProgram.BodyFacingRight;
    }

    /// <summary>Instruction $A8:87CB.</summary>
    private static void FinishEvirRegeneration(EvirEnemyState state)
    {
        state.Regenerating = false;
        state.InFlight = false;
        state.Function = EvirAiFunction.ProjectileIdle;
    }

    /// <summary>Records the desired list and installs it only when it differs from the active one.</summary>
    private static void RequestEvirInstruction(
        RoomEnemySlot slot,
        EvirEnemyState state,
        EvirProgram program)
    {
        state.RequestedProgram = program;
        InstallEvirInstruction(slot, state);
    }

    /// <summary>Resets the instruction timer once per list change, preserving held animations.</summary>
    private static void InstallEvirInstruction(RoomEnemySlot slot, EvirEnemyState state)
    {
        if (state.RequestedProgram == state.InstalledProgram)
            return;
        state.Program = AnimationProgramCursor<EvirProgram>.Start(state.RequestedProgram);
        state.InstalledProgram = state.RequestedProgram;
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Validates physical body/arms/spit offsets while tolerating a cleared header after initialization.</summary>
    private RoomEnemySlot RequireEvirRelativeSlot(
        RoomEnemySlot owner,
        int relativeSlot,
        EnemyDefinitionId expectedDefinition,
        string relationship,
        bool initializing = false)
    {
        int targetIndex = owner.SlotIndex + relativeSlot;
        // Native links are physical offsets, not live-header references. Contact can
        // clear the body after the active list was built; its deleted arms and spit
        // still execute once and read the cleared common words. Validate authored
        // identities at initialization, then retain the established physical alias.
        if ((uint)targetIndex >= _slotTable.Slots.Count ||
            (_slotTable.Slots[targetIndex].DefinitionId != expectedDefinition &&
                (initializing || _slotTable.Slots[targetIndex].SpeciesState<EvirEnemyState>() is null)))
        {
            throw new InvalidDataException(
                $"Evir {relationship} expected definition {expectedDefinition} " +
                $"at relative slot {relativeSlot:+#;-#;0} from {owner.SlotIndex}.");
        }
        return _slotTable.Slots[targetIndex];
    }

    /// <summary>
    /// Ports $A8:8B16 after common touch/shot/power-bomb damage. Native applies its +$40
    /// and +$80 physical-slot aliases relative to whichever $E63F record received the
    /// callback. Ordinary shots/touch can only reach the body because the arms carry
    /// property $0400, but the power-bomb walker deliberately ignores that property and can
    /// invoke the same code on the arms. Preserve that odd relative aliasing instead of
    /// replacing it with a body-only composite helper.
    /// </summary>
    private void ResolveEvirCombatAfterCommon(RoomEnemySlot actor)
    {
        int firstRelativeIndex = actor.SlotIndex + 1;
        int secondRelativeIndex = actor.SlotIndex + 2;
        if ((uint)secondRelativeIndex >= _slotTable.Slots.Count)
        {
            throw new InvalidDataException(
                $"Evir callback at slot {actor.SlotIndex} exceeds the native +$80 slot alias.");
        }
        RoomEnemySlot firstRelative = _slotTable.Slots[firstRelativeIndex];
        RoomEnemySlot secondRelative = _slotTable.Slots[secondRelativeIndex];

        if (actor.Health == 0)
        {
            firstRelative.Properties = firstRelative.Properties.With(EnemyProperties.Deleted);
            secondRelative.Properties = secondRelative.Properties.With(EnemyProperties.Deleted);
        }

        if (actor.FrozenTimer.IsZero)
            return;
        firstRelative.FrozenTimer = actor.FrozenTimer;
        firstRelative.AiStates = firstRelative.AiStates | EnemyAiStates.Frozen;
        // The cartridge compares the following-but-one record's function with the moving spit's. In the retail
        // Evir rooms that record is an Evir or an empty slot, never another species.
        if (secondRelative.SpeciesState<EvirEnemyState>()?.Function != EvirAiFunction.ProjectileMoving)
        {
            secondRelative.FrozenTimer = actor.FrozenTimer;
            secondRelative.AiStates = secondRelative.AiStates | EnemyAiStates.Frozen;
        }
    }

    /// <summary>Runs the shared post-common Evir combat tail after bomb hits.</summary>
    internal void RegisterEvirBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.Evir &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.EvirShot)
        {
            AfterCommon = c => ResolveEvirCombatAfterCommon(c.Enemy),
        });
    }

    /// <summary>Ports Evir's initialization AI.</summary>
    internal void RunEvirInitializationAi(RoomEnemySlot slot, EnemySpawnSurroundings surroundings)
    {
        InitializeEvir(slot, surroundings.Samus);
    }

    /// <summary>Ports Evir's main AI.</summary>
    internal void RunEvirMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunEvirMain(slot, slot.RequireSpeciesState<EvirEnemyState>(), frame.Samus);
    }

    /// <summary>Ports EvirProjectile's main AI.</summary>
    internal void RunEvirProjectileMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunEvirProjectileMain(
            slot,
            slot.RequireSpeciesState<EvirEnemyState>(),
            frame.Samus,
            frame.Camera);
    }

    /// <summary>Advances Evir's typed animation.</summary>
    internal void AdvanceEvirTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        var state = slot.RequireSpeciesState<EvirEnemyState>();
        AnimationProgramCursor<EvirProgram> cursor = state.Program ??
            throw new InvalidOperationException($"Evir slot {slot.SlotIndex} has no installed animation.");
        EnemyProgramAnimation.Run(slot, EvirAnimations.Programs, ref cursor, EvirFrameDefinitions.ShownFrames.Of,
                (cue, _) => ApplyEvirCue(slot, state, cue));
        state.Program = cursor;
    }

    /// <summary>Preserves power-bomb callbacks on both body and arms, including native relative aliases.</summary>
    internal void RegisterEvirPowerBombReactions(EnemySharedReactions table)
    {
        // Touch and shot collision lists exclude the arms through property $0400, but $A0:A306's power-bomb walker
        // does not. Both body and arms therefore dispatch $A8:8B0C and its intentionally physical +$40/+80 follow-up
        // aliases.
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Evir &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.EvirPowerBomb)
        {
            AfterCommon = (enemy, _) => ResolveEvirCombatAfterCommon(enemy),
        });
    }

    /// <summary>Copies accepted body-shot freeze state to arms and attached spit after common damage.</summary>
    internal void RegisterEvirShotReactions(EnemySharedReactions table)
    {
        // Evir's bank-$A8 shot tail runs after every accepted common shot, including the $FF ice result. That is
        // where the body copies the newly installed frozen timer to its arms and attached (idle) spit.
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Evir &&
                                                     enemy.Spawn.Population.Parameter1 == 0 &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.EvirShot)
        {
            AfterCommon = c => ResolveEvirCombatAfterCommon(c.Enemy),
        });
    }

    /// <summary>Runs common body contact and then Evir's physical-slot combat follow-up.</summary>
    internal void RegisterEvirTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.Evir &&
                               slot.Spawn.Population.Parameter1 == 0 &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.EvirTouch,
            context =>
            {
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus);
                ResolveEvirCombatAfterCommon(context.Slot);
            });
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastEvirSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastEvirSoundEffect is SoundEffectId sound)
            audio.QueueEnemySound(sound, maximumQueued: SoundQueueLimit.Max6);
    }
}

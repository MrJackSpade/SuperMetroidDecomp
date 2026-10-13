using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>The Fune/Namihe species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class FuneNamiheBehavior : IEnemySpecies
{
    /// <summary>Allocates the shared directional fireball fired by either species' animation list.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Connects the family AI to the room's enemy projectile slots.</summary>
    internal FuneNamiheBehavior(EnemyProjectileEngine projectiles)
    {
        _projectiles = projectiles;
    }

    /// <summary>Handles private sound, fireball-spawn, and activity-finish opcodes while advancing the mixed-list cursor past each command.</summary>
    private bool TryProcessFuneNamiheInstruction(RoomEnemySlot slot, ushort word, ref ushort cursor)
    {
        if (!IsFuneNamiheDefinition(slot.EnemyDefinitionPointer) ||
            !Enum.IsDefined((FuneNamiheInstruction)word))
            return false;

        switch ((FuneNamiheInstruction)word)
        {
            case FuneNamiheInstruction.QueueSpitSFX:
                // Shared mouth animation queues library-two sound $1F.
                QueueFuneNamiheSpitSound();
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case FuneNamiheInstruction.NamiheSpawnFireballFacingLeft:
                SpawnFuneNamiheFireball(slot, movingRight: false, RoomEnemyProjectileKind.NamiheFireball);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case FuneNamiheInstruction.NamiheSpawnFireballFacingRight:
                SpawnFuneNamiheFireball(slot, movingRight: true, RoomEnemyProjectileKind.NamiheFireball);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case FuneNamiheInstruction.FuneSpawnFireballFacingLeft:
                SpawnFuneNamiheFireball(slot, movingRight: false, RoomEnemyProjectileKind.FuneFireball);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case FuneNamiheInstruction.FuneSpawnFireballFacingRight:
                SpawnFuneNamiheFireball(slot, movingRight: true, RoomEnemyProjectileKind.FuneFireball);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case FuneNamiheInstruction.FinishActivity:
            case FuneNamiheInstruction.FinishActivityDuplicate:
                // Left/right lists use duplicate opcodes with byte-for-byte state effects.
                FinishFuneNamiheActivity(RequireFuneNamiheState(slot));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            default:
                throw new InvalidOperationException(
                    $"Fune/Namihe does not own instruction ${word:X4}.");
        }
    }

    /// <summary>Cooldown, proximity, list-cursor, and species state indexed by native enemy slot.</summary>
    internal readonly FuneNamiheEnemyState?[] _funeNamiheStates =
        new FuneNamiheEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>
    /// Most recent library-two spit sound requested by opcode $A8:9625. Audio mixing is an
    /// outer frontend concern, so the actor publishes the exact sound number for that seam.
    /// </summary>
    public ushort? LastFuneNamiheSoundEffect { get; private set; }

    /// <summary>Restricts the shared initializer, main AI, and private opcodes to Fune or Namihe enemy definitions.</summary>
    internal static bool IsFuneNamiheDefinition(EnemyDefinitionId definitionPointer) =>
        definitionPointer is EnemyDefinitionId.Fune or
            EnemyDefinitionId.Namihe;

    /// <summary>Decodes species, facing, cooldown, and Y proximity from population parameters, then installs the corresponding idle list.</summary>
    internal void InitializeFuneNamihe(RoomEnemySlot slot)
    {
        FuneNamiheEnemyState state = new(slot)
        {
            InstructionListPointerTableCursor = FuneNamiheDefinitions.FuneIdleLeftCursor,
            Function = FuneNamiheEnemyFunction.FuneWaitForCooldown,
            VariantIndex = unchecked((ushort)(slot.Parameter1 & 0x000f)),
            YProximity = SnesLittleEndian.HighByte(slot.Parameter2),
            CooldownTime = SnesLittleEndian.HighByte(slot.Parameter1),
            CooldownTimer = 0,
        };

        // Species is encoded in the population parameter rather than inferred from the
        // enemy header. This sounds redundant, but preserving it lets malformed/custom ROM
        // populations exhibit the same cross-species behavior as the retail routine.
        if (state.IsNamihe)
        {
            state.InstructionListPointerTableCursor = FuneNamiheDefinitions.NamiheIdleLeftCursor;
            state.Function = FuneNamiheEnemyFunction.NamiheWaitForSamus;
        }

        // Any bit in parameter one's upper low-byte nibble selects right-facing art. The
        // code does not mask to a single direction flag before making this decision.
        if ((slot.Parameter1 & 0x00f0) != 0)
        {
            state.InstructionListPointerTableCursor = unchecked((ushort)(
                state.InstructionListPointerTableCursor + FuneNamiheDefinitions.FacingRightCursorDelta));
        }

        _funeNamiheStates[slot.SlotIndex] = state;
        InstallFuneNamiheInstructionList(slot, state);
    }

    /// <summary>Starts Fune's activity after a wrapping cooldown or Namihe's after strict modular Y proximity; active lists own the return to idle.</summary>
    internal static void RunFuneNamiheMain(
        RoomEnemySlot slot,
        FuneNamiheEnemyState state,
        SamusState? samus)
    {
        switch (state.Function)
        {
            case FuneNamiheEnemyFunction.FuneWaitForCooldown:
                // INC occurs before CMP. BMI tests the signed subtraction result rather
                // than performing an unsigned branch; ordinary room parameters are small,
                // but the distinction remains visible at the $8000 wrap boundary.
                state.CooldownTimer = unchecked((ushort)(state.CooldownTimer + 1));
                if (unchecked((short)(state.CooldownTimer - state.CooldownTime)) < 0)
                    return;

                state.InstructionListPointerTableCursor = unchecked((ushort)(
                    state.InstructionListPointerTableCursor - FuneNamiheDefinitions.ActiveCursorDelta));
                InstallFuneNamiheInstructionList(slot, state);
                state.Function = FuneNamiheEnemyFunction.FuneActivityNoOp;
                state.CooldownTimer = 0;
                return;

            case FuneNamiheEnemyFunction.NamiheWaitForSamus:
                if (samus is null)
                {
                    throw new InvalidOperationException(
                        "Namihe proximity AI requires the active Samus actor.");
                }

                // IsSamusWithinAPixelRowsOfEnemy takes the modular absolute difference and
                // uses a strict comparison. Equality at the configured distance stays idle.
                if (!EnemyMath.IsWithinStrictModularDistance(
                        samus.Position.Y.Whole,
                        slot.Position.Y.Whole,
                        state.YProximity))
                {
                    return;
                }

                state.InstructionListPointerTableCursor = unchecked((ushort)(
                    state.InstructionListPointerTableCursor - FuneNamiheDefinitions.ActiveCursorDelta));
                InstallFuneNamiheInstructionList(slot, state);
                state.Function = FuneNamiheEnemyFunction.NamiheActivityNoOp;
                return;

            case FuneNamiheEnemyFunction.FuneActivityNoOp:
            case FuneNamiheEnemyFunction.NamiheActivityNoOp:
                // The active animation's finish opcode is the only owner allowed to leave
                // these RTS states. Main AI intentionally does nothing until that command.
                return;

            default:
                throw new InvalidDataException(
                    $"Fune/Namihe function $A8:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>
    /// Models <c>SetFuneNamiheInstList</c>: the state word points to a ROM word which in
    /// turn points to the actual bytecode list. Both instruction timers are reset together.
    /// </summary>
    private static void InstallFuneNamiheInstructionList(
        RoomEnemySlot slot,
        FuneNamiheEnemyState state)
    {
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        slot.CurrentInstruction = ReadFuneNamiheInstructionList(
            state.InstructionListPointerTableCursor);
    }

    /// <summary>
    /// The table lives at $A8:96D3; callers carry a same-bank cursor. This overload makes
    /// that two-stage address explicit without allowing a host addition to cross banks.
    /// </summary>
    private static ushort ReadFuneNamiheInstructionList(ushort tableCursor) =>
        FuneNamiheDefinitions.InstructionList(tableCursor);

    /// <summary>Publishes the shared library-two spit cue for the outer enemy audio queue.</summary>
    private void QueueFuneNamiheSpitSound() =>
        LastFuneNamiheSoundEffect = FuneNamiheDefinitions.SpitSoundEffect;

    /// <summary>Moves the list-table cursor back to idle and restores the species-specific waiting function after the active animation.</summary>
    private static void FinishFuneNamiheActivity(FuneNamiheEnemyState state)
    {
        state.InstructionListPointerTableCursor = unchecked((ushort)(
            state.InstructionListPointerTableCursor + FuneNamiheDefinitions.ActiveCursorDelta));
        state.Function = state.IsNamihe
            ? FuneNamiheEnemyFunction.NamiheWaitForSamus
            : FuneNamiheEnemyFunction.FuneWaitForCooldown;
    }

    /// <summary>Rejects a family AI or finish opcode before the slot's variant state has been initialized.</summary>
    private FuneNamiheEnemyState RequireFuneNamiheState(RoomEnemySlot slot) =>
        _funeNamiheStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Fune/Namihe state.");

    /// <summary>Binds the shared fireball pre-instruction to 8.8 horizontal travel and inclusive camera culling.</summary>
    internal static void RegisterFuneNamiheProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            // Fune/Namihe: directional 8.8 flight and cull.
            RunFuneNamiheFireballPreInstruction(context.Projectile, context.Camera);
        }, EnemyProjectilePreInstruction.NamiFuneFireball);
    }

    /// <summary>Registers the shared initializer only for Fune and Namihe records.</summary>
    internal void RegisterFuneNamiheInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Fune_Namihe, slot => IsFuneNamiheDefinition(slot.EnemyDefinitionPointer), (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeFuneNamihe(slot);
        });
    }

    /// <summary>Registers the shared cooldown/proximity dispatcher for both family definitions.</summary>
    internal void RegisterFuneNamiheMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Fune_Namihe, slot => IsFuneNamiheDefinition(slot.EnemyDefinitionPointer), (slot, frame, nmiFrameCounter8) =>
        {
            RunFuneNamiheMain(slot, RequireFuneNamiheState(slot), frame.Samus);
        });
    }

    /// <summary>Claims the family's private animation opcodes while yielding unrelated words to the common enemy interpreter.</summary>
    internal void RegisterFuneNamiheOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessFuneNamiheInstruction(slot, word, ref cursor);
        });
    }
    /// <summary>Shared projectile movement callback installed for both Fune and Namihe fireballs.</summary>
    private const ushort NamiFuneFireballPreInstruction =
        (ushort)EnemyProjectilePreInstruction.NamiFuneFireball;

    /// <summary>
    /// Ports the shared projectile initializer at $86:DED6. The source population's low
    /// parameter-two byte selects one four-byte {left,right} velocity pair. Left velocity
    /// is stored in the field normally named Y velocity—a cartridge quirk that the left
    /// movement function knowingly reads back as horizontal speed.
    /// </summary>
    internal void SpawnFuneNamiheFireball(
        RoomEnemySlot source,
        bool movingRight,
        RoomEnemyProjectileKind kind)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            kind,
            unchecked((ushort)(source.PaletteIndex | source.VramTilesIndex)));
        projectile.InstructionPointer = movingRight
            ? FuneNamiheFireballInstructionProgramDefinitions.Right
            : FuneNamiheFireballInstructionProgramDefinitions.Left;
        projectile.PreInstruction = NamiFuneFireballPreInstruction;
        projectile.Variable0 = movingRight
            ? (ushort)NamiFuneFireballFunction.MovingRight
            : (ushort)NamiFuneFireballFunction.MovingLeft;
        projectile.DirectionParameter = movingRight ? (ushort)1 : (ushort)0;

        projectile.Position = source.Position;

        // The initializer tests the actor parameter's species nibble, not the projectile
        // definition. Namihe's mouth is four pixels lower than Fune's in the shared art.
        if ((source.Parameter1 & 0x000f) != 0)
            projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(projectile.Position.Y.Whole + 4)));

        var velocities = EnemyFireballLaunchDefinitions.NamiFuneVelocities(source.Parameter2);
        projectile.Velocity = new(new SnesSignedEightEight(velocities.Right), new SnesSignedEightEight(velocities.Left));
    }

    /// <summary>Ports pre-instruction $86:DF39 and both directional movers.</summary>
    internal static void RunFuneNamiheFireballPreInstruction(
        RoomEnemyProjectileSlot projectile,
        Position16<RoomPixels> camera)
    {
        switch (ClosedNativeWords.Decode<NamiFuneFireballFunction>(projectile.Variable0, "Fune/Namihe fireball function"))
        {
            case NamiFuneFireballFunction.MovingLeft:
                // Yes, this is YVelocity. The retail initializer and mover agree on this
                // unconventional storage, so normalizing it would make debugger state lie.
                projectile.Position = projectile.Position.AdvanceX(projectile.Velocity.Y.ToSixteenSixteenDelta());
                break;

            case NamiFuneFireballFunction.MovingRight:
                projectile.Position = projectile.Position.AdvanceX(projectile.Velocity.X.ToSixteenSixteenDelta());
                break;

            default:
                throw new InvalidDataException(
                    $"Fune/Namihe fireball function $86:{projectile.Variable0:X4} is not translated.");
        }

        // The shared native cull treats both camera edges as inclusive: an origin exactly
        // at camera+256 survives this frame and is deleted only after crossing beyond it.
        CacatacBehavior.DeleteEnemyProjectileIfOutsideInclusiveViewport(projectile, camera);
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        LastFuneNamiheSoundEffect = null;
        Array.Clear(_funeNamiheStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastFuneNamiheSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastFuneNamiheSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }
}

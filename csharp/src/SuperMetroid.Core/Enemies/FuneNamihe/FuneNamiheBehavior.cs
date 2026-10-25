using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.FuneNamihe.Artwork;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>The Fune/Namihe species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class FuneNamiheBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Fune, EnemyDefinitionId.Namihe];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new FuneNamiheEntity(this, slot, common);

    /// <summary>Allocates the shared directional fireball fired by either species' animation list.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Connects the family AI to the room's enemy projectile slots.</summary>
    internal FuneNamiheBehavior(EnemyProjectileEngine projectiles)
    {
        _projectiles = projectiles;
    }

    /// <summary>Carries out one cue of the Fune's and Namihe's animations: the spit sound, a fireball, or finishing the spit.</summary>
    private FuneNamiheProgram? ApplyFuneNamiheCue(RoomEnemySlot slot, FuneNamiheEnemyState state, FuneNamiheCue cue)
    {
        switch (cue.Kind)
        {
            case FuneNamiheCueKind.SpitSound:
                // Shared mouth animation queues library-two sound $1F.
                QueueFuneNamiheSpitSound();
                break;
            case FuneNamiheCueKind.NamiheFireLeft:
                SpawnFuneNamiheFireball(slot, direction: HorizontalDirection.Left, RoomEnemyProjectileKind.NamiheFireball);
                break;
            case FuneNamiheCueKind.NamiheFireRight:
                SpawnFuneNamiheFireball(slot, direction: HorizontalDirection.Right, RoomEnemyProjectileKind.NamiheFireball);
                break;
            case FuneNamiheCueKind.FuneFireLeft:
                SpawnFuneNamiheFireball(slot, direction: HorizontalDirection.Left, RoomEnemyProjectileKind.FuneFireball);
                break;
            case FuneNamiheCueKind.FuneFireRight:
                SpawnFuneNamiheFireball(slot, direction: HorizontalDirection.Right, RoomEnemyProjectileKind.FuneFireball);
                break;
            case FuneNamiheCueKind.FinishActivity:
                // Left/right lists use duplicate native opcodes with byte-for-byte state effects.
                FinishFuneNamiheActivity(state);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(FuneNamiheCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>
    /// Most recent library-two spit sound requested by opcode $A8:9625. Audio mixing is an
    /// outer frontend concern, so the actor publishes the exact sound number for that seam.
    /// </summary>
    public SoundEffectId? LastFuneNamiheSoundEffect { get; private set; }

    /// <summary>Decodes species, facing, cooldown, and Y proximity from population parameters, then installs the corresponding idle list.</summary>
    internal static void InitializeFuneNamihe(RoomEnemySlot slot)
    {
        FuneNamiheEnemyState state = new()
        {
            ShowsActiveAnimation = false,
            Function = FuneNamiheEnemyFunction.FuneWaitForCooldown,
            VariantIndex = slot.Spawn.Population.Parameter1 & FuneNamiheParameterMasks.VariantBits,
            YProximity = SnesLittleEndian.HighByte(slot.Spawn.Population.Parameter2),
            CooldownTime = SnesLittleEndian.HighByte(slot.Spawn.Population.Parameter1),
            CooldownTimer = 0,
            // Any bit in parameter one's upper low-byte nibble selects right-facing art. The
            // code does not mask to a single direction flag before making this decision.
            FacingRight = (slot.Spawn.Population.Parameter1 & FuneNamiheParameterMasks.RightFacingBits) != 0,
        };

        // Species is encoded in the population parameter rather than inferred from the
        // enemy header. This sounds redundant, but preserving it lets malformed/custom ROM
        // populations exhibit the same cross-species behavior as the retail routine.
        if (state.IsNamihe)
            state.Function = FuneNamiheEnemyFunction.NamiheWaitForSamus;

        slot.SetSpeciesState(state, state.ClearWithRecord);
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

                state.ShowsActiveAnimation = true;
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
                        checked((ushort)state.YProximity)))
                {
                    return;
                }

                state.ShowsActiveAnimation = true;
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
                    $"Fune/Namihe function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>
    /// Models <c>SetFuneNamiheInstList</c>: installs the animation the species, activity and facing select. Both
    /// instruction timers are reset together.
    /// </summary>
    private static void InstallFuneNamiheInstructionList(
        RoomEnemySlot slot,
        FuneNamiheEnemyState state)
    {
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        state.Program = AnimationProgramCursor<FuneNamiheProgram>.Start(SelectFuneNamiheProgram(state));
    }

    /// <summary>The animation for <paramref name="state"/>'s species, activity and facing.</summary>
    internal static FuneNamiheProgram SelectFuneNamiheProgram(FuneNamiheEnemyState state) =>
        state.IsNamihe
            ? state.ShowsActiveAnimation
                ? state.FacingRight ? FuneNamiheProgram.NamiheActiveRight : FuneNamiheProgram.NamiheActiveLeft
                : state.FacingRight ? FuneNamiheProgram.NamiheIdleRight : FuneNamiheProgram.NamiheIdleLeft
            : state.ShowsActiveAnimation
                ? state.FacingRight ? FuneNamiheProgram.FuneActiveRight : FuneNamiheProgram.FuneActiveLeft
                : state.FacingRight ? FuneNamiheProgram.FuneIdleRight : FuneNamiheProgram.FuneIdleLeft;

    /// <summary>Publishes the shared library-two spit cue for the outer enemy audio queue.</summary>
    private void QueueFuneNamiheSpitSound() =>
        LastFuneNamiheSoundEffect = FuneNamiheDefinitions.SpitSoundEffect;

    /// <summary>Moves the list-table cursor back to idle and restores the species-specific waiting function after the active animation.</summary>
    private static void FinishFuneNamiheActivity(FuneNamiheEnemyState state)
    {
        state.ShowsActiveAnimation = false;
        state.Function = state.IsNamihe
            ? FuneNamiheEnemyFunction.NamiheWaitForSamus
            : FuneNamiheEnemyFunction.FuneWaitForCooldown;
    }

    /// <summary>Binds the shared fireball pre-instruction to 8.8 horizontal travel and inclusive camera culling.</summary>
    internal static void RegisterFuneNamiheProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            // Fune/Namihe: directional 8.8 flight and cull.
            RunFuneNamiheFireballPreInstruction(context.Projectile, context.Camera);
        }, EnemyProjectilePreInstruction.NamiFuneFireball);
    }

    /// <summary>Ports FuneNamihe's main AI.</summary>
    internal static void RunFuneNamiheMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunFuneNamiheMain(slot, slot.RequireSpeciesState<FuneNamiheEnemyState>(), frame.Samus);
    }

    /// <summary>Advances FuneNamihe's typed animation.</summary>
    internal void AdvanceFuneNamiheTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, FuneNamiheAnimations.Programs, FuneNamiheFrameDefinitions.ShownFrames.Of,
            static (FuneNamiheEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor,
            (state, cue, _) => ApplyFuneNamiheCue(slot, state, cue));

    /// <summary>Shared projectile movement callback installed for both Fune and Namihe fireballs.</summary>
    private const EnemyProjectilePreInstruction NamiFuneFireballPreInstruction =
        EnemyProjectilePreInstruction.NamiFuneFireball;

    /// <summary>
    /// Ports the shared projectile initializer at $86:DED6. The source population's low
    /// parameter-two byte selects one four-byte {left,right} velocity pair. Left velocity
    /// is stored in the field normally named Y velocity—a cartridge quirk that the left
    /// movement function knowingly reads back as horizontal speed.
    /// </summary>
    internal void SpawnFuneNamiheFireball(
        RoomEnemySlot source,
        HorizontalDirection direction,
        RoomEnemyProjectileKind kind)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            kind,
            unchecked((ushort)(source.PaletteIndex | source.TileIndexBase)));
        projectile.InstallAnimation(new FuneNamiheFireballAnimation(direction == HorizontalDirection.Right ? FuneNamiheFireballProgram.Right : FuneNamiheFireballProgram.Left));
        projectile.PreInstruction = NamiFuneFireballPreInstruction;
        projectile.Variable0 = direction == HorizontalDirection.Right
            ? (ushort)NamiFuneFireballFunction.MovingRight
            : (ushort)NamiFuneFireballFunction.MovingLeft;
        projectile.DirectionParameter = direction == HorizontalDirection.Right ? (ushort)1 : (ushort)0;

        projectile.Position = source.Position;

        // The initializer tests the actor parameter's species nibble, not the projectile
        // definition. Namihe's mouth is four pixels lower than Fune's in the shared art.
        if ((source.Spawn.Population.Parameter1 & FuneNamiheParameterMasks.VariantBits) != 0)
            projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(projectile.Position.Y.Whole + 4)));

        var velocities = EnemyFireballLaunchDefinitions.NamiFuneVelocities(checked((ushort)source.Spawn.Population.Parameter2));
        projectile.Velocity = new(new SnesSignedEightEight(checked((ushort)velocities.Right)), new SnesSignedEightEight(checked((ushort)velocities.Left)));
    }

    /// <summary>Ports pre-instruction $86:DF39 and both directional movers.</summary>
    internal static void RunFuneNamiheFireballPreInstruction(
        RoomEnemyProjectileSlot projectile,
        Position16<RoomPixels> camera)
    {
        switch (ClosedNativeWords.Decode<NamiFuneFireballFunction>(checked((ushort)projectile.Variable0), "Fune/Namihe fireball function"))
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
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastFuneNamiheSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueEnemySoundIfAny(LastFuneNamiheSoundEffect, maximumQueued: SoundQueueLimit.Max6);
    }
}

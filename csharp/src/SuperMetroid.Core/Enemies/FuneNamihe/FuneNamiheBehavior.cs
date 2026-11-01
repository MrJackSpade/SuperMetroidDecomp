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

    /// <summary>Receives the species' end-of-update sound requests.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects the family AI to the room's enemy projectile slots.</summary>
    internal FuneNamiheBehavior(EnemyProjectileEngine projectiles, EnemyAudioQueue audio)
    {
        _audio = audio;
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
                // Left/right lists use duplicate opcodes with byte-for-byte state effects.
                FinishFuneNamiheActivity(state);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(FuneNamiheCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>Decodes species, facing, cooldown, and Y proximity from population parameters, then installs the corresponding idle list.</summary>
    internal static void InitializeFuneNamihe(RoomEnemySlot slot)
    {
        FuneNamiheSpawnSettings settings = slot.SpawnSettings<FuneNamiheSpawnSettings>();
        FuneNamiheSetup setup = FuneNamiheSetup.Of(settings);
        FuneNamiheEnemyState state = new()
        {
            ShowsActiveAnimation = false,
            Function = FuneNamiheEnemyFunction.FuneWaitForCooldown,
            VariantIndex = setup.Variant,
            YProximity = settings.YProximity,
            CooldownTime = settings.CooldownTime,
            CooldownTimer = 0,
            FacingRight = setup.FacesRight,
        };

        // Species is encoded in the population parameter rather than inferred from the
        // enemy header. This sounds redundant, but preserving it lets malformed/custom
        // populations exhibit the same cross-species behavior as the retail routine.
        if (state.IsNamihe)
            state.Function = FuneNamiheEnemyFunction.NamiheWaitForSamus;

        slot.SetSpeciesState(state, state.ClearWithRecord);
        InstallFuneNamiheAnimationScript(slot, state);
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
                // but the distinction remains visible at the 32768 wrap boundary.
                state.CooldownTimer = WrappingCounter.IncrementWord(state.CooldownTimer);
                if (WordWrap.Signed(state.CooldownTimer - state.CooldownTime) < 0)
                    return;

                state.ShowsActiveAnimation = true;
                InstallFuneNamiheAnimationScript(slot, state);
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
                        Ensure.BetweenInclusive(0, ushort.MaxValue, state.YProximity)))
                {
                    return;
                }

                state.ShowsActiveAnimation = true;
                InstallFuneNamiheAnimationScript(slot, state);
                state.Function = FuneNamiheEnemyFunction.NamiheActivityNoOp;
                return;

            case FuneNamiheEnemyFunction.FuneActivityNoOp:
            case FuneNamiheEnemyFunction.NamiheActivityNoOp:
                // The active animation's finish opcode is the only owner allowed to leave
                // these inert states. Main AI intentionally does nothing until that command.
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
    private static void InstallFuneNamiheAnimationScript(
        RoomEnemySlot slot,
        FuneNamiheEnemyState state)
    {
        slot.FrameHold = WrappingCountdown.Start(1);
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
        _audio.RequestSoundAtUpdateEnd(EnemySoundSource.FuneNamihe, FuneNamiheDefinitions.SpitSoundEffect, priority: SoundRequestPriority.Normal);

    /// <summary>Moves the list-table cursor back to idle and restores the species-specific waiting function after the active animation.</summary>
    private static void FinishFuneNamiheActivity(FuneNamiheEnemyState state)
    {
        state.ShowsActiveAnimation = false;
        state.Function = state.IsNamihe
            ? FuneNamiheEnemyFunction.NamiheWaitForSamus
            : FuneNamiheEnemyFunction.FuneWaitForCooldown;
    }

    /// <summary>Binds the shared fireball pre-instruction to 8.8 horizontal travel and inclusive camera culling.</summary>
    internal static void AddFuneNamiheProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddFrameBehavior(context =>
        {
            // Fune/Namihe: directional 8.8 flight and cull.
            RunFuneNamiheFireballFrameBehavior(context.Projectile, context.Camera);
        }, EnemyProjectileFrameBehavior.FuneNamiheFireballFlight);
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
    private const EnemyProjectileFrameBehavior NamiFuneFireballFrameBehavior =
        EnemyProjectileFrameBehavior.FuneNamiheFireballFlight;

    /// <summary>
    /// Runs the shared projectile initializer. The source population's low
    /// parameter-two byte selects one {left,right} speed pair; the fireball flies at the
    /// speed for its direction.
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
            source.Graphics);
        projectile.InstallAnimation(new FuneNamiheFireballAnimation(direction == HorizontalDirection.Right ? FuneNamiheFireballProgram.Right : FuneNamiheFireballProgram.Left));
        projectile.FrameBehavior = NamiFuneFireballFrameBehavior;

        projectile.Position = source.Position;

        // The initializer tests the actor parameter's species nibble, not the projectile
        // definition. Namihe's mouth is four pixels lower than Fune's in the shared art.
        FuneNamiheSpawnSettings sourceSettings = source.SpawnSettings<FuneNamiheSpawnSettings>();
        if (FuneNamiheSetup.Of(sourceSettings).IsNamihe)
            projectile.Position = projectile.Position.WithWrappedWholeY(projectile.Position.Y.Whole + 4);

        var velocities = EnemyFireballLaunches.NamiFuneVelocities(sourceSettings.LaunchIndex);
        projectile.Velocity = new(
            new SignedEightEight(direction == HorizontalDirection.Right ? velocities.Right : velocities.Left),
            new SignedEightEight(0));
    }

    /// <summary>Runs the fireball's per-frame behaviour: the fireball flies sideways at its speed until it leaves the view.</summary>
    internal static void RunFuneNamiheFireballFrameBehavior(
        RoomEnemyProjectileSlot projectile,
        Position16<RoomPixels> camera)
    {
        projectile.Position = projectile.Position.AdvanceX(projectile.Velocity.X.ToSixteenSixteenDelta());

        // The shared cull treats both camera edges as inclusive: an origin exactly
        // at camera+256 survives this frame and is deleted only after crossing beyond it.
        CacatacBehavior.DeleteEnemyProjectileIfOutsideInclusiveViewport(projectile, camera);
    }
}

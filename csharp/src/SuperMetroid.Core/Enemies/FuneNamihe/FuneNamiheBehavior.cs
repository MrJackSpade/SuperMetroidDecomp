using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>The Fune/Namihe species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class FuneNamiheBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Fune, EnemyDefinitionId.Namihe];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new FuneNamiheEntity(this, slot, common);

    /// <summary>Allocates the shared directional fireball fired by either species' animation.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects the family AI to the room's enemy projectile slots.</summary>
    internal FuneNamiheBehavior(EnemyProjectileEngine projectiles, EnemyAudioQueue audio)
    {
        _audio = audio;
        _projectiles = projectiles;
    }

    /// <summary>Carries out one cue of the Fune's and Namihe's animations: the spit sound, a fireball, or finishing the spit.</summary>
    private FuneNamiheSegment? ApplyFuneNamiheCue(RoomEnemySlot slot, FuneNamiheEnemyState state, FuneNamiheCue cue)
    {
        switch (cue.Kind)
        {
            case FuneNamiheCueKind.SpitSound:
                // Shared mouth animation queues the spit sound.
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
                // Left/right animations use duplicate steps with identical state effects.
                FinishFuneNamiheActivity(state);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(FuneNamiheCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>Takes the variant, facing, cooldown and Y proximity from the spawn settings, then starts the corresponding idle animation.</summary>
    internal static void InitializeFuneNamihe(RoomEnemySlot slot)
    {
        FuneNamiheSpawnSettings settings = slot.SpawnSettings<FuneNamiheSpawnSettings>();
        FuneNamiheEnemyState state = new()
        {
            ShowsActiveAnimation = false,
            Action = FuneNamiheAction.FuneWaitForCooldown,
            Variant = settings.Variant,
            YProximity = settings.YProximity,
            CooldownTime = settings.CooldownTime,
            CooldownTimer = 0,
            FacingRight = settings.Facing == HorizontalDirection.Right,
        };

        // The behaviour follows the placement's variant rather than the enemy's species. This sounds redundant, but
        // preserving it lets custom populations give one species the other's behaviour, as the game does.
        if (state.IsNamihe)
            state.Action = FuneNamiheAction.NamiheWaitForSamus;

        slot.SetSpeciesState(state);
        StartFuneNamiheAnimation(slot, state);
    }

    /// <summary>Starts Fune's activity after a wrapping cooldown or Namihe's after strict modular Y proximity; active animations own the return to idle.</summary>
    internal static void RunFuneNamiheMain(
        RoomEnemySlot slot,
        FuneNamiheEnemyState state,
        SamusState? samus)
    {
        switch (state.Action)
        {
            case FuneNamiheAction.FuneWaitForCooldown:
                // The counter steps before the comparison. The comparison tests the sign of the difference rather
                // than comparing unsigned; ordinary room parameters are small,
                // but the distinction remains visible at the 32768 wrap boundary.
                state.CooldownTimer = WrappingCounter.IncrementCount(state.CooldownTimer);
                if (WrappingCounter.IsBehind(state.CooldownTimer, state.CooldownTime))
                    return;

                state.ShowsActiveAnimation = true;
                StartFuneNamiheAnimation(slot, state);
                state.Action = FuneNamiheAction.FuneActivityNoOp;
                state.CooldownTimer = 0;
                return;

            case FuneNamiheAction.NamiheWaitForSamus:
                if (samus is null)
                {
                    throw new InvalidOperationException(
                        "Namihe proximity AI requires the active Samus actor.");
                }

                // The row-proximity test takes the modular absolute difference and
                // uses a strict comparison. Equality at the configured distance stays idle.
                if (!EnemyMath.IsWithinStrictModularDistance(
                        samus.Position.Y.Whole,
                        slot.Position.Y.Whole,
                        state.YProximity))
                {
                    return;
                }

                state.ShowsActiveAnimation = true;
                StartFuneNamiheAnimation(slot, state);
                state.Action = FuneNamiheAction.NamiheActivityNoOp;
                return;

            case FuneNamiheAction.FuneActivityNoOp:
            case FuneNamiheAction.NamiheActivityNoOp:
                // The active animation's finish step is the only owner allowed to leave
                // these inert states. Main AI intentionally does nothing until that step.
                return;

            default:
                throw new InvalidDataException(
                    $"Fune/Namihe has no behaviour for action {state.Action?.ToString() ?? "none"}.");
        }
    }

    /// <summary>
    /// Installs the animation the species, activity and facing select. Both
    /// animation timers are reset together.
    /// </summary>
    private static void StartFuneNamiheAnimation(
        RoomEnemySlot slot,
        FuneNamiheEnemyState state)
    {
        state.Playback = EnemyAnimationPlayback.Start(slot, FuneNamiheAnimations.StartingWith(SelectFuneNamiheAnimation(state)));
    }

    /// <summary>The animation for <paramref name="state"/>'s species, activity and facing.</summary>
    internal static FuneNamiheSegment SelectFuneNamiheAnimation(FuneNamiheEnemyState state) =>
        state.IsNamihe
            ? state.ShowsActiveAnimation
                ? state.FacingRight ? FuneNamiheSegment.NamiheActiveRight : FuneNamiheSegment.NamiheActiveLeft
                : state.FacingRight ? FuneNamiheSegment.NamiheIdleRight : FuneNamiheSegment.NamiheIdleLeft
            : state.ShowsActiveAnimation
                ? state.FacingRight ? FuneNamiheSegment.FuneActiveRight : FuneNamiheSegment.FuneActiveLeft
                : state.FacingRight ? FuneNamiheSegment.FuneIdleRight : FuneNamiheSegment.FuneIdleLeft;

    /// <summary>Publishes the shared enemy-library spit cue for the outer enemy audio queue.</summary>
    private void QueueFuneNamiheSpitSound() =>
        _audio.QueueEnemySound(FuneNamiheDefinitions.SpitSoundEffect, priority: SoundRequestPriority.Normal);

    /// <summary>Moves the animation choice back to idle and restores the species-specific waiting function after the active animation.</summary>
    private static void FinishFuneNamiheActivity(FuneNamiheEnemyState state)
    {
        state.ShowsActiveAnimation = false;
        state.Action = state.IsNamihe
            ? FuneNamiheAction.NamiheWaitForSamus
            : FuneNamiheAction.FuneWaitForCooldown;
    }

    /// <summary>Binds the shared fireball frame behaviour to 8.8 horizontal travel and inclusive camera culling.</summary>
    internal static void AddFuneNamiheProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddFrameBehavior(context =>
        {
            // Fune/Namihe: directional 8.8 flight and cull.
            RunFuneNamiheFireballFrameBehavior(context.Projectile, context.Camera);
        }, EnemyProjectileFrameBehavior.FuneNamiheFireballFlight);
    }

    /// <summary>Runs FuneNamihe's main AI.</summary>
    internal static void RunFuneNamiheMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunFuneNamiheMain(slot, slot.RequireSpeciesState<FuneNamiheEnemyState>(), frame.Samus);
    }

    /// <summary>Advances FuneNamihe's typed animation.</summary>
    internal void AdvanceFuneNamiheTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, FuneNamiheAnimations.Segments,
            static (FuneNamiheEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyFuneNamiheCue(slot, state, cue) is { } target
                ? new AnimationContinuation<FuneNamiheSegment>(FuneNamiheAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);

    /// <summary>Shared projectile movement behaviour installed for both Fune and Namihe fireballs.</summary>
    private const EnemyProjectileFrameBehavior NamiFuneFireballFrameBehavior =
        EnemyProjectileFrameBehavior.FuneNamiheFireballFlight;

    /// <summary>
    /// Runs the shared projectile initializer. The fireball flies at the source placement's launch speed in its
    /// direction.
    /// </summary>
    internal void SpawnFuneNamiheFireball(
        RoomEnemySlot source,
        HorizontalDirection direction,
        RoomEnemyProjectileKind kind)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            kind,
            SpriteGraphics.SpawnedBy(source));
        projectile.InstallAnimation(new FuneNamiheFireballAnimation(direction == HorizontalDirection.Right ? FuneNamiheFireballSegment.Right : FuneNamiheFireballSegment.Left));
        projectile.FrameBehavior = NamiFuneFireballFrameBehavior;

        projectile.Position = source.Position;

        // The initializer tests the placement's variant, not the projectile definition. Namihe's mouth is four
        // pixels lower than Fune's in the shared art.
        FuneNamiheSpawnSettings sourceSettings = source.SpawnSettings<FuneNamiheSpawnSettings>();
        if (sourceSettings.Variant == FuneNamiheVariant.Namihe)
            projectile.Position = projectile.Position.WithWrappedWholeY(projectile.Position.Y.Whole + 4);

        projectile.Velocity = new(
            direction == HorizontalDirection.Right ? sourceSettings.LaunchSpeed : sourceSettings.LaunchSpeed.Negated(),
            SignedEightEight.Zero);
    }

    /// <summary>Runs the fireball's per-frame behaviour: the fireball flies sideways at its speed until it leaves the view.</summary>
    internal static void RunFuneNamiheFireballFrameBehavior(
        EnemyProjectile projectile,
        Position16<RoomPixels> camera)
    {
        projectile.Position = projectile.Position.AdvanceX(projectile.Velocity.X.ToSixteenSixteenDelta());

        // The shared cull treats both camera edges as inclusive: an origin exactly
        // at camera+256 survives this frame and is deleted only after crossing beyond it.
        CacatacBehavior.DeleteEnemyProjectileIfOutsideInclusiveViewport(projectile, camera);
    }
}

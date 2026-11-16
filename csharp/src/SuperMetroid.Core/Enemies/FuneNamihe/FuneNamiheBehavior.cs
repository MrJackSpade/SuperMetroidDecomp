using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>The Fune/Namihe species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class FuneNamiheBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Fune, EnemyDefinitionId.Namihe];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new FuneNamiheEntity(this, place, common);

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
    private FuneNamiheSegment? ApplyFuneNamiheCue(RoomEnemyPlace place, FuneNamiheEnemyState state, FuneNamiheCue cue)
    {
        switch (cue.Kind)
        {
            case FuneNamiheCueKind.SpitSound:
                // Shared mouth animation queues the spit sound.
                QueueFuneNamiheSpitSound();
                break;
            case FuneNamiheCueKind.NamiheFireLeft:
                SpawnFuneNamiheFireball(place, direction: HorizontalDirection.Left, RoomEnemyProjectileKind.NamiheFireball);
                break;
            case FuneNamiheCueKind.NamiheFireRight:
                SpawnFuneNamiheFireball(place, direction: HorizontalDirection.Right, RoomEnemyProjectileKind.NamiheFireball);
                break;
            case FuneNamiheCueKind.FuneFireLeft:
                SpawnFuneNamiheFireball(place, direction: HorizontalDirection.Left, RoomEnemyProjectileKind.FuneFireball);
                break;
            case FuneNamiheCueKind.FuneFireRight:
                SpawnFuneNamiheFireball(place, direction: HorizontalDirection.Right, RoomEnemyProjectileKind.FuneFireball);
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

    /// <summary>Takes the facing, cooldown and Y proximity from the spawn settings, then starts the species' idle animation.</summary>
    internal static void InitializeFuneNamihe(RoomEnemyPlace place)
    {
        FuneNamiheSpawnSettings settings = place.SpawnSettings<FuneNamiheSpawnSettings>();
        bool isNamihe = place.DefinitionId == EnemyDefinitionId.Namihe;
        FuneNamiheEnemyState state = new(isNamihe ? FuneNamiheAction.NamiheWaitForSamus : FuneNamiheAction.FuneWaitForCooldown)
        {
            ShowsActiveAnimation = false,
            IsNamihe = isNamihe,
            YProximity = settings.YProximity,
            CooldownTime = settings.CooldownTime,
            CooldownTimer = 0,
            FacingRight = settings.Facing == HorizontalDirection.Right,
        };

        place.SetSpeciesState(state);
        StartFuneNamiheAnimation(place, state);
    }

    /// <summary>Starts Fune's activity after its cooldown or Namihe's after strict modular Y proximity; active animations own the return to idle.</summary>
    internal static void RunFuneNamiheMain(
        RoomEnemyPlace place,
        FuneNamiheEnemyState state,
        SamusState samus)
    {
        switch (state.Action)
        {
            case FuneNamiheAction.FuneWaitForCooldown:
                // The count steps before the comparison; a cooldown of nothing spits on the first waiting update.
                state.CooldownTimer++;
                if (state.CooldownTimer < state.CooldownTime)
                    return;

                state.ShowsActiveAnimation = true;
                StartFuneNamiheAnimation(place, state);
                state.Action = FuneNamiheAction.Spitting;
                state.CooldownTimer = 0;
                return;

            case FuneNamiheAction.NamiheWaitForSamus:
                // The row-proximity test takes the modular absolute difference and
                // uses a strict comparison. Equality at the configured distance stays idle.
                if (!EnemyMath.IsWithinStrictModularDistance(
                        samus.Position.Y.Whole,
                        place.Position.Y.Whole,
                        state.YProximity))
                {
                    return;
                }

                state.ShowsActiveAnimation = true;
                StartFuneNamiheAnimation(place, state);
                state.Action = FuneNamiheAction.Spitting;
                return;

            case FuneNamiheAction.Spitting:
                // The active animation's finish step is the only owner allowed to leave
                // the spit. Main AI does nothing until that step.
                return;

            default:
                throw new InvalidDataException(
                    $"Fune/Namihe has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>
    /// Installs the animation the species, activity and facing select. Both
    /// animation timers are reset together.
    /// </summary>
    private static void StartFuneNamiheAnimation(
        RoomEnemyPlace place,
        FuneNamiheEnemyState state)
    {
        state.Playback = EnemyAnimationPlayback.Start(place, FuneNamiheAnimations.StartingWith(SelectFuneNamiheAnimation(state)));
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

    /// <summary>Moves the animation choice back to idle and restores the species-specific waiting action after the active animation.</summary>
    private static void FinishFuneNamiheActivity(FuneNamiheEnemyState state)
    {
        state.ShowsActiveAnimation = false;
        state.Action = state.IsNamihe
            ? FuneNamiheAction.NamiheWaitForSamus
            : FuneNamiheAction.FuneWaitForCooldown;
    }

    /// <summary>A Fune or Namihe fireball (#1760): it flies horizontally in 1/256-pixel steps until it leaves the camera.</summary>
    internal static readonly EnemyProjectileBehavior Fireball = new SingleMotionProjectileBehavior(
        context => FlyFuneNamiheFireball(context.Projectile, context.Camera));

    /// <summary>Runs FuneNamihe's main AI.</summary>
    internal static void RunFuneNamiheMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunFuneNamiheMain(place, place.RequireSpeciesState<FuneNamiheEnemyState>(), frame.Samus);
    }

    /// <summary>Advances FuneNamihe's typed animation.</summary>
    internal void AdvanceFuneNamiheTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, FuneNamiheAnimations.Segments,
            static (FuneNamiheEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyFuneNamiheCue(place, state, cue) is { } target
                ? new AnimationContinuation<FuneNamiheSegment>(FuneNamiheAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);

    /// <summary>
    /// Runs the shared projectile initializer. The fireball flies at the source placement's launch speed in its
    /// direction.
    /// </summary>
    internal void SpawnFuneNamiheFireball(
        RoomEnemyPlace source,
        HorizontalDirection direction,
        RoomEnemyProjectileKind kind)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            kind,
            Fireball,
            SpriteGraphics.SpawnedBy(source));
        projectile.InstallAnimation(new FuneNamiheFireballAnimation(direction == HorizontalDirection.Right ? FuneNamiheFireballSegment.Right : FuneNamiheFireballSegment.Left));

        projectile.Position = source.Position;

        // Namihe's mouth is four pixels lower than Fune's in the shared art.
        FuneNamiheSpawnSettings sourceSettings = source.SpawnSettings<FuneNamiheSpawnSettings>();
        if (source.DefinitionId == EnemyDefinitionId.Namihe)
            projectile.Position = projectile.Position.WithWrappedWholeY(projectile.Position.Y.Whole + 4);

        projectile.Velocity = new(
            direction == HorizontalDirection.Right ? sourceSettings.LaunchSpeed : sourceSettings.LaunchSpeed.Negated(),
            SignedEightEight.Zero);
    }

    /// <summary>Flies the fireball one update: it flies sideways at its speed until it leaves the view.</summary>
    internal static void FlyFuneNamiheFireball(
        EnemyProjectile projectile,
        Position16<RoomPixels> camera)
    {
        projectile.Position = projectile.Position.AdvanceX(projectile.Velocity.X.ToSixteenSixteenDelta());
        EnemyView.DeleteOutside(projectile, camera);
    }
}

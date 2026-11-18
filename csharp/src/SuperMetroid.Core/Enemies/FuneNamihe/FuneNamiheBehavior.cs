using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// The Fune and Namihe species owner: their per-room behaviour, the animations and fireball they share, and the
/// interactions they register. A Fune spits on its cooldown, a Namihe when Samus comes level with it (#2063).
/// </summary>
[RoomLifetime]
internal sealed class FuneNamiheBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Fune, EnemyDefinitionId.Namihe];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) =>
        Ensure.NotNull(place).DefinitionId == EnemyDefinitionId.Namihe
            ? new NamiheEntity(this, place, common)
            : new FuneEntity(this, place, common);

    /// <summary>Allocates the shared directional fireball fired by either species' animation.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects the family AI to the room's enemy projectiles.</summary>
    internal FuneNamiheBehavior(EnemyProjectileEngine projectiles, EnemyAudioQueue audio)
    {
        _audio = audio;
        _projectiles = projectiles;
    }

    /// <summary>Carries out one cue of the Fune's and Namihe's animations: the spit sound, a fireball, or finishing the spit.</summary>
    private FuneNamiheSegment? ApplyFuneNamiheCue(RoomEnemyPlace place, FuneNamiheEnemyState state, FuneNamiheCue cue)
    {
        if (cue is FuneNamiheCue.Fire fire)
        {
            SpawnFuneNamiheFireball(place, state, fire.Direction, state.Fireball);
            return null;
        }
        FuneNamiheCueKind kind = ((FuneNamiheCue.Signal)cue).Kind;
        switch (kind)
        {
            case FuneNamiheCueKind.SpitSound:
                // Shared mouth animation queues the spit sound.
                QueueFuneNamiheSpitSound();
                break;
            case FuneNamiheCueKind.FinishActivity:
                // Left/right animations use duplicate steps with identical state effects.
                state.FinishSpit();
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(FuneNamiheCueKind)} {kind}.");
        }
        return null;
    }

    /// <summary>Takes the facing, cooldown and launch speed from the Fune's spawn settings, then starts its idle animation.</summary>
    internal static void InitializeFune(RoomEnemyPlace place)
    {
        FuneSpawnSettings settings = place.SpawnSettings<FuneSpawnSettings>();
        var state = new FuneEnemyState(FuneAction.WaitForCooldown)
        {
            ShowsActiveAnimation = false,
            CooldownTime = settings.CooldownTime,
            CooldownTimer = 0,
            Facing = settings.Facing,
            LaunchSpeed = settings.LaunchSpeed,
        };
        place.SetSpeciesState(state);
        StartFuneNamiheAnimation(place, state);
    }

    /// <summary>Takes the facing, wake distance and launch speed from the Namihe's spawn settings, then starts its idle animation.</summary>
    internal static void InitializeNamihe(RoomEnemyPlace place)
    {
        NamiheSpawnSettings settings = place.SpawnSettings<NamiheSpawnSettings>();
        var state = new NamiheEnemyState(NamiheAction.WaitForSamus)
        {
            ShowsActiveAnimation = false,
            YProximity = settings.YProximity,
            Facing = settings.Facing,
            LaunchSpeed = settings.LaunchSpeed,
        };
        place.SetSpeciesState(state);
        StartFuneNamiheAnimation(place, state);
    }

    /// <summary>Starts the Fune's spit after its cooldown; the active animation owns the return to idle.</summary>
    internal static void RunFuneMain(RoomEnemyPlace place, FuneEnemyState state)
    {
        switch (state.Action)
        {
            case FuneAction.WaitForCooldown:
                // The count steps before the comparison; a cooldown of nothing spits on the first waiting update.
                state.CooldownTimer++;
                if (state.CooldownTimer < state.CooldownTime)
                    return;

                state.ShowsActiveAnimation = true;
                StartFuneNamiheAnimation(place, state);
                state.Action = FuneAction.Spitting;
                state.CooldownTimer = 0;
                return;

            case FuneAction.Spitting:
                // The active animation's finish step is the only owner allowed to leave
                // the spit. Main AI does nothing until that step.
                return;

            default:
                throw new InvalidDataException($"Fune has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Starts the Namihe's spit once Samus is vertically closer than its proximity; the spitting animation returns it to waiting.</summary>
    internal static void RunNamiheMain(RoomEnemyPlace place, NamiheEnemyState state, SamusState samus)
    {
        switch (state.Action)
        {
            case NamiheAction.WaitForSamus:
                // Samus exactly at the proximity distance does not wake the Namihe.
                if (Math.Abs(samus.Position.Y.Whole - place.Position.Y.Whole) >= state.YProximity)
                {
                    return;
                }

                state.ShowsActiveAnimation = true;
                StartFuneNamiheAnimation(place, state);
                state.Action = NamiheAction.Spitting;
                return;

            case NamiheAction.Spitting:
                // The active animation's finish step is the only owner allowed to leave
                // the spit. Main AI does nothing until that step.
                return;

            default:
                throw new InvalidDataException($"Namihe has no behaviour for action {state.Action}.");
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
    internal static FuneNamiheSegment SelectFuneNamiheAnimation(FuneNamiheEnemyState state) => Ensure.NotNull(state).Animation;

    /// <summary>Publishes the shared enemy-library spit cue for the outer enemy audio queue.</summary>
    private void QueueFuneNamiheSpitSound() =>
        _audio.QueueEnemySound(FuneNamiheDefinitions.SpitSoundEffect, priority: SoundRequestPriority.Normal);

    /// <summary>A Fune or Namihe fireball (#1760): it flies horizontally in 1/256-pixel steps until it leaves the camera.</summary>
    internal static readonly EnemyProjectileBehavior Fireball = new SingleMotionProjectileBehavior(
        context => FlyFuneNamiheFireball(context.Projectile, context.Camera));

    /// <summary>Advances a Fune's or Namihe's typed animation, whose state is a <typeparamref name="TState"/>.</summary>
    internal void AdvanceFuneNamiheTypedAnimation<TState>(RoomEnemyPlace place) where TState : FuneNamiheEnemyState =>
        EnemyAnimationPlayback.Advance(place, FuneNamiheAnimations.Segments,
            static (TState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyFuneNamiheCue(place, state, cue) is { } target
                ? new AnimationContinuation<FuneNamiheSegment>(FuneNamiheAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);

    /// <summary>
    /// Runs the shared projectile setup. The fireball flies at the spitter's launch speed in its direction, from its
    /// mouth (<see cref="FuneNamiheEnemyState.MouthDrop"/>).
    /// </summary>
    internal void SpawnFuneNamiheFireball(
        RoomEnemyPlace source,
        FuneNamiheEnemyState state,
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
        if (state.MouthDrop != 0)
            projectile.Position = projectile.Position.WithWholeY(projectile.Position.Y.Whole + state.MouthDrop);

        projectile.Velocity = new(
            direction == HorizontalDirection.Right ? state.LaunchSpeed : state.LaunchSpeed.Negated(),
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

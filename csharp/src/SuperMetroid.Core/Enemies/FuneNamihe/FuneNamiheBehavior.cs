using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// The Fune and Namihe species owner: their per-room behaviour, the animations and fireball they share, and the
/// interactions they register. A Fune spits on its cooldown, a Namihe when Samus comes level with it (#2063).
/// </summary>
[RoomLifetime]
internal sealed partial class FuneNamiheBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. PartFactories.Keys];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        PartFactories[Ensure.NotNull(occupant).DefinitionId](this, occupant, place, common);

    /// <summary>
    /// The factory of each part the species creates, by the definition that places it: the species serves these
    /// definitions, and each builds its own part from the species it is given.
    /// </summary>
    private static readonly Dictionary<EnemyDefinitionId, EnemyPartFactory<FuneNamiheBehavior>> PartFactories =
        new()
        {
            [EnemyDefinitionId.Fune] = static (species, occupant, place, common) => EnemyOccupant.Of(new FuneEntity(species, occupant, place, common)),
            [EnemyDefinitionId.Namihe] = static (species, occupant, place, common) => EnemyOccupant.Of(new NamiheEntity(species, occupant, place, common)),
        };

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
    private void ApplyFuneNamiheCue(RoomEnemyOccupant occupant, RoomEnemyPlace place, FuneNamiheEnemyState state, FuneNamiheCue cue)
    {
        if (cue is FuneNamiheCue.Fire fire)
        {
            SpawnFuneNamiheFireball(occupant, place, state, fire.Direction, state.Fireball);
            return;
        }
        FuneNamiheCueKind kind = ((FuneNamiheCue.Signal)cue).Kind;
        switch (kind)
        {
            case FuneNamiheCueKind.SpitSound:
                // Shared mouth animation queues the spit sound.
                QueueFuneNamiheSpitSound();
                break;
            case FuneNamiheCueKind.FinishActivity:
                // The spit ends the activity whichever way the Fune faces.
                state.FinishSpit();
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(FuneNamiheCueKind)} {kind}.");
        }
    }

    /// <summary>Starts the Fune waiting on its cooldown and its idle animation, facing as its spawn settings say.</summary>
    internal static void InitializeFune(RoomEnemyOccupant occupant, RoomEnemyPlace place)
    {
        var state = new FuneEnemyState(FuneAction.WaitForCooldown) { Cooldown = CooldownOf(occupant) };
        place.SetSpeciesState(state);
        StartFuneNamiheAnimation(occupant, place, state);
    }

    /// <summary>Starts the Namihe waiting for Samus and its idle animation, facing as its spawn settings say.</summary>
    internal static void InitializeNamihe(RoomEnemyOccupant occupant, RoomEnemyPlace place)
    {
        var state = new NamiheEnemyState(NamiheAction.WaitForSamus);
        place.SetSpeciesState(state);
        StartFuneNamiheAnimation(occupant, place, state);
    }

    /// <summary>
    /// The Fune's wait between spits, as its spawn settings give it: the cooldown time's updates, the last the one it spits
    /// on; a cooldown of nothing spits on the first waiting update, as a cooldown of one does.
    /// </summary>
    internal static UpdateCountdown CooldownOf(RoomEnemyOccupant occupant) =>
        UpdateCountdown.Lasting(Math.Max(1, occupant.SpawnSettings<FuneSpawnSettings>().CooldownTime));

    /// <summary>Starts the Fune's spit after its cooldown; the active animation owns the return to idle.</summary>
    internal static void RunFuneMain(RoomEnemyOccupant occupant, RoomEnemyPlace place, FuneEnemyState state)
    {
        switch (state.Action)
        {
            case FuneAction.WaitForCooldown:
                if (!state.Cooldown.Count())
                    return;

                state.Action = FuneAction.Spitting;
                StartFuneNamiheAnimation(occupant, place, state);
                state.Cooldown = CooldownOf(occupant);
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
    internal static void RunNamiheMain(RoomEnemyOccupant occupant, RoomEnemyPlace place, NamiheEnemyState state, SamusState samus)
    {
        switch (state.Action)
        {
            case NamiheAction.WaitForSamus:
                // Samus exactly at the proximity distance does not wake the Namihe.
                if (Math.Abs(samus.Position.Y.Whole - place.Position.Y.Whole) >= occupant.SpawnSettings<NamiheSpawnSettings>().YProximity)
                {
                    return;
                }

                state.Action = NamiheAction.Spitting;
                StartFuneNamiheAnimation(occupant, place, state);
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
    /// Starts, from its first frame, the animation the species, activity and facing select.
    /// </summary>
    private static void StartFuneNamiheAnimation(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        FuneNamiheEnemyState state)
    {
        state.Playback = EnemyAnimationPlayback.Start(place, FuneNamiheAnimations.StartingWith(SelectFuneNamiheAnimation(occupant, state)));
    }

    /// <summary>The animation for <paramref name="state"/>'s species and activity, facing as <paramref name="place"/>'s spawn settings say.</summary>
    internal static FuneNamiheSegment SelectFuneNamiheAnimation(RoomEnemyOccupant occupant, FuneNamiheEnemyState state) =>
        Ensure.NotNull(state).Animation(occupant.SpawnSettings<FuneNamiheSpawnSettings>().Facing);

    /// <summary>Publishes the shared enemy-library spit cue for the outer enemy audio queue.</summary>
    private void QueueFuneNamiheSpitSound() =>
        _audio.QueueEnemySound(FuneNamiheDefinitions.SpitSoundEffect, priority: SoundRequestPriority.Normal);

    /// <summary>A Fune or Namihe fireball (#1760): it flies horizontally at its speed until it leaves the camera.</summary>
    internal static readonly EnemyProjectileBehavior Fireball = new SingleMotionProjectileBehavior(
        context => FlyFuneNamiheFireball(context.Projectile, context.Camera));

    /// <summary>Advances a Fune's or Namihe's typed animation, whose state is a <typeparamref name="TState"/>.</summary>
    internal void AdvanceFuneNamiheTypedAnimation<TState>(RoomEnemyOccupant occupant, RoomEnemyPlace place) where TState : FuneNamiheEnemyState =>
        EnemyAnimationPlayback.AdvanceApplyingCues(place, FuneNamiheAnimations.Segments,
            static (TState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (TState state, FuneNamiheCue cue) => ApplyFuneNamiheCue(occupant, place, state, cue));

    /// <summary>
    /// Runs the shared projectile setup. The fireball flies at the spitter's spawn settings' launch speed in its direction,
    /// from its mouth (<see cref="FuneNamiheEnemyState.MouthDrop"/>).
    /// </summary>
    internal void SpawnFuneNamiheFireball(
        RoomEnemyOccupant occupant, RoomEnemyPlace source,
        FuneNamiheEnemyState state,
        HorizontalDirection direction,
        RoomEnemyProjectileKind kind)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(kind, Fireball, SpriteGraphics.SpawnedBy(occupant, source.Shading));
        if (projectile is null)
            return;

        projectile.InstallAnimation(new FuneNamiheFireballAnimation(direction == HorizontalDirection.Right ? FuneNamiheFireballSegment.Right : FuneNamiheFireballSegment.Left));

        projectile.Position = source.Position;

        // Namihe's mouth is lower than Fune's in the shared art.
        if (state.MouthDrop != 0)
            projectile.Position = projectile.Position.WithWholeY(projectile.Position.Y.Whole + state.MouthDrop);

        SignedEightEight launchSpeed = occupant.SpawnSettings<FuneNamiheSpawnSettings>().LaunchSpeed;
        projectile.Velocity = new(
            launchSpeed.Toward(direction),
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

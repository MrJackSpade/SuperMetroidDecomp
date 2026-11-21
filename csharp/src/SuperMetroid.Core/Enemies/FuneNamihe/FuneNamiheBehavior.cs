using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Projectiles;
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
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= new Dictionary<EnemyDefinitionId, EnemyEntityCreation>
    {
        [EnemyDefinitionId.Fune] = (occupant, place, common) => EnemyOccupant.Of(new FuneEntity(this, occupant, place, common)),
        [EnemyDefinitionId.Namihe] = (occupant, place, common) => EnemyOccupant.Of(new NamiheEntity(this, occupant, place, common)),
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

    /// <summary>What <see cref="ApplyFuneNamiheCue"/> does for each <see cref="FuneNamiheCueKind"/> member, each member's own arm.</summary>
    private IdentityTable<FuneNamiheCueKind, Action<FuneNamiheEnemyState>> ApplyFuneNamiheCueForFuneNamiheCueKind => field ??= new(new Dictionary<FuneNamiheCueKind, Action<FuneNamiheEnemyState>>
    {
        [FuneNamiheCueKind.SpitSound] = state =>
        {
            // Shared mouth animation queues the spit sound.
            QueueFuneNamiheSpitSound();
        },
        [FuneNamiheCueKind.FinishActivity] = state =>
        {
            // The spit ends the activity whichever way the Fune faces.
            state.FinishSpit();
        },
    });

    /// <summary>Carries out one cue of the Fune's and Namihe's animations: the spit sound, a fireball, or finishing the spit.</summary>
    private void ApplyFuneNamiheCue(RoomEnemyOccupant occupant, RoomEnemyPlace place, FuneNamiheEnemyState state, FuneNamiheCue cue)
    {
        if (cue is FuneNamiheCue.Fire fire)
        {
            SpawnFuneNamiheFireball(occupant, place, state, fire.Direction, state.Fireball);
            return;
        }
        FuneNamiheCueKind kind = ((FuneNamiheCue.Signal)cue).Kind;
        ApplyFuneNamiheCueForFuneNamiheCueKind[kind](state);
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
    private static UpdateCountdown CooldownOf(RoomEnemyOccupant occupant) =>
        UpdateCountdown.Lasting(Math.Max(1, occupant.SpawnSettings<FuneSpawnSettings>().CooldownTime));

    /// <summary>What <see cref="RunFuneMain"/> does for each <see cref="FuneAction"/> member, each member's own arm.</summary>
    private static IdentityTable<FuneAction, Action<RoomEnemyOccupant, RoomEnemyPlace, FuneEnemyState>> RunFuneMainForFuneAction => field ??= new(new Dictionary<FuneAction, Action<RoomEnemyOccupant, RoomEnemyPlace, FuneEnemyState>>
    {
        [FuneAction.WaitForCooldown] = static (occupant, place, state) =>
        {
            if (!state.Cooldown.Count())
                return;

            state.Action = FuneAction.Spitting;
            StartFuneNamiheAnimation(occupant, place, state);
            state.Cooldown = CooldownOf(occupant);
        },
        [FuneAction.Spitting] = static (_, place, state) =>
        {
            // The active animation's finish step is the only owner allowed to leave
            // the spit. Main AI does nothing until that step.
        },
    });

    /// <summary>Starts the Fune's spit after its cooldown; the active animation owns the return to idle.</summary>
    internal static void RunFuneMain(RoomEnemyOccupant occupant, RoomEnemyPlace place, FuneEnemyState state)
    {
        RunFuneMainForFuneAction[state.Action](occupant, place, state);
    }

    /// <summary>What <see cref="RunNamiheMain"/> does for each <see cref="NamiheAction"/> member, each member's own arm.</summary>
    private static IdentityTable<NamiheAction, Action<RoomEnemyOccupant, RoomEnemyPlace, NamiheEnemyState, SamusState>> RunNamiheMainForNamiheAction => field ??= new(new Dictionary<NamiheAction, Action<RoomEnemyOccupant, RoomEnemyPlace, NamiheEnemyState, SamusState>>
    {
        [NamiheAction.WaitForSamus] = static (occupant, place, state, samus) =>
        {
            // Samus exactly at the proximity distance does not wake the Namihe.
            if (Math.Abs(samus.Position.Y.Whole - place.Position.Y.Whole) >= occupant.SpawnSettings<NamiheSpawnSettings>().YProximity)
            {
                return;
            }

            state.Action = NamiheAction.Spitting;
            StartFuneNamiheAnimation(occupant, place, state);
        },
        [NamiheAction.Spitting] = static (_, place, state, samus) =>
        {
            // The active animation's finish step is the only owner allowed to leave
            // the spit. Main AI does nothing until that step.
        },
    });

    /// <summary>Starts the Namihe's spit once Samus is vertically closer than its proximity; the spitting animation returns it to waiting.</summary>
    internal static void RunNamiheMain(RoomEnemyOccupant occupant, RoomEnemyPlace place, NamiheEnemyState state, SamusState samus)
    {
        RunNamiheMainForNamiheAction[state.Action](occupant, place, state, samus);
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
    private static readonly EnemyProjectileBehavior Fireball = new SingleMotionProjectileBehavior(
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
    private void SpawnFuneNamiheFireball(
        RoomEnemyOccupant occupant, RoomEnemyPlace source,
        FuneNamiheEnemyState state,
        HorizontalDirection direction,
        RoomEnemyProjectileKind kind)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(kind, Fireball, SpriteGraphics.SpawnedBy(occupant, source.Shading), new FuneNamiheFireballAnimation(direction == HorizontalDirection.Right ? FuneNamiheFireballSegment.Right : FuneNamiheFireballSegment.Left));
        if (projectile is null)
            return;

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

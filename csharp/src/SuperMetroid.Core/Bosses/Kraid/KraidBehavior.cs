using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The Kraid species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed partial class KraidBehavior : IEnemySpecies, IEnemySpawnFactory<EnemySpawnSettings>
{
    /// <inheritdoc/>
    [field: NonSerialized]
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Kraid];

    /// <summary>
    /// Kraid's body, built the same way wherever it is set up; his seven other parts join the encounter his body offers
    /// them as it sets up (<see cref="BeginKraid"/>), a respawned fingernail joining it again.
    /// </summary>
    /// <param name="registrar">The room's registry being composed.</param>
    public void OfferCreations(EnemySpeciesRegistrar registrar)
    {
        Ensure.NotNull(registrar).Spawns<EnemySpawnSettings>(Definitions, Create);
        registrar.Joins([
            EnemyDefinitionId.KraidArm, EnemyDefinitionId.KraidFoot, EnemyDefinitionId.KraidLintBottom, EnemyDefinitionId.KraidLintMiddle,
            EnemyDefinitionId.KraidLintTop, EnemyDefinitionId.KraidFirstNail, EnemyDefinitionId.KraidSecondNail,
        ]);
    }

    /// <summary>Where his body offers his other parts the creation that builds each of them joining his encounter.</summary>
    private readonly EnemyPartOffers _partOffers;

    /// <summary>
    /// Sets his body up in <paramref name="body"/>, beginning the encounter, and offers each of his other parts, which the
    /// room lists after his body, the creation that builds it joining that encounter (#2218, #2554).
    /// </summary>
    /// <returns>The encounter, which the body holds from its setup.</returns>
    internal KraidEnemyState BeginKraid(RoomEnemyPlace body)
    {
        KraidEnemyState state = Rise.InitializeKraidBody(body);
        var joining = new PartJoining(this, state);
        _partOffers.Offer<EnemySpawnSettings>(EnemyDefinitionId.KraidArm, joining.CreateArm);
        _partOffers.Offer<EnemySpawnSettings>(EnemyDefinitionId.KraidFoot, joining.CreateFoot);
        _partOffers.Offer<EnemySpawnSettings>(EnemyDefinitionId.KraidLintTop, joining.CreateTopLint);
        _partOffers.Offer<EnemySpawnSettings>(EnemyDefinitionId.KraidLintMiddle, joining.CreateMiddleLint);
        _partOffers.Offer<EnemySpawnSettings>(EnemyDefinitionId.KraidLintBottom, joining.CreateBottomLint);
        _partOffers.Offer<EnemySpawnSettings>(EnemyDefinitionId.KraidFirstNail, joining.CreateFirstNail);
        _partOffers.Offer<EnemySpawnSettings>(EnemyDefinitionId.KraidSecondNail, joining.CreateSecondNail);
        return state;
    }

    /// <summary>
    /// The creations his body offers his other parts (#2554): each joining the encounter his body began, in the place his
    /// body found it in as his room lists it.
    /// </summary>
    /// <param name="species">The Kraid species owner.</param>
    /// <param name="encounter">The encounter his body began.</param>
    private sealed partial class PartJoining(KraidBehavior species, KraidEnemyState encounter)
    {
        /// <summary>His arm, joining his encounter.</summary>
        internal EnemyOccupant CreateArm(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
            EnemyOccupant.Running(KraidArmEntity.SetUp(species, occupant, Listed(EnemyDefinitionId.KraidArm, encounter.Parts.Arm, place), common, encounter));

        /// <summary>His foot, joining his encounter.</summary>
        internal EnemyOccupant CreateFoot(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
            EnemyOccupant.Running(KraidFootEntity.SetUp(species, occupant, Listed(EnemyDefinitionId.KraidFoot, encounter.Parts.Foot, place), common, encounter));

        /// <summary>His top lint, joining his encounter.</summary>
        internal EnemyOccupant CreateTopLint(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
            EnemyOccupant.Running(KraidLintEntity.SetUp(species, occupant, Listed(EnemyDefinitionId.KraidLintTop, encounter.Parts.TopLint, place), common, KraidLintPart.Top, encounter));

        /// <summary>His middle lint, joining his encounter.</summary>
        internal EnemyOccupant CreateMiddleLint(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
            EnemyOccupant.Running(KraidLintEntity.SetUp(species, occupant, Listed(EnemyDefinitionId.KraidLintMiddle, encounter.Parts.MiddleLint, place), common, KraidLintPart.Middle, encounter));

        /// <summary>His bottom lint, joining his encounter.</summary>
        internal EnemyOccupant CreateBottomLint(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
            EnemyOccupant.Running(KraidLintEntity.SetUp(species, occupant, Listed(EnemyDefinitionId.KraidLintBottom, encounter.Parts.BottomLint, place), common, KraidLintPart.Bottom, encounter));

        /// <summary>His first fingernail, joining his encounter.</summary>
        internal EnemyOccupant CreateFirstNail(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
            EnemyOccupant.Running(KraidNailEntity.SetUp(species, occupant, Listed(EnemyDefinitionId.KraidFirstNail, encounter.Parts.FirstNail, place), common, KraidNailPart.First, encounter));

        /// <summary>His second fingernail, joining his encounter.</summary>
        internal EnemyOccupant CreateSecondNail(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
            EnemyOccupant.Running(KraidNailEntity.SetUp(species, occupant, Listed(EnemyDefinitionId.KraidSecondNail, encounter.Parts.SecondNail, place), common, KraidNailPart.Second, encounter));

        /// <summary>
        /// The place a part is built in, <paramref name="place"/>, checked before the part is built to be where his body found
        /// it as his room lists it.
        /// </summary>
        /// <exception cref="InvalidDataException">The room lists the part elsewhere than his body found it.</exception>
        private static RoomEnemyPlace Listed(EnemyDefinitionId part, RoomEnemyPlace listed, RoomEnemyPlace place) => place == listed
            ? place
            : throw new InvalidDataException($"Kraid's {part} set up as enemy {place.Ordinal}, but his body lists enemy {listed.Ordinal} as it.");
    }

    /// <summary>Kraid's body, which joins its own reference as it is set up (#1877, #2146).</summary>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        EnemyOccupant.Running(new KraidBodyEntity(this, occupant, place, common));
    /// <summary>Kraid's body layer: the body frame he shows, drawn from his installed body sheet.</summary>
    internal KraidBodyLayer BodyLayer { get; }

    /// <summary>Kraid's arrival: his room setup, the rise through the floor and his growth into the second phase.</summary>
    private KraidRise Rise { get; }

    /// <summary>Kraid's part animations and states, random numbers and his sound and music requests.</summary>
    internal KraidEncounterAccess Access { get; }


    /// <summary>Kraid's room background: its fade, color transitions and the ceiling rocks.</summary>
    internal KraidBackground Background { get; }

    /// <summary>Kraid's head: the mouth-open attack and reaction, the eye glow and the health colours.</summary>
    internal KraidHead Head { get; }

    /// <summary>Kraid's death: the sink below the floor, hiding his body and the background fade.</summary>
    internal KraidDeath Death { get; }

    /// <summary>Kraid's arm and foot: their setup, animations, lunges, retreats and walks.</summary>
    internal KraidLimbs Limbs { get; }

    /// <summary>Kraid's lints, nails and spat rocks.</summary>
    internal KraidProjectiles Projectiles { get; }

    /// <summary>Kraid's hitboxes and touch, and what Samus's weapons do to his mouth and body.</summary>
    internal KraidCombat Combat { get; }
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;

    /// <summary>Binds Kraid's room systems and shared actor services.</summary>
    internal KraidBehavior(KraidBehaviorDependencies dependencies)
    {
        _bossArtwork = dependencies.Presentation.Artwork;
        _partOffers = dependencies.PartOffers;
        Access = new KraidEncounterAccess(dependencies.Room, dependencies.Audio);
        BodyLayer = new KraidBodyLayer(_bossArtwork);
        Projectiles = new KraidProjectiles(dependencies.Room, dependencies.Terrain, dependencies.Collision, dependencies.DeathsAndDrops, Access);
        Background = new KraidBackground(dependencies.Projectiles, dependencies.Presentation.Surfaces, Access, _bossArtwork, Projectiles.Rocks);
        Head = new KraidHead(dependencies.Room, dependencies.Projectiles, Access, Background, Projectiles.SpitRock);
        Rise = new KraidRise(new KraidRiseDependencies { PassEvents = dependencies.PassEvents, Room = dependencies.Room, Projectiles = dependencies.Projectiles, Earthquake = dependencies.Earthquake, Places = dependencies.Places, Presentation = dependencies.Presentation.Surfaces, CameraDistance = dependencies.CameraDistance, Access = Access, Background = Background, Head = Head, BodyLayer = BodyLayer });
        Death = new KraidDeath(new KraidDeathDependencies { PassEvents = dependencies.PassEvents, Room = dependencies.Room, Earthquake = dependencies.Earthquake, Presentation = dependencies.Presentation.Surfaces, DeathsAndDrops = dependencies.DeathsAndDrops, CameraDistance = dependencies.CameraDistance, Access = Access, Background = Background, Head = Head });
        Limbs = new KraidLimbs(dependencies.Room, dependencies.Earthquake, Access);
        Combat = new KraidCombat(dependencies.Projectiles, dependencies.Audio, dependencies.EffectActors, dependencies.Collision);
    }

    /// <summary>
    /// Runs Kraid's body for one update: mouth-projectile collision, colour handling,
    /// body-projectile collision and body-vs-Samus collision precede the phase. A
    /// killing mouth hit therefore reaches colour handling, which settles his hurt flash on his health band's colours,
    /// before the death setup runs in this same update.
    /// </summary>
    internal void RunKraidBodyMain(
        RoomEnemyOccupant kraid,
        KraidEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew,
        Position16<RoomPixels> camera,
        SamusProjectileSystem samusProjectiles,
        SamusBombProjectileSystem samusBombs)
    {
        RoomEnemyPlace body = kraid.Place;
        IReadOnlyList<KraidPassProjectile> tested = TestedByShotPasses(samusProjectiles, samusBombs);
        Combat.ResolveKraidMouthProjectileHits(kraid, state, tested);
        Head.RunKraidHurtColours(body, state);
        Combat.ResolveKraidBodyProjectileHits(body, state, tested);
        Combat.ResolveKraidBodyContact(kraid, state, samus, pseudoScrew);
        RunKraidBodyStage(body, state, camera.X, samus);
    }

    /// <summary>Runs Kraid's main AI for his body in <paramref name="place"/>.</summary>
    internal void RunKraidMainAi(RoomEnemyOccupant kraid, KraidEnemyState state, IEnemyAiFrame frame)
    {
        _ = kraid.Place;
        RunKraidBodyMain(kraid, state, frame.Samus, frame.PseudoScrew, frame.Camera, frame.SamusProjectiles, frame.SamusBombs);
    }

    /// <summary>
    /// The projectiles both of Kraid's shot passes test this update, in the order they test them
    /// (<see cref="KraidShotPassReach"/>): from the place Samus's shot count names down to her first shot, her first bomb
    /// standing past the last shot place.
    /// </summary>
    /// <param name="projectiles">Samus's shots.</param>
    /// <param name="bombs">Samus's bombs.</param>
    internal static IReadOnlyList<KraidPassProjectile> TestedByShotPasses(
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem bombs)
    {
        Ensure.NotNull(projectiles);
        Ensure.NotNull(bombs);
        return KraidShotPassReach.Tested<KraidPassProjectile>(
            projectiles.Pool.ShotCount.Count,
            [.. projectiles.Shots.Select(shot => new KraidPassProjectile.Shot(shot))],
            new KraidPassProjectile.Bomb(bombs.FirstBomb));
    }

    /// <summary>One projectile one of Kraid's shot passes tests (#2400): a shot of Samus's, or her first bomb.</summary>
    internal abstract partial record KraidPassProjectile
    {
        /// <summary>Closes the hierarchy to the shot and the bomb.</summary>
        private KraidPassProjectile()
        {
        }

        /// <summary>
        /// Whether a projectile holds the tested place (#2558). A pass reaches the place past Samus's counted shots and her
        /// first bomb whether or not either holds one; an empty place lies at rest at the room's origin with no extent,
        /// which Kraid's mouth and body, standing in his room's lower half, never meet, so a pass skips it.
        /// </summary>
        /// <value>
        /// The position, radius, type and damage of the projectile holding the place and the marking of its collision with
        /// Kraid, or null while none holds it.
        /// </value>
        public abstract KraidHeldPassProjectile? Held { get; }

        /// <summary>A shot the passes test, in one of Samus's shot places.</summary>
        internal sealed partial record Shot : KraidPassProjectile
        {
            /// <summary>The shot the passes test.</summary>
            private readonly SamusProjectile _shot;

            /// <summary>Tests <paramref name="shot"/>.</summary>
            /// <param name="shot">The projectile in one of Samus's shot places.</param>
            internal Shot(SamusProjectile shot) => _shot = Ensure.NotNull(shot);

            /// <inheritdoc/>
            public override KraidHeldPassProjectile? Held => _shot.Occupant is { } held
                ? new(new(held.Position.Whole, held.Radius, held.Type, held.Damage, IsPlacedBomb: false), held.Spend)
                : null;
        }

        /// <summary>The bomb the passes test past the last shot place: Samus's first bomb.</summary>
        internal sealed partial record Bomb : KraidPassProjectile
        {
            /// <summary>The bomb the passes test.</summary>
            private readonly SamusBomb _bomb;

            /// <summary>Tests <paramref name="bomb"/>.</summary>
            /// <param name="bomb">Samus's first bomb.</param>
            internal Bomb(SamusBomb bomb) => _bomb = Ensure.NotNull(bomb);

            /// <inheritdoc/>
            public override KraidHeldPassProjectile? Held => _bomb.Occupant is { } placed
                ? new(new(placed.Position.Whole, placed.Radius, placed.Type, placed.Damage, IsPlacedBomb: placed.SpreadFlight is null), placed.Spend)
                : null;
        }
    }

    /// <summary>Projectile geometry and attack data read by Kraid's mouth and body collision passes.</summary>
    /// <param name="Position">The projectile's whole-pixel room position.</param>
    /// <param name="Radius">The projectile's collision half-extents.</param>
    /// <param name="Type">The projectile's type: its family, beams and charge.</param>
    /// <param name="Damage">The projectile's damage before Kraid's vulnerability multiplier; a copy of the place's non-negative damage.</param>
    /// <param name="IsPlacedBomb">Whether the projectile is a placed bomb or Power Bomb rather than a fired or spread one.</param>
    internal readonly partial record struct KraidCollisionShot(Position16<RoomPixels> Position, Extent16<RoomPixels> Radius, SamusProjectileType Type, int Damage, bool IsPlacedBomb);

    /// <summary>The projectile holding a place one of Kraid's passes tests, and the marking of its collision with him.</summary>
    /// <param name="Shot">Its position, radius, type and damage.</param>
    /// <param name="MarkCollision">Marks the projectile holding the place as having collided with Kraid.</param>
    internal readonly partial record struct KraidHeldPassProjectile(KraidCollisionShot Shot, Action MarkCollision);


    /// <summary>Runs the phase of the stage the body is in: his rise, his fight, his growth or his death.</summary>
    internal void RunKraidBodyStage(
        RoomEnemyPlace body,
        KraidEnemyState state,
        int cameraX,
        SamusState samus) =>
        Ensure.NotNull(state).BodyState.Run.RunWith(new StageRunner(this, body, state, cameraX, samus));

    /// <summary>Runs one update of the stage Kraid's body is in, with what each stage's phase reads.</summary>
    /// <param name="behavior">Kraid's behaviour, whose parts run the phases.</param>
    /// <param name="body">The place holding his body.</param>
    /// <param name="state">His encounter's state.</param>
    /// <param name="cameraX">The camera's horizontal room position, which the sunk body's repair reads.</param>
    /// <param name="samus">Samus, whom his rise and death restrict.</param>
    private sealed partial class StageRunner(KraidBehavior behavior, RoomEnemyPlace body, KraidEnemyState state, int cameraX, SamusState samus)
        : IKraidStageRunner
    {
        /// <inheritdoc/>
        public void Run(KraidStageRun.Rising rising) => behavior.Rise.RunKraidRisePhase(body, state, rising, samus);

        /// <inheritdoc/>
        public void Run(KraidStageRun.Fighting fight) => behavior.RunKraidFightPhase(body, state, fight);

        /// <inheritdoc/>
        public void Run(KraidStageRun.Growing growth) => behavior.Rise.RunKraidGrowthPhase(body, state, growth);

        /// <inheritdoc/>
        public void Run(KraidStageRun.Dying dying) => behavior.Death.RunKraidDeathPhase(body, state, dying, samus);

        /// <inheritdoc/>
        public void Run(KraidStageRun.Sunk sunk) => behavior.Death.RunKraidSunkPhase(body, state, sunk, cameraX);
    }

    /// <summary>Runs the body's first- or second-phase fight phase.</summary>
    internal void RunKraidFightPhase(RoomEnemyPlace body, KraidEnemyState state, KraidStageRun.Fighting fight)
    {
        switch (fight.Phase)
        {
            case KraidFightPhase.FirstPhaseThinking:
                if (state.ThinkingWait.StepRunsOut())
                {
                    fight.Phase = KraidFightPhase.MainAttackWithMouthOpen;
                    fight.Head.Start(KraidHeadSegment.Roar, state.HeadHold);
                }
                return;

            case KraidFightPhase.MainAttackWithMouthOpen:
                Head.RunKraidMouthOpenAttack(body, state, fight);
                return;

            case KraidFightPhase.MouthOpenReaction:
                Head.RunKraidMouthOpenReaction(state, fight);
                return;

            case KraidFightPhase.InitializeEyeGlow:
                Head.InitializeKraidEyeGlow(state, fight);
                return;

            case KraidFightPhase.GlowEye:
                Head.GlowKraidEye(state, fight);
                return;

            case KraidFightPhase.UnglowEye:
                Head.UnglowKraidEye(body, state, fight);
                return;

            case KraidFightPhase.WaitBeforeEyeGlow:
                if (state.BodyState.PhaseWait.StepRunsOut())
                    fight.Phase = KraidFightPhase.InitializeEyeGlow;
                return;

            case KraidFightPhase.HeadAnimationBeforeGrowth:
                Head.AdvanceKraidHeadAnimation(state, fight.Head);
                if (state.BodyState.PhaseWait.StepRunsOut())
                    state.BodyState.Run = new KraidStageRun.Growing(KraidGrowthPhase.ReleaseCamera, fight.Head);
                return;

            case KraidFightPhase.SecondPhaseThinking:
                KraidLimbs.RunKraidSecondPhaseThinking(state, fight);
                return;

            default:
                throw new InvalidDataException($"Undefined {nameof(KraidFightPhase)} {fight.Phase}.");
        }
    }
}

using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The Kraid species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed partial class KraidBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    [field: NonSerialized]
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= new Dictionary<EnemyDefinitionId, EnemyEntityCreation>
    {
        [EnemyDefinitionId.Kraid] = (occupant, place, common) => EnemyOccupant.Of(new KraidBodyEntity(this, occupant, place, common)),
        [EnemyDefinitionId.KraidArm] = (occupant, place, common) => EnemyOccupant.Of(new KraidArmEntity(this, occupant, place, common)),
        [EnemyDefinitionId.KraidFoot] = (occupant, place, common) => EnemyOccupant.Of(new KraidFootEntity(this, occupant, place, common)),
        [EnemyDefinitionId.KraidLintBottom] = (occupant, place, common) => EnemyOccupant.Of(new KraidLintEntity(this, occupant, place, common, Parts.BottomLintReference, KraidLintPart.Bottom)),
        [EnemyDefinitionId.KraidLintMiddle] = (occupant, place, common) => EnemyOccupant.Of(new KraidLintEntity(this, occupant, place, common, Parts.MiddleLintReference, KraidLintPart.Middle)),
        [EnemyDefinitionId.KraidLintTop] = (occupant, place, common) => EnemyOccupant.Of(new KraidLintEntity(this, occupant, place, common, Parts.TopLintReference, KraidLintPart.Top)),
        [EnemyDefinitionId.KraidFirstNail] = (occupant, place, common) => EnemyOccupant.Of(new KraidNailEntity(this, occupant, place, common, Parts.FirstNailReference, KraidNailPart.First)),
        [EnemyDefinitionId.KraidSecondNail] = (occupant, place, common) => EnemyOccupant.Of(new KraidNailEntity(this, occupant, place, common, Parts.SecondNailReference, KraidNailPart.Second)),
    };

    /// <summary>Kraid's body layer: the body frame he shows, drawn from his installed body sheet.</summary>
    internal KraidBodyLayer BodyLayer { get; }

    /// <summary>Kraid's arrival: his room setup, the rise through the floor and his growth into the second phase.</summary>
    internal KraidRise Rise { get; }

    /// <summary>Kraid's part animations and states, random numbers and his sound and music requests.</summary>
    internal KraidEncounterAccess Access { get; }

    /// <summary>Kraid's eight parts in his room, in the order they run.</summary>
    internal KraidParts Parts { get; }

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
        Parts = new KraidParts();
        Access = new KraidEncounterAccess(dependencies.Room, dependencies.Audio);
        BodyLayer = new KraidBodyLayer(_bossArtwork, Parts);
        Projectiles = new KraidProjectiles(dependencies.Room, dependencies.Terrain, Parts, dependencies.Collision, dependencies.DeathsAndDrops, Access);
        Background = new KraidBackground(dependencies.Projectiles, dependencies.Presentation.Surfaces, Access, _bossArtwork, Projectiles.Rocks);
        Head = new KraidHead(dependencies.Room, dependencies.Projectiles, Parts, Access, Background, Projectiles.SpitRock);
        Rise = new KraidRise(new KraidRiseDependencies { PassEvents = dependencies.PassEvents, Room = dependencies.Room, Projectiles = dependencies.Projectiles, Earthquake = dependencies.Earthquake, Parts = Parts, Presentation = dependencies.Presentation.Surfaces, CameraDistance = dependencies.CameraDistance, Access = Access, Background = Background, Head = Head, BodyLayer = BodyLayer });
        Death = new KraidDeath(new KraidDeathDependencies { PassEvents = dependencies.PassEvents, Room = dependencies.Room, Earthquake = dependencies.Earthquake, Parts = Parts, Presentation = dependencies.Presentation.Surfaces, DeathsAndDrops = dependencies.DeathsAndDrops, CameraDistance = dependencies.CameraDistance, Access = Access, Background = Background, Head = Head });
        Limbs = new KraidLimbs(dependencies.Room, dependencies.Earthquake, Parts, Access);
        Combat = new KraidCombat(dependencies.Projectiles, dependencies.Audio, dependencies.EffectActors, Parts, dependencies.Collision);
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

    /// <summary>What <see cref="RunKraidBodyStage"/> does for each <see cref="KraidBodyStage"/> member, each member's own arm.</summary>
    [field: NonSerialized]
    private IdentityTable<KraidBodyStage, Action<RoomEnemyPlace, KraidEnemyState, int, SamusState>> RunKraidBodyStageForKraidBodyStage => field ??= new(new Dictionary<KraidBodyStage, Action<RoomEnemyPlace, KraidEnemyState, int, SamusState>>
    {
        [KraidBodyStage.Rising] = (body, state, cameraX, samus) => Rise.RunKraidRisePhase(body, state, samus),
        [KraidBodyStage.Fighting] = (body, state, cameraX, samus) => RunKraidFightPhase(body, state),
        [KraidBodyStage.Growing] = (body, state, cameraX, samus) => Rise.RunKraidGrowthPhase(body, state),
        [KraidBodyStage.Dying] = (body, state, cameraX, samus) => Death.RunKraidDeathPhase(body, state, cameraX, samus),
    });
    /// <summary>The projectile holding a place one of Kraid's passes tests, and the marking of its collision with him.</summary>
    /// <param name="Shot">Its position, radius, type and damage.</param>
    /// <param name="MarkCollision">Marks the projectile holding the place as having collided with Kraid.</param>
    internal readonly partial record struct KraidHeldPassProjectile(KraidCollisionShot Shot, Action MarkCollision);


    /// <summary>Runs the phase of the stage the body is in: his rise, his fight, his growth or his death.</summary>
    internal void RunKraidBodyStage(
        RoomEnemyPlace body,
        KraidEnemyState state,
        int cameraX,
        SamusState samus)
    {
        RunKraidBodyStageForKraidBodyStage[state.BodyState.Stage](body, state, cameraX, samus);
    }

    /// <summary>What <see cref="RunKraidFightPhase"/> does for each <see cref="KraidFightPhase"/> member, each member's own arm.</summary>
    [field: NonSerialized]
    private IdentityTable<KraidFightPhase, Action<RoomEnemyPlace, KraidEnemyState>> RunKraidFightPhaseForKraidFightPhase => field ??= new(new Dictionary<KraidFightPhase, Action<RoomEnemyPlace, KraidEnemyState>>
    {
        [KraidFightPhase.FirstPhaseThinking] = (body, state) =>
        {
            if (state.ThinkingWait.StepRunsOut())
            {
                state.BodyState.FightPhase = KraidFightPhase.MainAttackWithMouthOpen;
                state.StartHeadAnimation(KraidHeadSegment.Roar);
            }
        },
        [KraidFightPhase.MainAttackWithMouthOpen] = (body, state) => Head.RunKraidMouthOpenAttack(body, state),
        [KraidFightPhase.MouthOpenReaction] = (body, state) => Head.RunKraidMouthOpenReaction(state),
        [KraidFightPhase.InitializeEyeGlow] = (body, state) => Head.InitializeKraidEyeGlow(state),
        [KraidFightPhase.GlowEye] = (body, state) => Head.GlowKraidEye(state),
        [KraidFightPhase.UnglowEye] = (body, state) => Head.UnglowKraidEye(body, state),
        [KraidFightPhase.WaitBeforeEyeGlow] = (body, state) =>
        {
            if (state.BodyState.PhaseWait.StepRunsOut())
                state.BodyState.FightPhase = KraidFightPhase.InitializeEyeGlow;
        },
        [KraidFightPhase.HeadAnimationBeforeGrowth] = (body, state) =>
        {
            Head.AdvanceKraidHeadAnimation(state);
            if (state.BodyState.PhaseWait.StepRunsOut())
                state.BodyState.GrowthPhase = KraidGrowthPhase.ReleaseCamera;
        },
        [KraidFightPhase.SecondPhaseThinking] = (body, state) => KraidLimbs.RunKraidSecondPhaseThinking(state),
    });

    /// <summary>Runs the body's first- or second-phase fight phase.</summary>
    internal void RunKraidFightPhase(RoomEnemyPlace body, KraidEnemyState state)
    {
        RunKraidFightPhaseForKraidFightPhase[state.BodyState.FightPhase](body, state);
    }
}

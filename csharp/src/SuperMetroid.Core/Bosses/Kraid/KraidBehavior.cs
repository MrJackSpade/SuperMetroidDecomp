using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The Kraid species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class KraidBehavior : IEnemySpecies, IEnemyEntityFactory
{

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } =
    [
        EnemyDefinitionId.Kraid, EnemyDefinitionId.KraidArm, EnemyDefinitionId.KraidFoot, EnemyDefinitionId.KraidLintBottom,
        EnemyDefinitionId.KraidLintMiddle, EnemyDefinitionId.KraidLintTop, EnemyDefinitionId.KraidFirstNail,
        EnemyDefinitionId.KraidSecondNail,
    ];

    /// <summary>One of Kraid's eight parts, each joining its own reference as it initializes (#1877, #2146).</summary>
    /// <exception cref="InvalidOperationException">The place holds no Kraid definition.</exception>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup)
    {
        EnemyDefinitionId definition = Ensure.NotNull(place).DefinitionId;
        if (definition == EnemyDefinitionId.Kraid)
            return new KraidBodyEntity(this, place, common);
        if (definition == EnemyDefinitionId.KraidArm)
            return new KraidArmEntity(this, place, common);
        if (definition == EnemyDefinitionId.KraidFoot)
            return new KraidFootEntity(this, place, common);
        if (definition == EnemyDefinitionId.KraidLintTop)
            return new KraidLintEntity(this, place, common, Parts.TopLintReference, KraidLintPart.Top);
        if (definition == EnemyDefinitionId.KraidLintMiddle)
            return new KraidLintEntity(this, place, common, Parts.MiddleLintReference, KraidLintPart.Middle);
        if (definition == EnemyDefinitionId.KraidLintBottom)
            return new KraidLintEntity(this, place, common, Parts.BottomLintReference, KraidLintPart.Bottom);
        if (definition == EnemyDefinitionId.KraidFirstNail)
            return new KraidNailEntity(this, place, common, Parts.FirstNailReference, KraidNailPart.First);
        if (definition == EnemyDefinitionId.KraidSecondNail)
            return new KraidNailEntity(this, place, common, Parts.SecondNailReference, KraidNailPart.Second);
        throw new InvalidOperationException($"The Kraid species creates no {place.DefinitionId}.");
    }
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
        Background = new KraidBackground(dependencies.Projectiles, dependencies.Presentation.Surfaces, Access, _bossArtwork);
        Head = new KraidHead(dependencies.Room, dependencies.Projectiles, Parts, Access, Background);
        Rise = new KraidRise(new KraidRiseDependencies { PassEvents = dependencies.PassEvents, Room = dependencies.Room, Projectiles = dependencies.Projectiles, Earthquake = dependencies.Earthquake, Parts = Parts, Presentation = dependencies.Presentation.Surfaces, CameraDistance = dependencies.CameraDistance, Access = Access, Background = Background, Head = Head, BodyLayer = BodyLayer });
        Death = new KraidDeath(new KraidDeathDependencies { PassEvents = dependencies.PassEvents, Room = dependencies.Room, Earthquake = dependencies.Earthquake, Parts = Parts, Presentation = dependencies.Presentation.Surfaces, DeathsAndDrops = dependencies.DeathsAndDrops, CameraDistance = dependencies.CameraDistance, Access = Access, Background = Background, Head = Head });
        Limbs = new KraidLimbs(dependencies.Room, dependencies.Earthquake, Parts, Access);
        Projectiles = new KraidProjectiles(dependencies.Room, dependencies.Terrain, Parts, dependencies.Collision, dependencies.DeathsAndDrops, Access);
        Combat = new KraidCombat(dependencies.Projectiles, dependencies.Audio, dependencies.EffectActors, Parts, dependencies.Collision);
    }

    /// <summary>
    /// Runs Kraid's body for one update: mouth-projectile collision, colour handling,
    /// body-projectile collision and body-vs-Samus collision precede the phase. A
    /// killing mouth hit therefore reaches colour handling, which settles his hurt flash on his health band's colours,
    /// before the death setup runs in this same update.
    /// </summary>
    internal void RunKraidBodyMain(
        RoomEnemyPlace body,
        KraidEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew,
        Position16<RoomPixels> camera,
        SamusProjectileSystem samusProjectiles,
        SamusBombProjectileSystem samusBombs)
    {
        IReadOnlyList<KraidPassProjectile> tested = TestedByShotPasses(samusProjectiles, samusBombs);
        Combat.ResolveKraidMouthProjectileHits(body, state, tested);
        Head.RunKraidHurtColours(body, state);
        Combat.ResolveKraidBodyProjectileHits(body, state, tested);
        Combat.ResolveKraidBodyContact(body, state, samus, pseudoScrew);
        RunKraidBodyStage(body, state, camera.X, samus);
    }

    /// <summary>Runs Kraid's main AI for his body in <paramref name="place"/>.</summary>
    internal void RunKraidMainAi(RoomEnemyPlace place, KraidEnemyState state, IEnemyAiFrame frame)
    {
        RunKraidBodyMain(place, state, frame.Samus, frame.PseudoScrew, frame.Camera, frame.SamusProjectiles, frame.SamusBombs);
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
    internal abstract record KraidPassProjectile
    {
        /// <summary>Closes the hierarchy to the shot and the bomb.</summary>
        private KraidPassProjectile()
        {
        }

        /// <summary>Reads the projectile's position, radius, type and damage.</summary>
        public abstract KraidCollisionShot Read();

        /// <summary>Marks the projectile as having collided with Kraid.</summary>
        public abstract void MarkCollision();

        /// <summary>A shot the passes test, in one of Samus's shot places.</summary>
        internal sealed record Shot : KraidPassProjectile
        {
            /// <summary>The shot the passes test.</summary>
            private readonly SamusProjectile _shot;

            /// <summary>Tests <paramref name="shot"/>.</summary>
            /// <param name="shot">The projectile in one of Samus's shot places.</param>
            internal Shot(SamusProjectile shot) => _shot = Ensure.NotNull(shot);

            /// <inheritdoc/>
            public override KraidCollisionShot Read() =>
                new(_shot.Position.Whole, _shot.Radius, _shot.Type, _shot.Damage, IsPlacedBomb: false);

            /// <inheritdoc/>
            public override void MarkCollision() => _shot.Spend();
        }

        /// <summary>The bomb the passes test past the last shot place: Samus's first bomb.</summary>
        internal sealed record Bomb : KraidPassProjectile
        {
            /// <summary>The bomb the passes test.</summary>
            private readonly SamusBomb _bomb;

            /// <summary>Tests <paramref name="bomb"/>.</summary>
            /// <param name="bomb">Samus's first bomb.</param>
            internal Bomb(SamusBomb bomb) => _bomb = Ensure.NotNull(bomb);

            /// <inheritdoc/>
            public override KraidCollisionShot Read() =>
                new(_bomb.Position.Whole, _bomb.Radius, _bomb.Type, _bomb.Damage, IsPlacedBomb: !_bomb.IsBombSpread);

            /// <inheritdoc/>
            public override void MarkCollision() => _bomb.Spend();
        }
    }

    /// <summary>Projectile geometry and attack data read by Kraid's mouth and body collision passes.</summary>
    /// <param name="Position">The projectile's whole-pixel room position.</param>
    /// <param name="Radius">The projectile's collision half-extents.</param>
    /// <param name="Type">The projectile's type: its family, beams and charge.</param>
    /// <param name="Damage">The projectile's damage before Kraid's vulnerability multiplier; a copy of the place's non-negative damage.</param>
    /// <param name="IsPlacedBomb">Whether the projectile is a placed bomb or Power Bomb rather than a fired or spread one.</param>
    internal readonly record struct KraidCollisionShot(Position16<RoomPixels> Position, Extent16<RoomPixels> Radius, SamusProjectileType Type, int Damage, bool IsPlacedBomb);


    /// <summary>Runs the phase of the stage the body is in: his rise, his fight, his growth or his death.</summary>
    internal void RunKraidBodyStage(
        RoomEnemyPlace body,
        KraidEnemyState state,
        int cameraX,
        SamusState samus)
    {
        switch (state.BodyState.Stage)
        {
            case KraidBodyStage.Rising:
                Rise.RunKraidRisePhase(body, state, samus);
                return;
            case KraidBodyStage.Fighting:
                RunKraidFightPhase(body, state);
                return;
            case KraidBodyStage.Growing:
                Rise.RunKraidGrowthPhase(body, state);
                return;
            case KraidBodyStage.Dying:
                Death.RunKraidDeathPhase(body, state, cameraX, samus);
                return;
            default:
                throw new InvalidDataException($"Undefined {nameof(KraidBodyStage)} {state.BodyState.Stage}.");
        }
    }

    /// <summary>Runs the body's first- or second-phase fight phase.</summary>
    internal void RunKraidFightPhase(RoomEnemyPlace body, KraidEnemyState state)
    {
        switch (state.BodyState.FightPhase)
        {
            case KraidFightPhase.FirstPhaseThinking:
                if (state.ThinkingWait.StepRunsOut())
                {
                    state.BodyState.FightPhase = KraidFightPhase.MainAttackWithMouthOpen;
                    state.StartHeadAnimation(KraidHeadSegment.Roar);
                }
                return;

            case KraidFightPhase.MainAttackWithMouthOpen:
                Head.RunKraidMouthOpenAttack(body, state);
                return;

            case KraidFightPhase.MouthOpenReaction:
                Head.RunKraidMouthOpenReaction(state);
                return;

            case KraidFightPhase.InitializeEyeGlow:
                Head.InitializeKraidEyeGlow(state);
                return;

            case KraidFightPhase.GlowEye:
                Head.GlowKraidEye(state);
                return;

            case KraidFightPhase.UnglowEye:
                Head.UnglowKraidEye(body, state);
                return;

            case KraidFightPhase.WaitBeforeEyeGlow:
                if (state.BodyState.PhaseWait.StepRunsOut())
                    state.BodyState.FightPhase = KraidFightPhase.InitializeEyeGlow;
                return;

            case KraidFightPhase.HeadAnimationBeforeGrowth:
                _ = Head.AdvanceKraidHeadAnimation(state);
                if (state.BodyState.PhaseWait.StepRunsOut())
                    state.BodyState.GrowthPhase = KraidGrowthPhase.ReleaseCamera;
                return;

            case KraidFightPhase.SecondPhaseThinking:
                KraidLimbs.RunKraidSecondPhaseThinking(state);
                return;

            default:
                throw new InvalidDataException($"Undefined {nameof(KraidFightPhase)} {state.BodyState.FightPhase}.");
        }
    }
}

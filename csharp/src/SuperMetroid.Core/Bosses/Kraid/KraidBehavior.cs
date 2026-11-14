using SuperMetroid.Core.Rooms.Scrolling;
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
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. KraidPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new KraidEntity(this, slot, common);
    /// <summary>Kraid's body layer: the body frame he shows, drawn from his installed body sheet.</summary>
    internal KraidBodyLayer BodyLayer { get; }

    /// <summary>Kraid's arrival: his room setup, the rise through the floor and his growth into the second phase.</summary>
    internal KraidRise Rise { get; }

    /// <summary>Kraid's state and part lookups, part animations, phase timer, random numbers and the sound he last requested.</summary>
    internal KraidEncounterAccess Access { get; }

    /// <summary>Kraid's eight parts in his room, in the order they run.</summary>
    internal KraidParts Parts { get; }

    /// <summary>Kraid's room background: its fade, color transitions and the ceiling rocks.</summary>
    internal KraidBackground Background { get; }

    /// <summary>Kraid's head: the mouth-open attack and reaction, the eye glow and the health palettes.</summary>
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
        Access = new KraidEncounterAccess(dependencies.Room, dependencies.Audio, Parts);
        BodyLayer = new KraidBodyLayer(Access, _bossArtwork);
        Background = new KraidBackground(dependencies.Projectiles, dependencies.Presentation.Surfaces, Access, _bossArtwork);
        Head = new KraidHead(dependencies.Room, dependencies.Projectiles, Parts, Access, Background);
        Rise = new KraidRise(new KraidRiseDependencies { FrameResults = dependencies.FrameResults, Room = dependencies.Room, Projectiles = dependencies.Projectiles, Earthquake = dependencies.Earthquake, Parts = Parts, Presentation = dependencies.Presentation.Surfaces, CameraDistance = dependencies.CameraDistance, Access = Access, Background = Background, Head = Head });
        Death = new KraidDeath(new KraidDeathDependencies { FrameResults = dependencies.FrameResults, Room = dependencies.Room, Earthquake = dependencies.Earthquake, Parts = Parts, Presentation = dependencies.Presentation.Surfaces, DeathsAndDrops = dependencies.DeathsAndDrops, CameraDistance = dependencies.CameraDistance, Access = Access, Background = Background, Head = Head });
        Limbs = new KraidLimbs(dependencies.Room, dependencies.Earthquake, Parts, Access);
        Projectiles = new KraidProjectiles(dependencies.Room, dependencies.Terrain, Parts, dependencies.Collision, dependencies.DeathsAndDrops, Access);
        Combat = new KraidCombat(dependencies.Projectiles, dependencies.Audio, dependencies.SpriteObjects, Parts, dependencies.Collision, Access);
    }

    /// <summary>
    /// The longest wait a part's timer is loaded with, kept exact (#1627). The lint and fingernail frame timers wait behind
    /// it so their animations never advance before their behaviour starts them, and a dying Kraid's lints wait behind it as
    /// their phase timer, changing phase when it runs out, which gameplay sees.
    /// </summary>
    internal const int KraidStillPartTimer = 32767;

    /// <summary>
    /// Runs Kraid's body per-frame behaviour: mouth-projectile collision, palette handling,
    /// body-projectile collision and body-vs-Samus collision precede the phase. A
    /// killing mouth hit therefore reaches palette handling, which zeroes the hurt frame,
    /// before the death initializer runs in this same frame.
    /// </summary>
    internal void RunKraidBodyMain(
        RoomEnemySlot body,
        SamusState? samus,
        Position16<RoomPixels> camera,
        SamusProjectileSystem? samusProjectiles,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        KraidEnemyState state = Access.RequireKraidState(body);
        IReadOnlyList<KraidPassProjectile>? tested = samusProjectiles is not null && sharedProjectiles is not null
            ? TestedByShotPasses(samusProjectiles, sharedProjectiles)
            : null;
        if (tested is not null)
            Combat.ResolveKraidMouthProjectileHits(body, state, tested);
        Head.RunKraidHurtColours(body, state);
        if (tested is not null)
            Combat.ResolveKraidBodyProjectileHits(body, state, tested);
        if (samus is not null)
            Combat.ResolveKraidBodyContact(body, state, samus);
        // His body follows Kraid (#1617): its area lies his X radius, the authored body's right-edge origin adjustment, left
        // of him, and 152 pixels, the authored vertical anchor, above him.
        state.BodyOrigin = new Position16<RoomPixels>(
            CoordinateAxis.Wrap(body.Position.X.Whole - body.Radius.X),
            CoordinateAxis.Wrap(body.Position.Y.Whole - 152));
        if (state.BodyState.Phase is { } phase && KraidPhaseGroups.Rising.Contains(phase))
        {
            Rise.RunKraidRisePhase(body, state, samus);
            return;
        }
        RunKraidCombatPhase(body, state, camera.X);
    }

    /// <summary>Runs Kraid's main AI.</summary>
    internal void RunKraidMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunKraidBodyMain(slot, frame.Samus, frame.Camera, frame.SamusProjectiles, frame.SharedProjectiles);
    }

    /// <summary>Runs KraidArm's main AI.</summary>
    internal void RunKraidArmMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        Limbs.RunKraidArmMain(slot, frame.Camera.Y);
    }

    /// <summary>Runs KraidLintTop's main AI.</summary>
    internal void RunKraidLintTopMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        Projectiles.RunKraidLintMain(slot, frame.Samus);
    }

    /// <summary>Runs KraidFoot's main AI.</summary>
    internal void RunKraidFootMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        Limbs.RunKraidFootMain(slot, frame.Camera.Y);
    }

    /// <summary>Runs KraidNail's main AI.</summary>
    internal void RunKraidNailMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        Projectiles.RunKraidNailMain(slot, frame.Level);
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
        return KraidShotPassReach.Tested(
            projectiles.SlotTable.ProjectileCounter,
            [.. projectiles.Slots.Select(KraidPassProjectile.Shot)],
            KraidPassProjectile.Bomb(bombs.FirstBomb));
    }

    /// <summary>One projectile one of Kraid's shot passes tests: a shot of Samus's, or her first bomb.</summary>
    internal sealed class KraidPassProjectile
    {
        /// <summary>The shot, or null for the bomb.</summary>
        private readonly SamusProjectile? _shot;
        /// <summary>The bomb, or null for a shot.</summary>
        private readonly SamusBombProjectileSlot? _bomb;

        /// <summary>Holds exactly one of a shot and a bomb.</summary>
        private KraidPassProjectile(SamusProjectile? shot, SamusBombProjectileSlot? bomb)
        {
            _shot = shot;
            _bomb = bomb;
        }

        /// <summary>A shot the passes test.</summary>
        /// <param name="shot">The projectile in one of Samus's shot places.</param>
        internal static KraidPassProjectile Shot(SamusProjectile shot) => new(Ensure.NotNull(shot), null);

        /// <summary>The bomb the passes test past the last shot place.</summary>
        /// <param name="bomb">Samus's first bomb.</param>
        internal static KraidPassProjectile Bomb(SamusBombProjectileSlot bomb) => new(null, Ensure.NotNull(bomb));

        /// <summary>Reads the projectile's position, radius, type and damage.</summary>
        public KraidCollisionShot Read()
        {
            if (_shot is { } shot)
                return new(shot.Position.Whole, shot.Radius, shot.Type, shot.Damage, IsPlacedBomb: false);
            if (_bomb is { } bomb)
                return new(bomb.Position.Whole, bomb.Radius, bomb.Type, bomb.Damage, IsPlacedBomb: !bomb.IsBombSpread);
            throw new InvalidOperationException("A Kraid pass projectile holds exactly one of a shot and a bomb.");
        }

        /// <summary>Marks the projectile as having collided with Kraid.</summary>
        public void MarkCollision()
        {
            if (_shot is { } shot)
                shot.IsSpent = true;
            else if (_bomb is { } bomb)
                bomb.IsSpent = true;
            else
                throw new InvalidOperationException("A Kraid pass projectile holds exactly one of a shot and a bomb.");
        }
    }

    /// <summary>Projectile geometry and attack data read by Kraid's mouth and body collision passes.</summary>
    /// <param name="Position">The projectile's whole-pixel room position.</param>
    /// <param name="Radius">The projectile's collision half-extents.</param>
    /// <param name="Type">The projectile's type: its family, beams and charge.</param>
    /// <param name="Damage">The projectile's damage before Kraid's vulnerability multiplier; a copy of the slot's non-negative damage.</param>
    /// <param name="IsPlacedBomb">Whether the projectile is a placed bomb or Power Bomb rather than a fired or spread one.</param>
    internal readonly record struct KraidCollisionShot(Position16<RoomPixels> Position, Extent16<RoomPixels> Radius, SamusProjectileType Type, int Damage, bool IsPlacedBomb);


    /// <summary>Runs the body's active first- or second-phase combat phase.</summary>
    internal void RunKraidCombatPhase(
        RoomEnemySlot body,
        KraidEnemyState state,
        int cameraX)
    {
        switch (state.BodyState.Phase)
        {
            case KraidPhase.MainloopThinking:
                if (state.ThinkingTimer != 0 && --state.ThinkingTimer == 0)
                {
                    state.BodyState.Phase = KraidPhase.MainAttackWithMouthOpen;
                    state.HeadPlayback = KraidHeadAnimations.RoarAfterEntryFrame;
                    state.BodyState.HeadTimer = KraidHeadAnimations.RoarEntryDuration;
                }
                return;

            case KraidPhase.MainAttackWithMouthOpen:
                Head.RunKraidMouthOpenAttack(body, state);
                return;

            case KraidPhase.MouthOpenReaction:
                Head.RunKraidMouthOpenReaction(state);
                return;

            case KraidPhase.InitializeEyeGlow:
                Head.InitializeKraidEyeGlow(state);
                return;

            case KraidPhase.GlowEye:
                Head.GlowKraidEye(state);
                return;

            case KraidPhase.UnglowEye:
                Head.UnglowKraidEye(body, state);
                return;

            case KraidPhase.WaitToResumePhase:
                KraidEncounterAccess.TickKraidPhaseTimer(state.BodyState);
                return;

            case KraidPhase.AdvanceHeadAnimationAndTimer:
                _ = Head.AdvanceKraidHeadAnimation(state);
                KraidEncounterAccess.TickKraidPhaseTimer(state.BodyState);
                return;

            case KraidPhase.GrowReleaseCamera:
            case KraidPhase.GrowBreakCeilingPlatforms:
            case KraidPhase.GrowBringBodyInFront:
            case KraidPhase.GrowShowLowerBodyInFront:
            case KraidPhase.GrowDrawRoomBackground:
            case KraidPhase.GrowFadeInRoomBackground:
                Rise.RunKraidGrowthPhase(body, state);
                return;

            case KraidPhase.SecondPhaseThinking:
                KraidLimbs.RunKraidSecondPhaseThinking(state);
                return;

            case KraidPhase.DeathInitialize:
            case KraidPhase.DeathFadeOut:
            case KraidPhase.DeathShowUpperBody:
            case KraidPhase.DeathShowLowerBody:
            case KraidPhase.DeathSink:
            case KraidPhase.DeathHideUpperBody:
            case KraidPhase.DeathHideLowerBody:
            case KraidPhase.DeathWaitBeforeFadeIn1:
            case KraidPhase.DeathWaitBeforeFadeIn2:
            case KraidPhase.DeathWaitBeforeFadeIn3:
            case KraidPhase.DeathWaitBeforeFadeIn4:
            case KraidPhase.DeathFadeInBackground:
            case KraidPhase.DeathFinishedWasAlive:
            case KraidPhase.DeathFinishedWasDead:
                Death.RunKraidDeathPhase(body, state, cameraX);
                return;

            case KraidPhase.LintInactive:
            case KraidPhase.LintProduce:
            case KraidPhase.LintCharge:
            case KraidPhase.LintFire:
            case KraidPhase.FingernailWaitForLint:
            case KraidPhase.AlignPartToKraid:
            case KraidPhase.FootWaitBeforeWalk:
            case KraidPhase.FootFirstPhaseThinking:
            case KraidPhase.FootSecondPhaseThinking:
            case KraidPhase.FootSecondPhaseWalkingRight:
            case KraidPhase.FootSecondPhaseWalkToStart:
            case KraidPhase.FootSecondPhaseInitialize:
            case KraidPhase.FootSecondPhaseWalkingLeft:
            case KraidPhase.FingernailInitialize:
            case KraidPhase.FingernailFire:
            case KraidPhase.FootPrepareFirstPhaseLunge:
            case KraidPhase.FootFirstPhaseLunge:
            case KraidPhase.FootFirstPhaseRetreat:
            case KraidPhase.RestrictSamusToFirstScreen:
            case KraidPhase.RaiseKraidThroughFloor:
            case KraidPhase.RaiseShowLowerBody:
            case KraidPhase.RaiseRocksEvery16Frames:
            case KraidPhase.RaiseRocksEvery8Frames:
            case KraidPhase.RaiseBody:
            default:
                throw new InvalidDataException(
                    $"Kraid body has no behaviour for phase {state.BodyState.Phase}.");
        }
    }
}

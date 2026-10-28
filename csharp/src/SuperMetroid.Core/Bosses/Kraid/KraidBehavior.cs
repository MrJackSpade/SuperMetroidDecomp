using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The Kraid species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class KraidBehavior : IEnemySpecies, IEnemyEntityFactory
{

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. KraidPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new KraidEntity(this, slot, common);
    /// <summary>Kraid's BG2 body layer: his working map composed from the installed artwork and the tilemap of the frame he shows.</summary>
    internal KraidBodyLayer BodyLayer { get; }

    /// <summary>Kraid's arrival: his room setup, the rise through the floor and his growth into the second phase.</summary>
    internal KraidRise Rise { get; }

    /// <summary>Kraid's state and part lookups, part programs, function timer, random numbers and the sound he last requested.</summary>
    internal KraidEncounterAccess Access { get; }

    /// <summary>Kraid's room background: its fade, color transitions, top and bottom tilemap transfers and the ceiling rocks.</summary>
    internal KraidBackground Background { get; }

    /// <summary>Kraid's head: the mouth-open attack and reaction, the eye glow and the health palettes.</summary>
    internal KraidHead Head { get; }

    /// <summary>Kraid's death: the sink below the floor, the tilemap clears and the background fade.</summary>
    internal KraidDeath Death { get; }

    /// <summary>Kraid's arm and foot: their setup, programs, lunges, retreats and walks.</summary>
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
        Access = new KraidEncounterAccess(dependencies.Room);
        BodyLayer = new KraidBodyLayer(Access, _bossArtwork);
        Background = new KraidBackground(dependencies.Projectiles, dependencies.Presentation.Surfaces, Access, _bossArtwork);
        Head = new KraidHead(dependencies.Room, dependencies.Projectiles, dependencies.SlotTable, dependencies.Presentation.Surfaces, Access, Background);
        Rise = new KraidRise(new KraidRiseDependencies { FrameResults = dependencies.FrameResults, Room = dependencies.Room, Projectiles = dependencies.Projectiles, Earthquake = dependencies.Earthquake, SlotTable = dependencies.SlotTable, Presentation = dependencies.Presentation.Surfaces, ZebetiteBehavior = dependencies.ZebetiteBehavior, BodyLayer = BodyLayer, Access = Access, Background = Background, Head = Head , BossArtwork = _bossArtwork });
        Death = new KraidDeath(new KraidDeathDependencies { FrameResults = dependencies.FrameResults, Room = dependencies.Room, Earthquake = dependencies.Earthquake, SlotTable = dependencies.SlotTable, Presentation = dependencies.Presentation.Surfaces, DeathsAndDrops = dependencies.DeathsAndDrops, ZebetiteBehavior = dependencies.ZebetiteBehavior, Access = Access, Background = Background, Head = Head , BossArtwork = _bossArtwork });
        Limbs = new KraidLimbs(dependencies.Room, dependencies.Earthquake, dependencies.SlotTable, Access);
        Projectiles = new KraidProjectiles(dependencies.Room, dependencies.Terrain, dependencies.SlotTable, dependencies.Collision, dependencies.DeathsAndDrops, Access);
        Combat = new KraidCombat(dependencies.Projectiles, dependencies.Audio, dependencies.SpriteObjects, dependencies.SlotTable, dependencies.Collision, Access);
    }

    /// <summary>The native maximum timer the still lint and fingernail parts wait behind before their first frame.</summary>
    internal const ushort KraidStillPartTimer = 32767;

    /// <summary>
    /// Ports <c>MainAI_Kraid</c> ($A7:AC21): mouth-projectile collision, palette handling,
    /// body-projectile collision and body-vs-Samus collision precede the function. A
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
        KraidShotSlots? shots = samusProjectiles is not null && sharedProjectiles is not null
            ? new KraidShotSlots(samusProjectiles, sharedProjectiles)
            : null;
        if (shots is { } mouthShots)
            Combat.ResolveKraidMouthProjectileHits(body, state, mouthShots);
        Head.RunKraidPaletteHandling(body, state);
        if (shots is { } bodyShots)
            Combat.ResolveKraidBodyProjectileHits(body, state, bodyShots);
        if (samus is not null)
            Combat.ResolveKraidBodyContact(body, state, samus);
        // `$A7:AC21` makes BG2 follow Kraid rather than the room. X radius is the native
        // body's right-edge origin adjustment; 152 is the authored vertical anchor.
        state.Bg2Scroll = new Position16<BackgroundPixels>(
            unchecked((ushort)(camera.X - body.Position.X.Whole + body.Radius.X)),
            unchecked((ushort)(camera.Y - body.Position.Y.Whole + 152)));
        KraidAiFunction function = state.Parts[body.SlotIndex].Function;
        if (KraidFunctionGroups.Rise.Contains(function))
        {
            Rise.RunKraidRiseFunction(body, state, samus);
            return;
        }
        RunKraidCombatFunction(body, state, camera.X);
    }

    /// <summary>Ports Kraid's main AI.</summary>
    internal void RunKraidMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunKraidBodyMain(slot, frame.Samus, frame.Camera, frame.SamusProjectiles, frame.SharedProjectiles);
    }

    /// <summary>Ports KraidArm's main AI.</summary>
    internal void RunKraidArmMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        Limbs.RunKraidArmMain(slot, frame.Camera.Y);
    }

    /// <summary>Ports KraidLintTop's main AI.</summary>
    internal void RunKraidLintTopMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        Projectiles.RunKraidLintMain(slot, frame.Samus);
    }

    /// <summary>Ports KraidFoot's main AI.</summary>
    internal void RunKraidFootMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        Limbs.RunKraidFootMain(slot, frame.Camera.Y);
    }

    /// <summary>Ports KraidNail's main AI.</summary>
    internal void RunKraidNailMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        Projectiles.RunKraidNailMain(slot, frame.Level);
    }

    /// <summary>The room backdrop characters Kraid reveals.</summary>
    internal static readonly BgArtworkIdentity KraidRoomBackgroundIdentity = new("Kraid room background characters");

    /// <summary>
    /// The counter-indexed shot view both passes use. The cartridge starts at the count
    /// itself, not count - 1, and does not inspect active-slot sentinels; count five
    /// therefore includes the first physical bomb.
    /// </summary>
    [RoomScopeBoundary("A per-call input, built for one call and never kept.")]
    internal readonly struct KraidShotSlots
    {
        /// <summary>Five physical Samus shot slots indexed by the native shot counter.</summary>
        private readonly SamusProjectileSystem _projectiles;
        /// <summary>Shared bomb slot reached when the native count includes a sixth projectile.</summary>
        private readonly SamusBombProjectileSystem _sharedProjectiles;

        /// <summary>Captures the shot count and validates its bounded native slot domain.</summary>
        public KraidShotSlots(SamusProjectileSystem projectiles, SamusBombProjectileSystem sharedProjectiles)
        {
            _projectiles = projectiles;
            _sharedProjectiles = sharedProjectiles;
            LastSlot = projectiles.SlotTable.ProjectileCounter;
            if (LastSlot > SamusProjectileSystem.SlotCount)
                throw new InvalidDataException($"Kraid projectile counter {LastSlot} exceeds the bounded native slot domain.");
        }

        /// <summary>Inclusive last index selected by the native projectile counter.</summary>
        public int LastSlot { get; }

        /// <summary>Reads the selected shot or shared bomb's position, radius, type, and damage.</summary>
        public KraidCollisionShot Read(int index)
        {
            if (index < SamusProjectileSystem.SlotCount)
            {
                SamusProjectileSlot shot = _projectiles.Slots[index];
                return new(shot.Position.Whole, shot.Radius, shot.PackedType.Raw, shot.Damage);
            }
            SamusBombProjectileSlot bomb = _sharedProjectiles.Slots[0];
            return new(bomb.Position.Whole, bomb.Radius, bomb.PackedType.Raw, bomb.Damage);
        }

        /// <summary>Marks the selected shot or bomb as having collided with Kraid.</summary>
        public void MarkCollision(int index)
        {
            if (index < SamusProjectileSystem.SlotCount)
            {
                SamusProjectileSlot shot = _projectiles.Slots[index];
                shot.Direction = shot.PackedDirection.WithCollisionLifecycleState();
            }
            else
            {
                SamusBombProjectileSlot bomb = _sharedProjectiles.Slots[0];
                bomb.Direction = bomb.PackedDirection.WithCollisionLifecycleState();
            }
        }
    }

    /// <summary>Projectile geometry and attack data read by Kraid's mouth and body collision passes.</summary>
    /// <param name="Position">The projectile's whole-pixel room position.</param>
    /// <param name="Radius">The projectile's collision half-extents.</param>
    /// <param name="Type">The projectile's type: its family, beams and charge.</param>
    /// <param name="Damage">The projectile's damage before Kraid's vulnerability multiplier; a copy of the slot's non-negative damage.</param>
    internal readonly record struct KraidCollisionShot(Position16<RoomPixels> Position, Extent16<RoomPixels> Radius, SamusProjectileTypeWord Type, int Damage);

    /// <summary>
    /// The three bounds Kraid's mouth and body collision passes compare shots against. Between shots the damage and
    /// dud paths overwrite them with the shot's type, the vulnerability table and the dud sprite's index, and the next
    /// shot's wrapped signed comparisons read those leftovers: a Super Missile's type moves the next shot's mouth
    /// bound, which doubles close-range damage. Each bound therefore holds any of those leftovers as a value in 0
    /// through 65535: every store is a wrapped room coordinate or one of those words, and every comparison takes the
    /// signed sixteen-bit difference.
    /// </summary>
    internal struct KraidCollisionScratch
    {
        /// <summary>Lower vertical mouth bound in wrapped room coordinates, or the previous shot's type or dud sprite index the damage path stored over it.</summary>
        public int Bottom;
        /// <summary>Upper vertical mouth bound in wrapped room coordinates, or the vulnerability table or dud sprite's Y the damage path stored over it.</summary>
        public int Top;
        /// <summary>Left horizontal mouth bound in wrapped room coordinates, or the dud sprite's kind the dud path stored over it.</summary>
        public int Left;

        /// <summary>Tests the mouth using native signed results of wrapped coordinate subtraction.</summary>
        public readonly bool OverlapsMouth(KraidCollisionShot shot) =>
            unchecked((short)(shot.Position.Y - shot.Radius.Y - 1 - Bottom)) < 0 &&
            unchecked((short)(shot.Position.Y + shot.Radius.Y - Top)) >= 0 &&
            unchecked((short)(shot.Position.X + shot.Radius.X - Left)) >= 0;

        /// <summary>Tests the body contour against one shot and updates scratch limits as required.</summary>
        public bool OverlapsBody(RoomEnemySlot body, KraidCollisionShot shot)
        {
            if (unchecked((short)(shot.Position.Y - shot.Radius.Y - 1 - Bottom)) < 0)
                return unchecked((short)(shot.Position.Y + shot.Radius.Y - Top)) >= 0 &&
                    unchecked((short)(shot.Position.X + shot.Radius.X - Left)) >= 0;

            Bottom = unchecked((ushort)(shot.Position.X + shot.Radius.X));
            int relativeY = unchecked((short)(shot.Position.Y - body.Position.Y.Whole));
            return unchecked((short)(body.Position.X.Whole + KraidBodyContour.LeftEdge(relativeY) - Bottom)) < 0;
        }
    }
    /// <summary>Dispatches the active first- or second-phase body combat function.</summary>
    internal void RunKraidCombatFunction(
        RoomEnemySlot body,
        KraidEnemyState state,
        int cameraX)
    {
        switch (state.Parts[body.SlotIndex].Function)
        {
            case KraidAiFunction.MainloopThinking:
                if (!state.ThinkingTimer.IsZero && (state.ThinkingTimer = state.ThinkingTimer.Decrement().Remaining).IsZero)
                {
                    state.Parts[body.SlotIndex].Function = KraidAiFunction.MainAttackWithMouthOpen;
                    state.HeadProgram = KraidHeadAnimations.RoarAfterEntryFrame;
                    state.Parts[body.SlotIndex].HeadTimer = KraidHeadAnimations.RoarEntryDuration;
                }
                return;

            case KraidAiFunction.MainAttackWithMouthOpen:
                Head.RunKraidMouthOpenAttack(body, state);
                return;

            case KraidAiFunction.MouthOpenReaction:
                Head.RunKraidMouthOpenReaction(body, state);
                return;

            case KraidAiFunction.InitializeEyeGlow:
                Head.InitializeKraidEyeGlow(body, state);
                return;

            case KraidAiFunction.GlowEye:
                Head.GlowKraidEye(body, state);
                return;

            case KraidAiFunction.UnglowEye:
                Head.UnglowKraidEye(body, state);
                return;

            case KraidAiFunction.HandleFunctionTimer:
                KraidEncounterAccess.TickKraidFunctionTimer(state.Parts[0]);
                return;

            case KraidAiFunction.ProcessHeadInstructionAndTimer:
                _ = Head.ProcessKraidHeadInstruction(body, state);
                KraidEncounterAccess.TickKraidFunctionTimer(state.Parts[0]);
                return;

            case KraidAiFunction.GrowReleaseCamera:
            case KraidAiFunction.GrowBreakCeilingPlatforms:
            case KraidAiFunction.GrowSetBg2Priority:
            case KraidAiFunction.GrowFinishBg2Update:
            case KraidAiFunction.GrowDrawRoomBackground:
            case KraidAiFunction.GrowFadeInRoomBackground:
                Rise.RunKraidGrowthFunction(body, state);
                return;

            case KraidAiFunction.SecondPhaseThinking:
                KraidLimbs.RunKraidSecondPhaseThinking(body, state);
                return;

            case KraidAiFunction.DeathInitialize:
            case KraidAiFunction.DeathFadeOut:
            case KraidAiFunction.DeathUpdateTopTilemap:
            case KraidAiFunction.DeathUpdateBottomTilemap:
            case KraidAiFunction.DeathSink:
            case KraidAiFunction.DeathClearTopTilemap:
            case KraidAiFunction.DeathClearBottomTilemap:
            case KraidAiFunction.DeathLoadBg3Quarter1:
            case KraidAiFunction.DeathLoadBg3Quarter2:
            case KraidAiFunction.DeathLoadBg3Quarter3:
            case KraidAiFunction.DeathLoadBg3Quarter4:
            case KraidAiFunction.DeathFadeInBackground:
            case KraidAiFunction.DeathFinishedWasAlive:
            case KraidAiFunction.DeathFinishedWasDead:
                Death.RunKraidDeathFunction(body, state, cameraX);
                return;

            case KraidAiFunction.NoOperation:
            case KraidAiFunction.LintInactive:
            case KraidAiFunction.LintProduce:
            case KraidAiFunction.LintCharge:
            case KraidAiFunction.LintFire:
            case KraidAiFunction.FingernailWaitForLint:
            case KraidAiFunction.AlignPartToKraid:
            case KraidAiFunction.DecrementFunctionTimerAndStartWalk:
            case KraidAiFunction.FootFirstPhaseThinking:
            case KraidAiFunction.FootSecondPhaseThinking:
            case KraidAiFunction.FootSecondPhaseWalkingRight:
            case KraidAiFunction.FootSecondPhaseWalkToStart:
            case KraidAiFunction.FootSecondPhaseInitialize:
            case KraidAiFunction.FootSecondPhaseWalkingLeft:
            case KraidAiFunction.FingernailInitialize:
            case KraidAiFunction.FingernailFire:
            case KraidAiFunction.FootPrepareFirstPhaseLunge:
            case KraidAiFunction.FootFirstPhaseLunge:
            case KraidAiFunction.FootFirstPhaseRetreat:
            case KraidAiFunction.RestrictSamusToFirstScreen:
            case KraidAiFunction.RaiseKraidThroughFloor:
            case KraidAiFunction.RaiseLoadBottomTilemap:
            case KraidAiFunction.RaiseRocksEvery16Frames:
            case KraidAiFunction.RaiseRocksEvery8Frames:
            case KraidAiFunction.RaiseBody:
            case KraidAiFunction.None:
            default:
                throw new InvalidDataException(
                    $"Kraid body function {state.Parts[body.SlotIndex].Function} is not translated.");
        }
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        // Legacy publications are momentary like their native QueueSfx/QueueMusic calls; clearing them at the
        // EnemyMain frame boundary keeps a paused or frozen actor from replaying its previous request.
        if (Access.Kraid is not null)
            Access.Kraid.MusicRequest = null;
        Access.LastKraidSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (Access.LastKraidSoundEffect is { } kraidSound)
            audio.QueueEnemySound(kraidSound.SoundEffect, maximumQueued: SoundQueueLimit.Max6);
    }

    /// <summary>Queues this frame's published music requests through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacyMusic(EnemyAudioQueue audio)
    {
        audio.QueueLegacyMusic(Access.Kraid?.MusicRequest, MusicQueueDelay.Shortest);
    }
}

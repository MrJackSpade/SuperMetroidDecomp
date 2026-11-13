using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>The Dragon species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class DragonBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Dragon];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new DragonEntity(this, slot, common);

    /// <summary>Allocates Dragon's three-shot fireball volleys from the room projectile pool.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>The room's enemy places, where each part finds its partners when it is placed.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>The partners each Dragon part found when it was placed.</summary>
    private readonly EnemyPartLinks<DragonPartners> _partners = new();
    /// <summary>Performs common Samus touch resolution before Dragon's body and wing synchronization.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Receives each fireball's sound in call order, as the game queues it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects Dragon body AI, paired-wing state, fireball spawning and its sound to room systems.</summary>
    internal DragonBehavior(EnemyProjectileEngine projectiles, EnemySlotTable slotTable, EnemyCollision collision,
        EnemyAudioQueue audio)
    {
        _projectiles = projectiles;
        _slotTable = slotTable;
        _collision = collision;
        _audio = audio;
    }

    /// <summary>Carries out Dragon's animation cue: the attack body marks its animation complete for main AI.</summary>
    private static DragonSegment? ApplyDragonCue(RoomEnemySlot slot, DragonCue cue)
    {
        switch (cue.Kind)
        {
            case DragonCueKind.FinishAttack:
                FinishDragonAttackAnimation(slot);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DragonCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Frames spent in each body rise or sink movement phase.</summary>
    private const int DragonRiseOrSinkFrames = 48;
    /// <summary>Fireballs requested by one attacking volley.</summary>
    private const int DragonShotCount = 3;
    /// <summary>Pause after attack completion before the body begins sinking.</summary>
    private const int DragonWaitBeforeSinkFrames = 96;
    /// <summary>Pause beneath the platform before the next rise attempt.</summary>
    private const int DragonWaitBeforeRiseFrames = 128;


    /// <summary>Initializes the Dragon.</summary>
    internal void InitializeDragon(RoomEnemySlot slot)
    {
        DragonEnemyState state = CreateDragonState(slot);
        state.AnimationFinished = false;

        if (state.IsWing)
        {
            // The wing begins on table index two (left-facing wings) and never runs body AI.
            // Its intangibility property keeps the cosmetic record out of touch/shot collision passes.
            state.RequestedAnimation = DragonAnimationSelector.WingsFacingLeft;
            state.InstalledAnimation = DragonAnimationSelector.WingsFacingLeft;
            state.Playback = new(DragonAnimations.StartingWith(DragonPhases.Animation(
                DragonAnimationSelector.WingsFacingLeft)), AnimationPlace.StartOfAnimation);
            slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
            state.Function = DragonEnemyFunction.WingNoOp;
            return;
        }

        state.RequestedAnimation = DragonAnimationSelector.IdleFacingLeft;
        state.InstalledAnimation = DragonAnimationSelector.IdleFacingLeft;
        state.Playback = new(DragonAnimations.StartingWith(DragonPhases.Animation(
            DragonAnimationSelector.IdleFacingLeft)), AnimationPlace.StartOfAnimation);
        state.Function = DragonEnemyFunction.WaitToRise;
    }

    /// <summary>Runs the Dragon's main AI and its five body functions.</summary>
    private void RunDragonMain(RoomEnemySlot slot, DragonEnemyState state, SamusState? samus)
    {
        switch (state.Function)
        {
            case DragonEnemyFunction.WingNoOp:
                return;

            case DragonEnemyFunction.WaitToRise:
                RunDragonWaitToRise(slot, state, samus);
                return;

            case DragonEnemyFunction.Rising:
                RunDragonRising(slot, state);
                return;

            case DragonEnemyFunction.Attacking:
                RunDragonAttacking(slot, state);
                return;

            case DragonEnemyFunction.WaitToSink:
                RunDragonWaitToSink(slot, state);
                return;

            case DragonEnemyFunction.Sinking:
                RunDragonSinking(slot, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Dragon function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Waits for Samus before rising.</summary>
    internal void RunDragonWaitToRise(
        RoomEnemySlot body,
        DragonEnemyState bodyState,
        SamusState? samus)
    {
        if (--bodyState.FunctionTimer >= 0)
            return;
        if (samus is null)
            throw new InvalidOperationException("Dragon facing selection requires the active Samus actor.");

        RoomEnemySlot wing = GetDragonWing(body);
        DragonEnemyState wingState = wing.RequireSpeciesState<DragonEnemyState>();
        bodyState.FunctionTimer = DragonRiseOrSinkFrames;
        bodyState.Function = DragonEnemyFunction.Rising;

        // The wait records the sign of (Samus X - body X) as the body's facing. The following list
        // edits use that sign to select even/odd left/right table entries for both physical records.
        bool facingLeft = CoordinateAxis.Difference(samus.Position.X.Whole, body.Position.X.Whole) < 0;
        bodyState.FacingLeft = facingLeft;
        bodyState.RequestedAnimation = DragonPhases.WithFacing(
            bodyState.RequestedAnimation,
            facingLeft);
        wingState.RequestedAnimation = DragonPhases.WithFacing(
            wingState.RequestedAnimation,
            facingLeft);
        StartDragonAnimation(body, bodyState);
        StartDragonAnimation(wing, wingState);
    }

    /// <summary>Rises out of the lava.</summary>
    internal void RunDragonRising(RoomEnemySlot body, DragonEnemyState bodyState)
    {
        if (--bodyState.FunctionTimer < 0)
        {
            bodyState.RequestedAnimation =
                DragonPhases.AttackingWithSameFacing(
                    bodyState.RequestedAnimation);
            bodyState.AttackCounter = DragonShotCount;
            bodyState.Function = DragonEnemyFunction.Attacking;
        }

        // The 16.16 constant is exactly -1.0, so both
        // subpositions remain untouched while body and wing rise one whole pixel.
        RoomEnemySlot wing = GetDragonWing(body);
        body.Position = body.Position.WithWrappedWholeY(body.Position.Y.Whole - 1);
        wing.Position = wing.Position.WithWrappedWholeY(wing.Position.Y.Whole - 1);
    }

    /// <summary>Attacks, spitting fireballs.</summary>
    internal void RunDragonAttacking(RoomEnemySlot body, DragonEnemyState bodyState)
    {
        StartDragonAnimation(body, bodyState);
        if (!bodyState.AnimationFinished)
            return;

        bodyState.AnimationFinished = false;

        // The attack restarts after each animation reaches its sleep; that restart is what spaces the three fireballs
        // by a whole attack animation.
        bodyState.ReinstallRequested = true;
        SpawnDragonFireball(body, bodyState);
        // Each fireball queues one sound; two Dragons attacking in one frame queue two.
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.DragonFireballSound, priority: SoundRequestPriority.Normal);
        if (--bodyState.AttackCounter != 0)
            return;

        bodyState.RequestedAnimation =
            DragonPhases.IdleWithSameFacing(
                bodyState.RequestedAnimation);
        bodyState.FunctionTimer = DragonWaitBeforeSinkFrames;
        bodyState.Function = DragonEnemyFunction.WaitToSink;
    }

    /// <summary>Waits before sinking.</summary>
    private static void RunDragonWaitToSink(
        RoomEnemySlot body,
        DragonEnemyState bodyState)
    {
        if (--bodyState.FunctionTimer != 0)
            return;

        bodyState.FunctionTimer = DragonRiseOrSinkFrames;
        bodyState.Function = DragonEnemyFunction.Sinking;
        StartDragonAnimation(body, bodyState);
    }

    /// <summary>Sinks back into the lava.</summary>
    private void RunDragonSinking(RoomEnemySlot body, DragonEnemyState bodyState)
    {
        if (--bodyState.FunctionTimer < 0)
        {
            bodyState.FunctionTimer = DragonWaitBeforeRiseFrames;
            bodyState.Function = DragonEnemyFunction.WaitToRise;
        }

        // As with rising, the regionalized 16.16 displacement is exactly one pixel.
        RoomEnemySlot wing = GetDragonWing(body);
        body.Position = body.Position.WithWrappedWholeY(body.Position.Y.Whole + 1);
        wing.Position = wing.Position.WithWrappedWholeY(wing.Position.Y.Whole + 1);
    }

    /// <summary>The animation cue that finishes the attack animation.</summary>
    private static void FinishDragonAttackAnimation(RoomEnemySlot body)
    {
        DragonEnemyState state = body.RequireSpeciesState<DragonEnemyState>();
        if (state.IsWing)
        {
            throw new InvalidDataException(
                "Dragon wing reached the body-only attack-finished instruction.");
        }
        state.AnimationFinished = true;
    }

    /// <summary>
    /// Runs the shared tail after touch, shot, or power-bomb common AI. A dead
    /// body deletes its wing; a surviving body mirrors every timer and handler that affects
    /// visible hurt and freeze rendering into that cosmetic record.
    /// </summary>
    private void ResolveDragonCombatAfterCommon(RoomEnemySlot actor)
    {
        // The follow-up reaches the enemy placed after whichever part was hit: the body's own wing, or,
        // for a power bomb hitting the intangible wing, the next enemy (FollowUpAfterStruckPart).
        RoomEnemySlot following = _partners.Of(actor).FollowUpRecipient;
        if (actor.Health == 0)
        {
            following.Presence = following.Presence.With(EnemyPresence.Deleted);
            return;
        }

        following.ShakeTimer = actor.ShakeTimer;
        following.InvincibilityTimer = actor.InvincibilityTimer;
        following.FlashTimer = actor.FlashTimer;
        following.FrozenTimer = actor.FrozenTimer;
        following.AiStates = actor.AiStates;
    }

    /// <summary>Installs a newly requested animation list once and resets its frame and loop timers.</summary>
    internal static void StartDragonAnimation(
        RoomEnemySlot slot,
        DragonEnemyState state)
    {
        if (!state.ReinstallRequested && state.RequestedAnimation == state.InstalledAnimation)
            return;
        state.InstalledAnimation = state.RequestedAnimation;
        state.ReinstallRequested = false;
        state.Playback = EnemyAnimationPlayback.Start(slot, DragonAnimations.StartingWith(DragonPhases.Animation(
            state.RequestedAnimation)));
    }

    /// <summary>Requires the wing the Dragon body found when it was placed, still holding its authored wing record.</summary>
    private RoomEnemySlot GetDragonWing(RoomEnemySlot body)
    {
        RoomEnemySlot wing = _partners.Of(body).Wing
            ?? throw new InvalidDataException($"Dragon enemy {body.SlotIndex} is a wing, not a body with a wing.");
        if (body.SpawnSettings<DragonSpawnSettings>().Part == DragonPart.Wing || wing.DefinitionId != EnemyDefinitionId.Dragon ||
            wing.SpawnSettings<DragonSpawnSettings>().Part != DragonPart.Wing)
        {
            throw new InvalidDataException(
                $"Dragon body slot {body.SlotIndex} is not followed by its retail wing record.");
        }
        return wing;
    }

    /// <summary>
    /// Creates one Dragon slot's state, selecting body or wing from its spawn part. A body finds its wing,
    /// which its room lists right after it, and every part finds the enemy its follow-up to a hit reaches.
    /// </summary>
    private DragonEnemyState CreateDragonState(RoomEnemySlot slot)
    {
        bool isWing = slot.SpawnSettings<DragonSpawnSettings>().Part == DragonPart.Wing;
        _partners.Place(slot, new DragonPartners(
            Wing: isWing ? null : _slotTable.ListedAfter(slot, 1),
            FollowUpRecipient: FollowUpAfterStruckPart.Recipients(distance => _slotTable.ListedAfter(slot, distance), reach: 1)[0]));
        var state = new DragonEnemyState { IsWing = isWing };
        slot.SetSpeciesState(state);
        return state;
    }

    /// <summary>Applies Dragon's paired-body combat tail after the common bomb-as-shot callback.</summary>
    internal void AddDragonBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.Dragon &&
                                                       c.SelectedShotAi == EnemyReaction.DragonShot)
        {
            AfterCommon = c => ResolveDragonCombatAfterCommon(c.Enemy),
        });
    }

    /// <summary>Connects Dragon fireballs to their per-frame gravity and camera-boundary frame behaviour.</summary>
    internal static void AddDragonProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddFrameBehavior(context =>
        {
            RunDragonFireballFrameBehavior(context.Projectile, context.Camera.Y);
        }, EnemyProjectileFrameBehavior.DragonFireballArc);
    }

    /// <summary>Runs Dragon's main AI.</summary>
    internal void RunDragonMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunDragonMain(slot, slot.RequireSpeciesState<DragonEnemyState>(), frame.Samus);
    }

    /// <summary>Advances Dragon's typed animation.</summary>
    internal static void AdvanceDragonTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, DragonAnimations.Segments,
            static (DragonEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyDragonCue(slot, cue) is { } target
                ? new AnimationContinuation<DragonSegment>(DragonAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);

    /// <summary>Synchronizes wing and body state after a Dragon power-bomb reaction.</summary>
    internal void AddDragonPowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Dragon &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyReaction.DragonPowerBomb)
        {
            AfterCommon = (enemy, _) => ResolveDragonCombatAfterCommon(enemy),
        });
    }

    /// <summary>Synchronizes wing and body state after common projectile damage resolution.</summary>
    internal void AddDragonShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Dragon &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.DragonShot)
        {
            AfterCommon = c => ResolveDragonCombatAfterCommon(c.Enemy),
        });
    }

    /// <summary>Runs ordinary enemy touch damage before mirroring Dragon's hurt and freeze state into its wing.</summary>
    internal void AddDragonTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.Dragon &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.DragonTouch,
            context =>
            {
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus);
                ResolveDragonCombatAfterCommon(context.Slot);
            });
    }
    /// <summary>Upward launch velocity of each Dragon fireball: three pixels and 193 256ths per update.</summary>
    private static readonly SignedEightEight DragonFireballInitialYVelocity = -SignedEightEight.FromParts(3, 193);
    /// <summary>The fireball's sideways speed, two and three quarter pixels per update, toward the Dragon's facing.</summary>
    private static readonly SignedEightEight DragonFireballHorizontalSpeed = SignedEightEight.FromParts(2, 192);
    /// <summary>Downward acceleration applied to a live fireball, an eighth of a pixel per update each update.</summary>
    private static readonly SignedEightEight DragonFireballGravity = SignedEightEight.FromParts(0, 32);

    /// <summary>Spawns and initializes a Dragon fireball.</summary>
    internal void SpawnDragonFireball(RoomEnemySlot body, DragonEnemyState bodyState)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.DragonFireball,
            SpriteGraphics.SpawnedBy(body));
        projectile.Position = projectile.Position.WithY(new(CoordinateAxis.Wrap(body.Position.Y.Whole - 28), 0));
        projectile.Velocity = projectile.Velocity.WithY(DragonFireballInitialYVelocity);

        if (bodyState.FacingLeft)
        {
            projectile.Position = projectile.Position.WithWrappedWholeX(body.Position.X.Whole - 12);
            projectile.Velocity = projectile.Velocity.WithX(-DragonFireballHorizontalSpeed);
            projectile.InstallAnimation(new DragonFireballAnimation(DragonFireballSegment.RisingLeft));
        }
        else
        {
            projectile.Position = projectile.Position.WithWrappedWholeX(body.Position.X.Whole + 12);
            projectile.Velocity = projectile.Velocity.WithX(DragonFireballHorizontalSpeed);
            projectile.InstallAnimation(new DragonFireballAnimation(DragonFireballSegment.RisingRight));
        }
        projectile.Position = projectile.Position.WithFractionX(0);
    }

    /// <summary>Runs the fireball's per-frame behaviour.</summary>
    internal static void RunDragonFireballFrameBehavior(
        EnemyProjectile projectile,
        int cameraY)
    {
        projectile.Position = projectile.Position.Advance(projectile.Velocity.ToSixteenSixteen());

        if (projectile.Velocity.Y.IsNegative)
        {
            projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + DragonFireballGravity);
            if (projectile.Velocity.Y.IsNegative)
                return;

            // The exact zero crossing switches from the rising pair of two-frame loops to
            // the falling pair and forces an animation tick on this same projectile pass.
            projectile.StartAnimation(new DragonFireballAnimation(projectile.Velocity.X.IsNegative
                ? DragonFireballSegment.FallingLeft
                : DragonFireballSegment.FallingRight));
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + DragonFireballGravity);

        // The fireball is retained above the viewport; horizontal distance is never consulted.
        EnemyProjectileEngine.DeleteEnemyProjectileIfVerticallyOffScreen(projectile, cameraY);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        // Dragon keeps no per-frame publication; its sounds are queued as they happen.
    }
}

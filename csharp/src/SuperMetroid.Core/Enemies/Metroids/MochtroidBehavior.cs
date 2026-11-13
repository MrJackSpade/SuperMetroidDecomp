using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>The Mochtroid species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class MochtroidBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Mochtroid];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new MochtroidEntity(this, slot, common);

    /// <summary>Resolves axis movement against terrain in the order for each flight mode.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Queues an individual drain sound for each qualifying attached actor.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Provides the shared enemy counter that gates attached drain sounds.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Runs common touch damage when Samus has an active contact-damage mode.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Binds movement, audio, frame counter, and collision services for Mochtroid AI.</summary>
    internal MochtroidBehavior(EnemyTerrain terrain, EnemyAudioQueue audio, EnemyFrameContext frameContext, EnemyCollision collision)
    {
        _terrain = terrain;
        _audio = audio;
        _frameContext = frameContext;
        _collision = collision;
    }

    /// <summary>Number of overlapping contacts between attached health drains.</summary>
    private const int MochtroidAttachmentDamagePeriod = 80;
    /// <summary>Whole-pixel speed cap for proportional free flight.</summary>
    private const int MochtroidMaximumVelocity = 3;
    /// <summary>Speed of one pixel per attached-flight axis step.</summary>
    private static readonly SignedSixteenSixteen MochtroidAttachedVelocity = SignedSixteenSixteen.FromWholePixels(1);

    /// <summary>Initializes the Mochtroid.</summary>
    internal static void InitializeMochtroid(RoomEnemySlot slot)
    {
        // Room clearing initializes both parallel values to zero. Starting the
        // first nonzero animation is important: the equality shortcut is also the
        // source of its documented respawning-Mochtroid crash when stale extension state is
        // retained. Ordinary room loads start clean and therefore take the safe path.
        var state = new MochtroidEnemyState
        {
            InstalledAnimation = null,
            AttachmentDamageTimer = 0,
            MovementMode = MochtroidMovementMode.NotTouchingSamus,
        };
        slot.SetSpeciesState(state);
        SetMochtroidAnimation(
            slot, state, MochtroidSegment.FreeFlight);
    }

    /// <summary>Runs a Mochtroid's per-frame behaviour, resetting its state before dispatch.</summary>
    private void RunMochtroidMain(
        RoomEnemySlot slot,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Mochtroid AI requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("Mochtroid movement requires room level data.");

        MochtroidEnemyState state = slot.RequireSpeciesState<MochtroidEnemyState>();
        MochtroidMovementMode selectedMode = state.MovementMode;
        state.MovementMode = MochtroidMovementMode.NotTouchingSamus;
        switch (selectedMode)
        {
            case MochtroidMovementMode.NotTouchingSamus:
                RunMochtroidFreeFlight(slot, state, samus, level);
                return;
            case MochtroidMovementMode.TouchingSamus:
                RunMochtroidAttachedFlight(slot, state, samus, level);
                return;
            case MochtroidMovementMode.Shaking:
                RunMochtroidShake(slot, state);
                return;
            default:
                throw new InvalidDataException(
                    $"Mochtroid movement {selectedMode} is not one of its three movements.");
        }
    }

    /// <summary>Accelerates toward Samus, resolving Y before X and clearing velocity on terrain contact.</summary>
    private void RunMochtroidFreeFlight(
        RoomEnemySlot slot,
        MochtroidEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // The game treats each coordinate difference as signed, divides it by four,
        // takes that result as 256ths of a pixel, and subtracts it from velocity.
        // This is a proportional attraction toward Samus—not a fixed-speed seek—and the
        // accumulated fraction is observable before either whole component reaches ±3.
        int yDifference = CoordinateAxis.Difference(slot.Position.Y.Whole, samus.Position.Y.Whole);
        SignedSixteenSixteen yVelocity = ClampMochtroidVelocity(state.Velocity.Y - EnemyMath.HomingPull(yDifference));
        StoreMochtroidYVelocity(state, yVelocity);
        if (_terrain.MoveEnemyVertically(level, slot, yVelocity))
            StoreMochtroidYVelocity(state, SignedSixteenSixteen.Zero);

        // Y is intentionally resolved first. Horizontal collision therefore sees the
        // vertically adjusted position, matching the game's vertical then horizontal collision.
        int xDifference = CoordinateAxis.Difference(slot.Position.X.Whole, samus.Position.X.Whole);
        SignedSixteenSixteen xVelocity = ClampMochtroidVelocity(state.Velocity.X - EnemyMath.HomingPull(xDifference));
        StoreMochtroidXVelocity(state, xVelocity);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, xVelocity))
            StoreMochtroidXVelocity(state, SignedSixteenSixteen.Zero);

        SetMochtroidAnimation(
            slot, state, MochtroidSegment.FreeFlight);
    }

    /// <summary>Caps a signed 16.16 velocity by its whole-pixel component at three pixels.</summary>
    private static SignedSixteenSixteen ClampMochtroidVelocity(SignedSixteenSixteen velocity) => EnemyMath.ClampWholeVelocity(velocity, MochtroidMaximumVelocity);

    /// <summary>Moves one pixel per axis toward Samus, resolving X before Y while attached.</summary>
    private void RunMochtroidAttachedFlight(
        RoomEnemySlot slot,
        MochtroidEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // Attached mode discards proportional acceleration. It moves exactly one pixel per
        // axis toward Samus, with X collision resolved before Y and ignored return values.
        SignedSixteenSixteen xVelocity = slot.Position.X.Whole == samus.Position.X.Whole
            ? SignedSixteenSixteen.Zero
            : CoordinateAxis.Difference(slot.Position.X.Whole, samus.Position.X.Whole) > 0
                ? -MochtroidAttachedVelocity
                : MochtroidAttachedVelocity;
        StoreMochtroidXVelocity(state, xVelocity);
        _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, xVelocity);

        SignedSixteenSixteen yVelocity = slot.Position.Y.Whole == samus.Position.Y.Whole
            ? SignedSixteenSixteen.Zero
            : CoordinateAxis.Difference(slot.Position.Y.Whole, samus.Position.Y.Whole) > 0
                ? -MochtroidAttachedVelocity
                : MochtroidAttachedVelocity;
        StoreMochtroidYVelocity(state, yVelocity);
        _terrain.MoveEnemyVertically(level, slot, yVelocity);
    }

    /// <summary>Applies the authored four-direction shake offset and counts down to free flight.</summary>
    private static void RunMochtroidShake(RoomEnemySlot slot, MochtroidEnemyState state)
    {
        // This third dispatch entry is unused by retail callers but present in the shipped
        // table. The low two even bits select right/up/left/down, then the timer counts down.
        slot.Position += MochtroidShake.Offset(state.ShakeTimer);
        StoreMochtroidXVelocity(state, SignedSixteenSixteen.Zero);
        StoreMochtroidYVelocity(state, SignedSixteenSixteen.Zero);
        if (--state.ShakeTimer == 0)
            state.MovementMode = MochtroidMovementMode.NotTouchingSamus;
        SetMochtroidAnimation(
            slot, state, MochtroidSegment.FreeFlight);
    }

    /// <summary>Stores the current horizontal velocity in the species state.</summary>
    private static void StoreMochtroidXVelocity(MochtroidEnemyState state, SignedSixteenSixteen velocity)
    {
        state.Velocity = state.Velocity.WithX(velocity);
    }

    /// <summary>Stores the current vertical velocity in the species state.</summary>
    private static void StoreMochtroidYVelocity(MochtroidEnemyState state, SignedSixteenSixteen velocity)
    {
        state.Velocity = state.Velocity.WithY(velocity);
    }

    /// <summary>Installs a changed flight or attachment animation and restarts its frame timers.</summary>
    internal static void SetMochtroidAnimation(
        RoomEnemySlot slot,
        MochtroidEnemyState state,
        MochtroidSegment segment) =>
        EnemyAnimationPlayback.StartOnce(slot, state.InstalledAnimation, segment, installed =>
        {
            state.InstalledAnimation = installed;
            state.Playback = new(MochtroidAnimations.StartingWith(installed), AnimationPlace.StartOfAnimation);
        });

    /// <summary>Resolves the family-specific portion of a Mochtroid's touch.</summary>
    private void ResolveMochtroidTouch(
        RoomEnemySlot slot,
        MochtroidEnemyState state,
        SamusState samus)
    {
        state.MovementMode = MochtroidMovementMode.TouchingSamus;
        SetMochtroidAnimation(
            slot, state, MochtroidSegment.Attached);
        state.CountAttachedUpdate();

        bool applyCommonTouch = samus.HorizontalSpeed.ContactDamageIndex != SamusContactDamageIndex.None;
        if (!applyCommonTouch)
        {
            // Every overlapping actor queues its sound independently. Using the
            // legacy one-value family field here collapsed a room full of attached actors
            // into one request and erased the game's sound-backlog processing delay.
            if (MetroidCadenceDefinitions.DrainSoundEveryEighthEnemyPass.IsDue(_frameContext.RandomEnemyCounter) && samus.Health >= 30)
                _audio.QueueEnemySound(RoomSound.IncrementalEnergy, priority: SoundRequestPriority.Normal);

            if (state.HasReachedDamagePeriod(MochtroidAttachmentDamagePeriod))
            {
                state.AttachmentDamageTimer = 0;
                applyCommonTouch = true;
            }
        }

        if (!applyCommonTouch)
            return;

        if (samus.HorizontalSpeed.ContactDamageIndex == SamusContactDamageIndex.None)
        {
            // Common touch would publish 96/5 here, but the Mochtroid's touch clears both values before
            // Samus's movement phase can consume them. Apply only the suit-divided health
            // change; starting the host knockback state and trying to undo its pose would
            // introduce side effects the game never reaches.
            SamusSuitDamage.Hurt(samus, slot.Definition.Damage);
        }
        else
        {
            // A live Samus contact-damage mode takes common AI's enemy-damage branch. This
            // includes Speed Booster, shinespark, Screw Attack, and pseudo-Screw.
            _collision.ResolveNormalEnemyTouch(slot, samus);
        }

        // Unlike every ordinary contact handler, Mochtroid immediately cancels the timers
        // installed by common touch AI. Samus remains attached rather than entering a normal
        // invincibility/knockback grace period, permitting another 80-contact drain cycle.
        samus.InvincibilityTimer = WrappingCountdown.Start(0);
        samus.KnockbackTimer = 0;
    }

    /// <summary>Advances Mochtroid's typed animation.</summary>
    internal static void AdvanceMochtroidTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, MochtroidAnimations.Segments,
            static (MochtroidEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (state, cue) => throw new InvalidOperationException($"Mochtroid's cue-free animation raised {cue}."));

    /// <summary>Runs Mochtroid's main AI.</summary>
    internal void RunMochtroidMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunMochtroidMain(slot, frame.Samus, frame.Level);
    }

    /// <summary>Registers the named attachment and drain reaction for Samus overlap.</summary>
    internal void AddMochtroidTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.Mochtroid &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.MochtroidTouch,
            context => ResolveMochtroidTouch(context.Slot, context.Slot.RequireSpeciesState<MochtroidEnemyState>(), context.Samus));
    }


    /// <summary>Registers the Mochtroid-specific shot callback when its definition selects one.</summary>
    internal static void AddMochtroidShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Mochtroid &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.MochtroidShot));
    }
}

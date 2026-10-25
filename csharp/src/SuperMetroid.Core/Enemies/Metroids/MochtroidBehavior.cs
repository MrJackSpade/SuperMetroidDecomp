using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Metroids.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>The Mochtroid species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class MochtroidBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Mochtroid];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new MochtroidEntity(this, slot, common);

    /// <summary>Resolves axis movement against terrain in the native order for each flight mode.</summary>
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
    private const ushort MochtroidAttachmentDamagePeriod = 80;
    /// <summary>Whole-pixel speed cap for proportional free flight.</summary>
    private const int MochtroidMaximumVelocity = 3;
    /// <summary>Signed 16.16 speed of one pixel per attached-flight axis step.</summary>
    private const int MochtroidAttachedVelocity = 1 << 16;

    /// <summary>Ports <c>InitAI_Mochtroid</c> at $A3:A77D.</summary>
    internal static void InitializeMochtroid(RoomEnemySlot slot)
    {
        // Native room WRAM clearing initializes both parallel words to zero. Installing the
        // first nonzero list is important: the original's equality shortcut is also the
        // source of its documented respawning-Mochtroid crash when stale extension RAM is
        // retained. Ordinary room loads start clean and therefore take the safe path.
        var state = new MochtroidEnemyState
        {
            InstalledProgram = null,
            AttachmentDamageTimer = 0,
            MovementMode = MochtroidMovementMode.NotTouchingSamus,
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);
        slot.Layer = 2;
        SetMochtroidProgram(
            slot, state, MochtroidProgram.FreeFlight);
    }

    /// <summary>Ports the reset-before-dispatch behavior of <c>MainAI_Mochtroid</c>.</summary>
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
                    $"Mochtroid movement {selectedMode} exceeds its three-entry ROM table.");
        }
    }

    /// <summary>Accelerates toward Samus, resolving Y before X and clearing velocity on terrain contact.</summary>
    private void RunMochtroidFreeFlight(
        RoomEnemySlot slot,
        MochtroidEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // The cartridge treats each coordinate difference as signed, divides it by four,
        // shifts that result left eight fractional bits, and subtracts it from velocity.
        // This is a proportional attraction toward Samus—not a fixed-speed seek—and the
        // accumulated fraction is observable before either whole component reaches ±3.
        int yDifference = unchecked((short)(slot.Position.Y.Whole - samus.Position.Y.Whole));
        int yVelocity = unchecked(state.Velocity.Y.RawValue - SnesEightEight.FromWhole(yDifference >> 2));
        yVelocity = ClampMochtroidVelocity(yVelocity);
        StoreMochtroidYVelocity(state, yVelocity);
        if (_terrain.MoveEnemyVertically(level, slot, yVelocity))
            StoreMochtroidYVelocity(state, 0);

        // Y is intentionally resolved first. Horizontal collision therefore sees the
        // vertically adjusted position, matching $A3:A80D followed by $A3:A87C.
        int xDifference = unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole));
        int xVelocity = unchecked(state.Velocity.X.RawValue - SnesEightEight.FromWhole(xDifference >> 2));
        xVelocity = ClampMochtroidVelocity(xVelocity);
        StoreMochtroidXVelocity(state, xVelocity);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, xVelocity))
            StoreMochtroidXVelocity(state, 0);

        SetMochtroidProgram(
            slot, state, MochtroidProgram.FreeFlight);
    }

    /// <summary>Caps a signed 16.16 velocity by its whole-pixel component at three pixels.</summary>
    private static int ClampMochtroidVelocity(int velocity) => EnemyMath.ClampWholeVelocity(velocity, MochtroidMaximumVelocity);

    /// <summary>Moves one pixel per axis toward Samus, resolving X before Y while attached.</summary>
    private void RunMochtroidAttachedFlight(
        RoomEnemySlot slot,
        MochtroidEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // Attached mode discards proportional acceleration. It moves exactly one pixel per
        // axis toward Samus, with X collision resolved before Y and ignored return values.
        int xVelocity = slot.Position.X.Whole == samus.Position.X.Whole
            ? 0
            : unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)) > 0
                ? -MochtroidAttachedVelocity
                : MochtroidAttachedVelocity;
        StoreMochtroidXVelocity(state, xVelocity);
        _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, xVelocity);

        int yVelocity = slot.Position.Y.Whole == samus.Position.Y.Whole
            ? 0
            : unchecked((short)(slot.Position.Y.Whole - samus.Position.Y.Whole)) > 0
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
        slot.Position += MochtroidShakeDefinitions.Offset(state.ShakeTimer.RawValue);
        StoreMochtroidXVelocity(state, 0);
        StoreMochtroidYVelocity(state, 0);
        state.ShakeTimer = state.ShakeTimer.Decrement().Remaining;
        if (state.ShakeTimer.IsZero)
            state.MovementMode = MochtroidMovementMode.NotTouchingSamus;
        SetMochtroidProgram(
            slot, state, MochtroidProgram.FreeFlight);
    }

    /// <summary>Stores the current raw 16.16 horizontal velocity in the species state.</summary>
    private static void StoreMochtroidXVelocity(MochtroidEnemyState state, int velocity)
    {
        state.Velocity = state.Velocity.WithX(SnesSignedSixteenSixteen.FromRaw(velocity));
    }

    /// <summary>Stores the current raw 16.16 vertical velocity in the species state.</summary>
    private static void StoreMochtroidYVelocity(MochtroidEnemyState state, int velocity)
    {
        state.Velocity = state.Velocity.WithY(SnesSignedSixteenSixteen.FromRaw(velocity));
    }

    /// <summary>Installs a changed flight or attachment animation and restarts its instruction timers.</summary>
    internal static void SetMochtroidProgram(
        RoomEnemySlot slot,
        MochtroidEnemyState state,
        MochtroidProgram program) =>
        EnemyProgramAnimation.InstallOnce(slot, state.InstalledProgram, program, installed =>
        {
            state.InstalledProgram = installed;
            state.Program = AnimationProgramCursor<MochtroidProgram>.Start(installed);
        });

    /// <summary>Ports the family-specific portion of <c>EnemyTouch_Mochtroid</c>.</summary>
    private void ResolveMochtroidTouch(
        RoomEnemySlot slot,
        MochtroidEnemyState state,
        SamusState samus)
    {
        state.MovementMode = MochtroidMovementMode.TouchingSamus;
        SetMochtroidProgram(
            slot, state, MochtroidProgram.Attached);
        state.CountAttachedUpdate();

        bool applyCommonTouch = samus.HorizontalSpeed.ContactDamageIndex != SamusContactDamageIndex.None;
        if (!applyCommonTouch)
        {
            // Every overlapping actor invokes the queue routine independently. Using the
            // legacy one-value family field here collapsed a room full of attached actors
            // into one request and erased the cartridge's sound-backlog processing delay.
            if ((_frameContext.RandomEnemyCounter & 7) == 7 && samus.Health >= 30)
                _audio.QueueEnemySound(Library3Sound.IncrementalEnergy, maximumQueued: SoundQueueLimit.Max6);

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
            // Common touch would publish 96/5 here, but $A3:A99E clears both words before
            // Samus's movement phase can consume them. Apply only the suit-divided health
            // change; starting the host knockback state and trying to undo its pose would
            // introduce side effects the cartridge never reaches.
            int damage = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
                ? unchecked((ushort)(slot.Definition.Damage >> 2))
                : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                    ? unchecked((ushort)(slot.Definition.Damage >> 1))
                    : slot.Definition.Damage;
            samus.Health = samus.Health <= damage
                ? 0
                : samus.Health - damage;
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
        samus.KnockbackTimer = WrappingCountdown.Start(0);
    }

    /// <summary>Advances Mochtroid's typed animation.</summary>
    internal static void AdvanceMochtroidTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, MochtroidAnimations.Programs, MochtroidFrameDefinitions.ShownFrames.Of,
            static (MochtroidEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor);

    /// <summary>Ports Mochtroid's main AI.</summary>
    internal void RunMochtroidMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunMochtroidMain(slot, frame.Samus, frame.Level);
    }

    /// <summary>Registers the named attachment and drain reaction for Samus overlap.</summary>
    internal void AddMochtroidTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.Mochtroid &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.MochtroidTouch,
            context => ResolveMochtroidTouch(context.Slot, context.Slot.RequireSpeciesState<MochtroidEnemyState>(), context.Samus));
    }


    /// <summary>Registers the Mochtroid-specific shot callback when its definition selects one.</summary>
    internal static void AddMochtroidShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Mochtroid &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.MochtroidShot));
    }
}

using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Mochtroid species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class MochtroidBehavior : IEnemySpecies
{
    /// <summary>Stores terrain.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Stores audio.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Stores frame context.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Stores collision.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Initializes a new mochtroid behavior instance.</summary>
    internal MochtroidBehavior(EnemyTerrain terrain, EnemyAudioQueue audio, EnemyFrameContext frameContext, EnemyCollision collision)
    {
        _terrain = terrain;
        _audio = audio;
        _frameContext = frameContext;
        _collision = collision;
    }

    /// <summary>Stores mochtroid attachment damage period.</summary>
    private const ushort MochtroidAttachmentDamagePeriod = 0x0050;
    /// <summary>Stores mochtroid maximum velocity.</summary>
    private const int MochtroidMaximumVelocity = 3;
    /// <summary>Stores mochtroid attached velocity.</summary>
    private const int MochtroidAttachedVelocity = 1 << 16;

    /// <summary>Stores mochtroid states.</summary>
    internal readonly MochtroidEnemyState?[] _mochtroidStates =
        new MochtroidEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Mochtroid</c> at $A3:A77D.</summary>
    internal void InitializeMochtroid(RoomEnemySlot slot)
    {
        // Native room WRAM clearing initializes both parallel words to zero. Installing the
        // first nonzero list is important: the original's equality shortcut is also the
        // source of its documented respawning-Mochtroid crash when stale extension RAM is
        // retained. Ordinary room loads start clean and therefore take the safe path.
        var state = new MochtroidEnemyState(slot)
        {
            InstalledInstructionList = 0,
            AttachmentDamageTimer = 0,
            MovementMode = MochtroidMovementMode.NotTouchingSamus,
        };
        _mochtroidStates[slot.SlotIndex] = state;
        slot.Layer = 2;
        SetMochtroidInstructionList(
            slot, state, MochtroidInstructionProgramDefinitions.FreeFlight);
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

        MochtroidEnemyState state = RequireMochtroidState(slot);
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
                    $"Mochtroid movement index {(ushort)selectedMode} exceeds its three-entry ROM table.");
        }
    }

    /// <summary>Runs mochtroid free flight.</summary>
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

        SetMochtroidInstructionList(
            slot, state, MochtroidInstructionProgramDefinitions.FreeFlight);
    }

    /// <summary>Clamp mochtroid velocity.</summary>
    private static int ClampMochtroidVelocity(int velocity)
    {
        short whole = unchecked((short)(SnesSixteenSixteen.WholeOf(velocity)));
        if (whole >= MochtroidMaximumVelocity)
            return SnesSixteenSixteen.FromWhole(MochtroidMaximumVelocity);
        if (whole < -MochtroidMaximumVelocity)
            return SnesSixteenSixteen.FromWhole(-MochtroidMaximumVelocity);
        return velocity;
    }

    /// <summary>Runs mochtroid attached flight.</summary>
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

    /// <summary>Runs mochtroid shake.</summary>
    private static void RunMochtroidShake(RoomEnemySlot slot, MochtroidEnemyState state)
    {
        // This third dispatch entry is unused by retail callers but present in the shipped
        // table. The low two even bits select right/up/left/down, then the timer counts down.
        (int x, int y) = MochtroidShakeDefinitions.Offset(state.ShakeTimer);
        slot.Position += new Offset16<RoomPixels>(unchecked((short)(x)), unchecked((short)(y)));
        StoreMochtroidXVelocity(state, 0);
        StoreMochtroidYVelocity(state, 0);
        state.ShakeTimer = unchecked((ushort)(state.ShakeTimer - 1));
        if (state.ShakeTimer == 0)
            state.MovementMode = MochtroidMovementMode.NotTouchingSamus;
        SetMochtroidInstructionList(
            slot, state, MochtroidInstructionProgramDefinitions.FreeFlight);
    }

    /// <summary>Store mochtroid xvelocity.</summary>
    private static void StoreMochtroidXVelocity(MochtroidEnemyState state, int velocity)
    {
        state.Velocity = state.Velocity.WithX(SnesSignedSixteenSixteen.FromRaw(velocity));
    }

    /// <summary>Store mochtroid yvelocity.</summary>
    private static void StoreMochtroidYVelocity(MochtroidEnemyState state, int velocity)
    {
        state.Velocity = state.Velocity.WithY(SnesSignedSixteenSixteen.FromRaw(velocity));
    }

    /// <summary>Sets mochtroid instruction list.</summary>
    internal static void SetMochtroidInstructionList(
        RoomEnemySlot slot,
        MochtroidEnemyState state,
        ushort instructionList)
    {
        if (state.InstalledInstructionList == instructionList)
            return;
        state.InstalledInstructionList = instructionList;
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    /// <summary>Ports the family-specific portion of <c>EnemyTouch_Mochtroid</c>.</summary>
    private void ResolveMochtroidTouch(
        RoomEnemySlot slot,
        MochtroidEnemyState state,
        SamusState samus)
    {
        state.MovementMode = MochtroidMovementMode.TouchingSamus;
        SetMochtroidInstructionList(
            slot, state, MochtroidInstructionProgramDefinitions.Attached);
        state.AttachmentDamageTimer = unchecked((ushort)(state.AttachmentDamageTimer + 1));

        bool applyCommonTouch = samus.HorizontalSpeed.ContactDamageIndex != SamusContactDamageIndex.None;
        if (!applyCommonTouch)
        {
            // Every overlapping actor invokes the queue routine independently. Using the
            // legacy one-value family field here collapsed a room full of attached actors
            // into one request and erased the cartridge's sound-backlog processing delay.
            if ((_frameContext._randomEnemyCounter & 7) == 7 && unchecked((short)(samus.Health - 30)) >= 0)
                _audio.QueueEnemySound(SoundEffectLibrary3Sounds.AttachedEnemyDrain, maximumQueued: 6);

            if (unchecked((short)(state.AttachmentDamageTimer - MochtroidAttachmentDamagePeriod)) >= 0)
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
            ushort damage = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
                ? unchecked((ushort)(slot.Definition.Damage >> 2))
                : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                    ? unchecked((ushort)(slot.Definition.Damage >> 1))
                    : slot.Definition.Damage;
            samus.Health = samus.Health <= damage
                ? (ushort)0
                : unchecked((ushort)(samus.Health - damage));
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
        samus.InvincibilityTimer = 0;
        samus.KnockbackTimer = 0;
    }

    /// <summary>Requires mochtroid state.</summary>
    private MochtroidEnemyState RequireMochtroidState(RoomEnemySlot slot) =>
        _mochtroidStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Mochtroid state.");

    /// <summary>Registers mochtroid initialization ai.</summary>
    internal void RegisterMochtroidInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Mochtroid, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Mochtroid, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeMochtroid(slot);
        });
    }

    /// <summary>Registers mochtroid main ai.</summary>
    internal void RegisterMochtroidMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Mochtroid, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Mochtroid, (slot, frame, nmiFrameCounter8) =>
        {
            RunMochtroidMain(slot, frame.Samus, frame.Level);
        });
    }

    /// <summary>Registers mochtroid touch.</summary>
    internal void RegisterMochtroidTouch(EnemyAiTable table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Mochtroid &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.MochtroidTouch,
            context => ResolveMochtroidTouch(context.Slot, RequireMochtroidState(context.Slot), context.Samus));
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        Array.Clear(_mochtroidStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }

    /// <summary>Registers mochtroid shot reactions.</summary>
    internal static void RegisterMochtroidShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Mochtroid &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.MochtroidShot));
    }
}

using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Rios.Artwork;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>The Lower Norfair rio species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class LowerNorfairRioBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Holtz];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new LowerNorfairRioEntity(this, slot, common);

    /// <summary>Supplies the shared random draw consumed before every parent and follower AI dispatch.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves wall and floor collisions during Rio dives and returns.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Finds each flame follower in the slot immediately after its parent.</summary>
    private readonly EnemySlotTable _slotTable;

    /// <summary>Binds Holtz parent and follower behavior to room RNG, collision, and slot order.</summary>
    internal LowerNorfairRioBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemySlotTable slotTable)
    {
        _room = room;
        _terrain = terrain;
        _slotTable = slotTable;
    }

    /// <summary>Samus must be strictly within $70 horizontal pixels to permit a swoop.</summary>
    private const ushort LowerNorfairRioHorizontalTriggerDistance = 112;
    /// <summary>Each flight update subtracts 32 units from the signed 8.8 vertical velocity.</summary>
    private const ushort LowerNorfairRioGravity = 32;

    /// <summary>Most recent library-two return sound request produced during this frame.</summary>
    public SoundEffectId? LastLowerNorfairRioSoundEffect { get; private set; }

    /// <summary>Ports <c>LowerNorfairRio_Init</c> at $A2:C6F3.</summary>
    internal void InitializeLowerNorfairRio(RoomEnemySlot slot)
    {
        var state = new LowerNorfairRioEnemyState
        {
            AnimationSignal = false,
            FollowerVisible = false,
            IsFollower = slot.SpawnSettings<EnemyFollowerSpawnSettings>().Role == EnemyFollowerRole.Follower,
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);

        if (state.IsFollower)
        {
            ValidateLowerNorfairRioParent(slot);
            state.InstalledProgram = LowerNorfairRioProgram.Flames;
            state.Program = AnimationProgramCursor<LowerNorfairRioProgram>.Start(LowerNorfairRioProgram.Flames);
            state.Function = LowerNorfairRioEnemyFunction.FollowParent;
            return;
        }

        state.InstalledProgram = LowerNorfairRioProgram.Idle;
        state.Program = AnimationProgramCursor<LowerNorfairRioProgram>.Start(LowerNorfairRioProgram.Idle);
        state.Function = LowerNorfairRioEnemyFunction.WaitForAttackOpportunity;
    }

    /// <summary>
    /// Ports <c>LowerNorfairRio_Main</c> and functions one through six at $A2:C724-$C8A2.
    /// The shared RNG advances before every parent and follower dispatch, exactly as retail.
    /// </summary>
    private void RunLowerNorfairRioMain(
        RoomEnemySlot slot,
        LowerNorfairRioEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        int random = _room.Random.Generate();

        switch (state.Function)
        {
            case LowerNorfairRioEnemyFunction.FollowParent:
                FollowLowerNorfairRioParent(slot);
                return;

            case LowerNorfairRioEnemyFunction.WaitForAttackOpportunity:
                if (samus is null ||
                    (!RioLaunchDefinitions.AttackChanceLowBit.IsSetIn(random) && !RioLaunchDefinitions.AttackChanceHighBit.IsSetIn(random)) ||
                    !EnemyMath.IsWithinStrictModularDistance(
                        samus.Position.X.Whole,
                        slot.Position.X.Whole,
                        LowerNorfairRioHorizontalTriggerDistance))
                {
                    // Unlike upper Norfair Rio, this variant clears the handshake and calls
                    // its list installer on every rejected frame. The helper itself prevents
                    // an unchanged $C61A list from restarting.
                    state.AnimationSignal = false;
                    InstallLowerNorfairRioInstructionList(
                        slot,
                        state,
                        LowerNorfairRioProgram.Idle);
                    return;
                }

                state.Velocity = new(
                    new SignedEightEight(RioLaunchDefinitions.LowerNorfairXVelocity),
                    new SignedEightEight(RioLaunchDefinitions.LowerNorfairYVelocity));
                if (unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0)
                    NegateLowerNorfairRioXVelocity(state);
                InstallLowerNorfairRioInstructionList(
                    slot,
                    state,
                    LowerNorfairRioProgram.PrepareToSwoop);
                state.Function = LowerNorfairRioEnemyFunction.WaitForTakeoffAnimation;
                return;

            case LowerNorfairRioEnemyFunction.WaitForTakeoffAnimation:
                if (!state.AnimationSignal)
                    return;
                state.AnimationSignal = false;
                InstallLowerNorfairRioInstructionList(
                    slot,
                    state,
                    LowerNorfairRioProgram.Descending);
                state.Function = LowerNorfairRioEnemyFunction.Dive;
                return;

            case LowerNorfairRioEnemyFunction.Dive:
                RequireLowerNorfairRioLevel(level);
                MoveLowerNorfairRioHorizontally(level, slot, state);
                if (_terrain.MoveEnemyVertically(
                        level,
                        slot,
                        EnemyMath.ToEightBitVelocityDisplacement(state.Velocity.Y)))
                {
                    BeginLowerNorfairRioReturn(slot, state);
                    return;
                }

                ApplyLowerNorfairRioGravity(state);
                if (state.Velocity.Y.SignedRawValue < 0)
                    BeginLowerNorfairRioReturn(slot, state);
                return;

            case LowerNorfairRioEnemyFunction.ReturnToPerch:
                RequireLowerNorfairRioLevel(level);
                MoveLowerNorfairRioHorizontally(level, slot, state);
                if (_terrain.MoveEnemyVertically(
                        level,
                        slot,
                        EnemyMath.ToEightBitVelocityDisplacement(state.Velocity.Y)))
                {
                    InstallLowerNorfairRioInstructionList(
                        slot,
                        state,
                        LowerNorfairRioProgram.Cooldown);
                    state.Function = LowerNorfairRioEnemyFunction.WaitForLandingAnimation;
                    return;
                }

                ApplyLowerNorfairRioGravity(state);
                if (state.AnimationSignal)
                {
                    state.AnimationSignal = false;
                    InstallLowerNorfairRioInstructionList(
                        slot,
                        state,
                        LowerNorfairRioProgram.AscendingPart2);
                }
                return;

            case LowerNorfairRioEnemyFunction.WaitForLandingAnimation:
                if (!state.AnimationSignal)
                    return;
                state.AnimationSignal = false;
                InstallLowerNorfairRioInstructionList(
                    slot,
                    state,
                    LowerNorfairRioProgram.Cooldown);
                state.Function = LowerNorfairRioEnemyFunction.WaitForAttackOpportunity;
                return;

            default:
                throw new InvalidDataException(
                    $"Lower Norfair Rio function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Carries out one cue of the Lower Norfair Rio's animations: the finished handshake and flame visibility.</summary>
    private static LowerNorfairRioProgram? ApplyLowerNorfairRioCue(LowerNorfairRioEnemyState state, LowerNorfairRioCue cue)
    {
        switch (cue.Kind)
        {
            case LowerNorfairRioCueKind.AnimationFinished:
                state.AnimationSignal = true;
                break;
            case LowerNorfairRioCueKind.HideFlames:
                state.FollowerVisible = false;
                break;
            case LowerNorfairRioCueKind.ShowFlames:
                state.FollowerVisible = true;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(LowerNorfairRioCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>Mirrors the preceding parent's freeze and visibility, placing live flames twelve pixels below it.</summary>
    private void FollowLowerNorfairRioParent(RoomEnemySlot follower)
    {
        RoomEnemySlot parent = GetPrecedingLowerNorfairRioSlot(follower);
        if (parent.Health == 0)
        {
            follower.Properties = follower.Properties.With(EnemyProperties.Deleted);
            return;
        }
        ValidateLowerNorfairRioParent(follower);

        LowerNorfairRioEnemyState parentState = parent.RequireSpeciesState<LowerNorfairRioEnemyState>();
        follower.FrozenTimer = parent.FrozenTimer;
        if (!parent.FrozenTimer.IsZero || !parentState.FollowerVisible)
        {
            follower.Properties = follower.Properties.With(EnemyProperties.Invisible);
            return;
        }

        // gEnemySpawnData(child)[31].field_4 again aliases the preceding parent's family
        // extra word $02. Here it is Boolean rather than a signed offset. The drawing-queue
        // X/Y aliases supply the live parent origin and this variant adds a literal 12 px.
        follower.Properties = follower.Properties.Without(EnemyProperties.Invisible);
        follower.Position = follower.Position.WithWhole(new(parent.Position.X.Whole, unchecked((ushort)(parent.Position.Y.Whole + 12))));
    }

    /// <summary>Sets upward return velocity, animation, function, and its library-two sound event.</summary>
    private void BeginLowerNorfairRioReturn(
        RoomEnemySlot slot,
        LowerNorfairRioEnemyState state)
    {
        state.Velocity = state.Velocity.WithY(new SignedEightEight(unchecked((ushort)-1)));
        InstallLowerNorfairRioInstructionList(
            slot,
            state,
            LowerNorfairRioProgram.AscendingPart1);
        state.Function = LowerNorfairRioEnemyFunction.ReturnToPerch;
        LastLowerNorfairRioSoundEffect = EnemySoundEffectDefinitions.LowerNorfairRioReturnSound;
    }

    /// <summary>Advances the Rio horizontally and reverses its velocity on a wall collision.</summary>
    private void MoveLowerNorfairRioHorizontally(
        RoomLevelData level,
        RoomEnemySlot slot,
        LowerNorfairRioEnemyState state)
    {
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                EnemyMath.ToEightBitVelocityDisplacement(state.Velocity.X)))
        {
            NegateLowerNorfairRioXVelocity(state);
        }
    }

    /// <summary>Reverses the full signed 8.8 horizontal velocity after targeting or collision.</summary>
    private static void NegateLowerNorfairRioXVelocity(LowerNorfairRioEnemyState state) =>
        state.Velocity = state.Velocity.ReversedX();

    /// <summary>Applies the native negative 8.8 vertical acceleration after a flight step.</summary>
    private static void ApplyLowerNorfairRioGravity(LowerNorfairRioEnemyState state) =>
        state.Velocity = state.Velocity.WithY(new SignedEightEight(
            unchecked((ushort)(state.Velocity.Y.RawValue - LowerNorfairRioGravity))));

    /// <summary>Restarts a different Rio animation list while leaving an unchanged list's timer intact.</summary>
    private static void InstallLowerNorfairRioInstructionList(
        RoomEnemySlot slot,
        LowerNorfairRioEnemyState state,
        LowerNorfairRioProgram instructionList) =>
        EnemyProgramAnimation.InstallOnce(slot, state.InstalledProgram, instructionList, installed =>
        {
            state.InstalledProgram = installed;
            state.Program = AnimationProgramCursor<LowerNorfairRioProgram>.Start(installed);
        });

    /// <summary>Finds the parent slot immediately before a flame follower, rejecting slot zero.</summary>
    private RoomEnemySlot GetPrecedingLowerNorfairRioSlot(RoomEnemySlot follower)
    {
        if (follower.SlotIndex == 0)
        {
            throw new InvalidDataException(
                "A Lower Norfair Rio follower cannot occupy enemy slot zero.");
        }
        return _slotTable.Slots[follower.SlotIndex - 1];
    }

    /// <summary>Requires the preceding slot to be an unfollowing Holtz parent record.</summary>
    private void ValidateLowerNorfairRioParent(RoomEnemySlot follower)
    {
        RoomEnemySlot parent = GetPrecedingLowerNorfairRioSlot(follower);
        if (parent.DefinitionId != EnemyDefinitionId.Holtz ||
            parent.SpawnSettings<EnemyFollowerSpawnSettings>().Role == EnemyFollowerRole.Follower)
        {
            throw new InvalidDataException(
                $"Lower Norfair Rio follower in slot {follower.SlotIndex} must immediately " +
                "follow a parent record of the same definition.");
        }
    }

    /// <summary>Rejects flight movement before collision data for the room is available.</summary>
    private static void RequireLowerNorfairRioLevel([NotNull] RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Lower Norfair Rio movement requires room level data.");
    }

    /// <summary>Ports LowerNorfairRio's main AI.</summary>
    internal void RunLowerNorfairRioMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunLowerNorfairRioMain(
            slot,
            slot.RequireSpeciesState<LowerNorfairRioEnemyState>(),
            frame.Samus,
            frame.Level);
    }

    /// <summary>Advances LowerNorfairRio's typed animation.</summary>
    internal static void AdvanceLowerNorfairRioTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, LowerNorfairRioAnimations.Programs, LowerNorfairRioFrameDefinitions.ShownFrames.Of,
            static (LowerNorfairRioEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor,
            (state, cue, _) => ApplyLowerNorfairRioCue(state, cue));

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastLowerNorfairRioSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueEnemySoundIfAny(LastLowerNorfairRioSoundEffect, priority: SoundRequestPriority.Normal);
    }
}

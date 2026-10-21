using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>The escape animals species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class EscapeAnimalsBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. EscapeAnimalsPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => EscapeAnimalsPartDefinitions.Parts[slot.EnemyDefinitionPointer] switch
    {
        EscapeAnimalsPart.EtecoonEscape => new EscapeEtecoonEntity(this, slot, common),
        EscapeAnimalsPart.DachoraEscape => new EscapeDachoraEntity(this, slot, common),
        _ => throw new InvalidOperationException($"Escape animals do not own {slot.EnemyDefinitionPointer}."),
    };

    /// <summary>Provides room events and live Samus state used by both escape-animal branches.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Runs Etecoon's slope-ignoring horizontal move and settling ground probe.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Binds escape-animal AI to room events and terrain collision for the current enemy system.</summary>
    internal EscapeAnimalsBehavior(EnemyRoomBinding room, EnemyTerrain terrain)
    {
        _room = room;
        _terrain = terrain;
    }

    /// <summary>Whole-pixel horizontal step used by each Dachora movement-list callback.</summary>
    private const ushort EscapeDachoraPixelsPerCallback = 6;
    /// <summary>Lava Y threshold tested by both families' acid-height branch instructions.</summary>
    private const ushort EscapeAnimalLavaBranchY = 206;
    /// <summary>One unit of 16.16 downward acceleration applied during Etecoon's exit.</summary>
    private const int EscapeEtecoonGravity = 1 << 16;
    /// <summary>Signed 16.16 horizontal speed, 3.5 pixels, used when Etecoon exits without a wall collision.</summary>
    private const int EscapeEtecoonUncollidedExitSpeed = (7 << 16) / 2;

    /// <summary>Ports <c>EscapeEtecoon_Init</c> at <c>$B3:E6CB</c>.</summary>
    internal void InitializeEscapeEtecoon(RoomEnemySlot slot)
    {
        var state = new EscapeEtecoonEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);

        // Event $0F means the animals already escaped on an earlier visit. Native code
        // deletes the actor before changing any animation, position, palette, or AI fields.
        if (HasCrittersEscaped())
        {
            slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
            return;
        }

        // In the original property vocabulary, $2000 disables ordinary Samus collision and
        // $0400 marks the actor tangible. In this port those bits retain their independently
        // observed scheduler/collision meanings as ProcessInstructions and
        // IgnoreSamusCollision. Raw $8000 remains unnamed because this family never proves
        // its broader engine-wide meaning.
        slot.Properties = slot.Properties.With(
            EnemyProperties.ProcessInstructions | EnemyProperties.IgnoreSamusCollision);
        slot.Properties = slot.Properties.With(EnemyProperties.SolidToSamus);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        slot.PaletteIndex = 0;

        EscapeEtecoonInitialization initialization =
            EscapeEtecoonDefinitions.Initialization(checked((ushort)slot.Spawn.Population.Parameter1));
        slot.Position = slot.Position.WithWhole(initialization.Position);
        state.PreInstruction = initialization.PreInstruction;
        state.Program = AnimationProgramCursor<EscapeEtecoonProgram>.Start(initialization.Program);
        state.Velocity = initialization.Velocity;
    }

    /// <summary>Ports <c>EscapeDachora_Init</c> at <c>$B3:EAE5</c>.</summary>
    internal void InitializeEscapeDachora(RoomEnemySlot slot)
    {
        slot.SetSpeciesState(new EscapeDachoraEnemyState());
        if (HasCrittersEscaped())
        {
            slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
            return;
        }

        // Dachora's population already supplies $2400, but the initializer ORs $2000 again.
        // Expressing the idempotent write documents the executable routine and keeps a
        // synthetic/debug population faithful to retail initialization.
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.Frame = EnemyFrameId.Empty;
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        slot.RequireSpeciesState<EscapeDachoraEnemyState>().Program = AnimationProgramCursor<EscapeDachoraProgram>.Start(EscapeDachoraProgram.RunningAroundLowTide);
    }

    /// <summary>Ports <c>EscapeEtecoon_Main</c> and its complete indirect dispatch.</summary>
    private void RunEscapeEtecoonMain(
        RoomEnemySlot slot,
        EscapeEtecoonEnemyState state,
        RoomLevelData? level)
    {
        switch (state.PreInstruction)
        {
            case EscapeEtecoonPreInstruction.Cleared:
                return;

            case EscapeEtecoonPreInstruction.EscapeRight:
                AddEscapeAnimalX(slot, EscapeEtecoonUncollidedExitSpeed);
                return;

            case EscapeEtecoonPreInstruction.WaitForEscapeEvent:
                // $B3:E670 changes only the list pointer. It deliberately leaves the
                // existing instruction timer intact, so the new list begins on the same
                // scheduler boundary the SNES would observe.
                if (HasCrittersEscaped())
                    state.Program = AnimationProgramCursor<EscapeEtecoonProgram>.Start(EscapeEtecoonProgram.ExpressGratitudeThenEscape);
                return;

            case EscapeEtecoonPreInstruction.WalkAndFall:
                RunEscapeEtecoonWalking(slot, state, level);
                return;

            default:
                throw new InvalidDataException(
                    $"Escape Etecoon pre-instruction {state.PreInstruction?.ToString() ?? "none"} " +
                    "is not translated.");
        }
    }

    /// <summary>Ports <c>$B3:E680</c>, including its collision-only event branch.</summary>
    private void RunEscapeEtecoonWalking(
        RoomEnemySlot slot,
        EscapeEtecoonEnemyState state,
        RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Escape Etecoon walking requires room collision data.");

        // The stored speed is signed 8.8; Enemy_MoveRight_IgnoreSlopes receives signed
        // 16.16, hence the exact eight-bit shift. The event test is inside the collision
        // branch in the retail routine and must not turn an animal around in open space.
        int horizontalDisplacement = state.Velocity.X.ToSixteenSixteenDelta().RawValue;
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, horizontalDisplacement))
        {
            slot.InstructionTimer = WrappingCountdown.Start(1);
            state.Velocity = state.Velocity.ReversedX();
            state.Program = AnimationProgramCursor<EscapeEtecoonProgram>.Start(HasCrittersEscaped()
                ? EscapeEtecoonProgram.RunningForEscape
                : state.Velocity.X.SignedRawValue < 0
                    ? EscapeEtecoonProgram.RunningLeftLowTide
                    : EscapeEtecoonProgram.RunningRightLowTide);
        }

        // Enemy_MoveDown is called even after horizontal collision, and its carry result is
        // ignored. This one-pixel probe lets the common collision helper settle the actor on
        // the room's actual floor without introducing a bespoke ground model.
        _ = _terrain.MoveEnemyVertically(level, slot, EscapeEtecoonGravity);
    }

    /// <summary>Runs an escaping Etecoon's or Dachora's typed program where the enemy instruction interpreter would.</summary>
    private void AdvanceEscapeAnimal(RoomEnemySlot slot, SamusState? samus)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.EtecoonEscape)
        {
            EscapeEtecoonEnemyState state = slot.RequireSpeciesState<EscapeEtecoonEnemyState>();
            AnimationProgramCursor<EscapeEtecoonProgram> cursor = state.Program;
            EnemyProgramAnimation.Run(slot, EscapeEtecoonAnimations.Programs, ref cursor, EscapeEtecoonFrameDefinitions.ShownFrames.Of,
                (cue, _) => ApplyEscapeEtecoonCue(slot, state, samus, cue));
            state.Program = cursor;
            return;
        }
        EscapeDachoraEnemyState dachora = slot.RequireSpeciesState<EscapeDachoraEnemyState>();
        AnimationProgramCursor<EscapeDachoraProgram> dachoraCursor = dachora.Program;
        EnemyProgramAnimation.Run(slot, EscapeDachoraAnimations.Programs, ref dachoraCursor, EscapeDachoraFrameDefinitions.ShownFrames.Of,
            (cue, _) => ApplyEscapeDachoraCue(slot, samus, cue));
        dachora.Program = dachoraCursor;
    }

    /// <summary>Carries out one cue of an Etecoon's program, returning the program a branch continues at, or null to run on.</summary>
    private static EscapeEtecoonProgram? ApplyEscapeEtecoonCue(RoomEnemySlot slot, EscapeEtecoonEnemyState state, SamusState? samus, EscapeEtecoonCue cue)
    {
        switch (cue)
        {
            case EscapeEtecoonCue.SetPreInstruction setPreInstruction:
                state.PreInstruction = setPreInstruction.PreInstruction;
                return null;
            case EscapeEtecoonCue.Command { Kind: EscapeEtecoonCueKind.ClearPreInstruction }:
                state.PreInstruction = EscapeEtecoonPreInstruction.Cleared;
                return null;
            case EscapeEtecoonCue.GoToIfAcidHigh goToIfAcidHigh:
                return EscapeAnimalLavaY(samus) >= EscapeAnimalLavaBranchY ? null : goToIfAcidHigh.Target;
            case EscapeEtecoonCue.Move move:
                slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole + move.Offset)));
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EscapeEtecoonCueKind)} {cue}.");
        }
    }

    /// <summary>Carries out one cue of the Dachora's program, returning the program a branch continues at, or null to run on.</summary>
    private EscapeDachoraProgram? ApplyEscapeDachoraCue(RoomEnemySlot slot, SamusState? samus, EscapeDachoraCue cue)
    {
        switch (cue.Kind)
        {
            case EscapeDachoraCueKind.GoToIfAcidHigh:
                return EscapeAnimalLavaY(samus) >= EscapeAnimalLavaBranchY ? null : cue.Target;
            case EscapeDachoraCueKind.GoToIfCrittersEscaped:
                return HasCrittersEscaped() ? cue.Target : null;
            case EscapeDachoraCueKind.MoveLeft:
                slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole - EscapeDachoraPixelsPerCallback)));
                return null;
            case EscapeDachoraCueKind.MoveRight:
                slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole + EscapeDachoraPixelsPerCallback)));
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EscapeDachoraCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Samples event $0F for Dachora's instruction-list branch and room revisit handling.</summary>
    private bool HasCrittersEscaped() => _room.Services.Events.Has(EventNumber.CrittersEscaped);

    /// <summary>Treats absent Samus/liquid context as the native maximum Y for acid-height branches.</summary>
    private static int EscapeAnimalLavaY(SamusState? samus) =>
        samus?.LiquidPhysics.LavaAcidYPosition ?? ushort.MaxValue;

    /// <summary>Adds a signed 16.16 displacement with native 32-bit wraparound to the actor's X position.</summary>
    private static void AddEscapeAnimalX(RoomEnemySlot slot, int displacement)
    {
        uint fixedPosition = slot.Position.X.RawValue;
        fixedPosition = unchecked(fixedPosition + (uint)displacement);
        slot.Position = slot.Position.WithX(SnesFixedPosition.FromRaw(unchecked(fixedPosition)));
    }

    /// <summary>Ports the escape Etecoon's main AI.</summary>
    internal void RunEscapeEtecoonMainAi(RoomEnemySlot slot, IEnemyAiFrame frame) =>
        RunEscapeEtecoonMain(slot, slot.RequireSpeciesState<EscapeEtecoonEnemyState>(), frame.Level);

    /// <summary>Advances either escape animal's typed animation.</summary>
    internal void AdvanceEscapeAnimalTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context) =>
        AdvanceEscapeAnimal(slot, context.Samus);

}

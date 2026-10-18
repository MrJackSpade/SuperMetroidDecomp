using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The Ceres steam species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class CeresSteamBehavior : IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Steam];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new CeresSteamEntity(this, slot, common);

    /// <summary>Room services that supply the live RNG used to stagger vent activation.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Common contact resolution used after steam's extended hitbox selects its touch callback.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Stores room and collision services for steam initialization and contact handling.</summary>
    internal CeresSteamBehavior(EnemyRoomBinding room, EnemyCollision collision)
    {
        _room = room;
        _collision = collision;
    }

    /// <summary>
    /// Each vent slot's typed program cursor. Initialization sets a slot's cursor before its program runs, so a slot's
    /// entry always belongs to its current vent.
    /// </summary>
    internal readonly Dictionary<int, AnimationProgramCursor<CeresSteamProgram>> _ventPrograms = [];

    /// <summary>Runs a vent's typed program where the enemy instruction interpreter would.</summary>
    private void AdvanceCeresSteamAnimation(RoomEnemySlot slot)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        AnimationProgramCursor<CeresSteamProgram> cursor = _ventPrograms[slot.SlotIndex];
        EnemyProgramAnimation.Run(slot, CeresSteamAnimations.Programs, ref cursor, CeresSteamFrameDefinitions.ShownFrames.Of,
            (cue, _) => ApplyCeresSteamCue(slot, cue));
        _ventPrograms[slot.SlotIndex] = cursor;
    }

    /// <summary>Carries out one cue of a vent's program, returning the program a branch continues at, or null to run on.</summary>
    private static CeresSteamProgram? ApplyCeresSteamCue(RoomEnemySlot slot, CeresSteamCue cue)
    {
        switch (cue.Kind)
        {
            case CeresSteamCueKind.Vanish:
                slot.Properties = slot.Properties.With(
                    EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision);
                return null;
            case CeresSteamCueKind.CountDownToActivation:
            {
                // The vent's activation delay, which initialization staggers.
                CeresSteamEnemyState state = RequireCeresSteamState(slot);
                NativeWordCounterStep step = state.ActivationDelay.Decrement();
                state.ActivationDelay = step.Remaining;
                if (!step.IsZero)
                    return cue.Target;
                slot.Properties = slot.Properties.Without(
                    EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision);
                return cue.Alternate;
            }
            case CeresSteamCueKind.Appear:
                slot.Properties = slot.Properties.Without(
                    EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(CeresSteamCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Preloaded Ceres palette selected by the steam initializer.</summary>
    private static readonly ushort CeresSteamPaletteIndex = EnemyPaletteBits.Palette5;
    /// <summary>Health restored by steam's main and touch routines so the vent cannot be destroyed.</summary>
    private const ushort CeresSteamIndestructibleHealth = 32767;

    /// <summary>Ports <c>CeresSteam_Init</c> at <c>$A6:EFB1</c>.</summary>
    internal void InitializeCeresSteam(RoomEnemySlot slot)
    {
        CeresSteamInitialization initialization = CeresSteamDefinitions.Initialization(
            (CeresSteamVariant)slot.Parameter1);

        // Steam graphics are preloaded with the Ceres tileset and therefore use VRAM tile
        // base zero. Both property changes are literal ORs performed by the initializer:
        // normal instructions animate visibility, while extended maps supply per-frame art
        // and collision boxes whose geometry cannot be represented by header radii.
        slot.VramTilesIndex = 0;
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.ExtraProperties = slot.ExtraProperties.With(
            EnemyExtraProperties.UsesExtendedSpritemap);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        slot.PaletteIndex = CeresSteamPaletteIndex;

        // `$A6:EFD9-$EFE0` masks the live RNG to 0..31 and adds one. The delay is then
        // consumed by instruction `$F127`, giving each vent a cartridge-authored stagger.
        var state = new CeresSteamEnemyState
        {
            ActivationDelay = NativeCountdown.Start((_room.Random.Generate() & CeresSteamDefinitions.ActivationDelayMask) + 1),
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);

        _ventPrograms[slot.SlotIndex] = AnimationProgramCursor<CeresSteamProgram>.Start(initialization.Program);
        state.Function = initialization.Function;
    }

    /// <summary>Ports <c>CeresSteam_Main</c> at <c>$A6:F00D</c>.</summary>
    private static void RunCeresSteamMain(
        RoomEnemySlot slot,
        SamusMode7Transform? mode7Transform)
    {
        // Both main AI and touch AI restore this value. Steam is scenery/hazard state and
        // cannot be destroyed by a projectile that happened to overlap an active plume.
        slot.Health = CeresSteamIndestructibleHealth;

        CeresSteamEnemyState state = RequireCeresSteamState(slot);
        switch (state.Function)
        {
            // Parameters zero through three point at `$EFF4`, a literal RTS. Their base
            // world position is also their draw position, so no graphical word is changed.
            case CeresSteamFunction.None:
                return;

            // Parameters four and five are the vents inside the rotating elevator shaft.
            // `$A6:F019` calls the same bank-$8B transform used for Samus, then stores only
            // transformed-minus-base deltas in the enemy's graphical-offset words. Native
            // collision continues to use the untransformed world point; drawing consumes
            // these offsets later in WriteEnemyOAM.
            case CeresSteamFunction.ApplyRotatingElevatorOffset:
                if (mode7Transform is not SamusMode7Transform transform)
                {
                    throw new InvalidOperationException(
                        "Ceres steam elevator variant requires the active Mode-7 transform.");
                }

                Position16<RoomPixels> transformed = transform.Transform(slot.Position.Whole);
                slot.SpawnOffset = transformed - slot.Position.Whole;
                return;

            default:
                throw new InvalidDataException(
                    $"Ceres steam function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>The vent's state, failing if the vent never initialized.</summary>
    private static CeresSteamEnemyState RequireCeresSteamState(RoomEnemySlot slot) =>
        slot.SpeciesState<CeresSteamEnemyState>() ?? throw new InvalidOperationException("A Ceres steam vent has no state.");

    /// <summary>Ports CeresSteam's initialization AI.</summary>
    internal void RunCeresSteamInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeCeresSteam(slot);
    }

    /// <summary>Ports CeresSteam's main AI.</summary>
    internal static void RunCeresSteamMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunCeresSteamMain(slot, frame.Mode7Transform);
    }

    /// <summary>Advances CeresSteam's typed animation.</summary>
    internal void AdvanceCeresSteamTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        AdvanceCeresSteamAnimation(slot);
    }

    /// <summary>Registers contact handling only for steam's authored extended hitbox callback.</summary>
    internal void RegisterCeresSteamTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Steam &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.CeresSteamTouch,
            context =>
            {
                // Every visible steam frame stores `$A6:F03F` in its authored hitbox.
                // The last two dispersal frames use an empty hitbox list and cannot reach
                // this branch. Insisting on both facts keeps collision tied to ROM geometry.
                if (!context.UsesExtendedHitboxes || context.HitboxTouchAi != EnemyInteractionCallback.CeresSteamTouch)
                {
                    throw new InvalidDataException(
                        $"Ceres steam requires extended touch AI $A6:{EnemyAiCodePointers.BankA6.CeresSteamTouch:X4}; " +
                        $"extended={context.UsesExtendedHitboxes}, selected=$A6:{context.HitboxTouchAiPointer:X4}, " +
                        $"map=$A6:{context.Slot.Frame}.");
                }

                // `$A6:F03F` restores health immediately before entering common touch AI.
                // Header damage is deliberately zero: contact still installs the common
                // invincibility/knockback state, while Screw/Speed cannot destroy the vent.
                context.Slot.Health = CeresSteamIndestructibleHealth;
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus);
            });
    }

    /// <summary>Routes steam shots to ordinary common shot AI without a species-specific reaction.</summary>
    internal static void RegisterCeresSteamShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Steam &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.NoOp));
    }

    /// <summary>The callback of the first of the occupant's hitboxes the probe overlaps, or null outside its compiled frames.</summary>
    internal static bool? FindHitboxCallback(RoomEnemySlot enemy, EnemyHitboxProbe probe, out ushort callback)
    {
        callback = 0;
        if (!(enemy.Definition.Bank == CeresSteamCollisionDefinitions.Bank &&
            (CeresSteamCollisionDefinitions.Frames.Contains(enemy.Frame) ||
             probe.ArtworkInstalled)))
            return null;

        // The high byte of each steam frame's $1001 header is drawing
        // metadata, not an extra component. Both damaging rectangles and
        // callbacks remain fixed even if its installed OAM is edited.
        foreach (CeresSteamCollisionComponent component in
                 CeresSteamCollisionDefinitions.Frames.Of(enemy.Frame))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            foreach (CeresSteamCollisionHitbox hitbox in
                     component.Hitboxes)
            {
                ushort left = unchecked((ushort)(componentOrigin.X + hitbox.Left));
                ushort top = unchecked((ushort)(componentOrigin.Y + hitbox.Top));
                ushort right = unchecked((ushort)(componentOrigin.X + hitbox.Right));
                ushort bottom = unchecked((ushort)(componentOrigin.Y + hitbox.Bottom));
                if (!probe.Overlaps(left, top, right, bottom))
                    continue;
                callback = CeresSteamHitboxCallbackCodec.ToCallbackWord(probe.SelectShotCallback);
                return true;
            }
        }
        return false;
    
    }
}

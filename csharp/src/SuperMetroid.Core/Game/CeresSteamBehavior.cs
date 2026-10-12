namespace SuperMetroid.Core.Game;

/// <summary>The Ceres steam species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class CeresSteamBehavior
{
    /// <summary>Stores room.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Stores collision.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Initializes a new ceres steam behavior instance.</summary>
    internal CeresSteamBehavior(EnemyRoomBinding room, EnemyCollision collision)
    {
        _room = room;
        _collision = collision;
    }

    /// <summary>Executes the Ceres steam's private animation instructions.</summary>
    private static bool TryProcessCeresSteamInstruction(RoomEnemySlot slot, ushort word, ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Steam ||
            !Enum.IsDefined((CeresSteamInstruction)word))
            return false;

        switch ((CeresSteamInstruction)word)
        {
            case CeresSteamInstruction.SetToIntangibleAndInvisible:
                slot.Properties = slot.Properties.With(
                    EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case CeresSteamInstruction.DecrementActivationTimerGotoYOrY2:
                slot.VariableD = unchecked((ushort)(slot.VariableD - 1));
                if (slot.VariableD != 0)
                {
                    cursor = EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(
                        slot,
                        unchecked((ushort)(cursor + 2)));
                }
                else
                {
                    cursor = EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(
                        slot,
                        unchecked((ushort)(cursor + 4)));
                    slot.Properties = slot.Properties.Without(
                        EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision);
                }
                return true;
            case CeresSteamInstruction.SetToTangibleAndVisible:
                slot.Properties = slot.Properties.Without(
                    EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            default:
                throw new InvalidOperationException(
                    $"Ceres steam does not own instruction ${word:X4}.");
        }
    }

    /// <summary>Stores ceres steam palette index.</summary>
    private static readonly ushort CeresSteamPaletteIndex = EnemyPaletteBits.Palette5;
    /// <summary>Defines ceres steam indestructible health.</summary>
    private const ushort CeresSteamIndestructibleHealth = 0x7fff;

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
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.PaletteIndex = CeresSteamPaletteIndex;

        // `$A6:EFD9-$EFE0` masks the live RNG to 0..31 and adds one. Variable D is then
        // consumed by instruction `$F127`, giving each vent a cartridge-authored stagger.
        slot.VariableD = unchecked((ushort)((_room.Random.Generate() & 0x001f) + 1));

        slot.CurrentInstruction = initialization.InstructionList;
        slot.VariableA = (ushort)initialization.Function;
    }

    /// <summary>Ports <c>CeresSteam_Main</c> at <c>$A6:F00D</c>.</summary>
    private static void RunCeresSteamMain(
        RoomEnemySlot slot,
        SamusMode7Transform? mode7Transform)
    {
        // Both main AI and touch AI restore this value. Steam is scenery/hazard state and
        // cannot be destroyed by a projectile that happened to overlap an active plume.
        slot.Health = CeresSteamIndestructibleHealth;

        switch ((CeresSteamFunction)slot.VariableA)
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
                    $"Ceres steam function $A6:{slot.VariableA:X4} is not translated.");
        }
    }

    /// <summary>Register ceres steam initialization ai.</summary>
    internal void RegisterCeresSteamInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_CeresSteam, null, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeCeresSteam(slot);
        });
    }

    /// <summary>Register ceres steam main ai.</summary>
    internal static void RegisterCeresSteamMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_CeresSteam, null, (slot, frame, nmiFrameCounter8) =>
        {
            RunCeresSteamMain(slot, frame.Mode7Transform);
        });
    }

    /// <summary>Register ceres steam owned instructions.</summary>
    internal static void RegisterCeresSteamOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessCeresSteamInstruction(slot, word, ref cursor);
        });
    }

    /// <summary>Register ceres steam touch.</summary>
    internal void RegisterCeresSteamTouch(EnemyAiTable table)
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
                        $"map=$A6:{context.Slot.SpritemapPointer:X4}.");
                }

                // `$A6:F03F` restores health immediately before entering common touch AI.
                // Header damage is deliberately zero: contact still installs the common
                // invincibility/knockback state, while Screw/Speed cannot destroy the vent.
                context.Slot.Health = CeresSteamIndestructibleHealth;
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus);
            });
    }


    // Ordinary common shot AI with no species stage.
    /// <summary>Register ceres steam shot reactions.</summary>
    internal static void RegisterCeresSteamShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Steam &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.NoOp));
    }
}

namespace SuperMetroid.Core.Game;

/// <summary>
/// Cartridge-faithful Ceres steam initialization and graphical-offset behavior from
/// bank <c>$A6:EFB1-$F03E</c>. Animation instructions and extended spritemaps remain in
/// the shared ROM-backed enemy interpreters; this file owns only the actor's private AI.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private static readonly ushort CeresSteamPaletteIndex = EnemyPaletteBits.Palette5;
    private const ushort CeresSteamIndestructibleHealth = 0x7fff;

    /// <summary>Ports <c>CeresSteam_Init</c> at <c>$A6:EFB1</c>.</summary>
    private void InitializeCeresSteam(RoomEnemySlot slot)
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
        slot.VariableD = unchecked((ushort)((_nextRandom!() & 0x001f) + 1));

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

                SamusMode7Point transformed = transform.Transform(
                    slot.XPosition,
                    slot.YPosition);
                slot.SpawnXOffset = unchecked((ushort)(transformed.X - slot.XPosition));
                slot.SpawnYOffset = unchecked((ushort)(transformed.Y - slot.YPosition));
                return;

            default:
                throw new InvalidDataException(
                    $"Ceres steam function $A6:{slot.VariableA:X4} is not translated.");
        }
    }
}

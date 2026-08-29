namespace SuperMetroid.Core.Game;

/// <summary>
/// Cartridge-faithful Ceres steam initialization and graphical-offset behavior from
/// bank <c>$A6:EFB1-$F03E</c>. Animation instructions and extended spritemaps remain in
/// the shared ROM-backed enemy interpreters; this file owns only the actor's private AI.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>The retail enemy-definition pointer at <c>$A0:E1FF</c>.</summary>
    public const ushort CeresSteamDefinition = 0xe1ff;

    // `$A6:EFF5` contains one initial instruction pointer for each parameter-one value.
    // `$A6:F001` is the parallel main-function table. Keeping the addresses named here
    // documents that these choices come from cartridge data rather than host guesses.
    private const int CeresSteamInstructionTable = 0xa6eff5;
    private const int CeresSteamFunctionTable = 0xa6f001;
    private const ushort CeresSteamNullFunction = 0xeff4;
    private const ushort CeresSteamMode7Function = 0xf019;
    private const ushort CeresSteamPaletteIndex = 0x0a00;
    private const ushort CeresSteamIndestructibleHealth = 0x7fff;
    private const ushort CeresSteamVariantCount = 6;

    /// <summary>Ports <c>CeresSteam_Init</c> at <c>$A6:EFB1</c>.</summary>
    private void InitializeCeresSteam(RoomEnemySlot slot)
    {
        // Population parameter one is used directly as a word-table index by native code.
        // Reject corrupt values explicitly instead of allowing a host read into unrelated AI.
        if (slot.Parameter1 >= CeresSteamVariantCount)
        {
            throw new InvalidDataException(
                $"Ceres steam parameter one ${slot.Parameter1:X4} exceeds its " +
                $"{CeresSteamVariantCount}-entry tables.");
        }

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

        int tableOffset = slot.Parameter1 * sizeof(ushort);
        slot.CurrentInstruction = ReadWord(
            _bus!,
            CeresSteamInstructionTable + tableOffset);
        slot.VariableA = ReadWord(
            _bus!,
            CeresSteamFunctionTable + tableOffset);
    }

    /// <summary>Ports <c>CeresSteam_Main</c> at <c>$A6:F00D</c>.</summary>
    private static void RunCeresSteamMain(
        RoomEnemySlot slot,
        SamusMode7Transform? mode7Transform)
    {
        // Both main AI and touch AI restore this value. Steam is scenery/hazard state and
        // cannot be destroyed by a projectile that happened to overlap an active plume.
        slot.Health = CeresSteamIndestructibleHealth;

        switch (slot.VariableA)
        {
            // Parameters zero through three point at `$EFF4`, a literal RTS. Their base
            // world position is also their draw position, so no graphical word is changed.
            case CeresSteamNullFunction:
                return;

            // Parameters four and five are the vents inside the rotating elevator shaft.
            // `$A6:F019` calls the same bank-$8B transform used for Samus, then stores only
            // transformed-minus-base deltas in the enemy's graphical-offset words. Native
            // collision continues to use the untransformed world point; drawing consumes
            // these offsets later in WriteEnemyOAM.
            case CeresSteamMode7Function:
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
                throw new NotSupportedException(
                    $"Ceres steam function $A6:{slot.VariableA:X4} is not translated.");
        }
    }
}

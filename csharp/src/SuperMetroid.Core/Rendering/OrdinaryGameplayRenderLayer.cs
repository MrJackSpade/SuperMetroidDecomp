using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Fused backdrop/HUD/Mode-1/OBJ composition. This must be the first operation;
/// subsequent operations may apply color math and overlays. Scroll tables contain
/// literal register values for physical lines 32..223, not pre-offset source Y.
/// </summary>
public sealed record OrdinaryGameplayRenderLayer : RenderLayer
{
    /// <summary>Owned per-scanline TM register replacements copied from the capture.</summary>
    private readonly ushort[] mainScreenLayersByLine;
    /// <summary>Captured register values for this composition operation; PPU memory and OAM belong to the enclosing render snapshot, not this value.</summary>
    public OrdinaryGameplayRegisters Registers { get; }
    /// <summary>The room backgrounds BG1 and BG2 show, resolved from level data through the scroll registers (#1346).</summary>
    public RoomBackgrounds Rooms { get; }
    /// <summary>Owned BG2 HDMA scroll replacements for physical lines 32..223; an inactive axis uses <see cref="OrdinaryGameplayRegisters.Bg2Scroll"/> on every line.</summary>
    public Bg2HdmaScrollTables Bg2LineScrolls { get; }
    /// <summary>Owned TM layer-enable words for physical lines 32..223; nonempty entries replace the fallback gameplay mask, not the separate TMW window mask or fixed HUD composition.</summary>
    public ReadOnlySpan<ushort> MainScreenLayersByLine => mainScreenLayersByLine;

    /// <summary>Captures immutable register inputs and copies optional scanline tables for the first backdrop/HUD/BG/OBJ operation; Mode-1 priority composition precedes later color-math and overlay operations.</summary>
    /// <param name="registers">Captured Mode-1 state with a 32- or 64-cell BG2 tilemap on each axis.</param>
    /// <param name="rooms">The room backgrounds BG1 and BG2 show; BG2 uses the tilemap registers when it has no room plane.</param>
    /// <param name="bg2LineScrolls">Optional BG2 HDMA scroll replacements for physical lines 32..223.</param>
    /// <param name="mainScreenLayersByLine">Empty or exactly 192 TM replacement masks in <see cref="SnesMainScreenLayers"/> encoding.</param>
    /// <exception cref="ArgumentException">BG2 dimensions are unsupported or a nonempty TM table does not contain exactly 192 values.</exception>
    public OrdinaryGameplayRenderLayer(OrdinaryGameplayRegisters registers, RoomBackgrounds rooms,
        Bg2HdmaScrollTables bg2LineScrolls = default, ReadOnlySpan<ushort> mainScreenLayersByLine = default)
    {
        if (registers.Bg2MapSize.X is not (32 or 64) || registers.Bg2MapSize.Y is not (32 or 64)
            || registers.Bg2MapSize.X * registers.Bg2MapSize.Y is not (1024 or 2048 or 4096))
            throw new ArgumentException("Gameplay BG2 requires 32 or 64 tiles on each axis.", nameof(registers));
        if (!mainScreenLayersByLine.IsEmpty && mainScreenLayersByLine.Length != Bg2HdmaScrollTables.GameplayLines)
            throw new ArgumentException("Gameplay HDMA tables must contain exactly 192 register values.");
        Registers = registers;
        Rooms = Ensure.NotNull(rooms);
        Bg2LineScrolls = bg2LineScrolls;
        this.mainScreenLayersByLine = mainScreenLayersByLine.ToArray();
    }

    /// <summary>The BG2 scroll registers on gameplay line <paramref name="line"/> (0..191).</summary>
    public Position16<Bg2Pixels> Bg2ScrollAt(int line) => Bg2LineScrolls.At(line, Registers.Bg2Scroll);
}

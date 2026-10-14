namespace SuperMetroid.Core.Rooms;

/// <summary>One visible 16x16 PLM block redraw in BG1's two-screen ring.</summary>
/// <param name="TopRowDestination">VRAM word address of the block's upper-left character; the lower row begins 32 words later.</param>
/// <param name="TopRow">Upper-left and upper-right packed BG character words in horizontal transfer order.</param>
/// <param name="BottomRow">Lower-left and lower-right packed BG character words in horizontal transfer order.</param>
public sealed record PlmTilemapUpdate(
    ushort TopRowDestination,
    ushort[] TopRow,
    ushort[] BottomRow)
{
    /// <summary>Applies the two horizontal two-word transfers produced by bank $84.</summary>
    [PpuLayoutBoundary("codec: DrawPLM two-row BG1 tilemap DMA (VMADD)")]
    public void ExecuteTo(SnesVram vram)
    {
        Ensure.NotNull(vram);
        vram.ExecuteWordTransfer(TopRow, TopRowDestination, 1);
        vram.ExecuteWordTransfer(
            BottomRow,
            unchecked((ushort)(TopRowDestination + 0x20)),
            1);
    }
}

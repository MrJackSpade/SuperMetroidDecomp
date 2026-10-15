namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One timed lightning color record whose BGR555 payload remains live.</summary>
/// <param name="Pointer">Bank-$8D byte address of this record's duration word.</param>
/// <param name="ColorCount">Consecutive live CGRAM color words following the duration: eight for surface lightning or seven for dark lightning.</param>
public readonly record struct CrateriaLightningPaletteFrame(
    ushort Pointer,
    int ColorCount)
{
    /// <summary>The first live BGR555 presentation word after the duration.</summary>
    public ushort FirstColorPointer => unchecked((ushort)(Pointer + sizeof(ushort)));
}

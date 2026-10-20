using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Cartridge values and helpers of <see cref="SamusTileTransferState"/> that only verification reads.</summary>
internal static class SamusTileTransferStateConstants
{
    /// <summary>Splits one installed planar definition across Samus's paired dedicated OBJ VRAM regions.</summary>
    internal static void ExecuteInstalledDefinition(SamusBodyTileDefinition definition,
        VramRegion first, VramRegion second)
    {
        ReadOnlySpan<byte> planar = definition.Planar.Span;
        first.Load(planar[..definition.FirstSize]);
        if (definition.SecondSize != 0)
            second.Load(planar[definition.FirstSize..]);
    }
}

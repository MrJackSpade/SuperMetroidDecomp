using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="CgramPalettes"/> members production does not use.</summary>
internal static class CgramPalettesAccess
{
    /// <summary>Exposes <see cref="CgramPalettes"/> state that only verification reads.</summary>
    extension(CgramPalettes self)
    {
        /// <summary>The 128 OBJ colours.</summary>
        public CgramPalette Objects => self.All.Slice(PpuMemoryLayoutConstants.ObjectPalettes.Offset, PpuMemoryLayoutConstants.ObjectPalettes.Count);

        /// <summary>OBJ palette row <paramref name="row"/> (0-7).</summary>
        public CgramPalette ObjectRow(int row) => self.Objects.Slice(CgramPalettes.RowStart(row), PpuMemoryLayout.ColorsPerRow);
    }
}

using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Whole-image comparisons of PPU state that only verification performs (#630).</summary>
internal static class PpuImageAccess
{
    /// <summary>Adds full low-table and high-table OAM comparisons for exact image tests.</summary>
    extension(OamBuffer self)
    {
        /// <summary>Whether this OAM holds exactly the same low and high tables as <paramref name="other"/>.</summary>
        [PpuLayoutBoundary("exact hardware test: compares complete OAM low/high-table images")]
        internal bool ImageEquals(OamBuffer other) =>
            self.LowTable.SequenceEqual(other.LowTable) && self.HighTable.SequenceEqual(other.HighTable);
    }

    /// <summary>Adds a complete 64 KiB VRAM comparison for exact image tests.</summary>
    extension(SnesVram self)
    {
        /// <summary>Whether this VRAM holds exactly the same 64 KiB image as <paramref name="other"/>.</summary>
        [PpuLayoutBoundary("exact hardware test: compares complete VRAM images")]
        internal bool ImageEquals(SnesVram other) => self.Bytes.SequenceEqual(other.Bytes);
    }

    /// <summary>Adds a 256-color CGRAM image comparison for exact image tests.</summary>
    extension(CgramPalettes self)
    {
        /// <summary>Whether this CGRAM holds exactly the same 256-colour image as <paramref name="other"/>.</summary>
        internal bool ImageEquals(CgramPalettes other) => self.Image.SequenceEqual(other.Image);
    }
}

using SuperMetroid.Core.Hardware;

/// <summary>Whole-image comparisons of PPU state that only verification performs (#630).</summary>
internal static class PpuImageAccess
{
    extension(OamBuffer self)
    {
        /// <summary>Whether this OAM holds exactly the same low and high tables as <paramref name="other"/>.</summary>
        [PpuLayoutBoundary("exact hardware test: compares complete OAM low/high-table images")]
        internal bool ImageEquals(OamBuffer other) =>
            self.LowTable.SequenceEqual(other.LowTable) && self.HighTable.SequenceEqual(other.HighTable);
    }

    extension(SnesVram self)
    {
        /// <summary>Whether this VRAM holds exactly the same 64 KiB image as <paramref name="other"/>.</summary>
        [PpuLayoutBoundary("exact hardware test: compares complete VRAM images")]
        internal bool ImageEquals(SnesVram other) => self.Bytes.SequenceEqual(other.Bytes);
    }

    extension(CgramPalettes self)
    {
        /// <summary>Whether this CGRAM holds exactly the same 256-colour image as <paramref name="other"/>.</summary>
        internal bool ImageEquals(CgramPalettes other) => self.Image.SequenceEqual(other.Image);
    }
}

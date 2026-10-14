using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="CeresDestructionCinematicState"/> members production does not use.</summary>
internal static class CeresDestructionCinematicStateAccess
{
    /// <summary>Reads private cinematic geometry and actor state for focused verification.</summary>
    extension(CeresDestructionCinematicState self)
    {
        /// <summary>Current Mode-7 zoom scalar retained by the cinematic.</summary>
        internal ushort Zoom => self.zoom;

        /// <summary>Current fixed-point origin for the destruction background.</summary>
        internal FixedPosition<Mode7Pixels> Background => self.background;

        /// <summary>Number of cinematic actors still held in the actor list.</summary>
        internal int ActiveActorCount => self.actors.Count;

        /// <summary>
        /// Reads the low-byte Mode-7 map selected by the cinematic's current native transfers.
        /// </summary>
        [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
        internal byte ReadMode7MapByte(int mapByteIndex)
        {
            if ((uint)mapByteIndex >= CeresDestructionRomData.Vram.Mode7MapCapacityBytes)
                throw new ArgumentOutOfRangeException(nameof(mapByteIndex));
            return self.vram.ReadByte(mapByteIndex * 2);
        }
    }
}

using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Verification access to <see cref="CeresDestructionCinematicState"/> members production does not use.</summary>
internal static class CeresDestructionCinematicStateAccess
{
    /// <summary>Reads private cinematic geometry and actor state for focused verification.</summary>
    extension(CeresDestructionCinematicState self)
    {
        /// <summary>Current Mode-7 scene scale retained by the cinematic, in 1/256 units.</summary>
        internal int Zoom => self.Zoom;

        /// <summary>Current fixed-point origin for the destruction background.</summary>
        internal FixedPosition<Mode7Pixels> Background => self.Background;

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
            return self.Image()[mapByteIndex * 2];
        }
    }
}

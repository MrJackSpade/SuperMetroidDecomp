using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Verification access to <see cref="CeresDestructionCinematicState"/> members production does not use.</summary>
internal static class CeresDestructionCinematicStateAccess
{
    /// <summary>Reads private cinematic geometry and actor state for focused verification.</summary>
    extension(CeresDestructionCinematicState self)
    {
        /// <summary>Current floor zoom retained by the cinematic.</summary>
        internal FloorZoom Zoom => self.Zoom;

        /// <summary>Current fixed-point origin for the destruction background.</summary>
        internal FixedVector<FloorPixels> Background => self.Background;

        /// <summary>Number of cinematic actors the scene still holds: those over Ceres, or the Zebes reveal's.</summary>
        internal int ActiveActorCount =>
            PrivateState.Field<CeresDestructionCinematicState, List<CeresSceneActor>>(self, "ceresActors").Count +
            PrivateState.Field<CeresDestructionCinematicState, List<ZebesRevealActor>>(self, "revealActors").Count;

        /// <summary>Puts the cinematic at <paramref name="phase"/>, for a check that starts at that step without running the updates that reach it.</summary>
        internal void SkipToPhase(CeresDestructionPhase phase) =>
            PrivateFrontendState.Set(self, nameof(CeresDestructionCinematicState.Phase), phase);

        /// <summary>
        /// Reads the floor cell the cartridge's map holds for the cinematic's current floor shot.
        /// </summary>
        internal byte ReadMode7MapByte(int mapByteIndex)
        {
            if ((uint)mapByteIndex >= FloorSheetComposer.CellCount)
                throw new ArgumentOutOfRangeException(nameof(mapByteIndex));
            return CeresDestructionScreenMemory.Compose(self)[mapByteIndex * 2];
        }
    }
}

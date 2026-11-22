using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Verification access to <see cref="CeresDestructionCinematicState"/> members production does not use.</summary>
internal static class CeresDestructionCinematicStateAccess
{
    /// <summary>Reads private cinematic geometry and actor state for focused verification.</summary>
    extension(CeresDestructionCinematicState self)
    {
<<<<<<< HEAD
||||||| 418ca79935
        /// <summary>Number of cinematic actors the scene still holds: those over Ceres, or the Zebes reveal's.</summary>
        internal int ActiveActorCount =>
            PrivateState.Field<CeresDestructionCinematicState, List<CeresSceneActor>>(self, "ceresActors").Count +
            PrivateState.Field<CeresDestructionCinematicState, List<ZebesRevealActor>>(self, "revealActors").Count;

=======
        /// <summary>Number of cinematic actors the scene still holds: those over Ceres, or the Zebes reveal's.</summary>
        internal int ActiveActorCount =>
            self.ceresActors.Count +
            self.revealActors.Count;

>>>>>>> origin/main
        /// <summary>Puts the cinematic at <paramref name="phase"/>, for a check that starts at that step without running the updates that reach it.</summary>
        internal void SkipToPhase(CeresDestructionPhase phase) =>
            self.Phase = phase;

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

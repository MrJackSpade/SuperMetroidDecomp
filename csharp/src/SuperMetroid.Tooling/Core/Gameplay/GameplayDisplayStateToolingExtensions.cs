using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Gameplay;

/// <summary>Development-tool instance members of <see cref="GameplayDisplayState"/>.</summary>
internal static class GameplayDisplayStateToolingExtensions
{
    /// <summary>Development-tool instance members of <see cref="GameplayDisplayState"/>.</summary>
    extension(GameplayDisplayState self)
    {
        /// <summary>
        /// The Morph Ball eye beam's cone the displayed frame shows, latched at the frame's end with
        /// the runtime's displayed sprites; null while no beam exists or no room's frame is latched.
        /// </summary>
        public MorphBallEyeBeamCone? DisplayedMorphBallEyeBeam => (self.Latched as RoomFrameLatch)?.MorphBallEyeBeam;
    }
}

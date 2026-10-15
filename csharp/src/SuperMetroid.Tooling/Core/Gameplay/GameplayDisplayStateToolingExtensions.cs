using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Gameplay;

/// <summary>Development-tool instance members of <see cref="GameplayDisplayState"/>.</summary>
internal static class GameplayDisplayStateToolingExtensions
{
    /// <summary>Development-tool instance members of <see cref="GameplayDisplayState"/>.</summary>
    extension(GameplayDisplayState self)
    {
        /// <summary>
        /// Morph-ball eye beam geometry published with <see cref="SuperMetroidRuntime.DisplayedOam"/>. A nullable
        /// record distinguishes a genuinely inactive beam from a malformed active slot.
        /// </summary>
        public MorphBallEyeBeamRenderSnapshot? DisplayedMorphBallEyeBeam => self.Latched.MorphBallEyeBeam;
    }
}

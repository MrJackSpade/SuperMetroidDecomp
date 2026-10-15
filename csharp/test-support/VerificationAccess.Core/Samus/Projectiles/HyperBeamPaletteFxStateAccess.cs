using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Verification access to <see cref="HyperBeamPaletteFxState"/> members production does not use.</summary>
internal static class HyperBeamPaletteFxStateAccess
{
    /// <summary>Reads the active Hyper Beam palette-FX instruction cursor for timing diagnostics.</summary>
    extension(HyperBeamPaletteFxState self)
    {
        /// <summary>Where the object's typed program resumes, exposed for exact-step debugging.</summary>
        internal AnimationProgramCursor<PaletteFxProgram> Cursor => self._cursor;
    }
}

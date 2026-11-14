using SuperMetroid.Core.Fx;

namespace SuperMetroid.EnsureVerification.PaletteFx;

/// <summary>Verification view of the running colour effects.</summary>
internal static class PaletteFxAccess
{
    /// <summary>Reads the running colour effects from the production system during verification.</summary>
    extension(ColourEffectSystem fx)
    {
        /// <summary>Counts the running effects.</summary>
        internal int ActiveCount => fx.effects.Count;
    }
}

using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Verification access to <see cref="KraidEyeGlow"/> members production does not use.</summary>
internal static class KraidEyeGlowAccess
{
    /// <summary>Exposes <see cref="KraidEyeGlow"/> state that only verification reads.</summary>
    extension(KraidEyeGlow self)
    {
        /// <summary>The eye colours shown now.</summary>
        internal ReadOnlySpan<RgbColor> Shown => self.shown;
    }
}

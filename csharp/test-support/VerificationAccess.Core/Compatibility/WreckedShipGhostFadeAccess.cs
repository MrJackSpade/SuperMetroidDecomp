using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Compatibility;

/// <summary>Verification access to <see cref="WreckedShipGhostFade"/> members production does not use.</summary>
internal static class WreckedShipGhostFadeAccess
{
    /// <summary>Exposes <see cref="WreckedShipGhostFade"/> state that only verification reads.</summary>
    extension(WreckedShipGhostFade self)
    {
        /// <summary>The colours the current fade moves toward, colour zero first.</summary>
        internal ImmutableArray<RgbColor> Target => [.. self.target.Select(levels => levels.ToColour())];
    }
}

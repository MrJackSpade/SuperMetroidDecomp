using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Verification access to <see cref="CreditsObjectState"/> members production does not use.</summary>
internal static class CreditsObjectStateAccess
{
    /// <summary>Reads a credits object's private tilemap and destination row.</summary>
    extension(CreditsObjectState self)
    {
        /// <summary>BG row selected for the next credits tilemap transfer.</summary>
        internal int NextRow => self.NextRow;

        /// <summary>Retained credits tiles before their transfer.</summary>
        internal ReadOnlySpan<BackgroundTile> Tilemap => self.tilemap;
    }
}

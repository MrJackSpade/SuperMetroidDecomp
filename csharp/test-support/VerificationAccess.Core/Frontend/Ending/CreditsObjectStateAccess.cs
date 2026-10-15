namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Verification access to <see cref="CreditsObjectState"/> members production does not use.</summary>
internal static class CreditsObjectStateAccess
{
    /// <summary>Reads a credits object's private tilemap and destination row.</summary>
    extension(CreditsObjectState self)
    {
        /// <summary>BG row selected for the next credits tilemap transfer.</summary>
        internal int DestinationRow => self.destinationRow;

        /// <summary>Retained credits tilemap words before their BG transfer.</summary>
        internal ReadOnlySpan<ushort> Tilemap => self.tilemap;
    }
}

namespace SuperMetroid.Core.Assets;

/// <summary>Verification access to <see cref="BgArtworkBindings"/> members production does not use.</summary>
internal static class BgArtworkBindingsAccess
{
    /// <summary>Exposes <see cref="BgArtworkBindings"/> state that only verification reads.</summary>
    extension(BgArtworkBindings self)
    {
        /// <summary>The live bindings, oldest first, for verification's composition of what they show.</summary>
        internal IReadOnlyList<BgArtworkBinding> Bound => self.bindings;
    }
}

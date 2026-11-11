using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Verification access to <see cref="SpriteComposition"/> members production does not use.</summary>
internal static class SpriteCompositionAccess
{
    /// <summary>Exposes <see cref="SpriteComposition"/> state that only verification reads.</summary>
    extension(SpriteComposition self)
    {
        /// <summary>Number of parts drawn in composition order.</summary>
        [IntegerQuantity("number of sprite parts in the composition")]
        internal int PartCount => self.parts.Count;
        /// <summary>Selects one part by its drawing ordinal.</summary>
        internal CompiledSpritePart Part(int index) => self.parts[index];
    }
}

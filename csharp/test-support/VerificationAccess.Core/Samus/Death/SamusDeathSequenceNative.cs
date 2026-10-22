using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Samus.Death;

/// <summary>Verification's native view of the suit explosion: the Samus spritemap its current frame shows.</summary>
internal static class SamusDeathSequenceNative
{
    /// <summary>Native views of a death sequence.</summary>
    extension(SamusDeathSequenceState death)
    {
        /// <summary>Samus spritemap-table index of the current suit-explosion frame for the captured facing, or null outside the explosion.</summary>
        internal int? ExplosionSpritemapIndex => death.ExplosionFrame is int frame
            ? SamusDeathSheetComposer.ExplosionSpritemap(death.FacingLeft ? HorizontalDirection.Left : HorizontalDirection.Right, frame)
            : null;
    }
}

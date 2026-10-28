using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Movement;
using static SuperMetroid.Core.Samus.Artwork.SamusAtmosphericArtworkDefinitions;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The sprite attributes of each atmospheric effect frame.</summary>
internal static class SamusAtmosphericAttributes
{
    /// <summary>The native appearance of a bounded footstep or lava/dust frame, retaining palette and priority routing.</summary>
    internal static SpritePart Attributes(bool footstep, int frame)
    {
        if ((uint)frame >= SamusMovementDefinitions.Environment.DirectAtmosphericFrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return SpritePart.Single((footstep ? FootstepFirstTile : LavaDustFirstTile) + frame,
            Palette, Priority);
    }
}

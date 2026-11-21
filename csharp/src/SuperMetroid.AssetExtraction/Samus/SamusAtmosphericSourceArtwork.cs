using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Single-piece appearances for Samus's bank-$90 atmospheric effects, which the atmosphere sheet is composed from (#1463),
/// read from the import data (#1638). The game reads none of them; it draws each effect's frame from the pre-coloured
/// atmosphere sheet by name (#1616).
/// </summary>
/// <remarks>
/// Types four, six, and seven share the same retail list. Type two has a null retail
/// pointer and deliberately remains on the mutable-address-space path.
/// </remarks>
public sealed class SamusAtmosphericSourceArtwork
{
    /// <summary>Holds both installed four-frame appearance lists whole, without retaining caller arrays.</summary>
    /// <param name="footstepSplash">Four type-one footstep appearances in animation-frame order, corresponding to $90:8C0F.</param>
    /// <param name="lavaAndDust">Four shared lava/dust appearances for types 4, 6, and 7, corresponding to $90:8C17.</param>
    /// <exception cref="ArgumentNullException">Either input array is null.</exception>
    /// <exception cref="InvalidDataException">Either list does not contain exactly four appearances.</exception>
    public SamusAtmosphericSourceArtwork(SpritePart[] footstepSplash, SpritePart[] lavaAndDust)
    {
        Ensure.NotNull(footstepSplash);
        Ensure.NotNull(lavaAndDust);
        if (footstepSplash.Length != SamusAtmosphericArtworkDefinitions.DirectAtmosphericFrameCount ||
            lavaAndDust.Length != SamusAtmosphericArtworkDefinitions.DirectAtmosphericFrameCount)
            throw new InvalidDataException("Samus atmospheric small-OBJ lists must each contain four frames.");
        FootstepSplash = [.. footstepSplash];
        LavaAndDust = [.. lavaAndDust];
    }

    /// <summary>Immutable four-frame view of AtmosphericGraphics_SpriteTileNumberAttributes_1_Footstep ($90:8C0F), indexed by frame 0..3.</summary>
    private IReadOnlyList<SpritePart> FootstepSplash { get; }
    /// <summary>Immutable four-frame view of AtmosphericGraphics_SpriteTileNumberAttribute_4_6_7_LavaDust ($90:8C17), shared by types 4, 6, and 7.</summary>
    private IReadOnlyList<SpritePart> LavaAndDust { get; }

    /// <summary>Resolves one single-piece appearance: character, palette, depth and mirroring.</summary>
    /// <param name="pictures">The effect's pictures.</param>
    /// <param name="frame">The frame of the effect's four, from 0; it does not select an animation duration.</param>
    /// <param name="attributes">Selected appearance on success; the default piece on failure.</param>
    /// <returns>True for a frame the effect has; false otherwise.</returns>
    internal bool TryResolve(SamusAtmosphericPictures pictures, int frame, out SpritePart attributes)
    {
        if (frame is < 0 or >= SamusAtmosphericArtworkDefinitions.DirectAtmosphericFrameCount)
        {
            attributes = default;
            return false;
        }
        attributes = (pictures == SamusAtmosphericPictures.FootstepSplash ? FootstepSplash : LavaAndDust)[frame];
        return true;
    }
}

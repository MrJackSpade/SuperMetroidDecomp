namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// What one picture frame of a pose shows, as the import decodes it once from the cartridge's tables (#2556): the half
/// pictures it selects, where it places the body, and the compositions its top and bottom halves show.
/// </summary>
/// <param name="Pictures">The half pictures the frame selects.</param>
/// <param name="OriginRise">How many pixels above Samus's drawn point the frame places the body's origin, negative below it.</param>
/// <param name="TopComposition">The composition the frame's top half shows, or none for a top half that draws nothing.</param>
/// <param name="BottomComposition">
/// The composition the frame's bottom half shows, or none when the bottom half draws nothing or the frame's top half is
/// the complete body.
/// </param>
public sealed partial record SamusBodyDecodedFrame(SamusBodyFramePictures Pictures, int OriginRise,
    SamusSpritemapId? TopComposition, SamusSpritemapId? BottomComposition);

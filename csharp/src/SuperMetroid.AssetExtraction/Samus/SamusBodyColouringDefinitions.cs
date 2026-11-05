using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The colouring each of Samus's poses whose frames are always drawn in one is pre-coloured in on her body sheet (#1513),
/// over the colour rows its frames ink, so their pictures show the colours the game draws them in. Her draws still lay
/// the effect's shades over that base at runtime, so what she shows is unchanged.
/// </summary>
internal static class SamusBodyColouringDefinitions
{
    /// <summary>
    /// The Crystal Flash's first colouring: the first step of its body pulse and of its bubble's rotation, which activation
    /// starts both at and its first palette call lays over her beams' colours. The bubble, and her body where it shows
    /// inside it, ink her beams' colours, which show her beam's colours only outside the Crystal Flash.
    /// </summary>
    private static readonly PictureShade[] CrystalFlash = [SamusShades.CrystalFlashBody(0), SamusShades.CrystalFlashBubble(0)];

    /// <summary>The shades each pose's frames are pre-coloured in, in laying order, a later one winning where they overlap.</summary>
    internal static IReadOnlyDictionary<SamusPoseId, PictureShade[]> Poses { get; } = new Dictionary<SamusPoseId, PictureShade[]>
    {
        [SamusPoseId.CrystalFlashRightPose] = CrystalFlash,
        [SamusPoseId.CrystalFlashLeftPose] = CrystalFlash,
    };
}

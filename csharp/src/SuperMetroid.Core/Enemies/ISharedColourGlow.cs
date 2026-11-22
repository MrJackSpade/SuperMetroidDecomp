using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A room-wide glow other enemies show where they share its colours (#2622): the Blue Brinstar face blocks' glow, which the
/// Morph Ball eye in their room shows in its pupil, since the game draws both in one set of colours there. Whose pictures
/// share them is the installation's: an enemy whose sheet is pre-coloured in the glow's shade shows it.
/// </summary>
internal interface ISharedColourGlow
{
    /// <summary>The shade the glow shows now, or null before it has started.</summary>
    PictureShade? SharedShade { get; }
}

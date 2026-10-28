using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Dachora's selected colours: the normal palette and the speed and shine frames' deviations from it.</summary>
/// <param name="Normal">The sixteen supplied normal colors used as the basis of both animated phases.</param>
/// <param name="SpeedEdits">Speed-frame colors that differ from the normal palette's calculated blue ramp.</param>
/// <param name="ShineEdits">Shine-frame colors that differ from the calculated fade toward white.</param>
internal sealed record DachoraColorSelection(RgbColor[] Normal, Dictionary<ColourCell, RgbColor> SpeedEdits,
    Dictionary<ColourCell, RgbColor> ShineEdits);

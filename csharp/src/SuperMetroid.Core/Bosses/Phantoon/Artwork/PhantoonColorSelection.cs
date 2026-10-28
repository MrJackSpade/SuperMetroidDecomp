using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>Phantoon's selected colours, each kept only where it differs from the calculated paint.</summary>
/// <param name="HealthEdits">Health-band colors that differ from the calculated material and damage shades.</param>
/// <param name="FadeOutEdits">Nonblack targets supplied for Phantoon's fade-out palette.</param>
/// <param name="PowerEdits">Powered-ship colors that differ from the calculated paint palette.</param>
internal sealed record PhantoonColorSelection(Dictionary<ColourCell, RgbColor> HealthEdits,
    Dictionary<ColourCell, RgbColor> FadeOutEdits, Dictionary<ColourCell, RgbColor> PowerEdits);

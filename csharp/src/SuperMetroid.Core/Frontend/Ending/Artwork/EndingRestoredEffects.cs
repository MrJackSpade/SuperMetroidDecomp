using System.Collections.Immutable;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The colour effects a load within a scene restores the colours of (#1597): the fades and the palette effects' colours
/// whose look the load replaces, which then show no more for the rest of the scene.
/// </summary>
/// <param name="Fades">The fades the load ends.</param>
/// <param name="PaletteFx">The palette-effect colours whose settled look the load replaces.</param>
internal sealed record EndingRestoredEffects(ImmutableArray<EndingFade> Fades, ImmutableArray<PaletteFxColours> PaletteFx);

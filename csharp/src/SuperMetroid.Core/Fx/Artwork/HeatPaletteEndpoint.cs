namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>The two heat palette colors whose selected channels are paint while their other channels follow a base color.</summary>
internal enum HeatPaletteEndpoint
{
    /// <summary>Power phase seven, color three: the red endpoint; green and blue follow <see cref="HeatPaletteColors.RedEndpointBase"/>.</summary>
    Red = 1,

    /// <summary>Power phase seven, color four: the mixed endpoint; blue keeps <see cref="HeatPaletteColors.MixedEndpointBase"/>'s green-blue difference.</summary>
    Mixed,
}

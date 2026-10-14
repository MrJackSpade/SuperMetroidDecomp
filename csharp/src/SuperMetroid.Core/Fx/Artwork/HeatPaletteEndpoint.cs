namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>The two heat palette words whose selected channels are paint while their other channels follow a base color.</summary>
internal enum HeatPaletteEndpoint : ushort
{
    /// <summary>$8D:E55C: the red endpoint; green and blue follow <see cref="HeatPaletteColorDefinitions.RedEndpointBase"/>.</summary>
    Red = 0xe55c,

    /// <summary>$8D:E55E: the mixed endpoint; blue keeps <see cref="HeatPaletteColorDefinitions.MixedEndpointBase"/>'s green-blue difference.</summary>
    Mixed = 0xe55e,
}
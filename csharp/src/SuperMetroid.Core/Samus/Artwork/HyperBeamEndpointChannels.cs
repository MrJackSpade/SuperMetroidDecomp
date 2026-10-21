namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The red and green channels a hyper beam endpoint shares, where it shares them.</summary>
/// <param name="Red">The red.</param>
/// <param name="Green">The green.</param>
public readonly record struct HyperBeamEndpointChannels(int? Red, int? Green);

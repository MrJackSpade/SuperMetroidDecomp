namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// How the dust and lava spray Samus raises drift, and how large the small particles are; how long each of their pictures
/// shows is their animation's (<see cref="SamusParticleAnimations"/>).
/// </summary>
internal static class SamusAtmosphericAnimationDefinitions
{
    /// <summary>Dust rises one pixel on each update it is shown.</summary>
    internal static readonly Offset16<RoomPixels> DustDrift = new(0, -1);
    /// <summary>Lava spray rises one pixel on each update it is shown, as it drifts one pixel to its side.</summary>
    internal const int LavaSprayRise = -1;
    /// <summary>Half the width and height of a splash, spray or dust picture, which is drawn centred on its particle.</summary>
    internal const int SmallParticleHalfWidth = 4;
}

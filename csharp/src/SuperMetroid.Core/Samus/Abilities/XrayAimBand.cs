using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>One band of an X-ray scope's angles (#2557): the angle it ends below and the aim it shows.</summary>
/// <param name="Below">The angle the band ends below.</param>
/// <param name="Aim">The aim the scope shows across the band.</param>
internal readonly record struct XrayAimBand(RotationAngle Below, SamusAnimationFrame Aim);

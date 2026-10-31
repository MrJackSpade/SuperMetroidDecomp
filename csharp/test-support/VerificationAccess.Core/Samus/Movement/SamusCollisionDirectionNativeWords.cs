using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Verification's native view of <see cref="SamusCollisionDirection"/> (#1481): the direction value the cartridge's
/// collision routines take, which also indexes Samus's four solid-enemy collision words. Core's values are arbitrary.
/// </summary>
internal static class SamusCollisionDirectionNativeWords
{
    /// <summary>The native direction value naming each <see cref="SamusCollisionDirection"/>.</summary>
    internal static readonly NativeWordIdentities<SamusCollisionDirection> Directions = new(new Dictionary<SamusCollisionDirection, ushort>
    {
        [SamusCollisionDirection.Left] = 0,
        [SamusCollisionDirection.Right] = 1,
        [SamusCollisionDirection.Up] = 2,
        [SamusCollisionDirection.Down] = 3,
        [SamusCollisionDirection.NonDirectionalProbe] = 15,
    });
}

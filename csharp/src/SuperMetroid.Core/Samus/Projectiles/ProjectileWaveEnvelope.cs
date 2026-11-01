using static SuperMetroid.Core.Samus.Projectiles.ProjectileWaveEnvelopeDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The axial distance of each outward step of a Wave lobe.</summary>
internal static class ProjectileWaveEnvelope
{
    /// <summary>Four outward axial lobe offsets 8/13/15/16.
    /// These selected geometric source inputs remain REQUIRED under the artwork owner.</summary>
    internal static int AxialLobeDistance(int outwardStep) =>
        AxialLobeDistances.TryGetValue(outwardStep, out int value) ? value : throw new ArgumentOutOfRangeException(nameof(outwardStep));
}

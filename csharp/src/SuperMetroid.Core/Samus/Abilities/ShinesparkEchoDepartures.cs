using SuperMetroid.Core.Samus.Poses;
using static SuperMetroid.Core.Samus.Abilities.ShinesparkEchoDefinitions;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The angles a Shinespark crash sends its echoes out at.</summary>
public static class ShinesparkEchoDepartures
{
    /// <summary>
    /// The crash's two echo angle indices, indexed by (pose - $C9) * 2. The six
    /// crash poses select an axis and its opposite.
    /// </summary>
    /// <remarks>
    /// The routine indexes with whatever pose is installed when the crash finishes. A drained
    /// controller can replace the crash pose with $E8/$E9 while the crash keeps its
    /// movement handler, and those indices read on past the table into the following code:
    /// the next two byte pairs.
    /// </remarks>
    internal static ShinesparkDepartureAngles DepartureAngles(SamusPoseId pose) =>
        DepartureAngleTable.TryGetValue(pose, out ShinesparkDepartureAngles value) ? value : throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {pose}.");
}

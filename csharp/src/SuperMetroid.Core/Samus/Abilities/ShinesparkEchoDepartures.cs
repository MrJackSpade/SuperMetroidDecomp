using SuperMetroid.Core.Samus.Poses;
using static SuperMetroid.Core.Samus.Abilities.ShinesparkEchoDefinitions;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The angles a Shinespark crash sends its echoes out at.</summary>
public static class ShinesparkEchoDepartures
{
    /// <summary>
    /// $90:D4C6..D4D1, indexed by (pose - $C9) * 2: the crash's two echo angle indices. The six
    /// crash poses select an axis and its opposite.
    /// </summary>
    /// <remarks>
    /// The routine indexes with whatever pose is installed when the crash finishes. A drained
    /// controller ($91:E4F8) can replace the crash pose with $E8/$E9 while the crash keeps its
    /// movement handler, and those indices read on into the code of $90:D4D2: the bytes at
    /// $90:D504/$D505 and $90:D506/$D507.
    /// </remarks>
    internal static ShinesparkDepartureAngles DepartureAngles(SamusPoseId pose) =>
        DepartureAngleTable.TryGetValue(pose, out ShinesparkDepartureAngles value) ? value : throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {pose}.");
}

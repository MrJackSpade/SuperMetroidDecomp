using SuperMetroid.Core.Samus.Poses;
using static SuperMetroid.Core.Samus.Abilities.ShinesparkEchoDefinitions;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The angles a Shinespark crash sends its echoes out at.</summary>
public static class ShinesparkEchoDepartures
{
    /// <summary>
    /// The crash's two echo angles for the pose Samus holds when it finishes: one of the six crash poses, or a drained pose
    /// Mother Brain's drain installed while the crash kept running, whose angles <paramref name="drained"/> holds.
    /// </summary>
    /// <exception cref="InvalidOperationException">The pose is not a crash pose and no drained departures are installed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The pose is neither a crash pose nor a drained pose a crash can finish in.</exception>
    internal static ShinesparkDepartureAngles DepartureAngles(SamusPoseId pose,
        DrainedCrashEchoDepartures<SamusPoseId, ShinesparkDepartureAngles>? drained)
    {
        if (DepartureAngleTable.TryGetValue(pose, out ShinesparkDepartureAngles authored))
            return authored;
        return (drained ?? throw new InvalidOperationException(
            $"A crash finishing in {pose} reads its angles past the departure table, which needs the installed drained-crash departures.")).Departures(pose);
    }
}

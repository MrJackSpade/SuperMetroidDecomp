using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The angles a shinespark crash's two echoes depart at, by the pose the crash finishes in.</summary>
public static class ShinesparkEchoDefinitions
{
    /// <summary>The angles a crash finishing in each of the six crash poses sends its echoes out at.</summary>
    internal static readonly IReadOnlyDictionary<SamusPoseId, ShinesparkDepartureAngles> DepartureAngleTable = new Dictionary<SamusPoseId, ShinesparkDepartureAngles>
    {
        [SamusPoseId.ShinesparkHorizontalRightPose] = Departure(0, 128),
        [SamusPoseId.ShinesparkHorizontalLeftPose] = Departure(0, 128),
        [SamusPoseId.ShinesparkVerticalRightPose] = Departure(64, 192),
        [SamusPoseId.ShinesparkVerticalLeftPose] = Departure(64, 192),
        [SamusPoseId.ShinesparkDiagonalRightPose] = Departure(224, 96),
        [SamusPoseId.ShinesparkDiagonalLeftPose] = Departure(32, 160),
    };

    /// <summary>The angles the crash's two orbiting copies start at when Samus faces left.</summary>
    internal static readonly ShinesparkDepartureAngles CrashOrbitStartFacingLeft = Departure(32, 160);

    /// <summary>The angles the crash's two orbiting copies start at when Samus faces right.</summary>
    internal static readonly ShinesparkDepartureAngles CrashOrbitStartFacingRight = Departure(224, 96);

    /// <summary>The departure angles at two steps of the turn.</summary>
    internal static ShinesparkDepartureAngles Departure(int first, int second) =>
        new ShinesparkDepartureAngles(RotationAngle.FromSteps(first), RotationAngle.FromSteps(second));
}

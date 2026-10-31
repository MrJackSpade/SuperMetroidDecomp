using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Native identities for the ordinary projectile slots used by crash echoes.</summary>
public static class ShinesparkEchoDefinitions
{
    /// <summary>The value each key selects; DepartureAngles looks it up.</summary>
    internal static readonly IReadOnlyDictionary<SamusPoseId, ShinesparkDepartureAngles> DepartureAngleTable = new Dictionary<SamusPoseId, ShinesparkDepartureAngles>
    {
        [SamusPoseId.ShinesparkHorizontalRightPose] = Departure(0, 128),
        [SamusPoseId.ShinesparkHorizontalLeftPose] = Departure(0, 128),
        [SamusPoseId.ShinesparkVerticalRightPose] = Departure(64, 192),
        [SamusPoseId.ShinesparkVerticalLeftPose] = Departure(64, 192),
        [SamusPoseId.ShinesparkDiagonalRightPose] = Departure(224, 96),
        [SamusPoseId.ShinesparkDiagonalLeftPose] = Departure(32, 160),
        [SamusPoseId.DrainedCrouchingRightPose] = Departure(22, 157),
        [SamusPoseId.DrainedCrouchingLeftPose] = Departure(182, 10),
    };

    /// <summary>The departure angles at two table indexes.</summary>
    internal static ShinesparkDepartureAngles Departure(int first, int second) =>
        new ShinesparkDepartureAngles(RotationAngle.FromTableIndex(checked((byte)first)), RotationAngle.FromTableIndex(checked((byte)second)));
}

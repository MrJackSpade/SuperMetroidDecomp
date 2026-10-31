using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Native identities for the ordinary projectile slots used by crash echoes.</summary>
public static class ShinesparkEchoDefinitions
{
    /// <summary>The value each key selects; DepartureAngles looks it up.</summary>
    internal static readonly IReadOnlyDictionary<SamusPoseId, ShinesparkDepartureAngles> DepartureAngleTable = new Dictionary<SamusPoseId, ShinesparkDepartureAngles>
    {
        [SamusPoseId.ShinesparkHorizontalRightPose] = Departure(0x00, 0x80),
        [SamusPoseId.ShinesparkHorizontalLeftPose] = Departure(0x00, 0x80),
        [SamusPoseId.ShinesparkVerticalRightPose] = Departure(0x40, 0xc0),
        [SamusPoseId.ShinesparkVerticalLeftPose] = Departure(0x40, 0xc0),
        [SamusPoseId.ShinesparkDiagonalRightPose] = Departure(0xe0, 0x60),
        [SamusPoseId.ShinesparkDiagonalLeftPose] = Departure(0x20, 0xa0),
        [SamusPoseId.DrainedCrouchingRightPose] = Departure(0x16, 0x9d),
        [SamusPoseId.DrainedCrouchingLeftPose] = Departure(0xb6, 0x0a),
    };

    /// <summary>The departure angles at two table indexes.</summary>
    internal static ShinesparkDepartureAngles Departure(int first, int second) =>
        new ShinesparkDepartureAngles(RotationAngle.FromTableIndex(first), RotationAngle.FromTableIndex(second));
}

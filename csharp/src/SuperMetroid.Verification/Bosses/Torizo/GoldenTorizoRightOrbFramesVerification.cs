using SuperMetroid.Core.Bosses.Torizo;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>Derives the visual identities Golden Torizo's right-orb programs show.</summary>
internal static class GoldenTorizoRightOrbFramesVerification
{
    /// <summary>
    /// The distinct frames Golden Torizo's right-orb programs show, in ascending order. Collision itself comes from
    /// the single TorizoCollisionDefinitions owner.
    /// </summary>
    internal static ushort[] GoldenTorizoRightOrbFrames() =>
        [.. TorizoAnimationsConstants.Spritemaps(TorizoBodyProgram.GoldenSpewChozoOrbFacingLeftRightFootFwd0,
                TorizoBodyProgram.GoldenSpewChozoOrbFacingLeftRightFootFwd1)
            .Distinct().Order()];
}

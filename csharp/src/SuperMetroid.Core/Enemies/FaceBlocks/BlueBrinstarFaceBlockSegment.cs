namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>
/// The Blue Brinstar face block's neutral and turning animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="BlueBrinstarFaceBlockAnimations"/>.
/// </summary>
internal enum BlueBrinstarFaceBlockSegment
{
    /// <summary>The face looking ahead, then glancing and turning to look left toward Samus, held.</summary>
    SamusLeft,
    /// <summary>The face looking ahead, then glancing and turning to look right toward Samus, held.</summary>
    SamusRight,
    /// <summary>The face looking ahead, held.</summary>
    LookingAhead,
}

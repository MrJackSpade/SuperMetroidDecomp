namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>
/// The Blue Brinstar face block's neutral and turning animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="BlueBrinstarFaceBlockAnimations"/>.
/// </summary>
internal enum BlueBrinstarFaceBlockSegment
{
    /// <summary>The Samus left animation.</summary>
    SamusLeft,
    /// <summary>The Samus right animation.</summary>
    SamusRight,
    /// <summary>The initial animation.</summary>
    Initial,
}

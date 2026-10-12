using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// One call of Mother Brain's body walk helpers <c>$A9:C601/$C647</c>: the returned carry
/// (target or arena limit reached) and whether this call installed a walking program.
/// </summary>
internal readonly record struct MotherBrainWalkResult(bool ReachedTarget, bool Requested)
{
    /// <summary>Signals that the walk target or arena limit was reached without installing a new program.</summary>
    internal static MotherBrainWalkResult Reached => new(true, false);
    /// <summary>Signals that the walk remains incomplete and no new program was installed this call.</summary>
    internal static MotherBrainWalkResult Waiting => new(false, false);
    /// <summary>Signals that a walking instruction program was installed while travel remains incomplete.</summary>
    internal static MotherBrainWalkResult WalkInstalled => new(false, true);
}

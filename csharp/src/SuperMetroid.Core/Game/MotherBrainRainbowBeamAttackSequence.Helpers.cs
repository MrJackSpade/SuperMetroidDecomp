using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// One call of Mother Brain's body walk helpers <c>$A9:C601/$C647</c>: the returned carry
/// (target or arena limit reached) and whether this call installed a walking program.
/// </summary>
internal readonly record struct MotherBrainWalkResult(bool ReachedTarget, bool Requested)
{
    internal static MotherBrainWalkResult Reached => new(true, false);
    internal static MotherBrainWalkResult Waiting => new(false, false);
    internal static MotherBrainWalkResult WalkInstalled => new(false, true);
}

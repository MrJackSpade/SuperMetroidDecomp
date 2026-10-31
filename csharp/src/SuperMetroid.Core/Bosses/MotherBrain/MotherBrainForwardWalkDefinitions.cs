using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The random gate of Mother Brain's low-X forward walk.</summary>
public static class MotherBrainForwardWalkDefinitions
{
    /// <summary>$A9:C6F3: the low twelve bits of the shared random word, which the forward-walk gate compares.</summary>
    public static readonly RandomSampleField ForwardWalkRandom = new(0, 12);
    /// <summary>$A9:C6F6 admits the top 64 values of <see cref="ForwardWalkRandom"/> for the low-X forward-walk branch.</summary>
    public const int ForwardWalkRandomThreshold = 0x0fc0;
}

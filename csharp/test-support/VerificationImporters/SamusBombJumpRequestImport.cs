using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.ScenarioFixtures;

namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>Imports Samus's bomb-jump request from the native <c>$0A56</c> word of a recorded snapshot.</summary>
internal static class SamusBombJumpRequestImport
{
    /// <summary>Decodes a native word; a word no native writer stores is rejected.</summary>
    internal static SamusBombJumpRequest FromNative(int word)
    {
        if (word == 0)
            return SamusBombJumpRequest.None;
        SamusBombJumpDirection direction = (word & ~SamusBombJumpRequestCodec.AcceptedBit) switch
        {
            1 => SamusBombJumpDirection.Left,
            2 => SamusBombJumpDirection.Straight,
            3 => SamusBombJumpDirection.Right,
            _ => throw new InvalidDataException($"Bomb-jump word ${word:X4} is not a native request."),
        };
        SamusBombJumpRequest published = SamusBombJumpRequest.Published(direction);
        return (word & SamusBombJumpRequestCodec.AcceptedBit) != 0 ? published.Accept() : published;
    }
}

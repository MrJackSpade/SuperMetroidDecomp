using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.SamusNativeCodecs;

namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>Imports Samus's bomb-jump request from the native <c>$0A56</c> word of a recorded snapshot.</summary>
internal static class SamusBombJumpImport
{
    /// <summary>Decodes a native word, the zero word to no request; a word no native writer stores is rejected.</summary>
    internal static SamusBombJump? FromNative(int word)
    {
        if (word == 0)
            return null;
        SamusBombJumpDirection direction = (word & ~SamusBombJumpCodec.AcceptedBit) switch
        {
            1 => SamusBombJumpDirection.Left,
            2 => SamusBombJumpDirection.Straight,
            3 => SamusBombJumpDirection.Right,
            _ => throw new InvalidDataException($"Bomb-jump word ${word:X4} is not a native request."),
        };
        SamusBombJump published = SamusBombJump.FromBlast(direction);
        return (word & SamusBombJumpCodec.AcceptedBit) != 0 ? published.Arm() : published;
    }
}

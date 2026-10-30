using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.TestSupport.ScenarioFixtures;

/// <summary>
/// Converts Samus's bomb-jump request to and from the native WRAM <c>$0A56</c> word for native traces and comparisons:
/// the low byte is the direction (one left, two straight, three right) and bit 11 marks an accepted request.
/// </summary>
internal static class SamusBombJumpRequestCodec
{
    /// <summary>The bit native pose handling sets when it accepts the request.</summary>
    internal const int AcceptedBit = 1 << 11;

    /// <summary>The native word a request is stored as.</summary>
    internal static int ToNative(SamusBombJumpRequest request) =>
        DirectionByte(request.Direction) | (request.Accepted ? AcceptedBit : 0);

    /// <summary>The direction's native low byte.</summary>
    private static int DirectionByte(SamusBombJumpDirection direction) => direction switch
    {
        SamusBombJumpDirection.None => 0,
        SamusBombJumpDirection.Left => 1,
        SamusBombJumpDirection.Straight => 2,
        SamusBombJumpDirection.Right => 3,
        _ => throw new InvalidOperationException($"Undefined {nameof(SamusBombJumpDirection)} {direction}."),
    };
}

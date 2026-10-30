using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// The bomb jump a bomb's blast asked of Samus (#1470): which way it launches her, and whether her pose handling has
/// accepted it. A blast publishes a direction; the pose handling either rejects it, dropping it, or accepts it and arms
/// the jump, after which the same request cannot arm it again. The rising arc reads the direction until the jump ends.
/// </summary>
public readonly record struct SamusBombJumpRequest
{
    /// <summary>Stores a request not yet accepted; callers use <see cref="None"/>, <see cref="Published"/> and <see cref="Accept"/>.</summary>
    private SamusBombJumpRequest(SamusBombJumpDirection direction) => Direction = direction;

    /// <summary>No bomb jump requested.</summary>
    public static SamusBombJumpRequest None => default;

    /// <summary>A blast's request to launch Samus in <paramref name="direction"/>, not yet handled by her pose.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The direction is none or undefined.</exception>
    public static SamusBombJumpRequest Published(SamusBombJumpDirection direction) => direction switch
    {
        SamusBombJumpDirection.Left or SamusBombJumpDirection.Straight or SamusBombJumpDirection.Right => new(direction),
        SamusBombJumpDirection.None => throw new ArgumentOutOfRangeException(nameof(direction), direction, "A published bomb jump has a direction."),
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Undefined bomb-jump direction."),
    };

    /// <summary>Which way the jump launches Samus, or <see cref="SamusBombJumpDirection.None"/> when none is requested.</summary>
    public SamusBombJumpDirection Direction { get; }

    /// <summary>Whether Samus's pose handling has accepted the request and armed the jump.</summary>
    public bool Accepted { get; private init; }

    /// <summary>Whether a published request waits for Samus's pose handling to accept or reject it.</summary>
    public bool AwaitsPoseHandling => Direction != SamusBombJumpDirection.None && !Accepted;

    /// <summary>The request accepted by Samus's pose handling.</summary>
    /// <exception cref="InvalidOperationException">No published request waits for pose handling.</exception>
    public SamusBombJumpRequest Accept() => AwaitsPoseHandling
        ? this with { Accepted = true }
        : throw new InvalidOperationException($"Only a published bomb jump can be accepted, not {this}.");
}

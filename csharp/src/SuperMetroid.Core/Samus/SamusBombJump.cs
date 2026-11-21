using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// Samus's bomb jump (#1470, #2558), from the blast that starts it until it ends: which way it launches her, and whether
/// her pose handling has armed it. A blast starts it with a direction; the pose handling either rejects it, ending it, or
/// arms it, after which the same jump cannot be armed again. The rising arc reads the direction until the jump ends. A
/// bomb jump always has a direction; when Samus is not bomb jumping she holds none.
/// </summary>
public readonly partial record struct SamusBombJump
{
    /// <summary>Stores a jump not yet armed; a jump is made with <see cref="FromBlast"/> and <see cref="Arm"/>.</summary>
    private SamusBombJump(SamusBombJumpDirection direction) => Direction = direction;

    /// <summary>The jump a blast starts, launching Samus in <paramref name="direction"/>, not yet handled by her pose.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The direction is undefined.</exception>
    public static SamusBombJump FromBlast(SamusBombJumpDirection direction) => direction switch
    {
        SamusBombJumpDirection.Left or SamusBombJumpDirection.Straight or SamusBombJumpDirection.Right => new(direction),
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Undefined bomb-jump direction."),
    };

    /// <summary>Which way the jump launches Samus.</summary>
    public SamusBombJumpDirection Direction { get; }

    /// <summary>Whether Samus's pose handling has armed the jump.</summary>
    public bool IsArmed { get; private init; }

    /// <summary>Whether the jump waits for Samus's pose handling to arm or reject it.</summary>
    public bool AwaitsPoseHandling => !IsArmed;

    /// <summary>The jump armed by Samus's pose handling.</summary>
    /// <exception cref="InvalidOperationException">The jump is already armed.</exception>
    public SamusBombJump Arm() => AwaitsPoseHandling
        ? this with { IsArmed = true }
        : throw new InvalidOperationException($"Only a bomb jump a blast has just started can be armed, not {this}.");
}

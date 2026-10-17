namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Wall probe outcome including contact distance even when the launch chord does not trigger.</summary>
/// <param name="Triggered">Whether the jump chord launched a wall jump.</param>
/// <param name="Contact">Whether the probe touched a wall or solid enemy.</param>
/// <param name="Distance">Whole pixels the probe travelled before contact, the magnitude of a signed whole-pixel word.</param>
internal readonly record struct WallJumpCheckResult(
    bool Triggered,
    bool Contact,
    int Distance);

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The controller words Samus's projectile producer reads: held buttons, this frame's new presses, and the previous frame's new presses (#633).</summary>
public readonly record struct ProjectileButtonInput
{
    /// <summary>Held controller word after the runtime's action-binding normalization.</summary>
    public required ushort ControllerInput { get; init; }

    /// <summary>Current normalized press-edge word for firing decisions.</summary>
    public required ushort ControllerNewInput { get; init; }

    /// <summary>Previous normalized press-edge latch, independently consulted by native beam/missile firing rather than recomputed from held input.</summary>
    public ushort ControllerPreviousNewInput { get; init; }
}

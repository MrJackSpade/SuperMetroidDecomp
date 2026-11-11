using SuperMetroid.Core.Input;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The buttons Samus's projectile producer reads: held buttons, this frame's new presses, and the previous frame's new presses (#633).</summary>
public readonly record struct ProjectileButtonInput
{
    /// <summary>The held buttons, after the runtime maps the action bindings.</summary>
    public required ControllerButton ControllerInput { get; init; }

    /// <summary>This frame's newly pressed buttons, after binding mapping, for firing decisions.</summary>
    public required ControllerButton ControllerNewInput { get; init; }

    /// <summary>The previous frame's newly pressed buttons, after binding mapping, independently consulted by beam/missile firing rather than recomputed from held input.</summary>
    public ControllerButton ControllerPreviousNewInput { get; init; }
}

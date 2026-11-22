using SuperMetroid.Core.Input;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The actions Samus's projectile producer reads: held actions, this frame's new presses, and the previous frame's new presses (#633).</summary>
public readonly partial record struct ProjectileButtonInput
{
    /// <summary>The held actions.</summary>
    public required GameplayAction ControllerInput { get; init; }

    /// <summary>This frame's newly pressed actions, for firing decisions.</summary>
    public required GameplayAction ControllerNewInput { get; init; }

    /// <summary>The previous frame's newly pressed actions, independently consulted by beam/missile firing rather than recomputed from held input.</summary>
    public required GameplayAction ControllerPreviousNewInput { get; init; }
}

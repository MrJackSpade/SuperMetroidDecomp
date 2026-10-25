namespace SuperMetroid.Core.Input;

/// <summary>
/// Named combinations of the player's controller buttons; <see cref="ControllerWordCodec"/> converts a recorded controller word.
/// Keeping these combinations beside <see cref="ControllerButton"/> prevents gameplay code from
/// reintroducing opaque masks such as <c>$0300</c> for the horizontal directions.
/// </summary>
public static class ControllerButtons
{
    /// <summary>Every bit physically supplied by a standard twelve-button controller.</summary>
    public const ControllerButton All =
        ControllerButton.R | ControllerButton.L | ControllerButton.X | ControllerButton.A |
        ControllerButton.Right | ControllerButton.Left | ControllerButton.Down | ControllerButton.Up |
        ControllerButton.Start | ControllerButton.Select | ControllerButton.Y | ControllerButton.B;

    /// <summary>The four directional-pad bits.</summary>
    public const ControllerButton DirectionalPad =
        ControllerButton.Right | ControllerButton.Left | ControllerButton.Down | ControllerButton.Up;

    /// <summary>The mutually opposed horizontal directional bits.</summary>
    public const ControllerButton HorizontalDirections = ControllerButton.Right | ControllerButton.Left;

    /// <summary>The fixed portion of the retail Crystal Flash chord.</summary>
    public const ControllerButton CrystalFlashWithoutShot =
        ControllerButton.Down | ControllerButton.L | ControllerButton.R;

    /// <summary>Tests whether at least one requested button is present.</summary>
    public static bool HasAny(this ControllerButton input, ControllerButton buttons) =>
        (input & buttons) != ControllerButton.None;
}

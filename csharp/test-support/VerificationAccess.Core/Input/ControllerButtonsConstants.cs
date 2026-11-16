namespace SuperMetroid.Core.Input;

/// <summary>Cartridge values and helpers of <see cref="ControllerButtons"/> that only verification reads.</summary>
internal static class ControllerButtonsConstants
{
    /// <summary>The mutually opposed horizontal directional bits.</summary>
    public const ControllerButton HorizontalDirections = ControllerButton.Right | ControllerButton.Left;
}

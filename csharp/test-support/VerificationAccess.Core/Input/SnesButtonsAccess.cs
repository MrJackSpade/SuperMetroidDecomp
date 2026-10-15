namespace SuperMetroid.Core.Input;

/// <summary>Verification access to <see cref="SnesButtons"/> members production does not use.</summary>
internal static class SnesButtonsAccess
{
    /// <summary>Tests whether every requested button is present.</summary>
    internal static bool HasAll(this SnesButton input, SnesButton buttons) =>
        (input & buttons) == buttons;
}

namespace SuperMetroid.Core.Game;

/// <summary>
/// Native two-entry horizontal dispatcher used by Tripper and Kamer (the suspensor
/// platform). These values are indexes, not bank-$A3 function pointers: the ROM doubles
/// each word before indexing the pointer table at <c>$A3:9C97</c>.
/// </summary>
public enum PlatformHorizontalMovement : ushort
{
    /// <summary>Dispatcher index zero selects negative/left horizontal motion; a wall collision switches to index one and installs right-facing artwork.</summary>
    Left = 0,
    /// <summary>Dispatcher index one selects positive/right horizontal motion; a wall collision switches to index zero and installs left-facing artwork.</summary>
    Right = 1,
}

using SuperMetroid.Core;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Diagnostics.Input;

/// <summary>
/// The codec between <see cref="ControllerButton"/> and the sixteen-bit SNES joypad word that input recordings, save RAM,
/// movie replays and native traces store (#1524). It is a host concern: the game consumes typed buttons, and each button's
/// joypad bit is named once here, in both directions.
/// </summary>
[EncodingOwner("SNES joypad word")]
public static class ControllerWordCodec
{
    /// <summary>Every button a standard controller supplies, each converted through <see cref="JoypadBit"/>.</summary>
    private static readonly ControllerButton[] Buttons =
    [
        ControllerButton.R, ControllerButton.L, ControllerButton.X, ControllerButton.A,
        ControllerButton.Right, ControllerButton.Left, ControllerButton.Down, ControllerButton.Up,
        ControllerButton.Start, ControllerButton.Select, ControllerButton.Y, ControllerButton.B,
    ];

    /// <summary>
    /// Converts an external joypad word once at its boundary and rejects bits that the twelve-button controller cannot
    /// produce, so corrupt recordings and accidental non-input words fail at their source instead of influencing gameplay.
    /// </summary>
    /// <param name="word">The stored joypad word.</param>
    /// <param name="sourceContext">Where the word came from, named in the error.</param>
    /// <exception cref="InvalidDataException">The word holds bits no button produces.</exception>
    public static ControllerButton FromJoypadWord(ushort word, string sourceContext)
    {
        ControllerButton buttons = ControllerButton.None;
        int unknown = word;
        foreach (ControllerButton button in Buttons)
        {
            int bit = JoypadBit(button);
            if ((word & bit) == 0)
                continue;
            buttons |= button;
            unknown &= ~bit;
        }

        if (unknown != 0)
        {
            throw new InvalidDataException(
                $"{sourceContext} supplied non-controller bits ${unknown:X} in input word ${word:X4}.");
        }

        return buttons;
    }

    /// <summary>Converts a set of held buttons to the joypad word that stores it.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="buttons"/> holds a value that names no button.</exception>
    public static ushort ToJoypadWord(ControllerButton buttons)
    {
        ControllerButton unknown = buttons & ~ControllerButtons.All;
        if (unknown != ControllerButton.None)
            throw new ArgumentOutOfRangeException(nameof(buttons), buttons, "The buttons hold a value that names no controller button.");

        int word = 0;
        foreach (ControllerButton button in Buttons)
        {
            if ((buttons & button) != ControllerButton.None)
                word |= JoypadBit(button);
        }

        return (ushort)word;
    }

    /// <summary>The joypad bit that stores one button.</summary>
    private static int JoypadBit(ControllerButton button) => button switch
    {
        ControllerButton.R => 1 << 4,
        ControllerButton.L => 1 << 5,
        ControllerButton.X => 1 << 6,
        ControllerButton.A => 1 << 7,
        ControllerButton.Right => 1 << 8,
        ControllerButton.Left => 1 << 9,
        ControllerButton.Down => 1 << 10,
        ControllerButton.Up => 1 << 11,
        ControllerButton.Start => 1 << 12,
        ControllerButton.Select => 1 << 13,
        ControllerButton.Y => 1 << 14,
        ControllerButton.B => 1 << 15,
        ControllerButton.None => throw new ArgumentOutOfRangeException(nameof(button), button, "No buttons have no joypad bit."),
        _ => throw new ArgumentOutOfRangeException(nameof(button), button, "Only a single button has a joypad bit."),
    };
}

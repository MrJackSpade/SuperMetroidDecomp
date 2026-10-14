namespace SuperMetroid.Core.Game;

/// <summary>Command bytes in a charge-flare delay stream; every other byte is a delay.</summary>
internal enum ChargeFlareDelayCommand : byte
{
    /// <summary>$FE subtracts the following byte from the selected frame.</summary>
    Rewind = 0xfe,
    /// <summary>$FF resets the frame to zero.</summary>
    Restart = 0xff,
}

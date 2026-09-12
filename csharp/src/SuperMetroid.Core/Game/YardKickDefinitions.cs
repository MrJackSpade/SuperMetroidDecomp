namespace SuperMetroid.Core.Game;

/// <summary>Native Yard vertical launch words indexed by the player's whole horizontal displacement.</summary>
public static class YardKickDefinitions
{
    /// <summary>$A3:D517, KickYardIntoAir.YSubVelocity/YVelocity: sixteen fractional/whole pairs.</summary>
    public const int ReferenceAddress = 0xa3d517;

    /// <summary>Caps only the vertical-table selector at fifteen; caller retains its original horizontal speed.</summary>
    public static (ushort Fraction, ushort Whole) ForHorizontalSpeed(ushort wholeSpeed)
    {
        int index = Math.Min(wholeSpeed, (ushort)15);
        ushort fraction = (index % 3) switch { 1 => 0xa000, 2 => 0x4000, _ => 0 };
        return (fraction, unchecked((ushort)(-3 - index / 3)));
    }
}

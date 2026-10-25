namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Native Yard vertical launch words indexed by the player's whole horizontal displacement.</summary>
public static class YardKickDefinitions
{

    /// <summary>Caps only the vertical-table selector at fifteen; caller retains its original horizontal speed.</summary>
    public static SignedSixteenSixteen ForHorizontalSpeed(int wholeSpeed)
    {
        int index = Math.Min(wholeSpeed, 15);
        int fraction = (index % 3) switch { 1 => 0xa000, 2 => 0x4000, _ => 0 };
        return SignedSixteenSixteen.FromParts(checked((short)(-3 - index / 3)), fraction);
    }
}

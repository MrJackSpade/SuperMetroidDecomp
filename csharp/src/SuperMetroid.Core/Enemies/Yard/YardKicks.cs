namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>How hard Samus's kick launches a Yard upward.</summary>
public static class YardKicks
{
    /// <summary>Caps only the vertical kick's choice at fifteen; the Yard keeps its original horizontal speed.</summary>
    public static SignedSixteenSixteen ForHorizontalSpeed(int wholeSpeed) =>
        YardKickDefinitions.Kicks[Math.Min(wholeSpeed, YardKickDefinitions.FastestKickSpeed)];
}

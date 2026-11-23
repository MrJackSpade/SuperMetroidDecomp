namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>How hard Samus's kick launches a Yard upward.</summary>
public static partial class YardKicks
{
    /// <summary>Caps only the vertical kick's choice at <see cref="YardKickDefinitions.FastestKickSpeed"/>; the Yard keeps its original horizontal speed.</summary>
    public static SignedSixteenSixteen ForHorizontalSpeed(int wholeSpeed) =>
        YardKickDefinitions.Kicks[Math.Min(wholeSpeed, YardKickDefinitions.FastestKickSpeed)];
}

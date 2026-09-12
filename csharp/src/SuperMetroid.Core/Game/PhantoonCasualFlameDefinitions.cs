namespace SuperMetroid.Core.Game;

/// <summary>Phantoon mouth schedules retain the native header and reverse-read timer layout.</summary>
public static class PhantoonCasualFlameDefinitions
{
    /// <summary>$A7:CCFD pointer list selects $CD05/$CD13/$CD1D/$CD2F (CasualFlameTimers patterns 0..3). Word zero is the flame count; subsequent words are consumed backwards.</summary>
    public static ReadOnlySpan<ushort> Pattern(int index) => index switch
    {
        0 => [5, 180, 32, 32, 32, 32, 32],
        1 => [3, 180, 16, 16, 16],
        2 => [7, 180, 48, 48, 48, 48, 48, 48, 48],
        3 => [7, 180, 16, 64, 32, 64, 32, 16, 32],
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
}

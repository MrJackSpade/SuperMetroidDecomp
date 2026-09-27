namespace SuperMetroid.Core.Game;

/// <summary>Mother Brain's direct-index FX records following the room's default entry.</summary>
public static class MotherBrainFxRecordPointers
{
    /// <summary>$83:A0B4, selected by head/body room initialization when FX index is one.</summary>
    public const ushort Initial = 0xa0b4;

    /// <summary>$83:A0C4, selected by the fake-death descent when FX index is two.</summary>
    public const ushort FakeDeath = 0xa0c4;

    /// <summary>These records are selected by index, not by the normal door-list walk.</summary>
    public static ReadOnlySpan<ushort> DirectRecords => [Initial, FakeDeath];
}

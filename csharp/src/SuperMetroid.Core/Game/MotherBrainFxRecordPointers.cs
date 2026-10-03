namespace SuperMetroid.Core.Game;

/// <summary>Mother Brain's direct-index FX records following the room's default entry.</summary>
public static class MotherBrainFxRecordPointers
{
    /// <summary>$83:A0B4, selected by head/body room initialization when FX index is one.</summary>
    public const ushort Initial = 0xa0b4;

    /// <summary>$83:A0C4, selected by the fake-death descent when FX index is two.</summary>
    public const ushort FakeDeath = 0xa0c4;

    /// <summary>
    /// Enumerates the two consecutive sixteen-byte records selected by native FX
    /// indices one and two at $A9:86EB/$888C. Load_FX_Entry at $89:AB02 scales an
    /// index by sixteen; the default record precedes Initial. This bounded view
    /// supports native capture/generation without storing a second pointer roster.
    /// </summary>
    public static IEnumerable<ushort> DirectRecords
    {
        get
        {
            for (int pointer = Initial; pointer <= FakeDeath; pointer += RoomFxRomData.Record.ByteCount)
                yield return (ushort)pointer;
        }
    }
}

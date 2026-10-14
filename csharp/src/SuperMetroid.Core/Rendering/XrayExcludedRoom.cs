namespace SuperMetroid.Core.Rendering;

/// <summary>The two room headers CanXrayShowBlocks ($91:D143) explicitly excludes from revelation.</summary>
public enum XrayExcludedRoom : ushort
{
    /// <summary>$8F:A66A, one of the two explicitly excluded room headers in $91:D143.</summary>
    RoomA66A = 0xA66A,
    /// <summary>$8F:CEFB, excluded from revelation; $88:81A4 additionally removes BG2 from TM.</summary>
    HiddenBg2Room = 0xCEFB,
}

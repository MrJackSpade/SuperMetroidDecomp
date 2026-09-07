namespace SuperMetroid.Core.Rooms;

/// <summary>Bank-$91 revealed-block table and native command identities.</summary>
public static class XrayRevealCodePointers
{
    /// <summary>RevealedBlockTable_0 at $91:D2D6, keyed by the level word's block-type nibble.</summary>
    public const ushort BlockTypeTable = 0xd2d6;
    /// <summary>Bank containing the revealed-block tables and commands.</summary>
    public const int Bank = 0x910000;
    /// <summary>Native end marker for both block-type and BTS tables.</summary>
    public const ushort End = 0xffff;
    /// <summary>Native BTS wildcard; matches any byte BTS in the selected block family.</summary>
    public const ushort AnyBts = 0xff00;
    /// <summary>RevealedBlockCommand_VerticalExtension at $91:CE79.</summary>
    public const ushort VerticalExtension = 0xce79;
    /// <summary>RevealedBlockCommand_HorizontalExtension at $91:CEBB.</summary>
    public const ushort HorizontalExtension = 0xcebb;
    /// <summary>RevealedBlockCommand_Copy1x1BlockToXrayBG2Tilemap at $91:CF36.</summary>
    public const ushort CopyOne = 0xcf36;
    /// <summary>RevealedBlockCommand_Copy1x1BlockToXrayBG2TilemapIfBrinstar at $91:CF3E.</summary>
    public const ushort CopyBrinstar = 0xcf3e;
    /// <summary>RevealedBlockCommand_Copy2x1BlockToXrayBG2Tilemap at $91:CF4E.</summary>
    public const ushort CopyWide = 0xcf4e;
    /// <summary>RevealedBlockCommand_Copy1x2BlockToXrayBG2Tilemap at $91:CF62.</summary>
    public const ushort CopyTall = 0xcf62;
    /// <summary>RevealedBlockCommand_Copy2x2BlockToXrayBG2Tilemap at $91:CF6F.</summary>
    public const ushort CopySquare = 0xcf6f;
}

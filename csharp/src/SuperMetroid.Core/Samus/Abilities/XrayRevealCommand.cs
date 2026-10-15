namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The seven bank-$91 revealed-block commands named by the X-ray reveal tables.</summary>
public enum XrayRevealCommand : ushort
{
    /// <summary>RevealedBlockCommand_VerticalExtension at $91:CE79.</summary>
    VerticalExtension = 0xce79,
    /// <summary>RevealedBlockCommand_HorizontalExtension at $91:CEBB.</summary>
    HorizontalExtension = 0xcebb,
    /// <summary>RevealedBlockCommand_Copy1x1BlockToXrayBG2Tilemap at $91:CF36.</summary>
    CopyOne = 0xcf36,
    /// <summary>RevealedBlockCommand_Copy1x1BlockToXrayBG2TilemapIfBrinstar at $91:CF3E.</summary>
    CopyBrinstar = 0xcf3e,
    /// <summary>RevealedBlockCommand_Copy2x1BlockToXrayBG2Tilemap at $91:CF4E.</summary>
    CopyWide = 0xcf4e,
    /// <summary>RevealedBlockCommand_Copy1x2BlockToXrayBG2Tilemap at $91:CF62.</summary>
    CopyTall = 0xcf62,
    /// <summary>RevealedBlockCommand_Copy2x2BlockToXrayBG2Tilemap at $91:CF6F.</summary>
    CopySquare = 0xcf6f,
}

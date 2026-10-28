namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The seven bank-$91 revealed-block commands named by the X-ray reveal tables.</summary>
public enum XrayRevealCommand
{
    /// <summary>RevealedBlockCommand_VerticalExtension at $91:CE79.</summary>
    VerticalExtension,
    /// <summary>RevealedBlockCommand_HorizontalExtension at $91:CEBB.</summary>
    HorizontalExtension,
    /// <summary>RevealedBlockCommand_Copy1x1BlockToXrayBG2Tilemap at $91:CF36.</summary>
    CopyOne,
    /// <summary>RevealedBlockCommand_Copy1x1BlockToXrayBG2TilemapIfBrinstar at $91:CF3E.</summary>
    CopyBrinstar,
    /// <summary>RevealedBlockCommand_Copy2x1BlockToXrayBG2Tilemap at $91:CF4E.</summary>
    CopyWide,
    /// <summary>RevealedBlockCommand_Copy1x2BlockToXrayBG2Tilemap at $91:CF62.</summary>
    CopyTall,
    /// <summary>RevealedBlockCommand_Copy2x2BlockToXrayBG2Tilemap at $91:CF6F.</summary>
    CopySquare,
}

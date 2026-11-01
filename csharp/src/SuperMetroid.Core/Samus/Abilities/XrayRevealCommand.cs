namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The seven revealed-block commands named by the X-ray reveal tables.</summary>
public enum XrayRevealCommand
{
    /// <summary>RevealedBlockCommand_VerticalExtension.</summary>
    VerticalExtension,
    /// <summary>RevealedBlockCommand_HorizontalExtension.</summary>
    HorizontalExtension,
    /// <summary>RevealedBlockCommand_Copy1x1BlockToXrayBG2Tilemap.</summary>
    CopyOne,
    /// <summary>RevealedBlockCommand_Copy1x1BlockToXrayBG2TilemapIfBrinstar.</summary>
    CopyBrinstar,
    /// <summary>RevealedBlockCommand_Copy2x1BlockToXrayBG2Tilemap.</summary>
    CopyWide,
    /// <summary>RevealedBlockCommand_Copy1x2BlockToXrayBG2Tilemap.</summary>
    CopyTall,
    /// <summary>RevealedBlockCommand_Copy2x2BlockToXrayBG2Tilemap.</summary>
    CopySquare,
}

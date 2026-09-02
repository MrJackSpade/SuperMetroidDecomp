namespace SuperMetroid.Core.Rooms;

/// <summary>Bank-$82 command words accepted by room library-background lists.</summary>
internal enum LibraryBackgroundCommand : ushort
{
    /// <summary>Terminate the command list.</summary>
    End = 0x0000,
    /// <summary>Transfer a parameterized ROM range to VRAM.</summary>
    TransferToVram = 0x0002,
    /// <summary>Decompress a parameterized ROM stream into work RAM.</summary>
    DecompressToWorkRam = 0x0004,
    /// <summary>Fill and upload the shared BG3 FX tilemap.</summary>
    ClearFxTilemap = 0x0006,
    /// <summary>Transfer to VRAM and select Kraid's BG3 character base.</summary>
    TransferToVramForKraid = 0x0008,
    /// <summary>Clear the ordinary BG2 tilemap pages.</summary>
    ClearBg2 = 0x000a,
    /// <summary>Clear BG2 and its additional Kraid page.</summary>
    ClearBg2ForKraid = 0x000c,
    /// <summary>Execute the following transfer only for a matching door pointer.</summary>
    TransferForDoor = 0x000e,
}

namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge identities consumed while constructing a Landing Site room entry.</summary>
public static class LandingSiteRomData
{
    /// <summary>Synthetic bank-$83 door used by the intro landing cutscene.</summary>
    public const ushort LandingCutsceneDoorPointer = 0x88fe;

    /// <summary>Bank containing every door header.</summary>
    public const int DoorBank = 0x830000;

    /// <summary>Full address of <c>RoomHeader_LandingSite</c> at $8F:91F8.</summary>
    public const int RoomHeaderAddress = 0x8f0000 | RoomHeaderPointers.LandingSite;

    /// <summary>Resolved unconditional Landing Site room state at $8F:9213.</summary>
    public const int DefaultStateAddress = 0x8f9213;

    /// <summary>Landing Site scrolling-sky library-background command list at $8F:B76A.</summary>
    public const int LibraryBackgroundListAddress = 0x8fb76a;
}

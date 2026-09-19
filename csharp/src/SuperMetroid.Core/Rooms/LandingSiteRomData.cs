namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge identities consumed while constructing a Landing Site room entry.</summary>
public static class LandingSiteRomData
{
    /// <summary>Synthetic bank-$83 door used by the intro landing cutscene.</summary>
    public const ushort LandingCutsceneDoorPointer = 0x88fe;

    /// <summary>Landing Site scrolling-sky library-background command list at $8F:B76A.</summary>
    public const int LibraryBackgroundListAddress = 0x8fb76a;
}

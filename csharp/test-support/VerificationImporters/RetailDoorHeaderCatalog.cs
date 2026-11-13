using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>Physical bank-$83 ranges containing the 596 retail door headers of the game.</summary>
/// <remarks>
/// The cartridge stores Crateria through Lower Norfair doors before the FX tables, then
/// resumes with Wrecked Ship through Ceres doors after them. Keeping those two ranges as
/// named cartridge data lets exhaustive audits inspect every header without mistaking the
/// intervening FX records for doors or maintaining a hand-copied list of 596 addresses. The one header only the Norfair room
/// no door list names ($8F:B3E1) lists is left out (#1686): no room of the game holds it.
/// </remarks>
internal static class RetailDoorHeaderCatalog
{
    /// <summary>First Crateria door header, <c>Door_LandingSite_LandingCutscene</c>.</summary>
    private const ushort PreFxBlockStart = 0x88fe;

    /// <summary>Last Lower Norfair door header, <c>Door_LNSave_0</c>.</summary>
    private const ushort PreFxBlockEnd = 0x9ab6;

    /// <summary>First Wrecked Ship door header, <c>Door_BowlingAlley_0</c>.</summary>
    private const ushort PostFxBlockStart = 0xa18c;

    /// <summary>Last Ceres door header, <c>Door_CeresRidley</c>.</summary>
    private const ushort PostFxBlockEnd = 0xabb8;

    /// <summary>The door only the unreferenced Norfair room lists, <c>UNUSED_Door_83991E</c>.</summary>
    private const ushort UnreferencedRoomDoor = 0x991e;

    /// <summary>Total number of retail headers of the game encoded across both blocks.</summary>
    public const int HeaderCount = 596;

    /// <summary>Enumerates every retail door-header pointer in physical cartridge order.</summary>
    public static IEnumerable<ushort> EnumeratePointers()
    {
        foreach (ushort pointer in EnumerateRange(PreFxBlockStart, PreFxBlockEnd))
            yield return pointer;
        foreach (ushort pointer in EnumerateRange(PostFxBlockStart, PostFxBlockEnd))
            yield return pointer;
    }

    /// <summary>
    /// Walks an inclusive bank-$83 door-header block using the native header size,
    /// so intervening non-door records are never treated as headers.
    /// </summary>
    private static IEnumerable<ushort> EnumerateRange(ushort start, ushort end)
    {
        for (int pointer = start; pointer <= end; pointer += DoorDefinition.SizeInBytes)
        {
            if (pointer != UnreferencedRoomDoor)
                yield return unchecked((ushort)pointer);
        }
    }
}

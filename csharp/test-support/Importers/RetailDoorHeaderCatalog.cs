namespace SuperMetroid.Core.Rooms;

/// <summary>Physical bank-$83 ranges containing the 597 retail door headers.</summary>
/// <remarks>
/// The cartridge stores Crateria through Lower Norfair doors before the FX tables, then
/// resumes with Wrecked Ship through Ceres doors after them. Keeping those two ranges as
/// named cartridge data lets exhaustive audits inspect every header without mistaking the
/// intervening FX records for doors or maintaining a hand-copied list of 597 addresses.
/// </remarks>
internal static class RetailDoorHeaderCatalog
{
    /// <summary>First Crateria door header, <c>Door_LandingSite_LandingCutscene</c>.</summary>
    private const ushort PreFxBlockStart = DoorHeaderRomData.PreFxBlockStart;

    /// <summary>Last Lower Norfair door header, <c>Door_LNSave_0</c>.</summary>
    private const ushort PreFxBlockEnd = DoorHeaderRomData.PreFxBlockEnd;

    /// <summary>First Wrecked Ship door header, <c>Door_BowlingAlley_0</c>.</summary>
    private const ushort PostFxBlockStart = DoorHeaderRomData.PostFxBlockStart;

    /// <summary>Last Ceres door header, <c>Door_CeresRidley</c>.</summary>
    private const ushort PostFxBlockEnd = DoorHeaderRomData.PostFxBlockEnd;

    /// <summary>Total number of retail headers encoded across both blocks.</summary>
    public const int HeaderCount = DoorDefinitions.HeaderCount;

    /// <summary>Enumerates every retail door-header pointer in physical cartridge order.</summary>
    public static IEnumerable<ushort> EnumeratePointers()
    {
        foreach (ushort pointer in EnumerateRange(PreFxBlockStart, PreFxBlockEnd))
            yield return pointer;
        foreach (ushort pointer in EnumerateRange(PostFxBlockStart, PostFxBlockEnd))
            yield return pointer;
    }

    private static IEnumerable<ushort> EnumerateRange(ushort start, ushort end)
    {
        for (int pointer = start; pointer <= end; pointer += CartridgeDoorHeader.SizeInBytes)
            yield return unchecked((ushort)pointer);
    }
}

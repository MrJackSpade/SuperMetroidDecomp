using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>The bank-$83 record pointers of the doors the game holds, for audits that inspect every door header.</summary>
/// <remarks>
/// The walk is the import pipeline's own door map, so an audit reaches exactly the doors production holds. The cartridge
/// stores three physical records nothing in the game reaches, and the map leaves them out: the door only the Norfair room
/// no door list names ($8F:B3E1) lists (#1686), and the Halfie Climb and Maridia Missile Refill records no door list,
/// load station, demonstration or FX record names (#2566, #2585).
/// </remarks>
internal static class RetailDoorHeaderCatalog
{
    /// <summary>Enumerates every held door's record pointer in physical cartridge order.</summary>
    public static IEnumerable<ushort> EnumeratePointers() => DoorHeaderSourceDefinitions.ByPointer.Keys.Order();
}

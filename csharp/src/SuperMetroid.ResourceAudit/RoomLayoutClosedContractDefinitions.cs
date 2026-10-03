namespace SuperMetroid.ResourceAudit;

/// <summary>Complete production room-layout installation, separate from explicitly partial fixtures.</summary>
internal static class RoomLayoutClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomVisualLayoutCatalog", "room-layout-complete-required-source-installation", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Rooms/RoomVisualLayoutCatalog.cs", "E7B088F1ACA48545E65F7FAAD3F91AFAC132B6E55B00DB00DF22644180AC2EA6"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomVisualLayoutSourceDefinitions.cs", "0EB53CF2A7FE8B4CB0D9AD84FF9C921FDF8303124A2214C7EBE0E5EE8E75A60E"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomStateDefinitions.cs", "FD0E9AA1573C75DA565AA453B3FDF3C89BE9B0E5680322A539279308467C62F3"),
             new("csharp/src/SuperMetroid.Core/Rooms/CartridgeRoomHeader.cs", "456E784B457DC4B8361279DDA9069B6B039AA160EF819498EF60DF7F410A1D4B")],
            "Public construction requires a nonnull matching layout for every source in the immutable compiled room-state domain, and copies the dictionary. The partial verification factory is prohibited in Core by a semantic ownership guard. This proves required source membership, not arbitrary keys, caller source selection, room geometry, collision changes, host binding, placement or pixels."),
    ];
}

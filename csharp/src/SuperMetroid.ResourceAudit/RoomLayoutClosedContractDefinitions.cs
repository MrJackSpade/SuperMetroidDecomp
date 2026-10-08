namespace SuperMetroid.ResourceAudit;

/// <summary>Complete production room-layout installation, separate from explicitly partial fixtures.</summary>
internal static class RoomLayoutClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomVisualLayoutCatalog", "room-layout-complete-required-source-installation", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Rooms/RoomVisualLayoutCatalog.cs", "086D9777CCEAC6E4EC4DCD1D472E5DEB3460A77938173368B02301FC8F3B2379"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomVisualLayoutSourceDefinitions.cs", "0EDE39808D016A2893ACC65E1CF3D9C397713BD9E91BF549C16EBA62DFD9C266"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomStateDefinitions.cs", "FD0E9AA1573C75DA565AA453B3FDF3C89BE9B0E5680322A539279308467C62F3"),
             new("csharp/src/SuperMetroid.Core/Rooms/CartridgeRoomHeader.cs", "456E784B457DC4B8361279DDA9069B6B039AA160EF819498EF60DF7F410A1D4B")],
            "Public construction requires a nonnull matching layout for every source in the immutable compiled room-state domain, and copies the dictionary. The partial verification factory is prohibited in Core by a semantic ownership guard. This proves required source membership, not arbitrary keys, caller source selection, room geometry, collision changes, host binding, placement or pixels."),
    ];
}

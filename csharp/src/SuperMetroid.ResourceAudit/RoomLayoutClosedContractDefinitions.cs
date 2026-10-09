namespace SuperMetroid.ResourceAudit;

/// <summary>Complete production room-layout installation, separate from explicitly partial fixtures.</summary>
internal static class RoomLayoutClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomVisualLayoutCatalog", "room-layout-complete-required-source-installation", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Rooms/RoomVisualLayoutCatalog.cs", "6AC93B76892997E7BC708D2E10CD127E68AAC855B3ED6C9D831F4615CB5DB77E"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomVisualLayoutSourceDefinitions.cs", "7565C39F5976EADE518FEB6642B1342B23FFE3A16EDBB445CC602548D5DEA241"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomStateDefinitions.cs", "93A78AC57BDA8AB714CA6D250C9A3CA7008EF1F254D2047BA4277ACB1A9AEFD6"),
             new("csharp/src/SuperMetroid.Core/Rooms/CartridgeRoomHeader.cs", "42868C1BEC2CBEAEADDB79C45774FA21152564E70BB5FFD6F4D8A57B39DA53CB")]),
    ];
}

namespace SuperMetroid.ResourceAudit;

/// <summary>Complete production room-layout installation, separate from explicitly partial fixtures.</summary>
internal static class RoomLayoutClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomVisualLayoutCatalog", "room-layout-complete-required-source-installation", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Rooms/RoomVisualLayoutCatalog.cs", "C2322849C36DE87012F86AABDB744DA70352F3515845A27FEDD24837DE98BE14"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomVisualLayoutSourceDefinitions.cs", "7565C39F5976EADE518FEB6642B1342B23FFE3A16EDBB445CC602548D5DEA241"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomStateDefinitions.cs", "A81815A52EF266CE17FB4EE569D9E07B1A0F67D47F0F914591E5D5FD0E92B159"),
             new("csharp/src/SuperMetroid.Core/Rooms/CartridgeRoomHeader.cs", "AA97C7D584CBBEFE6E0CE5200CCA40FAD7D6C92076E2F760DE2022AE92CE411D")]),
    ];
}

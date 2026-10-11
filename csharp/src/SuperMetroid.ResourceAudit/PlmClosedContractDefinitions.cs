namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed complete bank-$84 artwork domains, separate from collision words and instruction timing.</summary>
internal static class PlmClosedContractDefinitions
{
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockDrawDefinitions.cs", "A12FA434EA63721B9FA6209C32B200522701C72552730B35052249373814E22C");

    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomPlmShotBlockVisualCatalog", "plm-shot-block-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockVisualCatalog.cs", "C0CBA15296CFC18DCED7A99B9474FA23BF5C8DCA65B71FD342686881FB369D51")]),
        new("SuperMetroid.Core.Rooms.RoomPlmStationVisualCatalog", "plm-station-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmStationVisualCatalog.cs", "D2A197C24AE77D2BEB8B8C66BAA5F5A22BD6622BBDB6280FBB3CE9DD499A9BFD"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmStationDrawDefinitions.cs", "345C930CC795B99B5A740CECFD2C26F8B446005340A8E33AB215AC6EEF64A722")]),
        new("SuperMetroid.Core.Rooms.RoomPlmBombTorizoHandVisualCatalog", "plm-torizo-hand-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmBombTorizoHandVisualCatalog.cs", "E01676321A8ACE64C952B6FBDD4D3EC12BC1FDCB76E12E9736326588050671A0"),
             new("csharp/src/SuperMetroid.Core/Rooms/BombTorizoHandPlmDrawDefinitions.cs", "3D2A5653625C20CD80EF320467D64298E1E6AB794F9F18B8CCDE0FDCB836DAD1")]),
        new("SuperMetroid.Core.Rooms.RoomPlmMotherBrainGlassVisualCatalog", "plm-mother-brain-glass-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmMotherBrainGlassVisualCatalog.cs", "C277F276DFD29C926E615E1B35EF664C93F8F361EFEFFD76EAD9B332441CBC0E"),
             new("csharp/src/SuperMetroid.Core/Rooms/MotherBrainGlassPlmDrawDefinitions.cs", "900E2757B47615B69121103310E4A3237734F7972C6391FBADDA935A9D861CE7")]),
        new("SuperMetroid.Core.Rooms.RoomPlmNoobTubeVisualCatalog", "plm-noob-tube-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmNoobTubeVisualCatalog.cs", "F91F5DA138778DBD4D4CCF853E6A78E9C2E3C0EFF1B5370BC9D11FD7BF2CDD73"),
             new("csharp/src/SuperMetroid.Core/Rooms/NoobTubePlmDrawDefinitions.cs", "B87E139ED048E6F07669A133ACAC0269187719865ACD237EC4637086DCD5E462")]),
    ];
}

namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed complete bank-$84 artwork domains, separate from collision words and instruction timing.</summary>
internal static class PlmClosedContractDefinitions
{
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockDrawDefinitions.cs", "1DA6DFC4917DCCCBD291864836978A1545C55A3D3C03A81B20D943885938BEAC");

    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomPlmShotBlockVisualCatalog", "plm-shot-block-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockVisualCatalog.cs", "C0CBA15296CFC18DCED7A99B9474FA23BF5C8DCA65B71FD342686881FB369D51")]),
        new("SuperMetroid.Core.Rooms.RoomPlmStationVisualCatalog", "plm-station-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmStationVisualCatalog.cs", "D2A197C24AE77D2BEB8B8C66BAA5F5A22BD6622BBDB6280FBB3CE9DD499A9BFD"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmStationDrawDefinitions.cs", "ED08BB90FB300A26B9185421291976D6A37B5686E0CC0C6570C57F7F5A9665E5")]),
        new("SuperMetroid.Core.Rooms.RoomPlmBombTorizoHandVisualCatalog", "plm-torizo-hand-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmBombTorizoHandVisualCatalog.cs", "E01676321A8ACE64C952B6FBDD4D3EC12BC1FDCB76E12E9736326588050671A0"),
             new("csharp/src/SuperMetroid.Core/Rooms/BombTorizoHandPlmDrawDefinitions.cs", "C700533E300A0889B61B4F93E5FB693359E6F627513F45AF3186B6DEE9473EE5")]),
        new("SuperMetroid.Core.Rooms.RoomPlmMotherBrainGlassVisualCatalog", "plm-mother-brain-glass-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmMotherBrainGlassVisualCatalog.cs", "C277F276DFD29C926E615E1B35EF664C93F8F361EFEFFD76EAD9B332441CBC0E"),
             new("csharp/src/SuperMetroid.Core/Rooms/MotherBrainGlassPlmDrawDefinitions.cs", "1E30752CB123E8F02AA6FC2DCA990396D12AEFB72412950A9494C5C97A9B8195")]),
        new("SuperMetroid.Core.Rooms.RoomPlmNoobTubeVisualCatalog", "plm-noob-tube-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmNoobTubeVisualCatalog.cs", "F91F5DA138778DBD4D4CCF853E6A78E9C2E3C0EFF1B5370BC9D11FD7BF2CDD73"),
             new("csharp/src/SuperMetroid.Core/Rooms/NoobTubePlmDrawDefinitions.cs", "845AE79958633B664742C0E595BBD2E923AE1AE2DD6D649210319CAFB96EC86D")]),
    ];
}

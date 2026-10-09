namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed complete bank-$84 artwork domains, separate from collision words and instruction timing.</summary>
internal static class PlmClosedContractDefinitions
{
    /// <summary>Fingerprint for the shared source shape used by reviewed PLM draw contracts.</summary>
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockDrawDefinitions.cs",
        "EE5DD1AAFD6BCCA4627D2D11582BCB88782327B9B764DAA7081B5DAD3822D132");

    /// <summary>Reviewed fingerprints that close the selected shared and station PLM artwork providers.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomPlmShotBlockVisualCatalog", "plm-shot-block-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockVisualCatalog.cs", "C0CBA15296CFC18DCED7A99B9474FA23BF5C8DCA65B71FD342686881FB369D51")]),
        new("SuperMetroid.Core.Rooms.RoomPlmStationVisualCatalog", "plm-station-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmStationVisualCatalog.cs", "D2A197C24AE77D2BEB8B8C66BAA5F5A22BD6622BBDB6280FBB3CE9DD499A9BFD"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmStationDrawDefinitions.cs", "18831FF54D50F0480FBB92F5157A0D206F185C10561CAD33BFEA1D7E729ADB70")]),
        new("SuperMetroid.Core.Rooms.RoomPlmBombTorizoHandVisualCatalog", "plm-torizo-hand-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmBombTorizoHandVisualCatalog.cs", "E01676321A8ACE64C952B6FBDD4D3EC12BC1FDCB76E12E9736326588050671A0"),
             new("csharp/src/SuperMetroid.Core/Rooms/BombTorizoHandPlmDrawDefinitions.cs", "2DC35D76BE3F276D4847BD5634ACDC5455D5469FA6CE492A3E9905CE4F2C2C2A")]),
        new("SuperMetroid.Core.Rooms.RoomPlmMotherBrainGlassVisualCatalog", "plm-mother-brain-glass-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmMotherBrainGlassVisualCatalog.cs", "C277F276DFD29C926E615E1B35EF664C93F8F361EFEFFD76EAD9B332441CBC0E"),
             new("csharp/src/SuperMetroid.Core/Rooms/MotherBrainGlassPlmDrawDefinitions.cs", "475D0FD043120234A45F5935BD1A7CB2816F7E1BCCCC811813DC122B889B8967")]),
        new("SuperMetroid.Core.Rooms.RoomPlmNoobTubeVisualCatalog", "plm-noob-tube-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmNoobTubeVisualCatalog.cs", "F91F5DA138778DBD4D4CCF853E6A78E9C2E3C0EFF1B5370BC9D11FD7BF2CDD73"),
             new("csharp/src/SuperMetroid.Core/Rooms/NoobTubePlmDrawDefinitions.cs", "F1F1B912F838A4FF484C592A9AC0A492F8966FC9481D606AE17ECC3A8A477916")]),
    ];
}

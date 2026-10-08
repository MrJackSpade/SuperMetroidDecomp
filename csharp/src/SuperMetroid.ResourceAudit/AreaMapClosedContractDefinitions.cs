namespace SuperMetroid.ResourceAudit;

/// <summary>The atomic seven-area install and its deliberately small VRAM source domain.</summary>
internal static class AreaMapClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.AreaMapPresentationCatalog", "map-atomic-seven-areas-and-owned-uploads", ["Get", "Resolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/AreaMapPresentationCatalog.cs", "8EF059B28CE319380BD93B60C88C32AFCA638FA3D4F6636A998F80DAEAD1FB55"),
             new("csharp/src/SuperMetroid.Core/Game/AreaId.cs", "6B88F8EE1B5ED42848833924AD9B5844B632D174C964D73E78D8E3774922E834"),
             new("csharp/src/SuperMetroid.Core/Assets/AreaMapPresentationAsset.cs", "C0EE04CDE0B62C1BA9DCAD29D4FF292429D0F2BA069FD862ABAC889A7361BBCC"),
             new("csharp/src/SuperMetroid.Core/Assets/AreaMapStockRules.cs", "1DD2CCD2C5DDC040198093F3C0921512EAA12E87DE06D4A27E51151D233E8762"),
             new("csharp/src/SuperMetroid.Core/Assets/HudTileAtlas.cs", "CF458EE39ACC8CB91CCC5C98D909A3E62DD5A09BD8F6FBEEF51BEE8C9664489A"),
             new("csharp/src/SuperMetroid.Core/Assets/MapTileAtlas.cs", "77C73DABEFEF89B1C47928C2138A40410D6C8852BC2CB526F0AA9F6A068F93C8"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs", "E2A5D6F4B5EF50E10D293A29679D4B64F3573FD140426DFB90A553ABBD67FE15"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerGlyphDefinitions.cs", "4942E6802A8F4B086F7C8D3F4A0FD3530E1C07B5C2F467D97CF76FE6855A1366"),
             new("csharp/src/SuperMetroid.Core/Game/KraidBackgroundRomData.cs", "9D5B72D902E311A9C7A434A10D9A24EDFB2E769B2D0FB53A0E0C67BC41354224"),
             new("csharp/src/SuperMetroid.Core/Hardware/IVramAssetProvider.cs", "73DCE6788B0BB9B1549CF04466AE14CB8AABA3AD9FCC2738E188ABBA2F2F8D44")]),
    ];
}

namespace SuperMetroid.ResourceAudit;

/// <summary>The atomic seven-area install and its deliberately small VRAM source domain.</summary>
internal static class AreaMapClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.AreaMapPresentationCatalog", "map-atomic-seven-areas-and-owned-uploads", ["Get", "Resolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/AreaMapPresentationCatalog.cs", "057212F5C89C8C6C72D0294C8CF4AC47189ED5FCFE50965A23E10D25430DA031"),
             new("csharp/src/SuperMetroid.Core/Game/AreaId.cs", "6B88F8EE1B5ED42848833924AD9B5844B632D174C964D73E78D8E3774922E834"),
             new("csharp/src/SuperMetroid.Core/Assets/AreaMapPresentationAsset.cs", "EE93C8816BEB13B34190D76915AD7C118DBBE6A2684867DFE2EBED01D4F49116"),
             new("csharp/src/SuperMetroid.Core/Assets/AreaMapStockRules.cs", "1DD2CCD2C5DDC040198093F3C0921512EAA12E87DE06D4A27E51151D233E8762"),
             new("csharp/src/SuperMetroid.Core/Assets/HudTileAtlas.cs", "CF458EE39ACC8CB91CCC5C98D909A3E62DD5A09BD8F6FBEEF51BEE8C9664489A"),
             new("csharp/src/SuperMetroid.Core/Assets/MapTileAtlas.cs", "77C73DABEFEF89B1C47928C2138A40410D6C8852BC2CB526F0AA9F6A068F93C8"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs", "4030EC86F2F18FB11FA9481298140D0D2B96F6E17DC45408FB81C48C8F9D6E7E"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerGlyphDefinitions.cs", "4942E6802A8F4B086F7C8D3F4A0FD3530E1C07B5C2F467D97CF76FE6855A1366"),
             new("csharp/src/SuperMetroid.Core/Game/KraidBackgroundRomData.cs", "9D5B72D902E311A9C7A434A10D9A24EDFB2E769B2D0FB53A0E0C67BC41354224"),
             new("csharp/src/SuperMetroid.Core/Hardware/IVramAssetProvider.cs", "D60B0DED6A14D23F5962FD93DA6AF5B48D83524D344A3ABB31548334EA2492E4")],
            "The sole private-constructor path validates and installs every one of the seven areas before publication; Get checks the typed area index. The same atomic loader installs complete HUD and escape-timer tile stores. Resolve owns only the standard HUD, four Kraid restoration quarters and two timer pages, not every VramAssetId. This proves resource presence in successfully loaded catalogs, not file availability, map centering/visibility, caller selections, instance binding, transfer placement or pixels."),
    ];
}

using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// The installation's sheets composed again in-process by the import's own composers (#1633), for the checks that compare an
/// installed picture with the cartridge's colours: an installed picture holds only its colours, so where each of its pixels
/// was composed from is known only by composing its sheet again from the installation's stock content and the cartridge.
/// Each family is composed once per process, when a check first needs a sheet of it. Enemy and spawned-object sheets are
/// not composed here: their composer composes a graphics set at a time, and a check that compares one with the cartridge
/// draws it from a set <see cref="EnemySheetFixtures"/> composes, whose sheets are recorded as they are composed.
/// </summary>
internal static class InstalledSheetCompositions
{
    /// <summary>The cartridge the sprite sheets' ending colours and the escape timer's sheet are read from.</summary>
    private static readonly Lazy<ISnesAddressSpace> Cartridge =
        new(() => CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    /// <summary>The sprite sheets' composer over the installation's stock content.</summary>
    private static readonly Lazy<SpriteSheetSourceCatalog> SpriteSheets = new(() =>
        new SpriteSheetSourceCatalog(CartridgeImportContent.Directory, EndingPaletteImport.Read(Cartridge.Value)));

    /// <summary>Each boss body sheet, composed by its installed name.</summary>
    private static readonly Lazy<Dictionary<string, ComposedSpriteSheet>> BossBodies = new(() => SharedFixtureTiming.Load("Fixture: boss body sheet compositions",
        () => BossBodySheetComposer.Compose(RoomBlockSheetInputs.Load(CartridgeImportContent.Directory))
            .ToDictionary(sheet => BossBodySheetComposer.SheetNameOf(sheet.Owner), sheet => sheet.Sheet, StringComparer.Ordinal)));

    /// <summary>Each room sheet (blocks, backgrounds, written and effect pictures, the HUD's pictures, message boxes, floors), composed by its installed name.</summary>
    private static readonly Lazy<Dictionary<string, ComposedSpriteSheet>> Rooms = new(() => SharedFixtureTiming.Load("Fixture: room sheet compositions", ComposeRooms));

    /// <summary>Every room sheet the installation holds, composed in-process by its installed name.</summary>
    private static Dictionary<string, ComposedSpriteSheet> ComposeRooms()
    {
        string content = CartridgeImportContent.Directory;
        RoomSheetComposition composition = RoomBlockSheetComposer.Compose(RoomBlockSheetInputs.Load(content), RoomBlockInventory.Load(content));
        var sheets = new Dictionary<string, ComposedSpriteSheet>(StringComparer.Ordinal);
        foreach ((TilesetDefinition tileset, ComposedSpriteSheet sheet) in composition.BlockSheets)
            sheets.Add(RoomBlockSheet.NameOf(tileset), sheet);
        foreach ((string name, ComposedRoomBackground background) in composition.Backgrounds)
            sheets.Add(name, background.Sheet);
        foreach (ComposedWrittenPictureSheet written in composition.Written.Values.Concat(composition.RoomEffects.Values))
            sheets.Add(WrittenPictureSheet.NameOf(written.Layer, written.Tileset), written.Sheet);
        foreach ((TilesetDefinition tileset, ComposedSpriteSheet sheet) in composition.Hud)
            sheets.Add(HudPictureSheet.NameOf(tileset), sheet);
        foreach ((TilesetDefinition tileset, ComposedSpriteSheet sheet) in composition.MessageBoxes)
            sheets.Add(MessageBoxSheet.NameOf(tileset), sheet);
        foreach ((TilesetDefinition tileset, ComposedSpriteSheet sheet) in composition.Floors)
            sheets.Add(RoomFloorSheet.NameOf(tileset), sheet);
        return sheets;
    }

    /// <summary>
    /// The compositions of every installed sheet named <paramref name="name"/>, in the families' order: sprite sheets, boss
    /// bodies, then the rooms' sheets. Two families may hold sheets of one name; the caller tells them apart by their pixels.
    /// </summary>
    internal static IEnumerable<ComposedSpriteSheet> Compose(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        foreach ((SpriteSheetId id, string fileName) in SpriteSheetDefinitions.FileNames)
        {
            if (string.Equals(fileName, name, StringComparison.Ordinal))
            {
                yield return id == SpriteSheetId.EscapeTimer ? EscapeTimerSheetComposer.Compose(Cartridge.Value) : SpriteSheets.Value.Compose(id);
                yield break;
            }
        }
        if (Enum.GetValues<BossBodyOwner>().Any(owner => string.Equals(BossBodySheetComposer.SheetNameOf(owner), name, StringComparison.Ordinal)))
        {
            yield return BossBodies.Value[name];
            yield break;
        }
        if (Rooms.Value.TryGetValue(name, out ComposedSpriteSheet? room))
            yield return room;
    }
}

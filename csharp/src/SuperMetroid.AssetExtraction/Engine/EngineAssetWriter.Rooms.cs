using System.Text.Json.Nodes;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.World.RoomStates;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// What belongs to the rooms themselves: the changes to a room's blocks that no boss makes, and the picture behind
/// each room that has one of its own.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The family of sheets the rooms' blocks and backgrounds are on.</summary>
    private const string RoomBlockFamily = GameInstallationLayout.RoomBlockSheetDirectoryName;

    /// <summary>Every state of every room that has a state <paramref name="chosen"/> picks.</summary>
    /// <exception cref="InvalidDataException">No room has such a state.</exception>
    private static List<(RoomDefinition Room, RoomStateSettings State)> StatesOfRoomsWith(Func<RoomStateSettings, bool> chosen, string what)
    {
        var rooms = new HashSet<RoomDefinition>();
        foreach ((RoomDefinition room, RoomStateSettings state) in EngineLayouts.States())
        {
            if (chosen(state))
                rooms.Add(room);
        }

        if (rooms.Count == 0)
            throw new InvalidDataException($"No room {what}.");
        return [.. EngineLayouts.States().Where(placed => rooms.Contains(placed.Room))];
    }

    /// <summary>
    /// Writes the patches of the rooms' own changes: the way the rescued animals leave by, in the room they leave
    /// through; the wall that is gone from the old Tourian shaft during the escape; and the wall that stands where
    /// the door of Phantoon's room is for as long as the fight lasts.
    /// </summary>
    private void WriteRoomPatches()
    {
        const string walls = GameInstallationLayout.RoomObjectRevealAndWallVisualDirectoryName;

        // A patch a room's scene names must be in the layout of every state of the room.
        foreach ((RoomDefinition room, RoomStateSettings state) in StatesOfRoomsWith(
            state => state.SetupBehavior == RoomSetupBehavior.OpenRescuedAnimalsPassageAndQuake, "opens a way for the rescued animals"))
        {
            Layouts.Patch(room, state, "animals_passage_open",
                new Position16<RoomBlocks>(CrateriaMainstreetEscapePassageObjectDefinitions.BlockX, CrateriaMainstreetEscapePassageObjectDefinitions.BlockY),
                Layouts.PatchCells(walls, (RoomObjectRevealAndWallVisualFiles.VisualId(CrateriaMainstreetEscapePassageObjectDefinitions.ClearPairDraw),
                    CrateriaMainstreetEscapePassageObjectDefinitions.OpenPassage)));
        }

        foreach ((RoomDefinition room, RoomStateSettings state) in StatesOfRoomsWith(
            state => state.SetupBehavior == RoomSetupBehavior.AutoDestroyWallDuringEscape, "loses a wall during the escape"))
        {
            Layouts.Patch(room, state, "escape_wall_destroyed",
                new Position16<RoomBlocks>(OldTourianEscapeShaftWallObjectDefinitions.BlockX, OldTourianEscapeShaftWallObjectDefinitions.BlockY),
                Layouts.PatchCells(walls, (RoomObjectRevealAndWallVisualFiles.VisualId(OldTourianEscapeShaftWallObjectDefinitions.OpenWallDraw),
                    OldTourianEscapeShaftWallObjectDefinitions.OpenedWall)));
        }

        // The entrance's treadmill: the door come in by starts its belt one way, and with the ship's power it carries.
        foreach ((RoomDefinition room, RoomStateSettings state) in EngineLayouts.States().Where(placed => placed.Room == RoomDefinitions.WreckedShipEntrance))
        {
            Layouts.PatchTreadmill(room, state, "treadmill_rightwards", WreckedShipTreadmillDirection.Rightwards);
            Layouts.PatchTreadmill(room, state, "treadmill_leftwards", WreckedShipTreadmillDirection.Leftwards);
        }

        // The old game makes the door's blocks a door that nothing opens; here they are wall, and the door's cap is put away.
        foreach ((RoomDefinition room, RoomStateSettings state) in EngineLayouts.States().Where(placed => placed.Room == RoomDefinitions.Phantoon))
        {
            // Until the door is drawn its top block is held solid, showing what it showed.
            Layouts.Patch(room, state, FightDoorHeldPatch, PhantoonDoorChangeDefinitions.Block,
                [Layouts.Kept(state, PhantoonDoorChangeDefinitions.Block, 0, 0, RoomCollisionType.SolidBlock)]);
            Layouts.Patch(room, state, FightDoorPatch, PhantoonDoorChangeDefinitions.Block,
                Layouts.PatchCells(GameInstallationLayout.RoomObjectPhantoonDoorVisualDirectoryName,
                    (PhantoonDoorDrawKeys.Installed.Of(PhantoonDoorDraw.FightDoor), PhantoonDoorObjectDrawDefinitions.Shapes[PhantoonDoorDraw.FightDoor]))
                    .Select(cell => cell with { Collision = RoomCollisionType.SolidBlock }));
        }
    }

    /// <summary>The patches of Phantoon's room that are his door walled for the fight, and its top block held solid until the door changes.</summary>
    private const string FightDoorPatch = "fight_door", FightDoorHeldPatch = "fight_door_held";

    /// <summary>The name of the folder a set of graphics' pictures are kept in, which a room's content puts for the set's place in a path.</summary>
    private static string GraphicsFolder(RoomStateSettings state) => EngineRoomObjects.GraphicsName(state.GraphicsSet.ToString());

    /// <summary>The path of the picture behind a room, as the room scenes name it, with the folder of the set of graphics it is coloured by.</summary>
    private static string BackdropPath(RoomStateSettings state, LibraryBackground background) =>
        $"backdrops/{GraphicsFolder(state)}/{EngineSheets.FileName(background.Name)}.png";

    /// <summary>
    /// The installed sheet that holds the picture behind one state of a room; null for a state with none of its own:
    /// one whose level draws it, one a boss draws on, or one whose floor turns.
    /// </summary>
    private static string? BackdropSheet(RoomStateSettings state)
    {
        if (state.Background is not { } background || state.TurningFloor != RoomTurningFloor.None || background == LibraryBackgroundDefinitions.KraidAlive)
            return null;
        TilesetDefinition tileset = RoomTilesets.Get(state.GraphicsSet);
        return background.View switch
        {
            LibraryBackgroundView.Picture => RoomBackgroundImage.NameOf(background, LibraryBackgroundPicture.Ordinary, tileset),
            LibraryBackgroundView.SkyRows => RoomBehaviorPictureDefinitions.MainSkies.TryGetValue(state.MainBehavior, out ScrollingSky sky)
                ? RoomBackgroundImage.SkyNameOf(sky, tileset)
                : throw new InvalidDataException($"The state {state.Name} shows the sky's rows but scrolls no sky."),
            LibraryBackgroundView.BossBody or LibraryBackgroundView.Nothing => null,
            _ => throw new InvalidDataException($"The background {background.Name} is of a kind this does not know: {background.View}."),
        };
    }

    /// <summary>
    /// Writes, for each set of graphics, the colours of its tiles a door's fade leaves lit: the cartridge fades every
    /// colour to black through a door but a handful, and a room's tiles drawn in those stay lit through the door's
    /// scroll, the doors' frames among them. A door between two rooms that both show their tiles through it keeps a
    /// few more. A colour lit in one region of the tiles' pictures and faded in another is written with the region.
    /// </summary>
    private void WriteDoorLitColours()
    {
        static string Written(int colour, int region)
        {
            string hex = colour.ToString("x6", System.Globalization.CultureInfo.InvariantCulture);
            return region == ShadeSwap.Everywhere ? hex : $"{hex}/{region}";
        }

        foreach (RoomGraphicsSet set in EngineLayouts.States().Select(placed => placed.State.GraphicsSet).Distinct())
        {
            string sheet = set.ToString();
            JsonArray Lit(SuperMetroid.Core.Assets.PictureShade dark) =>
                new([.. _sheets.ShadeKeeps(RoomBlockFamily, sheet, dark.Key).Select(kept => (JsonNode)Written(kept.Colour, kept.Region))]);
            _tree.WriteData($"tiles/{EngineRoomObjects.GraphicsName(sheet)}/door_lit.json", new JsonObject
            {
                ["lit"] = Lit(SuperMetroid.Core.World.DoorFadeShades.BackgroundDark),
                ["lit_between_common_rooms"] = Lit(SuperMetroid.Core.World.DoorFadeShades.BackgroundDarkBetweenCommonRooms),
            });
        }
    }

    /// <summary>The file of the bands a scrolling sky drifts in, which every room with such a sky shares.</summary>
    internal const string SkyBandsFile = "backdrops/scrolling_sky_bands.json";

    /// <summary>Writes the bands of the scrolling sky: from the top of the room down, the row each begins at and how far it drifts an update.</summary>
    private void WriteSkyBands()
    {
        var bands = new JsonArray();
        foreach (SuperMetroid.Core.Fx.SkyBand band in SuperMetroid.Core.Fx.ScrollingSkyBandDefinitions.Bands)
            bands.Add(new JsonObject { ["from"] = band.TopRow, ["drift"] = EngineNames.Number(band.Drift.Speed) });
        _tree.WriteData(SkyBandsFile, new JsonObject { ["bands"] = bands, ["to"] = SuperMetroid.Core.Fx.ScrollingSkyBandDefinitions.EndRow });
    }

    /// <summary>Writes the picture behind every room that has one of its own, once for each set of graphics it is coloured by.</summary>
    private void WriteBackdrops()
    {
        WriteSkyBands();
        foreach ((RoomDefinition _, RoomStateSettings state) in EngineLayouts.States())
        {
            if (BackdropSheet(state) is not { } sheet)
                continue;
            (RgbaPixel[] pixels, int width, int height) = _sheets.FrameShown(RoomBlockFamily, sheet, RoomBackgroundImage.FrameName);
            _tree.WritePicture(BackdropPath(state, state.Background!), width, height, pixels);
            if (_sheets.HasRegions(RoomBlockFamily, sheet))
                _tree.WriteRegionMap(BackdropPath(state, state.Background!), width, height, _sheets.FrameRegions(RoomBlockFamily, sheet, RoomBackgroundImage.FrameName));
        }
    }
}

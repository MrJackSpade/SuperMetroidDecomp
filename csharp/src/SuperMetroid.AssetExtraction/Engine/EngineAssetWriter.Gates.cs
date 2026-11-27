using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The gates that slide down from their housings, and the switches beside them. A gate is a barrier that is only ever
/// told to open and shut; its switch is a barrier that never opens and tells it each time the right shot lands.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>Writes the gate and each colour and side of switch.</summary>
    private void WriteGates()
    {
        const string directory = GameInstallationLayout.RoomObjectDownwardGateVisualDirectoryName, folder = "objects/gate";
        const int block = EngineRoomObjects.Block;

        // The old game draws the housing with the room's blocks and the column with a sprite that slides a pixel an
        // update, behind the part of the housing that is drawn in front. Here each update of the gate's way is one
        // picture of both: the housing's blocks as that update has them, and the column where it has slid to.
        const int way = (DownwardGateDefinitions.GateHeightInBlocks - 1) * block;
        if (DownwardGateDefinitions.ClosedPositionOffsetPixels != way)
            throw new InvalidDataException("A closed gate's column does not lie its whole way down, which the Godot gate's pictures take it to.");
        static string FrameOf(DownwardGateImage image) =>
            (image.Draw is RoomObjectFrameDraw<DownwardGateDraw> framed
                ? framed.Frame
                : throw new InvalidDataException("A gate shows something that is not one of its own frames.")) switch
            {
                DownwardGateDraw.Open => "column-open",
                DownwardGateDraw.QuarterLowered => "column-quarter-lowered",
                DownwardGateDraw.HalfLowered => "column-half-lowered",
                DownwardGateDraw.ThreeQuartersLowered => "column-three-quarters-lowered",
                DownwardGateDraw.FullyLowered => "column-fully-lowered",
                DownwardGateDraw.Closed => "column-closed",
                DownwardGateDraw draw => throw new InvalidDataException($"A gate shows {draw}, which is not a look of its column."),
            };

        // The gate with its column let down so many pixels, over the housing as the image has it. The old column is
        // a picture a block taller for each block it has passed, placed by its lowest block: so its top lies within
        // the housing's own block, and its foot that many pixels below the housing. As a block is passed the taller
        // picture is taken on the way down at once, and on the way up only once the next block is entered; shut, it
        // is the tallest, wholly below the housing.
        string Lowered(DownwardGateImage image, int pixels, bool rising)
        {
            string frame = FrameOf(image);
            if (pixels == 0)
                return RoomObjects.Picture(directory, frame, folder, name: $"{frame}-00", rows: DownwardGateDefinitions.GateHeightInBlocks);
            bool atBlock = pixels % block == 0;
            int blocks = atBlock && (rising || pixels == way) ? pixels / block : pixels / block + 1;
            int top = atBlock && (rising || pixels == way) ? block : pixels % block;
            (RgbaPixel[] column, int width, int height) = _sheets.SpawnedFrame(EngineSheets.SharedSpawner, EnemySheetDefinitions.ProjectileFrameName(blocks switch
            {
                1 => EnemyProjectileFrameId.DownwardGateOneBlockTall,
                2 => EnemyProjectileFrameId.DownwardGateTwoBlocksTall,
                3 => EnemyProjectileFrameId.DownwardGateThreeBlocksTall,
                4 => EnemyProjectileFrameId.DownwardGateFourBlocksTall,
                _ => throw new InvalidDataException($"A gate's column let down {pixels} pixels would be {blocks} blocks tall, and it has no such picture."),
            }));
            if (height != blocks * block)
                throw new InvalidDataException($"The picture of a gate's column {blocks} blocks tall is {height} pixels tall.");
            return RoomObjects.Picture(
                directory, frame, folder, name: $"{frame}-{pixels:00}", over: (column, width, height, 0, top), rows: DownwardGateDefinitions.GateHeightInBlocks);
        }

        // How far down from its top an image's blocks are solid: the old gate's blocks are filled a row at each image,
        // its housing's one row always, whatever the column's sprite shows between.
        static int SolidHeight(DownwardGateImage image)
        {
            DownwardGateDraw draw = image.Draw is RoomObjectFrameDraw<DownwardGateDraw> framed
                ? framed.Frame
                : throw new InvalidDataException("A gate shows something that is not one of its own frames.");
            int rows = DownwardGateObjectDraws.Describe(draw).Blocks.Cells.Count(cell => cell.Collision != SuperMetroid.Core.Rooms.LevelData.RoomCollisionType.Air);
            if (rows == 0 || DownwardGateObjectDraws.Describe(draw).Blocks.Cells.Take(rows).Any(cell => cell.Collision == SuperMetroid.Core.Rooms.LevelData.RoomCollisionType.Air))
                throw new InvalidDataException($"A gate's {draw} is not solid from its housing down without a gap, which the Godot gate's one solid part is.");
            return rows * block;
        }

        static JsonArray Standing(DownwardGateImage image) =>
            ["blocking", string.Create(System.Globalization.CultureInfo.InvariantCulture, $"solid_height:{SolidHeight(image)}")];

        // A movement, an update a step: the images of the housing in turn, each for its own updates, and the column a
        // pixel further each update from the image that starts it moving, until its way is done. The first step of
        // each image says how far down the gate is solid from then.
        JsonObject Moving(IReadOnlyList<DownwardGateImage> images, DownwardGateMovement movement)
        {
            bool rising = movement == DownwardGateMovement.Rising;
            var steps = new JsonArray();
            int? moved = null;
            foreach (DownwardGateImage image in images)
            {
                if (image.Starts == movement)
                    moved = 0;
                for (int update = 0; update < image.Updates; update++)
                {
                    if (moved is { } so && so < way)
                        moved = so + 1;
                    int lowered = moved is { } far ? rising ? way - far : far : rising ? way : 0;
                    string picture = Lowered(image, lowered, rising);
                    if (update > 0 && steps[^1]!["picture"]!.GetValue<string>() == picture)
                        steps[^1]!["updates"] = steps[^1]!["updates"]!.GetValue<int>() + 1;
                    else
                        steps.Add(new JsonObject { ["picture"] = picture, ["updates"] = 1 });
                    if (update == 0)
                        steps[^1]!["cues"] = Standing(image);
                }
            }

            if (moved != way)
                throw new InvalidDataException($"A gate's {movement} moves its column {moved} pixels of the {way} of its way.");
            return new JsonObject { ["steps"] = steps };
        }

        JsonObject Resting(DownwardGateImage image, int lowered) => new()
        {
            ["steps"] = new JsonArray(new JsonObject { ["picture"] = Lowered(image, lowered, rising: false), ["updates"] = 1, ["cues"] = Standing(image) }),
            ["loops_to"] = 0,
        };

        // Nothing wears a gate down: only its switch moves it, and, shut, Samus inside its column, for whom the old
        // gate rises as for its switch. It starts shut. With no hit taking anything off it, it never shows itself
        // hurt, so it has no look for that.
        new EngineObjectFile(_tree, "objects/gate.json", "Barrier")
            .Value("health", 1)
            .Value("any_hit", 0)
            .Value("opens_around_player", true)
            .Value("open_sound", EngineNames.Sound(DownwardGateDefinitions.MovementSound))
            .Value("close_sound", EngineNames.Sound(DownwardGateDefinitions.MovementSound))
            // Its housing is of the room's blocks, and lies a row up with the room's tiles; its column, a sprite, is
            // laid a row lower among them, where its place puts it.
            .Group("picture_offset", EngineRoomObjects.PictureCorner(0, 0))
            .Collider("Solid", 0, 0, block, DownwardGateDefinitions.GateHeightInBlocks * block)
            .Timeline("closed", Resting(DownwardGateMovementDefinitions.Closed, way))
            .Timeline("opening", Moving(DownwardGateMovementDefinitions.Reopening, DownwardGateMovement.Rising))
            .Timeline("open", Resting(DownwardGateMovementDefinitions.Open, 0))
            .Timeline("closing", Moving(DownwardGateMovementDefinitions.Closing, DownwardGateMovement.Lowering))
            .Write();

        // A switch is the one block beside the gate's housing, on its left or its right. The old switch looks the same
        // however often it is struck: it only tells its gate and is ready again, so it has no look for opening or open.
        foreach ((DownwardGateTrigger trigger, DownwardGateTriggerDefinition rules) in DownwardGateTriggerDefinitions.Triggers)
        {
            string colour = trigger.Color.ToString().ToLowerInvariant(), side = EngineNames.Side(trigger.Side);
            string frame = $"{colour}-{side}-trigger";
            static bool BesideTheHousing(int across, int down) => across != 0;
            (int left, int top, int columns, int rows) = RoomObjects.Bounds(directory, frame, BesideTheHousing);
            var look = new JsonObject
            {
                ["steps"] = new JsonArray(new JsonObject { ["picture"] = RoomObjects.Picture(directory, frame, folder, BesideTheHousing), ["updates"] = 1 }),
                ["loops_to"] = 0,
                // It is of the room's blocks, and lies a row up with the room's tiles.
                ["offset"] = EngineRoomObjects.PictureCorner(left * block, top * block),
            };
            var setOffBy = new JsonObject();
            foreach ((SamusProjectileFamily family, string kind) in new[]
                     {
                         (SamusProjectileFamily.Beam, "beam"), (SamusProjectileFamily.Bomb, "bomb"), (SamusProjectileFamily.Missile, "missile"),
                         (SamusProjectileFamily.PowerBomb, "power_bomb"), (SamusProjectileFamily.SuperMissile, "super_missile"),
                     })
            {
                if (rules.SetOffBy.Accepts(family))
                    setOffBy[kind] = 1;
            }

            EngineObjectFile file = new EngineObjectFile(_tree, $"objects/gate_switch_{colour}_{side}.json", "Barrier")
                .Value("health", 1)
                .Value("any_hit", 0)
                .Group("damaged_by", setOffBy)
                .Collider("Solid", left * block, top * block, columns * block, rows * block)
                .Collider("Hurtbox", left * block, top * block, columns * block, rows * block)
                .Timeline("closed", look);
            if (rules.RejectionSound is { } rejected)
                file.Value("refused_sound", EngineNames.Sound(rejected));
            file.Write();
        }
    }
}

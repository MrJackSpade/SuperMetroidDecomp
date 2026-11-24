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

        // The old game draws the housing with the room's blocks and the column with a sprite that slides a pixel at a
        // time. Here each of the gate's steps is one picture of both: the housing, and under it as much of the column
        // as that step has let down, so the gate moves a block at a step.
        string GatePicture(DownwardGateImage image)
        {
            DownwardGateDraw draw = image.Draw is RoomObjectFrameDraw<DownwardGateDraw> framed
                ? framed.Frame
                : throw new InvalidDataException("A gate shows something that is not one of its own frames.");
            (string frame, int columnBlocks) = draw switch
            {
                DownwardGateDraw.Open => ("column-open", 0),
                DownwardGateDraw.QuarterLowered => ("column-quarter-lowered", 1),
                DownwardGateDraw.HalfLowered => ("column-half-lowered", 2),
                DownwardGateDraw.ThreeQuartersLowered => ("column-three-quarters-lowered", 3),
                DownwardGateDraw.FullyLowered => ("column-fully-lowered", 4),
                DownwardGateDraw.Closed => ("column-closed", 4),
                _ => throw new InvalidDataException($"A gate shows {draw}, which is not a look of its column."),
            };
            (RgbaPixel[] Pixels, int Width, int Height, int Left, int Top)? column = null;
            if (columnBlocks > 0)
            {
                (RgbaPixel[] pixels, int width, int height) = _sheets.SpawnedFrame(EngineSheets.SharedSpawner, EnemySheetDefinitions.ProjectileFrameName(columnBlocks switch
                {
                    1 => EnemyProjectileFrameId.DownwardGateOneBlockTall,
                    2 => EnemyProjectileFrameId.DownwardGateTwoBlocksTall,
                    3 => EnemyProjectileFrameId.DownwardGateThreeBlocksTall,
                    _ => EnemyProjectileFrameId.DownwardGateFourBlocksTall,
                }));
                column = (pixels, width, height, 0, block);
            }

            return RoomObjects.Picture(directory, frame, folder, over: column, rows: DownwardGateDefinitions.GateHeightInBlocks);
        }

        JsonObject Moving(Animation<DownwardGateSegment> movement)
        {
            (List<IAnimationBeat<DownwardGateImage, NoAnimationCue>> lead, List<IAnimationBeat<DownwardGateImage, NoAnimationCue>>? cycle) =
                EngineTimelines.Played(DownwardGateAnimations.Segments, movement);
            return EngineTimelines.Of(lead, cycle, GatePicture, static (NoAnimationCue _) => null);
        }

        JsonObject Resting(DownwardGateImage image) =>
            EngineTimelines.Of<DownwardGateImage, NoAnimationCue>([new AnimationBeat<DownwardGateImage, NoAnimationCue>.HeldFrame(image)], null, GatePicture, static _ => null);

        // Nothing wears a gate down: only its switch moves it. It starts shut.
        new EngineObjectFile(_tree, "objects/gate.json", "Barrier")
            .Value("health", 1)
            .Value("any_hit", 0)
            .Value("open_sound", EngineNames.Sound(DownwardGateDefinitions.MovementSound))
            .Value("close_sound", EngineNames.Sound(DownwardGateDefinitions.MovementSound))
            .Collider("Solid", 0, 0, block, DownwardGateDefinitions.GateHeightInBlocks * block)
            .Timeline("closed", Resting(DownwardGateMovementDefinitions.Closed))
            .Timeline("opening", Moving(DownwardGateAnimations.Reopening))
            .Timeline("open", Resting(DownwardGateMovementDefinitions.Open))
            .Timeline("closing", Moving(DownwardGateAnimations.Closing))
            .Write();

        // A switch is the one block beside the gate's housing, on its left or its right.
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
                ["offset"] = new JsonObject { ["x"] = left * block, ["y"] = top * block },
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

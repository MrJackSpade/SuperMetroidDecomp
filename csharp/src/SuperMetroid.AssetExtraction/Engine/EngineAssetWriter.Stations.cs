using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.SaveStation;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The stations: how each looks at rest, at work and once it has nothing more to do, the arm that reaches out to
/// Samus's cannon, and the electricity of a save. A station's pictures are of the room's own blocks, so each is
/// written for every set of graphics.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>Writes every station's timelines.</summary>
    private void WriteStations()
    {
        const string directory = GameInstallationLayout.RoomObjectStationVisualDirectoryName, folder = "objects/station";
        const int block = EngineRoomObjects.Block;
        static string Frame(StationDraw draw) => EngineSheets.FileName(draw.ToString()).Replace('_', '-');
        static JsonObject At(int x, int y) => new() { ["x"] = x, ["y"] = y };

        // A station's display, placed from the station's own top-left corner. The Godot game's station stands where
        // Samus uses it from: one she runs against at the block of its left arm, in the arms' row; one she stands in
        // half a block right of the pad's left edge, three blocks above the pad. A display's cells are counted from
        // the block the old game places the station on, which is the pad's left block or lies -leftArm blocks right
        // of the left arm.
        JsonObject Display(StationAnimation animation, int stationX, int stationY)
        {
            (List<IAnimationBeat<StationDraw, NoAnimationCue>> lead, List<IAnimationBeat<StationDraw, NoAnimationCue>>? cycle) =
                EngineTimelines.Played(StationAnimationDefinitions.Segments, StationAnimationDefinitions.Animations[animation]);
            StationDraw first = lead.Concat(cycle ?? []).Select(beat => beat.ShownFrame).First(frame => frame.HasValue)!.Value;
            (int left, int top, int columns, int rows) = RoomObjects.Bounds(directory, Frame(first));
            foreach (StationDraw other in lead.Concat(cycle ?? []).Select(beat => beat.ShownFrame).Where(frame => frame.HasValue).Select(frame => frame!.Value))
            {
                if (RoomObjects.Bounds(directory, Frame(other)) != (left, top, columns, rows))
                    throw new InvalidDataException($"The station frames {first} and {other} of one animation do not cover the same blocks.");
            }

            JsonObject timeline = EngineTimelines.Of(lead, cycle, draw => RoomObjects.Picture(directory, Frame(draw), folder), static (NoAnimationCue _) => null);
            timeline["offset"] = At(stationX + left * block, stationY + top * block);
            return timeline;
        }

        // The save station: Samus stands in it.
        const int savePadX = -block / 2, savePadY = 3 * block;
        _tree.WriteData("objects/save_station_rest.json", Display(StationAnimation.SaveIdle, savePadX, savePadY));
        _tree.WriteData("objects/save_station_working.json", Display(StationAnimation.SaveFlash, savePadX, savePadY));

        // Its electricity: a band that sweeps down the pod over and over while the save is made. The old game places
        // it from the pad's block and moves it a step each update.
        {
            (List<IAnimationBeat<Offset16<RoomPixels>, EnemyProjectileCommonCue>> lead, List<IAnimationBeat<Offset16<RoomPixels>, EnemyProjectileCommonCue>>? cycle) =
                EngineTimelines.Played(SaveStationElectricityAnimations.Segments, SaveStationElectricityAnimations.Discharge);
            if (cycle is not null)
                throw new InvalidDataException("The save station's electricity plays over and over, which an effect that ends cannot.");
            string band = EnemySheetDefinitions.ProjectileFrameName(EnemyProjectileFrameId.SaveStationElectricityBand);
            EngineSheetPictures pictures = _sheets.CutSpawned(_tree, "objects/station/electricity", EngineSheets.SharedSpawner, [band]);
            Offset16<RoomBlocks> from = SaveStationElectricityDefinitions.BlocksFromTrigger;
            var steps = new JsonArray();
            foreach (IAnimationBeat<Offset16<RoomPixels>, EnemyProjectileCommonCue> beat in lead)
            {
                switch (beat)
                {
                    case AnimationBeat<Offset16<RoomPixels>, EnemyProjectileCommonCue>.Frame frame:
                        steps.Add(new JsonObject
                        {
                            ["picture"] = pictures.Of(band),
                            ["updates"] = frame.Duration,
                            ["offset"] = At(frame.Shown.X, frame.Shown.Y),
                        });
                        break;
                    case AnimationBeat<Offset16<RoomPixels>, EnemyProjectileCommonCue>.Event { Raised: EnemyProjectileCommonCue.Signal { Kind: EnemyProjectileCommonCueKind.Delete } }:
                        // The effect is gone when its timeline ends.
                        break;
                    default:
                        throw new InvalidDataException($"The save station's electricity has a beat this does not know: {beat.GetType().Name}.");
                }
            }

            _tree.WriteData("objects/save_station_electricity.json", new JsonObject
            {
                ["steps"] = steps,
                ["offset"] = At(savePadX + from.X * block + pictures.OffsetX, savePadY + from.Y * block + pictures.OffsetY),
            });
        }

        // The stations Samus runs against, each with an arm on either side.
        foreach ((StationKind kind, StationAccessRule rule) in StationAccessRuleDefinitions.Rules)
        {
            string name = EngineSheets.FileName(kind.ToString()) + "_station";
            int stationX = -rule.LeftOffset * block;
            // Its display runs on unbroken while its arm is at work, so it has no look of its own for that.
            _tree.WriteData($"objects/{name}_rest.json", Display(rule.Animation, stationX, 0));
            if (rule.AcquiredAnimation is { } acquired)
                _tree.WriteData($"objects/{name}_done.json", Display(acquired, stationX, 0));

            foreach (HorizontalDirection side in new[] { HorizontalDirection.Left, HorizontalDirection.Right })
            {
                StationAccessObjectDefinition arm = StationAccessObjectDefinitions.Arms[new StationAccess(kind, side)];

                // An arm's frame may also draw the other side's arm at rest, as the room's own blocks already do;
                // only the arm's own block is the effect's.
                static bool OwnBlock(int across, int down) => (across, down) == (0, 0);
                string Pose(StationAccessPose pose) => RoomObjects.Picture(
                    directory, Frame(pose == StationAccessPose.Extended ? arm.Extended : arm.Retracted), folder, OwnBlock,
                    name: $"{Frame(pose == StationAccessPose.Extended ? arm.Extended : arm.Retracted)}-arm");
                JsonObject timeline = EngineTimelines.Of(
                    StationAccessTimingDefinitions.Segments, StationAccessTimingDefinitions.Use, Pose,
                    static (StationAccessCue cue) => cue == StationAccessCue.Disengages ? EngineNames.SoundCue(EnemySound.StationDisengaged) : null);
                timeline["offset"] = At((rule.ColumnsToAccessOn(side) - rule.LeftOffset) * block, 0);
                _tree.WriteData($"objects/{name}_arm_{EngineNames.Side(side)}.json", timeline);
            }
        }
    }
}

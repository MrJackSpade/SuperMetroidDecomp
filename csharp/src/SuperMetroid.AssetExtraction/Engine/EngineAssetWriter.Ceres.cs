using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Enemies.Artwork;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>What the space station's rooms show of their own: the floors that turn, and the pieces of ceiling that fall.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The path of one of the pictures shown on a turning floor, with the set of graphics it is coloured by.</summary>
    private static string CeresFloorPicture(string graphics, CeresFloorActorPicture picture) =>
        $"backdrops/{graphics}/ceres_{EngineSheets.FileName(picture.ToString())}.png";

    /// <summary>
    /// Writes the floor of each room whose floor turns, and the pictures shown on it (the elevator's pad, the capsule
    /// Ridley carries and his wings), once for each set of graphics they are coloured by, where the room scenes look
    /// for them.
    /// </summary>
    private void WriteCeresFloors()
    {
        var written = new HashSet<string>(StringComparer.Ordinal);
        foreach ((RoomDefinition _, RoomStateSettings state) in EngineLayouts.States())
        {
            string? name = state.TurningFloor switch
            {
                RoomTurningFloor.None => null,
                RoomTurningFloor.ElevatorShaft => "ceres_elevator_shaft",
                RoomTurningFloor.RidleyGetaway => "ceres_ridley_floor",
                _ => throw new InvalidDataException($"The state {state.Name} has a turning floor this does not know: {state.TurningFloor}."),
            };
            string path = $"backdrops/{GraphicsFolder(state)}/{name}.png";
            if (name is null || !written.Add(path))
                continue;
            string sheet = RoomFloorSheet.NameOf(RoomTilesets.Get(state.GraphicsSet));
            (RgbaPixel[] pixels, int width, int height) = _sheets.FrameShown(RoomBlockFamily, sheet, RoomFloorSheet.FloorFrameName);
            _tree.WritePicture(path, width, height, pixels);
            foreach (CeresFloorActorPicture picture in Enum.GetValues<CeresFloorActorPicture>())
            {
                string shown = CeresFloorPicture(GraphicsFolder(state), picture);
                if (!written.Add(shown))
                    continue;
                (RgbaPixel[] own, int ownWidth, int ownHeight) = _sheets.FrameShown(RoomBlockFamily, sheet, RoomFloorSheet.PictureNameOf(picture));
                _tree.WritePicture(shown, ownWidth, ownHeight, own);
            }
        }
    }

    /// <summary>
    /// Writes the looks of what sets Samus down in the station: the pad under her feet, which flickers, and the
    /// platform she is lowered to.
    /// </summary>
    private void WriteCeresArrival()
    {
        foreach ((CeresElevatorArrivalSegment segment, string name) in new[]
                 {
                     (CeresElevatorArrivalSegment.Pad, "pad"), (CeresElevatorArrivalSegment.Platform, "platform"),
                 })
        {
            (List<IAnimationBeat<CeresElevatorArrivalPose, NoAnimationCue>> lead, List<IAnimationBeat<CeresElevatorArrivalPose, NoAnimationCue>>? cycle) =
                EngineTimelines.Played(CeresElevatorArrivalAnimations.Segments, CeresElevatorArrivalAnimations.StartingWith(segment));
            string file = $"objects/ceres_arrival_{name}.json";
            IEnumerable<string> frames = lead.Concat(cycle ?? []).Select(beat => beat.ShownFrame).Where(frame => frame.HasValue)
                .Select(frame => CeresElevatorArrivalPictures.SheetName(frame!.Value)).Distinct(StringComparer.Ordinal);
            EngineSheetPictures pictures = _sheets.CutSpawnedShades(
                _tree, file[..file.LastIndexOf('.')], EngineSheets.SharedSpawner, frames, CeresElevatorShades.ConcealerCleared.Key);
            JsonObject timeline = EngineTimelines.Of(lead, cycle, pose => pictures.Of(CeresElevatorArrivalPictures.SheetName(pose)), static (NoAnimationCue _) => null);
            timeline["offset"] = new JsonObject { ["x"] = pictures.OffsetX, ["y"] = pictures.OffsetY };
            _tree.WriteData(file, timeline);
        }
    }

    /// <summary>
    /// Writes the two pieces of ceiling that fall in the hall before the station's elevator: each a shot that falls
    /// faster and faster and breaks into dust where it lands.
    /// </summary>
    private void WriteCeresDebris()
    {
        EnemyProjectileDefinition old = EnemyProjectileDefinitionCatalog.Definitions[RoomEnemyProjectileKind.CeresFallingDebris];
        foreach ((CeresFallingDebrisLook look, string name) in new[] { (CeresFallingDebrisLook.Light, "light"), (CeresFallingDebrisLook.Dark, "dark") })
        {
            string file = $"effects/ceres_debris_{name}.json";
            string frame = EnemySheetDefinitions.ProjectileFrameName(CeresFallingDebrisVisualDefinitions.ShownProjectileFrames[look]);
            EngineSheetPictures pictures = _sheets.CutSpawnedShades(_tree, file[..file.LastIndexOf('.')], EngineSheets.SharedSpawner, [frame], CeresDoorShades.Escaping.Key);
            _tree.WriteData(file, new JsonObject
            {
                ["damage"] = old.Damage,
                // The old piece gains speed before it first moves; the Godot one after. So it is launched with the
                // first gain already in its speed.
                ["speed"] = EngineNames.Number(CeresFallingDebrisDefinitions.StartingFallSpeed + CeresFallingDebrisDefinitions.FallAcceleration),
                ["projectile"] = new JsonObject
                {
                    ["width"] = old.Radius.X * 2,
                    ["height"] = old.Radius.Y * 2,
                    ["meeting_target"] = old.Properties.HasFlag(EnemyProjectilePropertyFlags.PersistsOnContact) ? "passes_through_and_rehits" : "ends",
                    ["met_by_shot"] = "ignored",
                    ["view_margin"] = -1,
                    ["standing"] = EngineEnemy.StandingOf(old),

                    // A piece starts with its top in the hall's ceiling. It meets the room's blocks as the cartridge's
                    // enemy shots do: by the side it leads with, where each move ends.
                    ["terrain_by"] = "leading_side",
                    ["motion"] = "falling",
                    ["acceleration"] = EngineNames.Number(CeresFallingDebrisDefinitions.FallAcceleration),
                    ["picture"] = pictures.Of(frame),
                    ["picture_offset"] = new JsonObject { ["x"] = pictures.OffsetX, ["y"] = pictures.OffsetY },
                    ["ending_effect"] = DustFile(DustExplosionAnimation.SmallDustCloud),
                    // The old piece is deleted and its dust is then one more of the things the enemies launch.
                    ["ending_effect_holds_place"] = true,
                    ["ending_sound"] = EngineNames.Sound(EnemySound.CeresTileFall),
                },
            });
        }
    }
}

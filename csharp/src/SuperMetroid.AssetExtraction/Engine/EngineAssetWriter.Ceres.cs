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
    /// <summary>
    /// Writes the floor of each room whose floor turns, once for each set of graphics it is coloured by, where the room
    /// scenes look for it.
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
            (RgbaPixel[] pixels, int width, int height) =
                _sheets.FrameShown(RoomBlockFamily, RoomFloorSheet.NameOf(RoomTilesets.Get(state.GraphicsSet)), RoomFloorSheet.FloorFrameName);
            _tree.WritePicture(path, width, height, pixels);
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
                ["speed"] = EngineNames.Number(CeresFallingDebrisDefinitions.StartingFallSpeed),
                ["projectile"] = new JsonObject
                {
                    ["width"] = old.Radius.X * 2,
                    ["height"] = old.Radius.Y * 2,
                    ["meeting_target"] = old.Properties.HasFlag(EnemyProjectilePropertyFlags.PersistsOnContact) ? "passes_through_and_rehits" : "ends",
                    ["met_by_shot"] = "ignored",
                    ["view_margin"] = -1,
                    ["motion"] = "falling",
                    ["acceleration"] = EngineNames.Number(CeresFallingDebrisDefinitions.FallAcceleration),
                    ["picture"] = pictures.Of(frame),
                    ["picture_offset"] = new JsonObject { ["x"] = pictures.OffsetX, ["y"] = pictures.OffsetY },
                    ["ending_effect"] = DustFile(DustExplosionAnimation.SmallDustCloud),
                    ["ending_sound"] = EngineNames.Sound(EnemySound.CeresTileFall),
                },
            });
        }
    }
}

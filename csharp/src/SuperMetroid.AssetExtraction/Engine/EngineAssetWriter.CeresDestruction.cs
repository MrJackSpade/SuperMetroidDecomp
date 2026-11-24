using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>Ceres blowing up, and the gunship's flight down to Zebes.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>Writes the destruction of Ceres: its data file and everything it names.</summary>
    private void WriteCeresDestruction()
    {
        const string folder = "cinematics/ceres_destruction";
        IntroCinematicArtwork artwork = _installation.LoadIntroCinematic();
        var timelines = new JsonObject();
        var places = new JsonObject();

        // What drifts by the station.
        foreach (CeresDestructionActorRole role in CeresDestructionActorDefinitions.DestructionSpawnOrder)
        {
            string name = Named(role);
            CeresDestructionActorDefinition seen = CeresDestructionActors.InitialActor(role);
            timelines[name] = CeresTimeline(seen.Animation, seen.Colouring, SpriteSheetId.CeresDestructionSprites, $"{folder}/{name}");
            places[name] = At(artwork.CeresDestruction.DestructionActors[role].X, artwork.CeresDestruction.DestructionActors[role].Y);
        }

        // The blasts that break out over it.
        (string Name, CeresAnimation Animation)[] blasts =
        [
            ("small_explosion", CeresExplosionDefinitions.InitialActor.Animation),
            ("repeating_small_explosion", CeresExplosionDefinitions.RepeatingActor.Animation),
            ("large_explosion", CeresExplosionDefinitions.FinalWaveActor.Animation),
            ("station_blast", CeresExplosionDefinitions.StationBlastActor.Animation),
        ];
        foreach ((string name, CeresAnimation animation) in blasts)
            timelines[name] = CeresTimeline(animation, CeresSpriteColouring.Explosions, SpriteSheetId.CeresDestructionSprites, $"{folder}/{name}");

        // Zebes, its stars and its name.
        (string Name, ZebesRevealActorRole Role)[] revealed =
        [
            ("planet", ZebesRevealActorRole.Planet), ("stars_upper_left", ZebesRevealActorRole.StarsUpperLeft),
            ("stars_upper_right", ZebesRevealActorRole.StarsUpperRight), ("stars_lower_left", ZebesRevealActorRole.StarsLowerLeft),
            ("stars_lower_right", ZebesRevealActorRole.StarsLowerRight), ("title", ZebesRevealActorRole.Title),
            ("second_title", ZebesRevealActorRole.JapaneseSubtitle),
        ];
        foreach ((string name, ZebesRevealActorRole role) in revealed)
        {
            CeresDestructionActorDefinition seen = CeresDestructionActors.ZebesActor(role);
            timelines[name] = CeresTimeline(seen.Animation, seen.Colouring, SpriteSheetId.ZebesRevealSprites, $"{folder}/{name}", static cue => cue switch
            {
                CeresCue.FadeInZebesTitle => "fade_in",
                CeresCue.SpawnJapaneseTitleIfNeeded => "second_title",
                CeresCue.FadeOutZebesTitle => "fade_out",
                CeresCue.StartFlyingToZebes => "fly",
                _ => throw new InvalidDataException($"The name of Zebes raises {cue}, which this does not know."),
            });
            places[name] = At(artwork.CeresDestruction.RevealActors[role].X, artwork.CeresDestruction.RevealActors[role].Y);
        }

        string Whole(SpriteSheetId sheet, string frame, string name) => _sheets.WriteSprite(_tree, sheet, frame, $"{folder}/{name}.png");
        _tree.WriteData("cinematics/ceres_destruction.json", new JsonObject
        {
            ["backdrop"] = Colour(artwork.CeresFlight.Backdrop),
            ["station_floor"] = Whole(SpriteSheetId.CeresFloor, CeresFloors.FrameName(CeresFloorShot.Station), "station_floor"),
            ["explosion_floor"] = Whole(SpriteSheetId.CeresFloor, CeresFloors.FrameName(CeresFloorShot.Explosion), "explosion_floor"),
            ["approach_floor"] = Whole(SpriteSheetId.CeresFloor, CeresFloors.FrameName(CeresFloorShot.ZebesApproach), "approach_floor"),
            ["zebes"] = Whole(SpriteSheetId.CeresCinematicPictures, CeresCinematicPictures.ZebesPlanet, "zebes"),
            ["blast_sound"] = EngineNames.Sound(SamusSound.PowerBombExplosion),
            ["second_title_languages"] = new JsonArray(Languages[1]),
            ["places"] = places,
            ["timelines"] = timelines,
        });
    }
}

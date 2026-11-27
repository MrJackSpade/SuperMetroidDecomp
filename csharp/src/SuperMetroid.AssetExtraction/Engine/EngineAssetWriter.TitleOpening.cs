using System.Text.Json.Nodes;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// How the title comes up: four cards of words typed a letter at a time, each followed by a move across the backdrop
/// seen from close to, the last by the backdrop drawing back to its own size; then the title fades in over it, and
/// the copyright under that. The larva in its tube on the backdrop pulses throughout.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The opening of the title, for the title's data file; its pictures go under <paramref name="folder"/>.</summary>
    private JsonObject TitleOpening(string folder)
    {
        // A title sprite drawn at a place, by where its corner then lies.
        (int X, int Y) Corner(string frame, Offset16<ScreenPixels> origin)
        {
            (int _, int _, int originX, int originY) = _sheets.SpriteSize(SpriteSheetId.Title, frame);
            return (origin.X - originX, origin.Y - originY);
        }

        var steps = new JsonArray();
        foreach (TitleTextSequenceDefinition sequence in TitleSequenceDefinitions.TextSequences.All)
        {
            (_, TitleCardDefinition card, Offset16<ScreenPixels> origin, TitleSpriteColouring colouring) = sequence;
            var letters = new JsonArray();
            foreach (TitleCardTyping typing in TitleCards.Typings(card))
            {
                string frame = TitleSpriteDefinitions.LetterFrameName(typing);

                // A space is a letter that shows nothing: it still takes its turn to be typed.
                if (_sheets.SpriteIsBlank(SpriteSheetId.Title, frame))
                {
                    letters.Add(new JsonObject { ["picture"] = "", ["x"] = 0, ["y"] = 0 });
                    continue;
                }

                (int x, int y) = Corner(frame, origin);
                letters.Add(new JsonObject
                {
                    ["picture"] = _sheets.WriteSprite(
                        _tree, SpriteSheetId.Title, frame, $"{folder}/{Named(card.Card)}_{typing.Letters}.png", TitleShades.Drawn(colouring).Key),
                    ["x"] = x,
                    ["y"] = y,
                });
            }

            steps.Add(new JsonObject
            {
                ["card"] = new JsonObject { ["opening_updates"] = card.OpeningFrames, ["hold_updates"] = card.HoldFrames, ["letters"] = letters },
            });
            steps.Add(card.Ending switch
            {
                TitleCardCue.StartLowerPan => Pan(TitleSequenceDefinitions.Scenes.LowerPan, -1, 0, "x_below", TitleSequenceDefinitions.Scenes.LowerPanEndX),
                TitleCardCue.StartUpperPan => Pan(TitleSequenceDefinitions.Scenes.UpperPan, -1, 0, "x_below", TitleSequenceDefinitions.Scenes.UpperPanEndX),
                TitleCardCue.StartDownwardPan => Pan(TitleSequenceDefinitions.Scenes.DownwardPan, 0, 1, "y_at_least", TitleSequenceDefinitions.Scenes.DownwardPanEndY),
                TitleCardCue.StartFinalZoom => new JsonObject
                {
                    ["zoom"] = Scene(TitleSequenceDefinitions.Scenes.FinalZoom, new JsonObject
                    {
                        // The backdrop draws back by the finest step there is, on every so many updates.
                        ["grows_by"] = FloorZoom.OwnSize.FloorPixelsPerScreenPixel - FloorZoom.OwnSize.Narrowed(FloorZoomStep.Finest).FloorPixelsPerScreenPixel,
                        ["every"] = TitleSequenceDefinitions.Scenes.FinalZoomCadence.Period,
                    }),
                },
                _ => throw new InvalidDataException($"A card of the title ends by {card.Ending}, which this does not know."),
            });
        }

        // A picture that fades in: one picture, as its sheet has it, whose colours come up a step at a time and then
        // stay. Each step is the old fade's shade laid over the colouring the title draws it in; the last is what it
        // rests in.
        JsonObject Fading(string frame, Offset16<ScreenPixels> origin, TitleSpriteColouring colouring, ColourEffectId effect, string name)
        {
            string picture = _sheets.WriteSpriteWithRegions(_tree, SpriteSheetId.Title, frame, $"{folder}/{name}.png");
            var places = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal) { [picture] = [EngineSheets.SheetOf(SpriteSheetId.Title)] };
            (int x, int y) = Corner(frame, origin);
            return new JsonObject
            {
                ["x"] = x,
                ["y"] = y,
                ["picture"] = picture,
                ["fade"] = ColourSteps(effect, shade => ShadeChange($"{TitleShades.Drawn(colouring).Key}+{shade}", EngineSheets.SpriteFamily, places)),
            };
        }

        return new JsonObject
        {
            ["music"] = MusicName(TitleSequenceDefinitions.Music.OpeningTrack),
            ["letter_updates"] = TitleCardDefinitions.LetterFrames,
            // How far the letters already typed move as each further one is.
            ["letter_scroll"] = At(TitleCardDefinitions.TypedScroll.X, TitleCardDefinitions.TypedScroll.Y),
            ["steps"] = steps,
            ["logo"] = Fading(
                "Title.SuperMetroidLogo", TitleSequenceDefinitions.Sprites.LogoOrigin, TitleSequenceDefinitions.Sprites.LogoColouring,
                TitleSequenceDefinitions.Fades.Logo, "logo"),
            ["logo_hold"] = TitleSequenceDefinitions.Timing.LogoHoldFrames,
            ["copyright"] = Fading(
                "Title.NintendoCopyright", TitleSequenceDefinitions.Sprites.CopyrightOrigin, TitleSequenceDefinitions.Sprites.CopyrightColouring,
                TitleSequenceDefinitions.Fades.Copyright, "copyright"),
            ["copyright_hold"] = TitleSequenceDefinitions.Timing.CopyrightHoldFrames,
            // The screen's brightness has sixteen levels: skipping the opening takes them some at a time, leaving the
            // title one every so many updates.
            ["skip_fade_updates"] = 16 / TitleSequenceDefinitions.Timing.SkipFadeBrightnessStep,
            ["leave_fade_updates"] = 16 * TitleSequenceDefinitions.Timing.TitleFadeUpdatesPerStep,
        };

        static JsonObject Scene(TitleFloorSceneDefinition scene, JsonObject more)
        {
            more["floor_per_pixel"] = scene.Zoom.FloorZoom.FloorPixelsPerScreenPixel;
            more["x"] = scene.Position.X;
            more["y"] = scene.Position.Y;
            return more;
        }

        static JsonObject Pan(TitleFloorSceneDefinition scene, int across, int down, string ends, int at) => new()
        {
            ["pan"] = Scene(scene, new JsonObject
            {
                ["move_x"] = (double)(across * TitleSequenceDefinitions.Scenes.PanVelocity.Pixels),
                ["move_y"] = (double)(down * TitleSequenceDefinitions.Scenes.PanVelocity.Pixels),
                ["ends"] = ends,
                ["at"] = at,
            }),
        };
    }

    /// <summary>The larva in its tube on the title's backdrop: where it lies on the backdrop, and its pulsing.</summary>
    private JsonObject TitleBaby(string folder)
    {
        var looks = new JsonArray();
        (int _, int _, int originX, int originY) = _sheets.SpriteSize(SpriteSheetId.TitleFloor, TitleFloors.BabyFrameName(TitleBabyPose.Closed));
        foreach (TitleBabyPose pose in TitleSequenceDefinitions.Graphics.BabyAnimation)
        {
            string frame = TitleFloors.BabyFrameName(pose);
            if (_sheets.SpriteSize(SpriteSheetId.TitleFloor, frame) is var (_, _, x, y) && (x != originX || y != originY))
                throw new InvalidDataException("The larva's pictures on the title's backdrop do not all lie alike.");
            looks.Add(new JsonObject
            {
                ["picture"] = _sheets.WriteSprite(_tree, SpriteSheetId.TitleFloor, frame, $"{folder}/baby_{Named(pose)}.png"),
                ["updates"] = TitleSequenceDefinitions.Timing.BabyFrameDuration,
            });
        }

        return new JsonObject { ["x"] = -originX, ["y"] = -originY, ["timeline"] = new JsonObject { ["steps"] = looks, ["loops_to"] = 0 } };
    }
}

using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Colours;
using G = SuperMetroid.Core.Samus.Colours.SamusGlowSegment;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// Samus's colours: every colouring the old game gives her as a change of the colours of her one set of pictures, and
/// every glow as the colourings it shows in turn with the updates each shows for.
/// </summary>
/// <remarks>
/// <code>
/// {
///   "starting": "suit_starting_0",
///   "suits": { "power": "suit_power_0", "varia": "suit_varia_0", "gravity": "suit_gravity_0" },
///   "hurt": "hurt_hurt_0",
///   "drain_flash": { "power": "speed_booster_power_3" },
///   "glows": { "stored_shine_power": { "steps": [ { "shade": "stored_shine_power_0", "updates": 1 } ], "loops_from": 0 } },
///   "shades": { "suit_power_0": { "samus/body": { "5a5a00": "734200" }, "samus/death": { "5a5a00": "734200" } } }
/// }
/// </code>
/// A shade is named for the old colouring and its step. It lists, for the pictures of each place, the colours it
/// changes as <see cref="ShadeChange"/> writes them. A glow that "loops_from" a step goes back to it after its last,
/// and one that does not ends after its last.
/// </remarks>
public sealed partial class EngineAssetWriter
{
    /// <summary>The folder her body's pictures are written under.</summary>
    private const string SamusBodyPlace = "samus/body";

    /// <summary>The folder the pictures of her suit breaking up are written under.</summary>
    private const string SamusDeathPlace = "samus/death";

    /// <summary>The name her suit's and her beams' colourings in the opening's flashbacks are written under.</summary>
    private const string OpeningColours = "opening";

    /// <summary>Writes her colours.</summary>
    /// <exception cref="InvalidDataException">A glow has a beat that shows no colouring.</exception>
    private void WriteSamusColours()
    {
        string deathSheet = SpriteSheetDefinitions.FileNames[SpriteSheetId.SamusDeath];
        string grappleSheet = SpriteSheetDefinitions.FileNames[SpriteSheetId.Grapple];
        var shades = new JsonObject();

        // The name a colouring is written under; it is written the first time it is named.
        string Shade(PictureShade shade)
        {
            string name = $"{EngineSheets.FileName(shade.Effect)}_{shade.Step}";
            if (shades.ContainsKey(name))
                return name;
            var places = new Dictionary<string, IReadOnlyList<string>> { [SamusBodyPlace] = [SamusBodySheet] };
            if (_sheets.HasShade(SpriteFamily, deathSheet, shade.Key))
                places[SamusDeathPlace] = [deathSheet];

            // The grapple beam's own pictures are hers too, and take her colourings where their sheet has them.
            if (_sheets.HasShade(SpriteFamily, grappleSheet, shade.Key))
                places[SamusGrapplePlace] = [grappleSheet];
            shades[name] = ShadeChange(shade.Key, SpriteFamily, places);
            return name;
        }

        string SuitName(SamusSuit suit) => SamusSuits.Single(named => named.Suit == suit).Name;
        var glows = new SortedDictionary<string, JsonObject>(StringComparer.Ordinal);
        foreach ((G glow, Animation<G> animation) in SamusGlowAnimationDefinitions.Animations)
        {
            (List<IAnimationBeat<PictureShade, NoAnimationCue>> lead, List<IAnimationBeat<PictureShade, NoAnimationCue>>? cycle) =
                EngineTimelines.Played(SamusGlowAnimationDefinitions.Segments, animation);
            var steps = new JsonArray();
            foreach (IAnimationBeat<PictureShade, NoAnimationCue> beat in lead.Concat(cycle ?? []))
            {
                if (beat is not AnimationBeat<PictureShade, NoAnimationCue>.Frame shown)
                    throw new InvalidDataException($"Samus's glow {glow} has a beat this does not know: {beat.GetType().Name}.");
                steps.Add(new JsonObject { ["shade"] = Shade(shown.Shown), ["updates"] = shown.Duration });
            }

            var written = new JsonObject { ["steps"] = steps };
            if (cycle is not null)
                written["loops_from"] = lead.Count;
            glows.Add(EngineSheets.FileName(glow.Glow.ToString()) + (glow.Suit is { } suit ? "_" + SuitName(suit) : ""), written);
        }

        var suits = new JsonObject();
        var drainFlash = new JsonObject();
        foreach ((string name, SamusSuit suit) in SamusSuits)
        {
            suits[name] = Shade(SamusShades.Suit(suit));
            drainFlash[name] = Shade(SamusShades.FullBodyCycle(SamusFullBodyCycleShades.SpeedBoostShade(suit)));
        }

        // The opening's flashbacks show her suit and her beams in the opening's own colours, whatever she wears.
        suits[OpeningColours] = Shade(SamusShades.IntroSuit);

        // Her beams' colours, for each set of beams she can have switched on, named as the old game names the set.
        var beams = new JsonObject();
        foreach (SuperMetroid.Core.Progression.SamusBeamCombination combination in Enum.GetValues<SuperMetroid.Core.Progression.SamusBeamCombination>())
            beams[EngineSheets.FileName(combination.ToString())] = Shade(SamusShades.Beam(combination));
        beams[OpeningColours] = Shade(SamusShades.IntroBeams);

        // Her death's colourings, step by step: her suit's in each suit, those of her body without the suit, and those
        // her beams' colours whiten through. Each changes the pictures of whichever of her sheets has it.
        string DeathShade(PictureShade shade)
        {
            string name = $"{EngineSheets.FileName(shade.Effect)}_{shade.Step}";
            if (shades.ContainsKey(name))
                return name;
            var places = new Dictionary<string, IReadOnlyList<string>>();
            if (_sheets.HasShade(SpriteFamily, SamusBodySheet, shade.Key))
                places[SamusBodyPlace] = [SamusBodySheet];
            if (_sheets.HasShade(SpriteFamily, deathSheet, shade.Key))
                places[SamusDeathPlace] = [deathSheet];
            if (places.Count == 0)
                throw new InvalidDataException($"Neither of Samus's sheets has the shade {shade.Key} of her death.");
            shades[name] = ShadeChange(shade.Key, SpriteFamily, places);
            return name;
        }

        var deathSuits = new JsonObject();
        foreach ((string name, SamusSuit suit) in SamusSuits)
        {
            var steps = new JsonArray();
            foreach (SuperMetroid.Core.Samus.Death.SamusDeathShade step in SuperMetroid.Core.Samus.Death.SamusDeathShade.All)
                steps.Add(DeathShade(SamusShades.DeathSuit(suit, step)));
            deathSuits[name] = steps;
        }

        var deathSuitless = new JsonArray();
        foreach (SuperMetroid.Core.Samus.Death.SamusDeathShade step in SuperMetroid.Core.Samus.Death.SamusDeathShade.All)
            deathSuitless.Add(DeathShade(SamusShades.DeathSuitless(step)));
        var deathWhiteout = new JsonArray();
        foreach (SuperMetroid.Core.Samus.Death.SamusWhiteoutShade step in SuperMetroid.Core.Samus.Death.SamusWhiteoutShade.All)
            deathWhiteout.Add(DeathShade(SamusShades.DeathWhiteout(step)));

        var glowsWritten = new JsonObject();
        foreach ((string name, JsonObject glow) in glows)
            glowsWritten[name] = glow;
        _tree.WriteData("samus/colours.json", new JsonObject
        {
            ["starting"] = Shade(SamusShades.StartingSuit),
            ["starting_beams"] = Shade(SamusShades.StartingBeams),
            ["beams"] = beams,
            ["suits"] = suits,
            ["hurt"] = Shade(SamusShades.Hurt(SamusHurtColorVariant.Hurt)),
            ["drain_flash"] = drainFlash,
            ["death"] = new JsonObject { ["suits"] = deathSuits, ["suitless"] = deathSuitless, ["whiteout"] = deathWhiteout },
            ["glows"] = glowsWritten,
            ["shades"] = shades,
        });
    }
}

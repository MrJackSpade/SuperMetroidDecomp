using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The caps of doors. Each is a barrier of the Godot game: a column or a row of four blocks that shows itself shut,
/// flashing, opening and open by the old game's own steps, and that the right kinds of hit wear down.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The folder every cap's pictures are written under.</summary>
    private const string DoorCapFolder = "objects/door_cap";

    /// <summary>The kinds of hit that open a blue cap: every weapon, and the grapple beam's touch.</summary>
    private static readonly string[] OpensBlueCaps = ["beam", "missile", "super_missile", "bomb", "power_bomb", "grapple"];

    /// <summary>Writes every door cap: blue, yellow, green, red, grey and the one behind an eye, each way it can face.</summary>
    private void WriteDoorCaps()
    {
        foreach (ColoredDoorOrientation way in Enum.GetValues<ColoredDoorOrientation>())
        {
            string side = way.ToString().ToLowerInvariant();

            EngineObjectFile blue = Cap($"objects/door_cap_blue_{side}.json", way, 1, OpensBlueCaps.ToDictionary(kind => kind, _ => 1));
            BlueLooks(blue, way, "");
            blue.Write();

            foreach (ColoredDoorColor colour in Enum.GetValues<ColoredDoorColor>())
            {
                // The old game has no red cap that faces up or down, and the Godot project asks for none.
                if (colour == ColoredDoorColor.Red && way is ColoredDoorOrientation.Up or ColoredDoorOrientation.Down)
                    continue;
                ColoredDoorColorDefinition rules = ColoredDoorDefinitions.Colors[colour];
                ColoredDoorSequences looks = ColoredDoorDefinitions.Sequences[new ColoredDoorVariant(colour, way)];
                string own = colour.ToString().ToLowerInvariant();
                // Each shot it counts takes one off; a super missile, where it is counted, opens it at once.
                var wornBy = rules.CountedShots.ToDictionary(Kind, family => family == SamusProjectileFamily.SuperMissile ? rules.HitsToOpen : 1);
                EngineObjectFile coloured = Cap($"objects/door_cap_{own}_{side}.json", way, rules.HitsToOpen, wornBy)
                    .Timeline("closed", CapHeld(own, way, FirstShown(looks.Waiting)))
                    .Timeline("hurt", CapTimeline(own, way, looks.Flash, untilCleared: false))
                    .Timeline("opening", CapTimeline(own, way, looks.Opening, untilCleared: true))
                    .Timeline("open", CapOpen(way))
                    .Timeline("closing", CapTimeline(own, way, DoorCapSequenceDefinitions.CapClosing, untilCleared: false));
                if (colour == ColoredDoorColor.Red)
                    coloured.Value("hurt_sound", EngineNames.Sound(DoorCapSequenceDefinitions.RedHitSound));
                // Once opened and remembered it comes back as a blue cap.
                BlueLooks(coloured, way, "plain_");
                coloured.Group("plain_opened_by", new JsonArray([.. OpensBlueCaps.Select(kind => (JsonNode)kind)])).Write();
            }

            // A grey cap is held shut until its condition holds; then it flashes until a shot opens it.
            EngineObjectFile grey = Cap($"objects/door_cap_grey_{side}.json", way, 1, OpensBlueCaps.ToDictionary(kind => kind, _ => 1))
                .Value("sealed_sound", EngineNames.Sound(EnemySound.DudShot))
                .Timeline("sealed", CapHeld("grey", way, FirstShown(GreyDoorDefinitions.Locked)))
                .Timeline("sealing", CapTimeline("grey", way, DoorCapSequenceDefinitions.CapClosing, untilCleared: false))
                .Timeline("closed", CapTimeline("grey", way, GreyDoorDefinitions.Flash, untilCleared: false))
                .Timeline("opening", CapTimeline("grey", way, GreyDoorDefinitions.Opening, untilCleared: true))
                .Timeline("open", CapOpen(way))
                .Timeline("closing", CapTimeline("grey", way, DoorCapSequenceDefinitions.CapClosing, untilCleared: false));
            BlueLooks(grey, way, "plain_");
            grey.Group("plain_opened_by", new JsonArray([.. OpensBlueCaps.Select(kind => (JsonNode)kind)])).Write();

            // The cap behind an eye: while the eye lives it shows nothing of its own and only stands in the way; with
            // the eye gone it is a blue cap.
            if (way is ColoredDoorOrientation.Left or ColoredDoorOrientation.Right)
            {
                EngineObjectFile eye = Cap($"objects/door_cap_eye_{side}.json", way, 1, OpensBlueCaps.ToDictionary(kind => kind, _ => 1))
                    .Timeline("sealed", new JsonObject { ["steps"] = new JsonArray(new JsonObject { ["picture"] = "", ["updates"] = 1 }), ["loops_to"] = 0 });
                BlueLooks(eye, way, "");
                eye.Write();
            }
        }
    }

    /// <summary>Starts a cap's data file: its size, what wears it down and how much, and the sounds of every cap.</summary>
    private EngineObjectFile Cap(string file, ColoredDoorOrientation way, int health, Dictionary<string, int> wornBy)
    {
        bool column = way is ColoredDoorOrientation.Left or ColoredDoorOrientation.Right;
        int width = (column ? 1 : EngineLayouts.DoorCapBlocks) * EngineRoomObjects.Block, height = (column ? EngineLayouts.DoorCapBlocks : 1) * EngineRoomObjects.Block;
        var damagedBy = new JsonObject();
        foreach ((string kind, int amount) in wornBy.OrderBy(worn => worn.Key, StringComparer.Ordinal))
            damagedBy[kind] = amount;
        return new EngineObjectFile(_tree, file, "Barrier")
            .Value("health", health)
            .Value("any_hit", 0)
            .Value("open_sound", EngineNames.Sound(DoorCapSequenceDefinitions.OpenSound))
            .Value("close_sound", EngineNames.Sound(DoorCapSequenceDefinitions.CloseSound))
            .Group("damaged_by", damagedBy)
            // Its pictures are of the room's blocks, and lie a row up with the room's tiles.
            .Group("picture_offset", EngineRoomObjects.PictureCorner(0, 0))
            .Collider("Solid", 0, 0, width, height)
            .Collider("Hurtbox", 0, 0, width, height);
    }

    /// <summary>Gives a cap the looks of a blue one, each under its name with <paramref name="before"/> put first.</summary>
    private void BlueLooks(EngineObjectFile cap, ColoredDoorOrientation way, string before) => cap
        .Timeline(before + "closed", CapHeld("blue", way, FirstShown(DoorCapSequenceDefinitions.BlueClosed)))
        .Timeline(before + "opening", CapTimeline("blue", way, DoorCapSequenceDefinitions.BlueOpening, untilCleared: true))
        .Timeline(before + "open", CapOpen(way))
        .Timeline(before + "closing", CapTimeline("blue", way, DoorCapSequenceDefinitions.BlueClosing, untilCleared: false));

    /// <summary>The kind of hit one of the old game's shots is, as a cap's data names it.</summary>
    private static string Kind(SamusProjectileFamily family) => family switch
    {
        SamusProjectileFamily.Missile => "missile",
        SamusProjectileFamily.SuperMissile => "super_missile",
        SamusProjectileFamily.PowerBomb => "power_bomb",
        _ => throw new InvalidDataException($"A door counts shots of the family {family}, which this does not know."),
    };

    /// <summary>What the first frame of one of the old game's cap sequences shows.</summary>
    private static DoorCapShown FirstShown(Animation<DoorCapSegment> sequence) =>
        EngineTimelines.Played(DoorCapSequenceDefinitions.Segments, sequence).Lead.Select(beat => beat.ShownFrame).First(shown => shown.HasValue)!.Value;

    /// <summary>The timeline of a cap that rests on one look.</summary>
    private JsonObject CapHeld(string own, ColoredDoorOrientation way, DoorCapShown shown) =>
        EngineTimelines.Of<DoorCapShown, SoundEffectId>([new AnimationBeat<DoorCapShown, SoundEffectId>.HeldFrame(shown)], null,
            look => CapPicture(own, way, look), static _ => null);

    /// <summary>The timeline of an open cap: the frame of the doorway with nothing in it.</summary>
    private JsonObject CapOpen(ColoredDoorOrientation way) => CapHeld("grey", way, new DoorCapShown(DoorCapLook.Cleared, DoorCapShownPaint.Grey));

    /// <summary>
    /// The timeline of one of the old game's cap sequences. Its sounds are the barrier's own to ask for, so they are
    /// left out. An opening is cut off where the old sequence clears the doorway: from there the cap is open, and
    /// the Godot game stops it blocking as its opening ends.
    /// </summary>
    private JsonObject CapTimeline(string own, ColoredDoorOrientation way, Animation<DoorCapSegment> sequence, bool untilCleared)
    {
        (List<IAnimationBeat<DoorCapShown, SoundEffectId>> lead, List<IAnimationBeat<DoorCapShown, SoundEffectId>>? cycle) =
            EngineTimelines.Played(DoorCapSequenceDefinitions.Segments, sequence);
        if (untilCleared)
        {
            if (cycle is not null)
                throw new InvalidDataException("A cap's opening never ends.");
            lead = [.. lead.TakeWhile(beat => beat.ShownFrame is not { Look: DoorCapLook.Cleared })];
        }

        JsonObject timeline = EngineTimelines.Of<DoorCapShown, SoundEffectId>(lead, cycle, look => CapPicture(own, way, look), static _ => null);

        // The old cap's collision goes with the look it is drawn in: nearly open, many caps are solid only at their two
        // end blocks. A timeline that passes through such a look says, step by step, how the cap stands.
        var middleClear = new Dictionary<string, bool>();
        foreach (DoorCapShown shown in lead.Concat(cycle ?? []).Select(beat => beat.ShownFrame).OfType<DoorCapShown>())
        {
            if (shown.Look != DoorCapLook.Cleared)
                middleClear[CapPicture(own, way, shown)] = HasClearMiddle(own, way, shown);
        }

        if (middleClear.ContainsValue(true))
        {
            foreach (JsonNode? node in timeline["steps"]!.AsArray())
            {
                JsonObject step = node!.AsObject();
                if (!middleClear.TryGetValue(step["picture"]!.GetValue<string>(), out bool ends))
                    continue;
                if (step["cues"] is not JsonArray cues)
                    step["cues"] = cues = [];
                cues.Add(ends ? $"solid_ends:{CapEndPixels}" : "solid_whole");
            }
        }

        return timeline;
    }

    /// <summary>How many pixels of each end of a nearly open cap are solid: one block of its four.</summary>
    private const int CapEndPixels = 16;

    /// <summary>
    /// Whether the old game's draw of a cap in this look leaves its two middle blocks clear, by the table of the draw
    /// that paints it: a look painted blue or grey is the blue or the grey cap's draw, whatever cap it is.
    /// </summary>
    private static bool HasClearMiddle(string own, ColoredDoorOrientation way, DoorCapShown shown)
    {
        string painted = shown.Paint switch
        {
            DoorCapShownPaint.Own => own,
            DoorCapShownPaint.Blue => "blue",
            DoorCapShownPaint.Grey => "grey",
            _ => throw new InvalidDataException($"A cap is shown in the paint {shown.Paint}, which this does not know."),
        };
        IReadOnlyList<RoomCollisionType>? collision = painted switch
        {
            "blue" => BlueDoorObjectDrawDefinitions.Collisions.GetValueOrDefault(new BlueDoorCaps.Draw(way, shown.Look)),
            "grey" => GreyDoorObjectDrawDefinitions.Collisions.GetValueOrDefault(new GreyDoorCaps.Draw(way, shown.Look)),
            _ => ColoredDoorObjectDrawDefinitions.Collisions.GetValueOrDefault(
                new ColoredDoorCaps.Draw(Enum.Parse<ColoredDoorColor>(painted, ignoreCase: true), way, shown.Look)),
        };
        if (collision is null)
            throw new InvalidDataException($"The old game has no collision for a {painted} cap facing {way} in the look {shown.Look}.");
        return collision.SequenceEqual(DoorCapCollisionDefinitions.MiddleClearCap);
    }

    /// <summary>
    /// The picture of a cap in one of its looks. A look is in the cap's own colour, or in blue for the flash of a
    /// hit, or in grey, which is also the colour the empty doorway is kept in.
    /// </summary>
    private string CapPicture(string own, ColoredDoorOrientation way, DoorCapShown shown)
    {
        string side = way.ToString().ToLowerInvariant();
        string colour = shown.Paint switch
        {
            DoorCapShownPaint.Own => own,
            DoorCapShownPaint.Blue => "blue",
            DoorCapShownPaint.Grey => "grey",
            _ => throw new InvalidDataException($"A cap is shown in the paint {shown.Paint}, which this does not know."),
        };
        if (shown.Look == DoorCapLook.Cleared)
        {
            return colour == "grey"
                ? RoomObjects.Picture(GameInstallationLayout.RoomObjectGreyDoorVisualDirectoryName, $"clear-{side}", DoorCapFolder)
                : throw new InvalidDataException($"A cleared doorway is shown in {colour}, where the installation keeps it only in grey.");
        }

        string look = shown.Look switch
        {
            DoorCapLook.Closed or DoorCapLook.ClosedSolid => "closed",
            DoorCapLook.Ajar => "ajar",
            DoorCapLook.HalfOpen => "half-open",
            DoorCapLook.NearlyOpen => "nearly-open",
            _ => throw new InvalidDataException($"A cap is shown in the look {shown.Look}, which this does not know."),
        };
        return colour switch
        {
            "blue" => RoomObjects.Picture(GameInstallationLayout.RoomObjectBlueDoorVisualDirectoryName, $"{side}-{look}", DoorCapFolder),
            "grey" => RoomObjects.Picture(GameInstallationLayout.RoomObjectGreyDoorVisualDirectoryName, $"grey-{side}-{look}", DoorCapFolder),
            _ => RoomObjects.Picture(GameInstallationLayout.RoomObjectColoredDoorVisualDirectoryName, $"{colour}-{side}-{look}", DoorCapFolder),
        };
    }
}

using System.Globalization;
using System.Text.Json.Nodes;
using SuperMetroid.Core;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>The names and numbers the Godot game's data is written in.</summary>
internal static partial class EngineNames
{
    /// <summary>
    /// The name the Godot game asks for one of the old game's sounds by: which of the three sound libraries it is in,
    /// and its name there, in lower case with underscores. A request that only stops a sound has no name.
    /// </summary>
    internal static string? Sound(SoundEffectId sound) => sound switch
    {
        SoundEffectId.OfSamus samus => "samus_" + EngineSheets.FileName(samus.Sound.ToString()),
        SoundEffectId.OfEnemies enemies => "enemy_" + EngineSheets.FileName(enemies.Sound.ToString()),
        SoundEffectId.OfRoom room => "room_" + EngineSheets.FileName(room.Sound.ToString()),
        _ => null,
    };

    /// <summary>
    /// The name of one of the old game's requests that only stops sounds, or none for a sound. Such a request is asked
    /// for like a sound and waits its turn in its library like one: "stop" after its library for the one that stops
    /// everything the library is sounding, and what it stops after that for the others.
    /// </summary>
    internal static string? Stop(SoundEffectId sound) => sound switch
    {
        SoundEffectId.OfSamusStop samus => StopName("samus", samus.Stop == SamusSoundStop.Any, samus.Stop.ToString()),
        SoundEffectId.OfEnemiesStop enemies => StopName("enemy", enemies.Stop == EnemySoundStop.Any, enemies.Stop.ToString()),
        SoundEffectId.OfRoomStop room => StopName("room", room.Stop == RoomSoundStop.Any, room.Stop.ToString()),
        _ => null,
    };

    private static string StopName(string library, bool any, string what) =>
        any ? library + "_stop" : library + "_stop_" + EngineSheets.FileName(what);

    /// <summary>The name of whatever the old game can ask its sound queues for: a sound, or a request that only stops sounds.</summary>
    internal static string SoundOrStop(SoundEffectId sound) => Sound(sound) ?? Stop(sound)
        ?? throw new InvalidDataException($"The request {sound} is neither a sound nor a stop of sounds.");

    /// <summary>The cue that asks for one of the old game's sounds, or for one of its stops of sounds.</summary>
    internal static string SoundCue(SoundEffectId sound) => "sound:" + SoundOrStop(sound);

    /// <summary>
    /// The name of a sound, or of a stop of sounds, as it is asked for with <paramref name="priority"/>: where that
    /// is not the usual one the name says it after a colon, which the Godot game reads as the request's priority.
    /// </summary>
    internal static string Sound(SoundEffectId sound, SuperMetroid.Core.Compatibility.SoundRequestPriority priority) =>
        SoundOrStop(sound) + priority.Requests switch
        {
            1 => ":when_idle",
            3 => ":low",
            6 => "",
            9 => ":high",
            15 => ":essential",
            _ => throw new InvalidDataException($"The sound {sound} is asked for with a priority this does not know: {priority}."),
        };

    /// <summary>The name of an old request's sound, saying its priority where that is not the usual one.</summary>
    internal static string Sound(SoundRequest request) => Sound(request.SoundEffect, request.Priority);

    /// <summary>The cue that asks for a sound with <paramref name="priority"/>.</summary>
    internal static string SoundCue(SoundEffectId sound, SuperMetroid.Core.Compatibility.SoundRequestPriority priority) => "sound:" + Sound(sound, priority);

    /// <summary>The cue that makes an enemy go on behaving however far from the view it is.</summary>
    internal const string AlwaysActive = "always_active";

    /// <summary>The cue that ends <see cref="AlwaysActive"/>.</summary>
    internal const string NotAlwaysActive = "not_always_active";

    /// <summary>A side, as the Godot game's cues name it.</summary>
    internal static string Side(HorizontalDirection direction) => direction == HorizontalDirection.Left ? "left" : "right";

    /// <summary>A distance or speed as a plain number of pixels, as the Godot game's data writes one.</summary>
    internal static JsonNode Number(SignedSixteenSixteen value) => JsonValue.Create(value.Pixels);

    /// <summary>A distance or speed as a plain number of pixels.</summary>
    internal static JsonNode Number(SignedEightEight value) => JsonValue.Create(value.Pixels);

    /// <summary>A place on the curve of speeds as a whole number of its marks.</summary>
    /// <exception cref="InvalidDataException">The place lies between two marks.</exception>
    internal static int Marks(SuperMetroid.Core.Enemies.QuadraticSpeedCurvePlace place) =>
        place.InTwoHundredFiftySixthsOfMark == SuperMetroid.Core.Enemies.QuadraticSpeedCurvePlace.FromMarks(place.WholeMarks).InTwoHundredFiftySixthsOfMark
            ? place.WholeMarks
            : throw new InvalidDataException("A place on the curve of speeds lies between two marks, where a whole mark was wanted.");

    /// <summary>Every outcome of one of the old game's rolls, in the order of the numbers rolled: the first for a roll of nought.</summary>
    internal static IEnumerable<TOutcome> Each<TOutcome>(SuperMetroid.Core.Compatibility.WeightedRandomChoice<TOutcome> choice)
    {
        for (int rolled = 0; rolled < choice.Field.Outcomes; rolled++)
            yield return choice.Choose(rolled << choice.Field.Shift);
    }

    /// <summary>A list of numbers, as a data file writes one.</summary>
    internal static JsonArray List(IEnumerable<JsonNode> numbers)
    {
        var list = new JsonArray();
        foreach (JsonNode number in numbers)
            list.Add(number);
        return list;
    }

    /// <summary>A list of whole numbers, as a data file writes one.</summary>
    internal static JsonArray List(IEnumerable<int> numbers) => List(numbers.Select(number => (JsonNode)JsonValue.Create(number)));

    /// <summary>A distance or speed as it is written inside a cue.</summary>
    internal static string InCue(SignedSixteenSixteen value) => value.Pixels.ToString(CultureInfo.InvariantCulture);
}

using SuperMetroid.Core.Earthquakes;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>How the old game's numbers are written for the values of the Godot enemies that are whole numbers or named in words.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>A distance or speed of the old game that is a whole number of pixels.</summary>
    /// <exception cref="InvalidDataException">It has a part of a pixel.</exception>
    private static int Whole(SignedSixteenSixteen value) => value.Pixels == decimal.Truncate(value.Pixels)
        ? (int)value.Pixels
        : throw new InvalidDataException($"{value.Pixels} is not a whole number of pixels, which the value it is written to must be.");

    /// <summary>A place on the speed curve in 256ths of a mark.</summary>
    private static int MarkParts(QuadraticSpeedCurvePlace place) => place.InTwoHundredFiftySixthsOfMark;

    /// <summary>How hard one of the old game's shakes is, as the Godot game numbers them.</summary>
    /// <exception cref="InvalidDataException">The shake is of a strength this does not know.</exception>
    private static int ShakeStrength(EarthquakeShake shake) => shake.Strength switch
    {
        EarthquakeStrength.Light => 1,
        EarthquakeStrength.Medium => 2,
        EarthquakeStrength.Strong => 3,
        _ => throw new InvalidDataException($"A shake of the room is of a strength this does not know: {shake.Strength}."),
    };

    /// <summary>The way one of the old game's shakes goes, in the Godot game's word for it.</summary>
    /// <exception cref="InvalidDataException">The shake goes a way this does not know.</exception>
    private static string ShakeWayName(EarthquakeShake shake) => shake.Direction switch
    {
        EarthquakeDirection.Horizontal => "across",
        EarthquakeDirection.Vertical => "down",
        EarthquakeDirection.Diagonal => "both",
        _ => throw new InvalidDataException($"A shake of the room goes a way this does not know: {shake.Direction}."),
    };

    /// <summary>What of the room one of the old game's shakes throws about, in the Godot game's word for it.</summary>
    /// <exception cref="InvalidDataException">The shake reaches what this does not know.</exception>
    private static string ShakeReachName(EarthquakeShake shake) => shake.Reach switch
    {
        EarthquakeReach.RoomBlocks => "tiles",
        EarthquakeReach.RoomBlocksAndScenery => "tiles_and_backdrops",
        EarthquakeReach.RoomBlocksSceneryAndEnemies => "everything",
        EarthquakeReach.SceneryAndEnemies => "backdrops_and_things",
        _ => throw new InvalidDataException($"A shake of the room reaches what this does not know: {shake.Reach}."),
    };
}

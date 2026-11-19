namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Where Phantoon's room lists his parts: his eye, tentacles and mouth in a row right after his body (#2252).</summary>
internal static class PhantoonPartListingDefinitions
{
    /// <summary>The parts the room lists after his body: his eye, his tentacles and his mouth.</summary>
    internal const int PartsAfterBody = 3;

    /// <summary>The places after his body the room lists his eye: right after it.</summary>
    internal const int EyePlacesAfterBody = 1;

    /// <summary>The places after his body the room lists his tentacles: after his eye.</summary>
    internal const int TentaclesPlacesAfterBody = 2;

    /// <summary>The places after his body the room lists his mouth: after his tentacles.</summary>
    internal const int MouthPlacesAfterBody = 3;
}

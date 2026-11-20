namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Where Draygon's room lists his parts: his eye, tail and arms in a row right after his body.</summary>
internal static class DraygonPartListingDefinitions
{
    /// <summary>The parts the room lists after his body: his eye, his tail and his arms.</summary>
    internal const int PartsAfterBody = 3;

    /// <summary>The places after his body the room lists his eye: right after it.</summary>
    internal const int EyePlacesAfterBody = 1;

    /// <summary>The places after his body the room lists his tail: after his eye.</summary>
    internal const int TailPlacesAfterBody = 2;

    /// <summary>The places after his body the room lists his arms: after his tail.</summary>
    internal const int ArmsPlacesAfterBody = 3;
}

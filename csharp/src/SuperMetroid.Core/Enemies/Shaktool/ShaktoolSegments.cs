namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Finds one of Shaktool's linked body segments.</summary>
internal static class ShaktoolSegments
{
    /// <summary>Returns one of the seven physical linked-segment definitions.</summary>
    /// <exception cref="InvalidDataException"><paramref name="index"/> is outside the seven records.</exception>
    internal static ShaktoolSegmentDefinition ForIndex(int index) =>
        (uint)index < (uint)ShaktoolSegmentDefinitions.Segments.Length
            ? ShaktoolSegmentDefinitions.Segments[index]
            : throw new InvalidDataException(
                $"Shaktool segment {index} is outside the seven native definition records.");
}

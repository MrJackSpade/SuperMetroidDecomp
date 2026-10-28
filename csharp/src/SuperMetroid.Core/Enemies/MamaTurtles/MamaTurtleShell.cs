using static SuperMetroid.Core.Enemies.MamaTurtles.MamaTurtleShellContourDefinitions;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>Where a sleeping Mama Turtle's shell surface is above an actor beside its origin.</summary>
internal static class MamaTurtleShell
{
    /// <summary>
    /// Returns the native signed vertical contour offset for a parent-minus-actor X
    /// difference whose magnitude is below twenty-four pixels.
    /// </summary>
    internal static short GetOffset(int parentMinusActorX)
    {
        int distance = Math.Abs(parentMinusActorX);
        if (distance >= HalfWidth)
            throw new ArgumentOutOfRangeException(nameof(parentMinusActorX));
        int index = parentMinusActorX < 0 ? distance + HalfWidth : distance;
        return Offsets[index];
    }
}

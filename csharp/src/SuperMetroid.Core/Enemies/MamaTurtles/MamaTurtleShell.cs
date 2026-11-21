using static SuperMetroid.Core.Enemies.MamaTurtles.MamaTurtleShellContourDefinitions;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>Where a dormant Mama Turtle's shell surface is above an actor beside its origin.</summary>
internal static partial class MamaTurtleShell
{
    /// <summary>
    /// The shell's surface above an actor whose X is <paramref name="parentMinusActorX"/> pixels from the Mama Turtle's
    /// origin, measured from her origin back to the actor: the contour of the actor's side, at its distance, which must
    /// be below <see cref="HalfWidth"/>.
    /// </summary>
    internal static int GetOffset(int parentMinusActorX)
    {
        int distance = Math.Abs(parentMinusActorX);
        if (distance >= HalfWidth)
            throw new ArgumentOutOfRangeException(nameof(parentMinusActorX));
        return parentMinusActorX < 0 ? RightOfOrigin[distance - 1] : LeftOfOrigin[distance];
    }
}

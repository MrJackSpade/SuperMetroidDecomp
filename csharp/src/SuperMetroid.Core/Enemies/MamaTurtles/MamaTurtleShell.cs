using static SuperMetroid.Core.Enemies.MamaTurtles.MamaTurtleShellContourDefinitions;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>Where a dormant Mama Turtle's shell surface is above an actor beside its origin.</summary>
internal static partial class MamaTurtleShell
{
    /// <summary>
    /// The shell's surface above an actor whose X is <paramref name="parentMinusActorX"/> pixels from the Mama Turtle's
    /// origin, measured from her origin back to the actor: the contour of the actor's side, at its distance, when the
    /// actor stands within <see cref="HalfWidth"/> of her origin; an actor at that distance or beyond has no shell above it.
    /// </summary>
    /// <param name="parentMinusActorX">The Mama Turtle's X less the actor's.</param>
    /// <param name="surface">The contour above the actor, when it stands over the shell.</param>
    internal static bool TryGetSurface(int parentMinusActorX, out int surface)
    {
        int distance = Math.Abs(parentMinusActorX);
        if (distance >= HalfWidth)
        {
            surface = 0;
            return false;
        }

        surface = parentMinusActorX < 0 ? RightOfOrigin[distance - 1] : LeftOfOrigin[distance];
        return true;
    }
}

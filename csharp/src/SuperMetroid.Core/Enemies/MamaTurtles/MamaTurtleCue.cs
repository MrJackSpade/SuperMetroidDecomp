namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>One cue of the Mama Turtle's animations: what it does, with the values it acts on.</summary>
internal abstract partial record MamaTurtleCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private MamaTurtleCue()
    {
    }

    /// <summary>Starts the Mama Turtle's rise the way she leaves her shell facing, and continues at its spin (#2151).</summary>
    /// <param name="Direction">The way she faces as she rises.</param>
    internal sealed partial record RiseToHover(HorizontalDirection Direction) : MamaTurtleCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(MamaTurtleCueKind Kind) : MamaTurtleCue;
}

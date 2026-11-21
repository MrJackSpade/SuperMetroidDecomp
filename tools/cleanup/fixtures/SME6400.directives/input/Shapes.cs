namespace Fixture
{
    /// <summary>A shape.</summary>
    public sealed class Shapes
    {
        /// <summary>The number of sides.</summary>
        public int Sides { get; init; }
    }

#if DEBUG
    /// <summary>A marker only debug builds hold.</summary>
    public sealed class DebugMarker
    {
    }
#endif

#nullable disable
    /// <summary>A banner whose text spans lines.</summary>
    public static class Banner
    {
        /// <summary>The banner's text.</summary>
        public const string Text = """
            two
              lines
            """;

        /// <summary>A name that may be null without saying so.</summary>
        public static string Name { get; set; }
    }
#nullable restore

    /// <summary>A corner, whose file holds another type already.</summary>
    public sealed class Corner
    {
    }
}

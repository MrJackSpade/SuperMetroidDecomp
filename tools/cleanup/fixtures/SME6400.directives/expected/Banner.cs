namespace Fixture;

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

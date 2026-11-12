namespace Fixture;

/// <summary>A widget whose hand-written code is in two parts beside a generated one.</summary>
public sealed partial class Widget
{
    /// <summary>The widget's size.</summary>
    public int Size { get; init; }

    /// <summary>Twice the widget's size.</summary>
    public int Doubled => Size * 2;
}

namespace SuperMetroid.Core.Frontend;

/// <summary>Which of a frontend screen's two colour owners holds a colour (#1464).</summary>
public enum ScreenColourOwner
{
    /// <summary>The screen's background sets, which its background images show.</summary>
    Backgrounds,

    /// <summary>The screen's sprite sets, which its actors draw in.</summary>
    Sprites,
}

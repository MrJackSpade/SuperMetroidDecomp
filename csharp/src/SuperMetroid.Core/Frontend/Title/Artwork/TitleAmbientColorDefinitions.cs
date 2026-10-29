namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>Title tube shading and display paint bindings at $8D:C800-C873.</summary>
/// <remarks>The initial palette remains the independent paint owner. The selected three-unit
/// shade scale, rounded channel interpolation and dim-paint roles describe this animation;
/// no isolated green-channel exception or duplicate stock endpoint is stored.</remarks>
internal static class TitleAmbientColorDefinitions
{
    /// <summary>$8D:C800-C85B: nominal three-unit RGB5 dimming step, with shorter channels distributed to black.</summary>
    internal const int ShadeStep = 3;
}

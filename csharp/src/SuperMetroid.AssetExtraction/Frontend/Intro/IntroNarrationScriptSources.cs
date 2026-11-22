namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>Selects each narration page's stock script from <see cref="IntroNarrationScriptDefinitions"/>.</summary>
internal static partial class IntroNarrationScriptSources
{

    /// <summary>The six stock page scripts in narrative order, page one through page six.</summary>
    internal static IEnumerable<IntroNarrationScriptPage> Pages => IntroNarrationScriptDefinitions.Pages;
}

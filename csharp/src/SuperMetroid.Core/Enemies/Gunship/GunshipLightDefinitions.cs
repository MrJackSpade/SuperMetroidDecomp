using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>The gunship's lights (#1733).</summary>
internal static class GunshipLightDefinitions
{
    /// <summary>
    /// The colour effect the top hull's placement starts, on every placement: its lights' glow over the colour every piece
    /// draws its lights in, which runs for as long as the room does.
    /// </summary>
    internal const PaletteFxObject Glow = PaletteFxObject.GunshipGlow;
}

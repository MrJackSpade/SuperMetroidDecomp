namespace SuperMetroid.Core.Rendering;

/// <summary>
/// The installed key of each <see cref="GradientBlend"/> (#2520), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static class GradientBlendKeys
{
    /// <summary>Each GradientBlend member's installed key.</summary>
    internal static InstalledKeys<GradientBlend> Installed { get; } = new(new Dictionary<GradientBlend, string>
    {
        [GradientBlend.DarkensFloorAndBackdrop] = "DarkensFloorAndBackdrop",
        [GradientBlend.BrightensFloorBackdropAndSprites] = "BrightensFloorBackdropAndSprites",
    });
}

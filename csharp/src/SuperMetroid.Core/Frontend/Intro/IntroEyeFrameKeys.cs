namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The installed key of each <see cref="IntroEyeFrame"/> (#2520), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class IntroEyeFrameKeys
{
    /// <summary>Each IntroEyeFrame member's installed key.</summary>
    internal static InstalledKeys<IntroEyeFrame> Installed { get; } = new(new Dictionary<IntroEyeFrame, string>
    {
        [IntroEyeFrame.Open] = "Open",
        [IntroEyeFrame.HalfOpen] = "HalfOpen",
        [IntroEyeFrame.Closed] = "Closed",
        [IntroEyeFrame.Deadpan] = "Deadpan",
    });
}

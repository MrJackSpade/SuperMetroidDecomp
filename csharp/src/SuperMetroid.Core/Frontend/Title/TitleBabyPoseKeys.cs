namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// The installed key of each <see cref="TitleBabyPose"/> (#2520), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class TitleBabyPoseKeys
{
    /// <summary>Each TitleBabyPose member's installed key.</summary>
    internal static InstalledKeys<TitleBabyPose> Installed { get; } = new(new Dictionary<TitleBabyPose, string>
    {
        [TitleBabyPose.Closed] = "Closed",
        [TitleBabyPose.Middle] = "Middle",
        [TitleBabyPose.Open] = "Open",
    });
}

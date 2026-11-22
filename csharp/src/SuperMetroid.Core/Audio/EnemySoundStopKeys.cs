namespace SuperMetroid.Core.Audio;

/// <summary>
/// The installed key of each <see cref="EnemySoundStop"/> (#2580), written out so renaming a member never changes a name
/// the installation spells it by.
/// </summary>
internal static partial class EnemySoundStopKeys
{
    /// <summary>Each EnemySoundStop member's installed key.</summary>
    internal static InstalledKeys<EnemySoundStop> Installed { get; } = new(new Dictionary<EnemySoundStop, string>
    {
        [EnemySoundStop.Any] = "Any",
        [EnemySoundStop.Elevatube] = "Elevatube",
    });
}

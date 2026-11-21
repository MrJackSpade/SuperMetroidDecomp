namespace SuperMetroid.Core.Audio;

/// <summary>
/// The installed key of each <see cref="SamusSoundStop"/> (#2580), written out so renaming a member never changes a name
/// the installation spells it by.
/// </summary>
internal static partial class SamusSoundStopKeys
{
    /// <summary>Each SamusSoundStop member's installed key.</summary>
    internal static InstalledKeys<SamusSoundStop> Installed { get; } = new(new Dictionary<SamusSoundStop, string>
    {
        [SamusSoundStop.Any] = "Any",
        [SamusSoundStop.Xray] = "Xray",
        [SamusSoundStop.WaveSpecialAttack] = "WaveSpecialAttack",
        [SamusSoundStop.SpinJump] = "SpinJump",
        [SamusSoundStop.ScrewAttack] = "ScrewAttack",
    });
}

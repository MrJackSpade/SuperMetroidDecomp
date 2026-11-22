namespace SuperMetroid.Core.Samus.Colours;

/// <summary>
/// The installed key of each <see cref="SamusSuit"/> (#2520, #2565), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class SamusSuitKeys
{
    /// <summary>Each SamusSuit member's installed key.</summary>
    internal static InstalledKeys<SamusSuit> Installed { get; } = new(new Dictionary<SamusSuit, string>
    {
        [SamusSuit.Power] = "Power",
        [SamusSuit.Varia] = "Varia",
        [SamusSuit.Gravity] = "Gravity",
    });
}

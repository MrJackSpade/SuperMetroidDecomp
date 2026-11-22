namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The installed key of each <see cref="SamusTurningAim"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class SamusTurningAimKeys
{
    /// <summary>Each SamusTurningAim member's installed key.</summary>
    internal static InstalledKeys<SamusTurningAim> Installed { get; } = new(new Dictionary<SamusTurningAim, string>
    {
        [SamusTurningAim.Up] = "Up",
        [SamusTurningAim.Neutral] = "Neutral",
        [SamusTurningAim.Down] = "Down",
    });
}

namespace SuperMetroid.Core.Progression;

/// <summary>
/// The installed key of each <see cref="ReserveTankMode"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class ReserveTankModeKeys
{
    /// <summary>Each ReserveTankMode member's installed key.</summary>
    internal static InstalledKeys<ReserveTankMode> Installed { get; } = new(new Dictionary<ReserveTankMode, string>
    {
        [ReserveTankMode.None] = "None",
        [ReserveTankMode.Auto] = "Auto",
        [ReserveTankMode.Manual] = "Manual",
    });
}

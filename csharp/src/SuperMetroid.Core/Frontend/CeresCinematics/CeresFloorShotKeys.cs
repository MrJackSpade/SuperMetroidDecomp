namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// The installed key of each <see cref="CeresFloorShot"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class CeresFloorShotKeys
{
    /// <summary>Each CeresFloorShot member's installed key.</summary>
    internal static InstalledKeys<CeresFloorShot> Installed { get; } = new(new Dictionary<CeresFloorShot, string>
    {
        [CeresFloorShot.FlightFront] = "FlightFront",
        [CeresFloorShot.FlightRear] = "FlightRear",
        [CeresFloorShot.Station] = "Station",
        [CeresFloorShot.Explosion] = "Explosion",
        [CeresFloorShot.ZebesApproach] = "ZebesApproach",
    });
}

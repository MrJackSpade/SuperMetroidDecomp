using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Samus's held beam charge as her Pseudo Screw contact with an enemy spends it (#1405, #2159): her projectiles hold the
/// charge and its flare, and her colours show her suit's shade once it is spent. The enemy pass hands it to every touch it
/// runs, with the Samus the touch meets.
/// </summary>
/// <param name="Projectiles">Samus's beams and missiles, which hold the charge and its flare.</param>
/// <param name="Colours">Samus's colours, which show her suit's shade once the charge is spent.</param>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal readonly record struct PseudoScrewCharge(SamusProjectileSystem Projectiles, SamusColours Colours)
{
    /// <summary>
    /// Spends the charge, as a Pseudo Screw contact does before it reads the enemy's vulnerability: even an immune enemy
    /// takes Pseudo Screw's flare away.
    /// </summary>
    /// <param name="samus">Samus, whose suit shade the spent charge shows.</param>
    internal void Spend(SamusState samus) => Projectiles.Firing.ConsumePseudoScrewCharge(Colours, samus);
}

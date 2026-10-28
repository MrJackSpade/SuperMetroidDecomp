
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Compiled cartridge mechanics for Samus's special beam attacks.</summary>
internal static class SamusComboMechanicsDefinitions
{
    /// <summary>
    /// IcePlasmaSBAProjectileOriginAngles at $90:CD08: four evenly spaced
    /// eight-bit angles assigned in projectile-slot order.
    /// </summary>
    /// <remarks>Independently reviewed for #1165 against NTSC J/U v1.0 and pinned
    /// bank_90.asm: 64*slot for validated slot=0..3. Native Ice/Plasma initialization
    /// reads word offsets 6,4,2,0; the caller supplies undoubled projectile slots.
    /// The adjacent unused diagonal-angle words are not further projectile slots;
}

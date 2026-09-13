using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class GoldenTorizoAudit
{
    private static void VerifyProjectileContactGuards(
        SuperMetroidAddressSpace bus, CartridgeRoomHeader room, CartridgeRoomAssets assets)
    {
        GoldenProjectilePhase[] phases = [GoldenProjectilePhase.ChozoOrbFloorExplosion,
            GoldenProjectilePhase.SonicBoomFlight, GoldenProjectilePhase.HatchedEgg,
            GoldenProjectilePhase.EyeBeamFloorExplosion, GoldenProjectilePhase.SuperMissileExplosion];
        ushort[] suits = [0, (ushort)SamusEquipmentFlags.VariaSuit,
            (ushort)SamusEquipmentFlags.GravitySuit,
            (ushort)(SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit)];
        int cases = 0;
        foreach (var phase in phases)
        foreach (ushort suit in suits)
        foreach (ushort initialInvincibility in new ushort[] { 0, 1, 96 })
        foreach (ushort contactDamage in new ushort[] { 0, 1 })
        {
            var (loaded, target, _) = AdvanceToNaturalProjectilePhase(bus, room, assets, phase);
            foreach (var sibling in loaded.Enemies.EnemyProjectiles)
                if (sibling != target) sibling.CanDamageSamus = false;
            var samus = loaded.Samus;
            samus.Pose = SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus);
            samus.XPosition = target.XPosition;
            samus.YPosition = target.YPosition;
            samus.Health = 999;
            samus.EquippedItems = suit;
            samus.InvincibilityTimer = initialInvincibility;
            samus.HorizontalSpeed.ContactDamageIndex = contactDamage;
            samus.KnockbackTimer = 0;
            samus.KnockbackXDirection = 0;
            ushort list = target.InstructionPointer;
            ushort timer = target.InstructionTimer;
            bool persists = target.PersistsOnSamusContact;
            // Independent $A0:9923 -> SuitDamageDivision expectation; do not call
            // the production reduction helper to compute the expected damage.
            int reduction = (suit & (ushort)SamusEquipmentFlags.GravitySuit) != 0 ? 2 :
                (suit & (ushort)SamusEquipmentFlags.VariaSuit) != 0 ? 1 : 0;
            int damage = target.Damage >> reduction;
            bool blocked = initialInvincibility != 0 || contactDamage != 0;
            loaded.Enemies.ResolveEnemyProjectileSamusHits(samus);
            if (samus.Health != (blocked ? 999 : 999 - damage) ||
                samus.InvincibilityTimer != (blocked ? initialInvincibility : 96) ||
                samus.KnockbackTimer != (blocked ? 0 : 5) ||
                samus.KnockbackXDirection != (blocked ? 0 : 1) ||
                target.IsActive != (blocked || persists) ||
                (blocked && (target.InstructionPointer != list || target.InstructionTimer != timer)))
                throw new InvalidDataException($"Golden contact guard differs: phase={phase}, suit={suit}, " +
                    $"invincibility={initialInvincibility}, contact={contactDamage}; health={samus.Health}, " +
                    $"timer={samus.InvincibilityTimer}, knockback={samus.KnockbackTimer}, active={target.IsActive}.");
            cases++;
        }
        Console.WriteLine($"Golden projectile contact: {cases} natural-phase suit/immunity controls pass.");
    }
}

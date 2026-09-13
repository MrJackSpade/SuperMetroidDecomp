using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class NinjaSpacePirateAudit
{
    /// <summary>Exercises the ROM's empty extended map through the actual enemy collision walker.</summary>
    private static void VerifyCommonHitboxShot(SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room, CartridgeRoomAssets assets)
    {
        var loaded = Load(bus, room, assets);
        var actor = KeepOnly(loaded, 0);
        PrimeActive(loaded, assets, actor);
        // $B2:804F is not a null pointer: its single zero-sized box selects
        // CommonB2_NormalEnemyShotAI. A projectile radius can overlap that point.
        actor.SpritemapPointer = EnemyAiCodePointers.BankB2.EmptyExtendedSpritemap;
        ArmProjectile(loaded.Projectiles.Slots[0], actor.XPosition, actor.YPosition);
        ushort before = actor.Health;
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(bus, loaded.Projectiles,
            loaded.SharedProjectiles, loaded.Samus);
        // Gold Ninja's retail Super Missile vulnerability doubles the seeded 300 damage.
        if (hits != 1 || actor.Health != before - 600 || actor.FlashTimer == 0)
            throw new InvalidDataException($"#615 common pirate hitbox: hits={hits}, HP={before}->{actor.Health}, flash={actor.FlashTimer}.");
        var bombCase = Load(bus, room, assets);
        var bombActor = KeepOnly(bombCase, 0);
        PrimeActive(bombCase, assets, bombActor);
        bombActor.SpritemapPointer = EnemyAiCodePointers.BankB2.EmptyExtendedSpritemap;
        var bomb = EnemyProjectileAuditAssertions.ArmExplodingNormalBomb(bombCase.SharedProjectiles,
            bombActor.XPosition, bombActor.YPosition);
        if (bombCase.Enemies.ResolveOrdinaryBombHits(bombCase.SharedProjectiles, bombCase.Projectiles, bombCase.Samus) != 1)
            throw new InvalidDataException("#615 common point hitbox did not dispatch the overlapping bomb.");
        if (bombActor.Health != 1800 || bombActor.FlashTimer != 0)
            throw new InvalidDataException("Common bomb callback must preserve the retail zero bomb vulnerability.");
        var lethalCase = Load(bus, room, assets);
        var lethalActor = KeepOnly(lethalCase, 0);
        PrimeActive(lethalCase, assets, lethalActor);
        lethalActor.SpritemapPointer = EnemyAiCodePointers.BankB2.EmptyExtendedSpritemap;
        lethalActor.Health = 1;
        ArmProjectile(lethalCase.Projectiles.Slots[0], lethalActor.XPosition, lethalActor.YPosition);
        if (lethalCase.Enemies.ResolveOrdinaryProjectileHits(bus, lethalCase.Projectiles,
            lethalCase.SharedProjectiles, lethalCase.Samus) != 1 || lethalCase.Enemies.EnemiesKilled != 1)
            throw new InvalidDataException("Common lethal shot must run exactly one normal death tail.");
        if (lethalCase.Enemies.EnemyProjectiles.Count(p => p.IsActive) != 1)
            throw new InvalidDataException("Common lethal shot must create its explosion without the gold Ninja five-drop scatter.");
        Console.WriteLine($"PASS #615 common empty-map pirate shot: real multibox collision deals 600 damage and flashes; bomb callback also completes (health={bombActor.Health}).");
    }
}

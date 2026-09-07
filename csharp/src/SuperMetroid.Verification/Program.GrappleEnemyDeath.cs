using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyGrappleEnemyDeath()
    {
        var samus = CreateDropTestSamus();
        samus.Health = 50;
        samus.XPosition = 220;
        samus.YPosition = 180;
        var fixture = CreateEnemyDropFixture(samus, [1]);
        var retail = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (int bank in new[] { 0x860000, 0x8d0000 })
        {
            var data = new byte[0x8000];
            for (int i = 0; i < data.Length; i++) data[i] = retail.ReadByte(bank + 0x8000 + i);
            fixture.Bus.WriteBytes(bank + 0x8000, data);
        }
        const ushort header = 0x9000;
        WriteWord(fixture.Bus, 0xa00000 | header | 58, 0x8000);
        WriteSingleDropChance(fixture.Bus, EnemyPickupKind.SmallEnergy);
        var enemy = fixture.System.Slots[0];
        enemy.EnemyDefinitionPointer = header;
        enemy.Definition = default(RoomEnemyDefinition) with
        {
            GrappleAiPointer = EnemyAiCodePointers.BankA0.GrappleKill,
            DeathAnimation = 4,
        };
        enemy.XPosition = 100;
        enemy.YPosition = 100;
        enemy.XRadius = enemy.YRadius = 8;
        enemy.Health = 500;
        fixture.System.StepFrame(0, 0, timeIsFrozen: true, samus, level: fixture.Level);
        var contact = fixture.System.ResolveGrappleEndpoint(100, 100);
        AssertEqual(GrappleEnemyReaction.Kill, contact.Reaction, "header selects grapple kill independently of enemy health");
        samus.Grapple.Phase = GrapplePhase.Firing;
        fixture.System.StepFrame(0, 0, timeIsFrozen: false, samus, level: fixture.Level);
        var explosion = fixture.System.EnemyProjectiles.Single(p => p.IsActive);
        AssertEqual(RoomEnemyProjectileKind.EnemyDeathExplosion, explosion.Kind, "grapple kill creates the ordinary death actor");
        AssertEqual(1, fixture.System.EnemiesKilled, "grapple death increments kill count once");
        AssertEqual(GrapplePhase.Dropped, samus.Grapple.Phase, "native death routine drops the active grapple");
        AssertEqual(100, explosion.XPosition, "death explosion preserves enemy X");
        AssertEqual(100, explosion.YPosition, "death explosion preserves enemy Y");
        AssertEqual(header, explosion.EnemyHeaderPointer, "death actor retains drop-table identity");
        var spriteMaps = new HashSet<ushort>();
        bool sawPickup = false;
        bool heardDeath = false;
        for (int frame = 0; frame < 100; frame++)
        {
            fixture.System.StepEnemyProjectiles(fixture.Level, samus);
            // This focused projectile pass publishes the legacy sound field; the
            // full enemy-frame adapter forwards it to library two with Max1.
            heardDeath |= fixture.System.LastEnemyDeathSoundEffectLibrary2 == 9;
            if (explosion.Variable0 == (ushort)EnemyPickupKind.SmallEnergy * 2)
            {
                AssertEqual(31, frame, "small explosion uses the cartridge's complete 31-frame animation");
                sawPickup = true;
                break;
            }
            var oam = new OamBuffer();
            fixture.System.DrawEnemyProjectiles(oam, 0, 0);
            AssertTrue(oam.NextByteOffset > 0, $"grapple death explosion is visible on frame {frame}");
            spriteMaps.Add(explosion.SpritemapPointer);
        }
        AssertEqual(6, spriteMaps.Count, "grapple death displays all six retail small-explosion frames");
        AssertTrue(sawPickup, "retail death instruction list converts the explosion into a pickup");
        AssertTrue(heardDeath,
            "grapple death emits the retail enemy-killed sound from its instruction list");
        Console.WriteLine("  Grapple enemy death: visible animated explosion, drop conversion, position and kill counter agree.");
    }
}

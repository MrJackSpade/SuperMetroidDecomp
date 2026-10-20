using static SuperMetroid.Verification.Bosses.Kraid.KraidHeadInstructionsVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Kraid.KraidContourCaseMappingsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Kraid's body-contour collision edge against native signed-Y lookup behavior.</summary>
internal static class KraidBodyContourVerification
{
    /// <summary>Builds a direct probe for the private contour branch at its exact projectile-bottom threshold.</summary>
    internal static Func<RoomEnemySlot, SamusProjectileSlot, bool> CreateKraidBodyContourProbe() => (body, shot) =>
    {
        // Enter the body-contour branch at its exact Y threshold. The surrounding
        // mouth/body dispatch is covered by the encounter collision fixtures.
        var scratch = default(KraidBehavior.KraidCollisionScratch);
        scratch.Bottom = unchecked((ushort)(shot.Position.Y.Whole - shot.Radius.Y - 1));
        return scratch.OverlapsBody(body, new KraidBehavior.KraidCollisionShot(shot.Position.Whole, shot.Radius, shot.PackedType.Raw, shot.Damage));
    };

    /// <summary>Compares the real overlap branch across signed Y positions, translated body X values, shot radii, and edge offsets.</summary>
    internal static void VerifyKraidBodyContour(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyKraidBodyContourCases), () => VerifyKraidBodyContourCases(rom));
        var enemies = new RoomEnemySystem(new EnemySession());
        var overlaps = CreateKraidBodyContourProbe();
        var body = enemies.SlotTable.Slots[0];
        var shot = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None).Slots[0];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            short y = unchecked((short)raw);
            short expected = NativeKraidBodyLeftEdge(rom, y);
            body.Position = body.Position.WithWholeY((ushort)raw);
            shot.Position = shot.Position.WithWholeY(unchecked((ushort)(raw + y)));
            foreach (ushort bodyX in new ushort[] { 256, 32768, 65520 })
            foreach (ushort radius in new ushort[] { 0, 4, 16 })
            for (int delta = -1; delta <= 1; delta++)
            {
                body.Position = body.Position.WithWholeX(bodyX);
                shot.Radius = shot.Radius.WithX(radius);
                shot.Position = shot.Position.WithWholeX(unchecked((ushort)(bodyX + expected - radius + delta)));
                bool reference = unchecked((short)(bodyX + expected -
                    unchecked((ushort)(shot.Position.X.Whole + radius)))) < 0;
                AssertEqual(reference, overlaps(body, shot), "Actual contour collision edge, translation and strict inequality");
            }
        }
        Console.WriteLine("Kraid body contour: every signed Y and 1769472 actual collision boundary probes pass with ROM reads forbidden.");
    }

    /// <summary>Runs the Kraid contour and head-instruction checks with a retail cartridge oracle.</summary>
    [VerificationCommand("--kraid-contour-head-fixtures")]
    internal static int KraidContourHeadFixturesCommand()
    {
        var kraidFixtureRom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyKraidBodyContour), () => VerifyKraidBodyContour(kraidFixtureRom));
        Suite(nameof(VerifyKraidHeadInstructionDefinitions), () => VerifyKraidHeadInstructionDefinitions(kraidFixtureRom));
        return 0;
    }
}

using SuperMetroid.Core.Bosses.Kraid;
using static SuperMetroid.Verification.Bosses.Kraid.KraidNailContourMappingsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks native Kraid body contour selection and nail reflection at every height a nail in his room can lie from him.</summary>
internal static class KraidNailContourVerification
{
    /// <summary>Runs signed edge and velocity probes, including reads into adjacent contour words.</summary>
    internal static void VerifyKraidNailContour(CartridgeImportAddressSpace rom)
    {
        ushort Word(int a) => LittleEndianBytes.Word(KraidNailContourRomByte(rom, a), KraidNailContourRomByte(rom, a + 1));
        Suite(nameof(VerifyKraidNailLeftOffsets), () => VerifyKraidNailLeftOffsets(rom));
        Suite(nameof(VerifyKraidNailTopBoundaries), () => VerifyKraidNailTopBoundaries(rom));
        Suite(nameof(VerifyKraidNailPastLedges), () => VerifyKraidNailPastLedges(rom));
        var session = EnemySessionFixtures.WithInstalledServices();
        var enemies = EnemySystemFixtures.New(session, RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        enemies.Places.BindKraidPartsInRoomOrder();
        // His setup reads whether his room's boss is defeated, which this room's is not, and writes the screen
        // scrolls his fight frames, which this room takes.
        enemies.VerificationServices().IsAreaBossDefeated ??= () => false;
        enemies.VerificationServices().SetRoomScrollState ??= (_, _) => { };
        KraidEnemyState state = enemies.Places.SetUpKraidInRoomOrder();
        Func<RoomEnemyPlace, bool> reflect = place => enemies.KraidBehavior.Projectiles.KraidBodyContourReflectsNail(place, ((KraidNailState)place.RequireSpeciesState<KraidPartState>()));
        var body = enemies.Places.InUpdateOrder[0];
        var nail = enemies.Places.InUpdateOrder[6];
        nail.Radius = nail.Radius.WithX(4);
        int overreadCases = 0;
        var histogram = new int[6];
        // Every height a nail in Kraid's two-screen room can lie from his body, with his body where his walks take it (#2299).
        for (int raw = -1024; raw < 1024; raw++)
        {
            body.Position = body.Position.WithWhole(new(KraidArenaDefinitions.LungeReachX + (raw & 255), 0x1234));
            nail.Position = nail.Position.WithWholeY(body.Position.Y.Whole + raw);
            int record = 0;
            // Follow the native ROM walk independently, not the compiled span bound.
            while (unchecked((short)(Word(0xa7bf1f + record * 4) - raw)) >= 0)
            {
                record++;
                if (record >= 256) throw new InvalidOperationException("Native contour diagnostic did not terminate.");
            }
            AssertTrue(record < 6, "Every native relative-Y walk terminates within six records");
            histogram[record]++;
            if (record >= 4) overreadCases++;
            if (record > KraidNailContourDefinitions.Ledges.Count)
            {
                // No nail in Kraid's room lies this far above his body; the installed record past the ledges rejects it.
                AssertThrows<ArgumentOutOfRangeException>(() => reflect(nail), "A nail beyond the record past the ledges is rejected");
                continue;
            }
            ushort nativeLeft = Word(0xa7bf1d + record * 4);

            int edge = body.Position.X.Whole + unchecked((short)nativeLeft);
            foreach (ushort velocity in new ushort[] { 0, 1, 0x7fff, 0x8000, 0xffff })
            for (int delta = -1; delta <= 1; delta++)
            {
                nail.Position = nail.Position.WithWholeX(edge - nail.Radius.X + delta);
                ((KraidNailState)nail.RequireSpeciesState<KraidPartState>()).NailVelocity = ((KraidNailState)nail.RequireSpeciesState<KraidPartState>()).NailVelocity.WithX(SignedSixteenSixteen.FromParts(unchecked((short)velocity), ((KraidNailState)nail.RequireSpeciesState<KraidPartState>()).NailVelocity.X.Fraction));
                bool expected = nail.Position.X.Whole + nail.Radius.X >= edge && (short)velocity >= 0;
                AssertEqual(expected, reflect(nail), "Actual contour reflection retains signed edge and velocity gates");
            }
        }
        AssertTrue(overreadCases > 0, "Fixture exercises adjacent instruction words, not just authored geometry");
        int[] expectedHistogram = [1007, 56, 56, 32, 897, 0];
        for (int i = 0; i < histogram.Length; i++) AssertEqual(expectedHistogram[i], histogram[i], "Native contour selection distribution");
        Console.WriteLine($"Kraid nail contour: actual edge/direction probes pass bus-free; {overreadCases} relative-Y words select records past the ledges.");
    }

    /// <summary>Reads the retail contour as an independent oracle for the compiled lookup.</summary>
    private static byte KraidNailContourRomByte(CartridgeImportAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}

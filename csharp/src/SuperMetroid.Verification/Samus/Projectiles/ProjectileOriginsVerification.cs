using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Checks Samus projectile muzzle origins against native direction and pose records.</summary>
internal static class ProjectileOriginsVerification
{
    /// <summary>Exercises authored and adjacent table words through physical projectile initialization.</summary>
    internal static void VerifyProjectileOrigins(CartridgeImportAddressSpace rom)
    {
        short Word(int address) => unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
        var initialize = new Action<SamusState, SamusProjectile>(BeamFiring.InitializePosition);
        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
        var slot = new SamusProjectile(0);

        void Check(SamusPoseId pose, SamusProjectileDirection direction, ushort x, ushort y)
        {
            samus.Pose = pose; samus.Position = samus.Position.WithWhole(new(x, y)); slot.Direction = direction;
            bool running = rom.ReadCartridgeByte(0x91b629 + SamusPoseNumbers.Number(pose) * 8 + 1) == 1 || pose is SamusPoseId.MoonwalkAimUpLeftPose or SamusPoseId.MoonwalkAimUpRightPose;
            int offset = direction.ToAimIndex() * 2;
            short dx = Word((running ? 0x90c22c : 0x90c204) + offset);
            short dy = Word((running ? 0x90c240 : 0x90c218) + offset);
            byte correction = rom.ReadCartridgeByte(0x91b629 + SamusPoseNumbers.Number(pose) * 8 + 4);
            initialize(samus, slot);
            AssertEqual(unchecked((ushort)(x + dx)), slot.Position.X.Whole, "Physical muzzle X follows native direction/table selection");
            AssertEqual(unchecked((ushort)(y + dy - correction)), slot.Position.Y.Whole, "Physical muzzle Y follows native signed origin and unsigned pose correction");
            AssertEqual(direction, slot.Direction, "Origin lookup does not rewrite the direction");
            AssertEqual(x, samus.Position.X.Whole, "Origin setup leaves Samus X unchanged");
            AssertEqual(y, samus.Position.Y.Whole, "Origin setup leaves Samus Y unchanged");
        }

        // Every direction at every coordinate, through standing, running, both special
        // Moonwalk poses and ordinary Moonwalk.
        foreach (SamusPoseId pose in new[]
        {
            SamusPoseId.FacingRightNormalPose, SamusPoseId.MovingRightNormalPose,
            SamusPoseId.MoonwalkAimUpLeftPose, SamusPoseId.MoonwalkAimUpRightPose,
            SamusPoseId.MoonwalkFacingLeftPose
        })
        foreach (SamusProjectileDirection direction in Enum.GetValues<SamusProjectileDirection>())
        for (int coordinate = 0; coordinate <= ushort.MaxValue; coordinate++)
            Check(pose, direction, (ushort)coordinate, unchecked((ushort)~coordinate));
        foreach (ushort boundary in new ushort[] { 0, 127, 0x8000, ushort.MaxValue })
        foreach (int pose in Enumerable.Range(0, SamusPoseNumbering.RecordCount).Where(SamusPoseNumbering.NamesPose))
        foreach (SamusProjectileDirection direction in Enum.GetValues<SamusProjectileDirection>())
            Check(SamusPoseNumbering.FromNumber(pose), direction, boundary, boundary);

        // Ten authored directions; a projectile's aim is always one of them (#1506).
        foreach (bool running in new[] { false, true })
        foreach (SamusProjectileDirection direction in Enum.GetValues<SamusProjectileDirection>())
        {
            int aimIndex = direction.ToAimIndex();
            var actual = SamusMuzzleOrigins.Read(running, direction);
            AssertEqual(new Offset16<RoomPixels>(Word((running ? 0x90c22c : 0x90c204) + aimIndex * 2),
                Word((running ? 0x90c240 : 0x90c218) + aimIndex * 2)), actual,
                "Forty native origin words");
        }
        Console.WriteLine("Projectile origins: forty native words and 343872 real position initializations cover every direction, every pose and coordinate boundaries with authored reads forbidden.");
    }

}

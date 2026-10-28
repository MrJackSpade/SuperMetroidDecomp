using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Checks Samus projectile muzzle origins against native direction and pose records.</summary>
internal static class ProjectileOriginsVerification
{
    /// <summary>Exercises authored and adjacent table words through physical projectile initialization.</summary>
    internal static void VerifyProjectileOrigins(SuperMetroidAddressSpace rom)
    {
        short Word(int address) => unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
        var initialize = new Action<SamusState, SamusProjectileSlot>(BeamFiring.InitializePosition);
        var samus = new SamusState();
        var slot = new SamusProjectileSlot(0);

        void Check(SamusPoseId pose, ushort direction, ushort x, ushort y)
        {
            samus.Pose = pose; samus.Position = samus.Position.WithWhole(new(x, y)); slot.Direction = direction;
            bool running = rom.ReadCartridgeByte(0x91b629 + (int)pose * 8 + 1) == 1 || pose is SamusPoseId.MoonwalkAimUpLeftPose or SamusPoseId.MoonwalkAimUpRightPose;
            int offset = (direction & 15) * 2;
            short dx = Word((running ? 0x90c22c : 0x90c204) + offset);
            short dy = Word((running ? 0x90c240 : 0x90c218) + offset);
            byte correction = rom.ReadCartridgeByte(0x91b629 + (int)pose * 8 + 4);
            initialize(samus, slot);
            AssertEqual(unchecked((ushort)(x + dx)), slot.Position.X.Whole, "Physical muzzle X follows native direction/table selection");
            AssertEqual(unchecked((ushort)(y + dy - correction)), slot.Position.Y.Whole, "Physical muzzle Y follows native signed origin and unsigned pose correction");
            AssertEqual(direction, slot.Direction, "Origin lookup does not rewrite lifecycle/direction bits");
            AssertEqual(x, samus.Position.X.Whole, "Origin setup leaves Samus X unchanged");
            AssertEqual(y, samus.Position.Y.Whole, "Origin setup leaves Samus Y unchanged");
        }

        // All direction words, including lifecycle high bits and low-nibble overreads,
        // through standing, running, both special Moonwalk poses and ordinary Moonwalk.
        foreach (SamusPoseId pose in new[]
        {
            SamusPoseId.FacingRightNormalPose, SamusPoseId.MovingRightNormalPose,
            SamusPoseId.MoonwalkAimUpLeftPose, SamusPoseId.MoonwalkAimUpRightPose,
            SamusPoseId.MoonwalkFacingLeftPose
        })
        for (int word = 0; word <= ushort.MaxValue; word++)
            Check(pose, (ushort)word, (ushort)word, unchecked((ushort)~word));
        foreach (ushort boundary in new ushort[] { 0, 127, 0x8000, ushort.MaxValue })
        for (int pose = 0; pose <= 0xfc; pose++)
        for (ushort direction = 0; direction < 16; direction++)
            Check((SamusPoseId)pose, direction, boundary, boundary);

        // Ten authored directions cover every word. Extra nibble values intentionally
        // cross table boundaries; running Y reaches adjacent cooldown bytes unchanged.
        foreach (bool running in new[] { false, true })
        for (ushort direction = 0; direction < 16; direction++)
        {
            var actual = SamusProjectileOriginDefinitions.Read(running, direction);
            AssertEqual(new Offset16<RoomPixels>(Word((running ? 0x90c22c : 0x90c204) + direction * 2),
                Word((running ? 0x90c240 : 0x90c218) + direction * 2)), actual,
                "Forty native origin words and cross-row/cooldown overreads");
        }
        Console.WriteLine("Projectile origins: forty native words and 343872 real position initializations cover all direction words, every pose, coordinate boundaries and adjacent cooldown reads with authored reads forbidden.");
    }

}

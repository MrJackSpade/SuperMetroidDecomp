using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Checks Samus projectile muzzle origins against native direction and pose records.</summary>
internal static class ProjectileOriginsVerification
{
    /// <summary>Exercises authored and adjacent table words through physical projectile initialization.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for projectile origins")]
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
            AssertEqual((Word((running ? 0x90c22c : 0x90c204) + direction * 2),
                Word((running ? 0x90c240 : 0x90c218) + direction * 2)), actual,
                "Forty native origin words and cross-row/cooldown overreads");
        }
        Console.WriteLine("Projectile origins: forty native words and 343872 real position initializations cover all direction words, every pose, coordinate boundaries and adjacent cooldown reads with authored reads forbidden.");
    }

    /// <summary>Forbids physical-origin table reads while substituting sprite presentation bytes to check their separation.</summary>
    [CpuBusBoundary("exact bus test: serves substitute presentation bytes ($90:C1A8-C203) while forbidding origin rows, proving physical origins ignore presentation data")]
    internal sealed class ProjectileOriginReadGuard(ISnesAddressSpace source) : ISnesWritableAddressSpace, IImportCartridgeSource
    {
        /// <summary>Rejects compiled origin reads and substitutes presentation bytes to prove independence.</summary>
        public byte ReadCartridgeByte(int address)
        {
            if (address is >= 0x90c204 and < 0x90c28f)
                throw new InvalidOperationException($"Compiled projectile origin read ROM ${address:X6}.");
            // Deliberately different presentation bytes cannot affect physical origins.
            if (address is >= 0x90c1a8 and < 0x90c204) return 0x5a;
            return source.ReadCartridgeByte(address);
        }
        /// <summary>Forwards unrelated bus writes to the wrapped address space.</summary>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}

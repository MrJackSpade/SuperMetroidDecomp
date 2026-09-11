using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyCrystalPaletteNative(string rom, string path)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        AssertEqual("7DD811C738134358AB18F3C4BD609F455E6099AC90A2FAA29C9B9FECE5F209C8",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(path))),
            "accepted native Crystal Flash palette trace");
        using var trace = File.OpenText(path);
        AssertEqual("beam,offset,frame,type,bubbleFrame,bubbleTimer,bodyOffset,colors", trace.ReadLine(), "palette schema");
        ushort[] beams = [0, (ushort)SamusBeamFlags.Charge,
            (ushort)(SamusBeamFlags.Wave | SamusBeamFlags.Ice), (ushort)SamusBeamFlags.Plasma];
        ushort[] offsets = [0, 4, 36];
        int compared = 0;
        for (int beam = 0; beam < beams.Length; beam++)
        for (int offset = 0; offset < offsets.Length; offset++)
        {
            var samus = new SamusState
            {
                Pose = SamusPoseIds.MorphBallGroundRightPose, Health = 49, MaxHealth = 99,
                Missiles = 10, SuperMissiles = 10, PowerBombs = 10, EquippedBeams = beams[beam],
            };
            samus.RefreshCollisionRadii(bus);
            samus.HorizontalSpeed.SpecialPaletteTimer = offsets[offset];
            AssertTrue(samus.CrystalFlash.TryBegin(bus, samus,
                (ushort)(SnesButton.Down | SnesButton.L | SnesButton.R | SnesButton.X)), "palette activation");
            var cgram = new SnesCgram();
            for (int frame = 0; frame <= 180; frame++)
            {
                // Isolate the palette handler's finish branch. Movement-owned finish
                // timing is covered by the separate original-CPU lifetime comparison.
                if (frame == 180)
                    typeof(SamusCrystalFlashState).GetProperty(nameof(SamusCrystalFlashState.SpecialPaletteTimer))!
                        .SetValue(samus.CrystalFlash, ushort.MaxValue);
                samus.CrystalFlash.UpdatePalette(bus, cgram, samus);
                var flash = samus.CrystalFlash;
                string colors = string.Concat(cgram.Colors.Slice(224, 16).ToArray().Select(color => color.ToString("X4")));
                string actual = $"{beam},{offset},{frame},{flash.SpecialPaletteType:X4},{flash.SpecialPaletteFrame:X4},{flash.SpecialPaletteTimer:X4},{flash.CommonPaletteTimer:X4},{colors}";
                AssertEqual(trace.ReadLine(), actual, "native independent body/bubble palette and beam restoration");
                compared++;
            }
        }
        AssertTrue(trace.ReadLine() is null, "native palette trace fully consumed");
        Console.WriteLine($"Crystal Flash palette: {compared} original-CPU calls match, including beam restoration.");
    }
}

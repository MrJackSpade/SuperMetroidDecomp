using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Fx.RoomFxBlockAnimationArtworkVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Fx;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks Maridia sand animation selection, timing, NMI transfers, and room departure.</summary>
internal static class SandAnimatedTilesVerification
{
    /// <summary>ROM-free bank-$83 selection, bank-$87 cadence, and NMI transfer regression for sand.</summary>
    internal static void VerifySandAnimatedTiles()
    {
        var bus = new TestAddressSpace();
        const ushort sandFxRecord = 0x9e44;
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        AssertEqual((byte)0x0c, rom.ReadCartridgeByte(0x830000 | (sandFxRecord + 14)),
            "selected retail FX record enables ceiling and falling sand");
        BlockAnimation[] definitions = [BlockAnimation.MaridiaSandCeiling, BlockAnimation.MaridiaSandFalling];
        for (int slot = 0; slot < 2; slot++)
        {
            AssertTrue(BlockAnimationLoops.TryResolve(
                    definitions[slot], out BlockAnimationLoop? definition),
                $"sand animation {slot} resolves compiled mechanics");
            for (int frame = 0; frame < (definition ?? throw new InvalidOperationException("Verification requires definition.")).Frames.Count; frame++)
            {
                int source = AnimatedTileImageSources.OfCursor(
                    definition, definition.FrameInstructionPointer(frame));
                bus.WriteBytes(source,
                    Enumerable.Repeat((byte)(1 + slot * 4 + frame),
                        definition.PictureByteCount).ToArray());
            }
        }
        var guarded = NewRoomFxArtworkForbiddenBus(bus);
        var sand = new RoomBlockAnimations(RoomFxDefinitions.Suffix(sandFxRecord), null);
        AssertEqual(2, sand.EffectAnimations.Count(definitions.Contains), "both FX sand bits start animations");
        var frames = new ArtworkFrameSelections();
        for (int tick = 0; tick < 31; tick++)
        {
            var before = new ArtworkFrame?[2];
            for (int slot = 0; slot < 2; slot++)
                before[slot] = frames.TryShown(BlockAnimationFrames.Of(definitions[slot]), out ArtworkFrame previous) ? previous : null;
            sand.Step(areaBossDefeated: false, frames);
            for (int slot = 0; slot < 2; slot++)
            {
                var animation = definitions[slot];
                AssertTrue(frames.TryShown(BlockAnimationFrames.Of(animation), out ArtworkFrame shown),
                    $"sand animation {slot} shows a frame from its first update");
                AssertEqual(BlockAnimationFrames.Frame(animation, tick / 10 % 4), shown,
                    "sand ceiling/fall frames loop independently on the compiled cartridge durations");
                AssertEqual(tick % 10 != 0, before[slot] == shown,
                    "sand frames change only on the updates their durations end");
            }
        }
        sand = new RoomBlockAnimations(null, null);
        AssertEqual(0, sand.EffectAnimations.Count, "next room without FX clears sand owners");
        var departed = new ArtworkFrameSelections();
        sand.Step(areaBossDefeated: false, departed);
        for (int slot = 0; slot < 2; slot++)
            AssertTrue(!departed.TryShown(BlockAnimationFrames.Of(definitions[slot]), out _),
                "departed room cannot continue showing sand frames");
        AssertEqual(0, guarded.ForbiddenReadAttempts, "sand runtime never reads artwork from ROM");
        Console.WriteLine("  Sand animation: FX selection, timed loops, the frames shown, and room reset agree.");
    }

    /// <summary>Runs the ROM-free sand animation transfer check.</summary>
    [VerificationCommand("--sand-animated-tiles")]
    internal static int SandAnimatedTilesCommand()
    {
        Suite(nameof(VerifySandAnimatedTiles), () => VerifySandAnimatedTiles());
        return 0;
    }
}

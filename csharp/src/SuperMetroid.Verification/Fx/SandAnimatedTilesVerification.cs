using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Fx.RoomFxAnimatedTileArtworkVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Fx;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Progression;

/// <summary>Checks Maridia sand animation selection, timing, NMI transfers, and room departure.</summary>
internal static class SandAnimatedTilesVerification
{
    /// <summary>ROM-free bank-$83 selection, bank-$87 cadence, and NMI transfer regression for sand.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifySandAnimatedTiles()
    {
        var bus = new TestAddressSpace();
        const ushort sandFxRecord = 0x9e44;
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        AssertEqual((byte)0x0c, rom.ReadCartridgeByte(0x830000 | (sandFxRecord + 14)),
            "selected retail FX record enables ceiling and falling sand");
        AnimatedTileObject[] definitions = [AnimatedTileObject.MaridiaSandCeiling, AnimatedTileObject.MaridiaSandFalling];
        for (int slot = 0; slot < 2; slot++)
        {
            AssertTrue(RoomFxAnimatedTileMechanicsDefinitions.TryResolve(
                    definitions[slot], out RoomFxAnimatedTileObjectDefinition? definition),
                $"sand object {slot} resolves compiled mechanics");
            for (int frame = 0; frame < (definition ?? throw new InvalidOperationException("Verification requires definition.")).Frames.Count; frame++)
            {
                int source = AnimatedTileImageSources.OfCursor(
                    definition, definition.Frames[frame].InstructionPointer);
                bus.WriteBytes(source,
                    Enumerable.Repeat((byte)(1 + slot * 4 + frame),
                        definition.FrameByteCount).ToArray());
            }
        }
        var guarded = NewRoomFxArtworkForbiddenBus(bus);
        var sand = new RoomSandAnimatedTilesState(RoomFxDefinitions.Suffix(sandFxRecord), null, AreaId.Maridia);
        AssertEqual(2, sand.Count, "both FX sand bits create objects");
        var frames = new ArtworkFrameSelections();
        for (int tick = 0; tick < 31; tick++)
        {
            var before = new ArtworkFrame?[2];
            for (int slot = 0; slot < 2; slot++)
                before[slot] = frames.TryShown(AnimatedTileFrames.Of(definitions[slot]), out ArtworkFrame previous) ? previous : null;
            sand.Step(frames);
            for (int slot = 0; slot < 2; slot++)
            {
                var animatedObject = definitions[slot];
                AssertTrue(frames.TryShown(AnimatedTileFrames.Of(animatedObject), out ArtworkFrame shown),
                    $"sand object {slot} shows a frame from its first update");
                AssertEqual(AnimatedTileFrames.Frame(animatedObject, tick / 10 % 4), shown,
                    "sand ceiling/fall frames loop independently on the compiled cartridge durations");
                AssertEqual(tick % 10 != 0, before[slot] == shown,
                    "sand frames change only on the updates their durations end");
            }
        }
        sand = new RoomSandAnimatedTilesState(null, null, AreaId.Maridia);
        AssertEqual(0, sand.Count, "next room without FX clears sand owners");
        var departed = new ArtworkFrameSelections();
        sand.Step(departed);
        for (int slot = 0; slot < 2; slot++)
            AssertTrue(!departed.TryShown(AnimatedTileFrames.Of(definitions[slot]), out _),
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

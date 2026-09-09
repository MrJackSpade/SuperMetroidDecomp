using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyEndingExplosionSlotOrder()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        var audio = new CartridgeAudioState();
        var ending = new EndingCreditsState(bus, audio, 0, 0);
        for (int frame = 0; frame < 20000 && ending.Phase != EndingCreditsPhase.PlanetEscapeFast; frame++)
        {
            ending.Step();
            audio.AdvanceFrame(bus, default);
        }
        AssertEqual(EndingCreditsPhase.PlanetEscapeFast, ending.Phase, "native flyaway handoff reached");
        for (int frame = 0; frame < 450; frame++) ending.Step();
        var definition = EndingCreditsRomData.Sprites.ExplosionAfterglow;
        var afterglow = new IntroDiscoverySprite(definition.X, definition.Y,
            definition.Attributes.Raw, definition.InstructionPointer);
        afterglow.Step(bus);
        var expected = new OamBuffer();
        expected.BeginFrame();
        afterglow.Draw(bus, expected);
        expected.FinalizeFrame();
        // F2FA installs the afterglow at byte slot 6 (index 3); the starfields occupy
        // byte slots 2 and 0. The native descending draw must give the complete
        // afterglow first access to OAM, ahead of both 53-entry starfields.
        AssertEqual(37, expected.LastFinalizedSpriteCount, "retail afterglow component count");
        var actual = ending.CaptureRenderSnapshot().Memory;
        AssertTrue(actual.Oam[..(37 * 4)].SequenceEqual(expected.LowTable[..(37 * 4)]),
            "native slot order preserves all 37 afterglow pieces before starfield OAM overflow");
    }
}

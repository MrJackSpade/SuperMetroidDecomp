using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyBeamCallbackTables()
    {
        var retail = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var room = new RoomLevelData(16, 16, new ushort[256], new byte[256], new ushort[256], new byte[8]);
        foreach (bool charged in new[] { false, true })
        foreach (var (pointer, expected) in new[] {
            (SamusBeamPreInstructionCodes.NoWave, SamusProjectilePreInstruction.NoWaveBeam),
            (SamusBeamPreInstructionCodes.WaveThreeFrameTrail, SamusProjectilePreInstruction.WaveBeamThreeFrameTrail),
            (SamusBeamPreInstructionCodes.WaveFourFrameTrail, SamusProjectilePreInstruction.WaveBeamFourFrameTrail) })
        {
            // Keep the beam word fixed; only the cartridge table changes. This catches
            // synthesized dispatch that passes all retail combinations by coincidence.
            var bus = new BeamSpeedRowAddressSpace(retail);
            bus.SetWord(charged ? SamusBeamPreInstructionCodes.ChargedTable :
                SamusBeamPreInstructionCodes.UnchargedTable, pointer);
            var samus = new SamusState { Pose = 1, XPosition = 128, YPosition = 128,
                EquippedBeams = charged ? (ushort)0x1000 : (ushort)0 };
            var projectiles = new SamusProjectileSystem();
            var shared = new SamusBombProjectileSystem();
            if (charged)
            {
                for (int frame = 0; frame < 60; frame++)
                {
                    shared.StepFrame(bus, room, samus, 0, 0);
                    projectiles.StepFrame(bus, room, samus, (ushort)SnesButton.X,
                        frame == 0 ? (ushort)SnesButton.X : (ushort)0, 0, 0, shared);
                }
            }
            var result = projectiles.StepFrame(bus, room, samus,
                charged ? (ushort)0 : (ushort)SnesButton.X,
                charged ? (ushort)0 : (ushort)SnesButton.X, 0, 0, shared);
            AssertEqual(true, result.FiredSlot.HasValue, "callback table fixture fires");
            AssertEqual(expected, projectiles.Slots[result.FiredSlot!.Value].PreInstruction,
                $"charged={charged} callback pointer {pointer:X4} controls production dispatch");
        }
        Console.WriteLine("Beam callback tables: both producers honor all three translated pointers independently of beam bits.");
    }
}

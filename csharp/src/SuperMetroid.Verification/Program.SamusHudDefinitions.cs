using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifySamusHudDefinitions(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        bool Transition(byte pose, bool active) => pose >= 0xf1 ||
            (pose < 0xdb && (rom.ReadByte(0x90ddaa + pose - 0x35) == 0 || active));
        var metadata = new GrappleFiringReadGuard(rom);
        var bus = new HudPolicyReadGuard(metadata);
        for (int pose = 0; pose <= byte.MaxValue; pose++)
        foreach (bool active in new[] { false, true })
        {
            bool authored = pose is >= 0x35 and <= 0x40;
            AssertEqual(authored, SamusHudDefinitions.TryGetPostureFlag((byte)pose, out byte flag), "Twelve authored posture flags only");
            if (authored) AssertEqual(rom.ReadByte(0x90ddaa + pose - 0x35), flag, "Native posture flag byte");
            AssertEqual(Transition((byte)pose, active), SamusHudInput.PostureTransitionAdmitsWeapons(bus, (byte)pose, active), "Every posture pose and active-Grapple branch");
        }
        var samus = new SamusState();
        var setTurn = typeof(SamusState).GetProperty(nameof(SamusState.PoseTransitionShotDirection))!.SetMethod!
            .CreateDelegate<Action<SamusState, ushort>>();
        for (byte movement = 0; movement < 28; movement++)
        {
            ushort handler = Word(0x90dd05 + movement * 2);
            AssertEqual(handler, SamusHudDefinitions.MovementHandler((SamusMovementType)movement), "All 28 native HUD handler identities");
            for (int pose = 0; pose <= byte.MaxValue; pose++)
            foreach (bool locked in new[] { false, true })
            foreach (ushort selected in new ushort[] { 3, 4 })
            foreach (ushort turn in new ushort[] { 0, 1, ushort.MaxValue })
            {
                metadata.SourcePose = samus.Pose = (byte)pose;
                metadata.Movement = movement;
                samus.InputLocked = locked; samus.SelectedHudItem = selected; setTurn(samus, turn);
                bool expected = !locked && pose is not (0 or 0x9b) && selected == 4 &&
                    (handler is 0xdd3d or 0xdd6f or 0xddd8 || handler == 0xdd74 && turn != 0 ||
                        handler == 0xdd8c && Transition((byte)pose, false));
                AssertEqual(expected, SamusGrappleHudInput.IsSelectedAndAdmitted(bus, samus), "Actual Grapple HUD admission");
            }
        }
        // Exercise the other consumer, the real beam producer. Earn a partial charge
        // normally, then test whether the native HUD class holds it or advances it.
        var level = CreateRoom(64, 64, new ushort[4096], new byte[4096]);
        for (byte movement = 0; movement < 28; movement++)
        foreach (byte pose in new byte[] { 1, 0x35, 0x37, 0x3b, 0x3d, 0xdb, 0xf1 })
        foreach (ushort turn in new ushort[] { 0, 1 })
        {
            metadata.SourcePose = 1; metadata.Movement = 0; metadata.Direction = 2;
            samus = new SamusState { Pose = 1, XPosition = 512, YPosition = 512, EquippedBeams = (ushort)SamusBeamFlags.Charge };
            var shots = new SamusProjectileSystem();
            var bombs = new SamusBombProjectileSystem();
            for (int frame = 0; frame < 10; frame++)
                shots.StepFrame(bus, level, samus, (ushort)SnesButton.X, 0, 0, 0, bombs);
            AssertEqual(10, shots.FlareCounter, "Fixture charge earned through real producer");
            metadata.SourcePose = samus.Pose = pose; metadata.Movement = movement;
            setTurn(samus, turn);
            ushort handler = Word(0x90dd05 + movement * 2);
            bool preserves = handler == 0xddb6 || handler == 0xdd74 && turn == 0 ||
                handler == 0xdd8c && !Transition(pose, false);
            var result = shots.StepFrame(bus, level, samus, (ushort)SnesButton.X, 0, 0, 0, bombs);
            // An admitted nonzero turn handoff forces release before held-input charge
            // processing; with no new Shoot edge this partial charge emits no projectile.
            AssertEqual(preserves ? 10 : turn != 0 ? 0 : 11, shots.FlareCounter, $"Actual projectile HUD charge preservation movement={movement:X2} pose={pose:X2} turn={turn}");
            AssertEqual((int?)null, result.FiredSlot, "Held partial charge does not emit shot");
        }
        Console.WriteLine("Samus HUD definitions: 28 handler words, 12 posture bytes, 512 transition cases, 86016 Grapple admission cases and 392 real charge-preservation cases pass with authored reads forbidden.");
    }

    private sealed class HudPolicyReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address is >= 0x90dd05 and < 0x90dd3d or >= 0x90ddaa and < 0x90ddb6)
                throw new InvalidOperationException($"Compiled HUD policy read ROM ${address:X6}.");
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}

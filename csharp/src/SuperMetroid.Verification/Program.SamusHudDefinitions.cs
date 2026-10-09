using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    /// <summary>Checks compiled HUD movement and posture policies against native handlers and their gameplay consumers.</summary>
    /// <param name="rom">Cartridge address space supplying the movement-handler table and posture policy bytes.</param>
    private static void VerifySamusHudDefinitions(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        bool Transition(byte pose, bool active) => pose >= SamusHudRomData.StandardTransitionStart ||
            (pose < SamusHudRomData.NonFiringTransitionStart &&
                (rom.ReadByte(SamusHudRomData.TransitionFlags + pose -
                    SamusHudRomData.FirstTransitionPose) == 0 || active));
        var metadata = new GrappleFiringReadGuard(rom);
        var bus = new HudPolicyReadGuard(metadata);
        for (int pose = 0; pose <= byte.MaxValue; pose++)
        foreach (bool active in new[] { false, true })
        {
            if (pose < SamusHudRomData.NonFiringTransitionStart)
                AssertEqual(rom.ReadByte(SamusHudRomData.TransitionFlags + pose -
                        SamusHudRomData.FirstTransitionPose),
                    SamusHudDefinitions.PostureObservation((byte)pose),
                    "Complete bounded posture-index observation");
            AssertEqual(Transition((byte)pose, active),
                SamusHudInput.PostureTransitionAdmitsWeapons((byte)pose, active),
                "Every posture pose and active-Grapple branch");
        }
        var samus = new SamusState();
        var setTurn = typeof(SamusState).GetProperty(nameof(SamusState.PoseTransitionShotDirection))!.SetMethod!
            .CreateDelegate<Action<SamusState, ushort>>();
        for (byte movement = 0; movement < 28; movement++)
        {
            ushort handler = Word(0x90dd05 + movement * 2);
            AssertEqual(handler, SamusHudDefinitions.MovementHandler((SamusMovementType)movement), "All 28 native HUD handler identities");
            for (int pose = 0; pose < 253; pose++)
            foreach (bool locked in new[] { false, true })
            foreach (ushort selected in new ushort[] { 3, 4 })
            foreach (ushort turn in new ushort[] { 0, 1, ushort.MaxValue })
            {
                if (rom.ReadByte(SamusMovementRomData.Poses.Definitions + pose * 8 + 1) != movement) continue;
                metadata.SourcePose = samus.Pose = (byte)pose;
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
        for (int poseIndex = 0; poseIndex < 253; poseIndex++)
        foreach (ushort turn in new ushort[] { 0, 1 })
        {
            byte pose = (byte)poseIndex;
            byte movement = rom.ReadByte(SamusMovementRomData.Poses.Definitions + pose * 8 + 1);
            metadata.SourcePose = 1; metadata.Direction = 2;
            samus = new SamusState { Pose = 1, XPosition = 512, YPosition = 512, EquippedBeams = (ushort)SamusBeamFlags.Charge };
            var shots = new SamusProjectileSystem();
            var bombs = new SamusBombProjectileSystem();
            for (int frame = 0; frame < 10; frame++)
                shots.StepFrame(bus, level, samus, (ushort)SnesButton.X, 0, 0, 0, bombs);
            AssertEqual(10, shots.FlareCounter, "Fixture charge earned through real producer");
            metadata.SourcePose = samus.Pose = pose;
            setTurn(samus, turn);
            ushort handler = Word(0x90dd05 + movement * 2);
            bool ball = pose is >= 0x1d and <= 0x1f or 0x31 or 0x32 or 0x41 or >= 0x79 and <= 0x80;
            bool preserves = ball || pose is 0 or 0x9b || handler == 0xddb6 || handler == 0xdd74 && turn == 0 ||
                handler == 0xdd8c && !Transition(pose, false);
            shots.StepFrame(bus, level, samus, (ushort)SnesButton.X, 0, 0, 0, bombs);
            // An admitted nonzero turn handoff forces release before held-input charge
            // processing; with no new Shoot edge this partial charge emits no projectile.
            AssertEqual(preserves ? 10 : turn != 0 ? 0 : 11, shots.FlareCounter, $"Actual projectile HUD charge preservation movement={movement:X2} pose={pose:X2} turn={turn}");
            AssertTrue(shots.Slots.All(slot => !slot.IsActive), "Held partial charge does not emit shot");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => SamusHudDefinitions.PostureObservation(
                SamusHudRomData.NonFiringTransitionStart),
            "posture observation rejects the native prefiltered range");
        Console.WriteLine("Samus HUD definitions: 28 handler words, all 219 bounded posture-index observations, 512 transition cases, 3036 native-pose Grapple admission cases and 506 real charge-preservation cases pass with policy reads forbidden.");
    }

    /// <summary>Prevents consumers from rereading compiled HUD policy tables while forwarding unrelated memory access.</summary>
    /// <param name="source">Underlying mutable address space for reads and writes outside the guarded ROM ranges.</param>
    private sealed class HudPolicyReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        /// <summary>Routes cartridge imports through the guard for compiled HUD policy reads.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The permitted byte from the wrapped address space.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to a compiled HUD policy range.</exception>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Forwards a work-RAM read to the wrapped mutable memory.</summary>
        /// <param name="address">Work-RAM address to read.</param>
        /// <returns>The byte stored at that address.</returns>
        public byte ReadWorkRamByte(int address) =>
            ((ISnesMutableMemory)source).ReadWorkRamByte(address);

        /// <summary>Forwards a save-RAM read to the wrapped mutable memory.</summary>
        /// <param name="address">Save-RAM address to read.</param>
        /// <returns>The byte stored at that address.</returns>
        public byte ReadSaveRamByte(int address) =>
            ((ISnesMutableMemory)source).ReadSaveRamByte(address);

        /// <summary>Rejects reads of the compiled movement-handler and posture-policy ROM ranges.</summary>
        /// <param name="address">Address of the byte requested.</param>
        /// <returns>The wrapped source byte when the address is outside both guarded ranges.</returns>
        /// <exception cref="InvalidOperationException">The address targets a compiled HUD policy byte.</exception>
        public byte ReadByte(int address)
        {
            int postureStart = SamusHudRomData.TransitionFlags -
                SamusHudRomData.FirstTransitionPose;
            int postureEnd = SamusHudRomData.TransitionFlags +
                SamusHudRomData.NonFiringTransitionStart -
                SamusHudRomData.FirstTransitionPose;
            if (address >= SamusHudRomData.MovementHandlers && address < 0x90dd3d ||
                address >= postureStart && address < postureEnd)
                throw new InvalidOperationException($"Compiled HUD policy read ROM ${address:X6}.");
            return source.ReadByte(address);
        }
        /// <summary>Forwards a write unchanged to the wrapped mutable address space.</summary>
        /// <param name="address">Address to write.</param>
        /// <param name="value">Byte value to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}

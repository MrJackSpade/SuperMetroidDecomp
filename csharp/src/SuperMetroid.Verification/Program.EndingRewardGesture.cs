using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyEndingRewardGesture()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        VerifyEndingRewardActorDefinitions(bus);
        VerifyEndingPostShot(bus);
        VerifyEndingLogo(bus);
        VerifyEndingCloudMotion();
        var uploadBus = new EndingRewardUploadDefinitionReadGuard(bus);
        var graphicsUpload = new EndingRewardGraphicsUpload(uploadBus);
        var graphicsVram = new SnesVram();
        byte[] expectedGraphics = RomDataReader.Decompress(bus,
            EndingCreditsRomData.Assets.PostCreditsMode7Characters,
            EndingCreditsRomData.Rendering.DecompressionLimit);
        graphicsVram.LoadBytes(0, Enumerable.Repeat((byte)0xa5, SnesVram.ByteCount).ToArray());
        for (int chunk = 0; chunk < 16; chunk++)
        {
            AssertEqual(RomDataReader.ReadWordFixedBank(bus,
                    EndingRewardGraphicsUploadDefinitions.SourceTable + chunk * sizeof(ushort)),
                EndingRewardGraphicsUploadDefinitions.SourceWord(chunk),
                $"reward graphics source address {chunk} matches pinned cartridge");
            AssertEqual(RomDataReader.ReadWordFixedBank(bus,
                    EndingRewardGraphicsUploadDefinitions.DestinationTable + chunk * sizeof(ushort)),
                EndingRewardGraphicsUploadDefinitions.DestinationWord(chunk),
                $"reward graphics destination address {chunk} matches pinned cartridge");
            graphicsUpload.Upload(graphicsVram, chunk);
            int uploadedBytes = (chunk + 1) * 2048;
            AssertTrue(graphicsVram.Bytes[..uploadedBytes].SequenceEqual(expectedGraphics.AsSpan(0, uploadedBytes)),
                "reward graphics DMA preserves both interleaved lanes at every upload boundary");
            AssertTrue(graphicsVram.Bytes[uploadedBytes..].ToArray().All(value => value == 0xa5),
                "reward graphics upload preserves pending chunks and the shooting OBJ sheet");
        }
        AssertEqual(0, uploadBus.ForbiddenReadAttempts,
            "reward icon upload no longer rereads either native address table");
        AssertThrows<ArgumentOutOfRangeException>(
            () => EndingRewardGraphicsUploadDefinitions.SourceWord(16),
            "reward icon source rejects the seventeenth transfer");
        AssertThrows<ArgumentOutOfRangeException>(
            () => EndingRewardGraphicsUploadDefinitions.DestinationWord(-1),
            "reward icon destination rejects a negative transfer");
        var rewardBus = new EndingRewardDefinitionReadGuard(bus);
        foreach (EndingReward reward in Enum.GetValues<EndingReward>())
        {
            var gesture = new EndingRewardGesture(rewardBus, reward);
            var maps = new HashSet<string>();
            int calls = 0;
            while (!gesture.JumpRequested && calls < 1000)
            {
                gesture.Step();
                calls++;
                maps.Add(Convert.ToHexString(gesture.Draw().LowTable));
            }
            AssertTrue(gesture.JumpRequested, "native reward gesture reaches jumping-actor instruction");
            AssertEqual(reward == EndingReward.Suitless, gesture.SuitlessJumpRequested, "reward-specific jump request");
            AssertTrue(maps.Count >= 8, "reward gesture produces changing OAM, not a static idle pose");
            AssertEqual(reward == EndingReward.Suitless ? 244 : 329, calls,
                "native ED2D/EDD3 gesture durations before jump spawn");
            Console.WriteLine($"  {reward}: {calls} gesture frames, {maps.Count} OAM poses, native jump handoff.");
            var uploads = new List<int>();
            var jump = new EndingRewardJump(rewardBus, reward, uploads.Add);
            bool switched = false, firstMotion = false;
            int minimumY = jump.BodyY;
            int jumpCalls = 0;
            while (!jump.ShotRequested && jumpCalls++ < 500)
            {
                int beforeVelocity = jump.VerticalVelocity;
                jump.Step();
                if (beforeVelocity == -16 * 65536 && !firstMotion)
                {
                    AssertEqual(-16 * 65536 + (reward == EndingReward.Suitless ? 0x3800 : 0x7000),
                        jump.VerticalVelocity, "native jump accelerates once per active actor, before moving");
                    firstMotion = true;
                }
                minimumY = Math.Min(minimumY, jump.BodyY);
                if (!switched && jump.ObjectSelection == 3)
                {
                    AssertTrue(jump.BodyY < -80, "native reward sheet switches only beyond the upper screen edge");
                    switched = true;
                }
            }
            AssertTrue(jump.ShotRequested && switched && firstMotion, "jump executes flight, sheet handoff, landing and shooting");
            AssertEqual((short)136, jump.BodyY, "native reward landing center");
            AssertTrue(uploads.SequenceEqual(Enumerable.Range(0, 16)), "landing publishes all sixteen native graphics queue entries exactly once");
            Console.WriteLine($"  {reward}: jump/landing {jumpCalls} frames, apex {minimumY}, 16 uploads, shot request.");
        }
        AssertEqual(0, rewardBus.ForbiddenReadAttempts,
            "ending reward actors never reread compiled definition records");
    }

    private sealed class EndingRewardUploadDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (address >= EndingRewardGraphicsUploadDefinitions.SourceTable &&
                address < EndingRewardGraphicsUploadDefinitions.DestinationTable +
                EndingRewardJumpDefinitions.UploadCount * sizeof(ushort))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Reward icon upload reread address-table byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private static void VerifyEndingRewardActorDefinitions(ISnesAddressSpace bus)
    {
        static ushort ReadWord(ISnesAddressSpace source, int address) => unchecked((ushort)(
            source.ReadByte(address) | source.ReadByte(address + 1) << 8));

        foreach (ushort pointer in EndingRewardActorDefinitions.KnownDefinitions)
        {
            EndingRewardActorDefinition actual = EndingRewardActorDefinitions.Get(pointer);
            int address = EndingRewardActorDefinitions.NativeDefinitionBank | pointer;
            AssertEqual(ReadWord(bus, address), actual.Initialization,
                $"reward actor $8B:{pointer:X4} initialization callback");
            AssertEqual(ReadWord(bus, address + 2), actual.PreInstruction,
                $"reward actor $8B:{pointer:X4} pre-instruction callback");
            AssertEqual(ReadWord(bus, address + 4), actual.InstructionList,
                $"reward actor $8B:{pointer:X4} initial instruction list");
        }

        AssertThrows<InvalidDataException>(
            () => EndingRewardActorDefinitions.Get(0),
            "unknown reward actor definition");
        Console.WriteLine(
            "  Reward definitions: thirty native callback/list words match the compiled catalog.");
    }

    private sealed class EndingRewardDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        private static readonly HashSet<int> Forbidden = CreateForbidden();

        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (Forbidden.Contains(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Ending reward actor reread definition byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        private static HashSet<int> CreateForbidden()
        {
            var result = new HashSet<int>();
            foreach (ushort pointer in EndingRewardActorDefinitions.KnownDefinitions)
            {
                int address = EndingRewardActorDefinitions.NativeDefinitionBank | pointer;
                for (int offset = 0; offset < 3 * sizeof(ushort); offset++)
                    result.Add(address + offset);
            }
            return result;
        }
    }
}

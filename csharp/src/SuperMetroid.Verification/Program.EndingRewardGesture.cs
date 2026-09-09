using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyEndingRewardGesture()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        var graphicsUpload = new EndingRewardGraphicsUpload(bus);
        var graphicsVram = new SnesVram();
        byte[] expectedGraphics = RomDataReader.Decompress(bus,
            EndingCreditsRomData.Assets.PostCreditsMode7Characters,
            EndingCreditsRomData.Rendering.DecompressionLimit);
        graphicsVram.LoadBytes(0, Enumerable.Repeat((byte)0xa5, SnesVram.ByteCount).ToArray());
        for (int chunk = 0; chunk < 16; chunk++)
        {
            graphicsUpload.Upload(graphicsVram, chunk);
            int uploadedBytes = (chunk + 1) * 2048;
            AssertTrue(graphicsVram.Bytes[..uploadedBytes].SequenceEqual(expectedGraphics.AsSpan(0, uploadedBytes)),
                "reward graphics DMA preserves both interleaved lanes at every upload boundary");
            AssertTrue(graphicsVram.Bytes[uploadedBytes..].ToArray().All(value => value == 0xa5),
                "reward graphics upload preserves pending chunks and the shooting OBJ sheet");
        }
        foreach (EndingReward reward in Enum.GetValues<EndingReward>())
        {
            var gesture = new EndingRewardGesture(bus, reward);
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
            var jump = new EndingRewardJump(bus, reward, uploads.Add);
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
    }
}

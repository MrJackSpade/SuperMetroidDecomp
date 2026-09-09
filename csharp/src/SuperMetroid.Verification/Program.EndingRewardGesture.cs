using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyEndingRewardGesture()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
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
        }
    }
}

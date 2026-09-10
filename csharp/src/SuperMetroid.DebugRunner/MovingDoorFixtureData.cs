/// <summary>Authored spin-turn door boundary and native movement witnesses.</summary>
internal static class MovingDoorFixtureData
{
    /// <summary>Exclusive right boundary of left door column 61; left boundary of right column 66.</summary>
    public static int DoorEdge(bool left) => (left ? 62 : 66) * 16;
    /// <summary>Largest seed gap that triggers at frame one with zero extra speed and immediate turn.</summary>
    public static int LastSuccessfulGap(bool left) => left ? 10 : 9;
    /// <summary>Retained base velocity $0000:E000 at that remote-trigger frame.</summary>
    public const uint TriggerBaseSpeed = 0x0000e000;
}

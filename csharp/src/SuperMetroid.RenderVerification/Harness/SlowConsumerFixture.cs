namespace SuperMetroid.RenderVerification.Harness;

/// <summary>Retail room headers shared by slow-consumer audio and pause-capture checks.</summary>
internal static class SlowConsumerFixture
{
    /// <summary>Retail $8F:A3AE room header: room-local gameplay/pause fixture, matching portable capture tests.</summary>
    internal const ushort AlphaPowerBombRoom = 0xa3ae;
    /// <summary>Retail $8F:CEFB tube room: bounded Maridia liquid/pause coverage.</summary>
    internal const ushort MaridiaTubeRoom = 0xcefb;
}

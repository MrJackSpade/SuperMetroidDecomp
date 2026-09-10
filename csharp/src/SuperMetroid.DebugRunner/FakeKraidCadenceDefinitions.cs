/// <summary>Retail identities and pinned oracle data owned by the Mini-Kraid cadence audit.</summary>
internal static class FakeKraidCadenceDefinitions
{
    /// <summary>Room $8F:A521, the six-screen Mini-Kraid corridor.</summary>
    public const ushort RoomPointer = 0xa521;

    /// <summary>Enemy definition $A0:E0FF, Fake Kraid/Mini-Kraid.</summary>
    public const ushort EnemyDefinition = 0xe0ff;

    /// <summary>SHA-256 of the complete original-CPU 4800-frame cadence trace.</summary>
    public const string NativeTraceSha256 =
        "7B86D0E79091CD79A3CCA5E17EFCF6E3B90E9E53503F4610A37ADB63D2187655";
}

namespace SuperMetroid.Core.Enemies;

/// <summary>Native addresses and sizes of the cartridge's two enemy staging surfaces in bank-$7E WRAM.</summary>
[SnesEncodingOwner]
internal static class EnemyStagingDefinitions
{
    /// <summary><c>$7E:2000</c> <c>EnemyBG2Tilemap</c>, the start of group A.</summary>
    internal const int Bg2NativeAddress = 0x7e2000;

    /// <summary>Group A spans <c>$7E:2000-$7E:2FFF</c>.</summary>
    internal const int Bg2ByteCount = 0x1000;

    /// <summary><c>$7E:9000</c>, the start of the corpse-rotting heap (group B).</summary>
    internal const int RottingHeapNativeAddress = 0x7e9000;

    /// <summary>Group B spans <c>$7E:9000-$7E:97FF</c>.</summary>
    internal const int RottingHeapByteCount = 0x800;

    /// <summary>Mother Brain's six-row corpse tiles occupy <c>$7E:9000-$7E:96FF</c>.</summary>
    internal const int MotherBrainCorpseTileByteCount = 0x700;

    /// <summary>Mother Brain's rot table at <c>$7E:9700</c>, as a heap offset.</summary>
    internal const int MotherBrainRotTableOffset = 0x700;

    /// <summary>Dead Torizo's sand strips at <c>$7E:9500</c>, as a heap offset.</summary>
    internal const int DeadTorizoSandOffset = 0x500;

    /// <summary>The sand strips span <c>$7E:9500-$7E:973F</c>.</summary>
    internal const int DeadTorizoSandByteCount = 0x240;
}

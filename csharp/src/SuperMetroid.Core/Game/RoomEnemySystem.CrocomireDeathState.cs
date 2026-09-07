namespace SuperMetroid.Core.Game;

/// <summary>One literal <c>SpawnHardcodedPLM</c> call made by bank $A4's Crocomire AI.</summary>
public readonly record struct CrocomirePlmRequest(byte BlockX, byte BlockY, ushort Header);

/// <summary>One delayed music request emitted by Crocomire's death graph.</summary>
public readonly record struct CrocomireMusicRequest(MusicCommand Command, MusicCommandDelay Delay);

/// <summary>Boss-specific pickup request emitted by <c>Enemy_ItemDrop_Crocomire</c>.</summary>
public readonly record struct CrocomireDropRequest(
    ushort X,
    ushort Y,
    ushort ItemDropChancesPointer);

/// <summary>
/// Typed host projection of Crocomire's non-slot WRAM. The cartridge stores these values in
/// the shared $0688-$069A words, $7E:7800/$8000 extensions, a $0E00-byte melting-graphics
/// buffer, and a 128-byte per-column height table. Keeping that layout out of unrelated empty
/// enemy slots makes the sequence debuggable without changing any of its 16-bit arithmetic.
/// </summary>
public sealed class CrocomireDeathState
{
    internal const int MeltingColumnCount = 128;
    // $A4:943D contains a shipped off-by-one copy: each nominal $0200-word chunk copies
    // $0201 words, and the seventh second-melt chunk reaches just beyond the authored
    // $0E00-byte image. Keep enough adjacent scratch to preserve that overlap safely.
    internal const int MeltingGraphicsByteCount = 0x1200;
    internal const int Bg2WorkingWordCount = 0x0800;

    private readonly byte[] _meltingColumnHeights = new byte[MeltingColumnCount];
    private readonly byte[] _meltingGraphics = new byte[MeltingGraphicsByteCount];
    private readonly ushort[] _bg2WorkingTilemap = new ushort[Bg2WorkingWordCount];
    private readonly ushort[] _bg2ScrollByScanline = new ushort[256];

    /// <summary>Unaligned $7E:9016 bridge-fragment parameter: 0,2,...,22.</summary>
    public ushort BridgeFragmentCursor { get; internal set; }

    /// <summary>Six-frame acid smoke cadence at $7E:9018.</summary>
    public ushort AcidSmokeTimer { get; internal set; }

    /// <summary>Thirty-two-frame acid damage sound cadence (Crocomire var $20).</summary>
    public ushort AcidSoundTimer { get; internal set; }

    /// <summary>One-shot dust latch for the $0600 bridge approach threshold.</summary>
    public bool BridgeDustAt1536Spawned { get; internal set; }

    /// <summary>One-shot crumble latch for bridge block $61.</summary>
    public bool BridgeBlockAt1568Crumbling { get; internal set; }

    /// <summary>One-shot crumble latch for bridge blocks $62/$63.</summary>
    public bool BridgeBlocksAt1584Crumbling { get; internal set; }

    /// <summary>Offset of the active subtable from $A4:9BC5.</summary>
    public ushort MeltingTableOffset { get; internal set; }

    /// <summary>Eight-byte VRAM record cursor relative to the active transfer subtable.</summary>
    public ushort MeltingTransferOffset { get; internal set; }

    /// <summary>Current byte index into the cartridge's 128-entry random X-order table.</summary>
    public ushort MeltingColumnCursor { get; internal set; }

    /// <summary>Native constant $30 pixels erased from the selected column in one frame.</summary>
    public ushort PixelsToErasePerColumn { get; internal set; }

    /// <summary>Target erased height, also reused as the skeleton-tile loading byte offset.</summary>
    public ushort TargetHeightOrSkeletonTileIndex { get; internal set; }

    /// <summary>Maximum adjusted destination Y from the active melting header.</summary>
    public ushort MaximumAdjustedDestinationY { get; internal set; }

    /// <summary>Current adjusted destination Y used to build the per-scanline distortion.</summary>
    public ushort AdjustedDestinationY { get; internal set; }

    /// <summary>16-bit fractional slope accumulator increment at $7E:0692.</summary>
    public ushort DistortionStep { get; internal set; }

    /// <summary>Height at which the current distortion band stops.</summary>
    public ushort DistortionEndY { get; internal set; }

    /// <summary>Body X captured immediately before each melting distortion starts.</summary>
    public ushort BodyXBeforeMelting { get; internal set; }

    /// <summary>Rumble table byte offset used after the skeleton reaches the left wall.</summary>
    public ushort RumbleIndex { get; internal set; }

    /// <summary>Signed skeleton Y jitter added to its captured X-position word.</summary>
    public ushort RumbleYOffset { get; internal set; }

    /// <summary>Countdown consumed by negative rumble-table entries.</summary>
    public ushort RumbleCooldown { get; internal set; }

    /// <summary>Unsigned per-frame rumble approach delta.</summary>
    public ushort RumbleDelta { get; internal set; }

    /// <summary>True once $A4:9B86 publishes area-miniboss bit $0002.</summary>
    public bool BossBitSet { get; internal set; }

    /// <summary>True once the skeleton-collapse state invokes the boss item-drop routine.</summary>
    public bool ItemDropRequested { get; internal set; }

    /// <summary>Resultant per-column erase heights, exposed read-only to the debugger.</summary>
    public IReadOnlyList<byte> MeltingColumnHeights => _meltingColumnHeights;

    /// <summary>Resultant 256-line BG2 vertical-scroll table used by the melting window.</summary>
    public IReadOnlyList<ushort> Bg2ScrollByScanline => _bg2ScrollByScanline;

    /// <summary>
    /// Whether $A4:9555 has spawned the melting scroll HDMA object and $A4:95CE has
    /// not cleared its channel. Allocated scratch table storage alone does not enable HDMA.
    /// </summary>
    public bool MeltingHdmaActive { get; internal set; }

    internal Span<byte> MutableMeltingColumnHeights => _meltingColumnHeights;
    internal Span<byte> MutableMeltingGraphics => _meltingGraphics;
    internal Span<ushort> MutableBg2WorkingTilemap => _bg2WorkingTilemap;
    internal Span<ushort> MutableBg2ScrollByScanline => _bg2ScrollByScanline;
    internal ReadOnlySpan<byte> MeltingGraphics => _meltingGraphics;
    internal ReadOnlySpan<ushort> Bg2WorkingTilemap => _bg2WorkingTilemap;
}

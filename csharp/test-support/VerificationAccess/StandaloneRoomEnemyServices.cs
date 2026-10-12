using SuperMetroid.Core;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;

/// <summary>
/// Room enemy services for hosts that do not model every cartridge subsystem — focused fixtures and
/// tools. The RNG generator is required; each other service is installed explicitly, and an AI routine
/// that reaches an uninstalled one fails at that exact dependency instead of reading a false default.
/// </summary>
internal sealed class StandaloneRoomEnemyServices : IRoomEnemyServices
{
    private readonly RandomNumbers random;

    /// <summary>Services whose RNG advances with <paramref name="generate"/>.</summary>
    public StandaloneRoomEnemyServices(Func<ushort> generate) => random = new RandomNumbers(Ensure.NotNull(generate));

    /// <inheritdoc />
    public IEnemyRandomNumbers Random => random;

    /// <inheritdoc />
    public IGlobalEventFlags Events { get; init; } = Unavailable.Instance;

    /// <inheritdoc />
    public IAreaBossFlags Bosses { get; init; } = Unavailable.Instance;

    /// <inheritdoc />
    public IRoomPlmPresence Plms { get; init; } = Unavailable.Instance;

    /// <inheritdoc />
    public IRoomScrollZoneAccess RoomScroll { get; init; } = Unavailable.Instance;

    /// <inheritdoc />
    public ISamusControlLock SamusControls { get; init; } = Unavailable.Instance;

    /// <inheritdoc />
    public IMotherBrainRoomEffects MotherBrain { get; init; } = Unavailable.Instance;

    /// <summary>Installs the RNG seed's read and write, which some initializers sample or overwrite.</summary>
    public Func<ushort> ReadRandom { init => random.Read = Ensure.NotNull(value); }

    /// <summary>Installs the RNG seed write.</summary>
    public Action<ushort> SeedRandom { init => random.Write = Ensure.NotNull(value); }

    private sealed class RandomNumbers(Func<ushort> generate) : IEnemyRandomNumbers
    {
        internal Func<ushort>? Read;
        internal Action<ushort>? Write;

        public ushort Generate() => generate();

        ushort IEnemyRandomNumbers.Read() => (Read ?? throw Unavailable.Missing("read the current random-number word"))();

        public void Seed(ushort value) => (Write ?? throw Unavailable.Missing("seed the random-number word"))(value);
    }

    /// <summary>The services a standalone host does not provide: every call fails at the dependency.</summary>
    private sealed class Unavailable : IGlobalEventFlags, IAreaBossFlags, IRoomPlmPresence, IRoomScrollZoneAccess,
        ISamusControlLock, IMotherBrainRoomEffects
    {
        internal static readonly Unavailable Instance = new();

        internal static InvalidOperationException Missing(string operation) => new(
            $"Room enemy AI attempted to {operation}, but this host's RoomEnemyLoadRequest did not install " +
            "the required cartridge-state service.");

        public bool Has(EventNumber eventNumber) => throw Missing("read global event bits");
        public void Set(EventNumber eventNumber) => throw Missing("set a global event bit");
        public void Clear(EventNumber eventNumber) => throw Missing("clear a global event bit");
        public bool IsDefeated(BossBits bits) => throw Missing($"read the area {bits} flag");
        public void SetDefeated(BossBits bits) => throw Missing($"set the area {bits} flag");
        public bool HasActiveHeader(PlmHeaderId header) => throw Missing("query the active room PLM population");
        public RoomScrollState Read(int index) => throw Missing("read a room scroll state");
        public void Write(int index, RoomScrollState state) => throw Missing("write a room scroll state");
        public void SetControlsEnabled(bool enabled) => throw Missing("change Samus control enablement");
        public void IncrementGlassRoomArgument() => throw Missing("advance Mother Brain's glass-room argument");
        public void SetLayerBlendingDefault(LayerBlendingConfiguration configuration) =>
            throw Missing("write Mother Brain's layer-blending configuration");
        public void SetBg2Scroll(Position16<Bg2Pixels> scroll) => throw Missing("write Mother Brain's BG2 scroll registers");
    }
}

using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// Room enemy services for hosts that do not model every cartridge subsystem — focused fixtures and
/// tools. The RNG generator is required; each other service is installed explicitly, and an AI routine
/// that reaches an uninstalled one fails at that exact dependency instead of reading a false default.
/// </summary>
internal sealed class StandaloneRoomEnemyServices : IRoomEnemyServices
{
    /// <summary>Only RNG generation is mandatory; reading or seeding its current word needs explicit delegates.</summary>
    private readonly RandomNumbers random;

    /// <summary>The installed target of words written over BG1, or null when the host provides none.</summary>
    private readonly Action<int, ushort[]>? bg1Overlay;

    /// <summary>The installed target of words written over BG2, or null when the host provides none.</summary>
    private readonly Action<int, ushort[]>? bg2Overlay;

    /// <summary>The installed Mode 7 map, or null when the host provides none.</summary>
    private readonly FloorMap? floorMap;

    /// <summary>Services whose RNG advances with <paramref name="generate"/>.</summary>
    public StandaloneRoomEnemyServices(Func<int> generate) => random = new RandomNumbers(Ensure.NotNull(generate));

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
    public Func<int> ReadRandom { init => random.Read = Ensure.NotNull(value); }

    /// <summary>Installs the RNG seed write.</summary>
    public Action<int> SeedRandom { init => random.Write = Ensure.NotNull(value); }

    /// <summary>Installs the target of words written over BG1, which the Ceres escape's warning text writes.</summary>
    public Action<int, ushort[]> DrawBg1OverlayWords { init => bg1Overlay = Ensure.NotNull(value); }

    /// <inheritdoc />
    public void DrawBg1Overlay(int firstWord, ReadOnlySpan<ushort> words) =>
        (bg1Overlay ?? throw Unavailable.Missing("write words over BG1"))(firstWord, words.ToArray());

    /// <summary>Installs the Mode 7 map the Ceres door and Ridley's getaway write.</summary>
    public FloorMap FloorMap { init => floorMap = Ensure.NotNull(value); }

    /// <inheritdoc />
    public void DrawFloorMap(int firstCell, ReadOnlySpan<byte> tiles) =>
        (floorMap ?? throw Unavailable.Missing("write the room's Mode 7 map")).Write(firstCell, tiles);

    /// <summary>Installs the target of words written over BG2, which the Zebes escape typewriter writes.</summary>
    public Action<int, ushort[]> DrawBg2OverlayWords { init => bg2Overlay = Ensure.NotNull(value); }

    /// <inheritdoc />
    public void DrawBg2Overlay(int firstWord, ReadOnlySpan<ushort> words) =>
        (bg2Overlay ?? throw Unavailable.Missing("write words over BG2"))(firstWord, words.ToArray());

    /// <summary>Routes generated values through a required fixture callback and current-word access through optional callbacks.</summary>
    private sealed class RandomNumbers(Func<int> generate) : IEnemyRandomNumbers
    {
        /// <summary>Optional source for the current RNG seed without advancing it.</summary>
        internal Func<int>? Read;
        /// <summary>Optional sink for writes to the current RNG seed.</summary>
        internal Action<int>? Write;

        /// <summary>Advances the host-supplied RNG generator exactly once.</summary>
        public int Generate() => generate();

        /// <summary>Reads a configured seed source or fails at the missing RNG dependency.</summary>
        int IEnemyRandomNumbers.Read() => (Read ?? throw Unavailable.Missing("read the current random-number word"))();

        /// <summary>Writes through a configured seed sink or fails at the missing RNG dependency.</summary>
        public void Seed(int value) => (Write ?? throw Unavailable.Missing("seed the random-number word"))(value);
    }

    /// <summary>The services a standalone host does not provide: every call fails at the dependency.</summary>
    private sealed class Unavailable : IGlobalEventFlags, IAreaBossFlags, IRoomPlmPresence, IRoomScrollZoneAccess,
        ISamusControlLock, IMotherBrainRoomEffects
    {
        /// <summary>Shared failure object for every cartridge-state capability absent from this standalone host.</summary>
        internal static readonly Unavailable Instance = new();

        /// <summary>Names the missing room-enemy service operation at the point an AI routine requests it.</summary>
        internal static InvalidOperationException Missing(string operation) => new(
            $"Room enemy AI attempted to {operation}, but this host's RoomEnemyLoadRequest did not install " +
            "the required cartridge-state service.");

        /// <summary>Fails when an actor queries event bits without an installed event service.</summary>
        public bool Has(EventNumber eventNumber) => throw Missing("read global event bits");
        /// <summary>Fails when an actor tries to publish an event without an installed service.</summary>
        public void Set(EventNumber eventNumber) => throw Missing("set a global event bit");
        /// <summary>Fails when an actor tries to clear an event without an installed service.</summary>
        public void Clear(EventNumber eventNumber) => throw Missing("clear a global event bit");
        /// <summary>Fails when boss status is read without an installed area-boss service.</summary>
        public bool IsDefeated(BossBits bits) => throw Missing($"read the area {bits} flag");
        /// <summary>Fails when boss status is written without an installed area-boss service.</summary>
        public void SetDefeated(BossBits bits) => throw Missing($"set the area {bits} flag");
        /// <summary>Fails when AI inspects room PLM population without an installed presence service.</summary>
        public bool HasActiveHeader(PlmHeaderId header) => throw Missing("query the active room PLM population");
        /// <summary>Fails when AI reads scroll-zone state without an installed room-scroll service.</summary>
        public RoomScrollState Read(int index) => throw Missing("read a room scroll state");
        /// <summary>Fails when AI changes a scroll zone without an installed room-scroll service.</summary>
        public void Write(int index, RoomScrollState state) => throw Missing("write a room scroll state");
        /// <summary>Fails when AI enables Samus's controls without an installed lock service.</summary>
        public void EnableControls() => throw Missing("enable Samus's controls");
        /// <summary>Fails when AI disables Samus's controls without an installed lock service.</summary>
        public void DisableControls() => throw Missing("disable Samus's controls");
        /// <summary>Fails when Mother Brain glass setup lacks its room-effects service.</summary>
        public void IncrementGlassRoomArgument() => throw Missing("advance Mother Brain's glass-room argument");
        /// <summary>Fails when Mother Brain changes blending without an installed room-effects service.</summary>
        public void SetLayerBlendingDefault(LayerBlendingConfiguration configuration) =>
            throw Missing("write Mother Brain's layer-blending configuration");
    }
}

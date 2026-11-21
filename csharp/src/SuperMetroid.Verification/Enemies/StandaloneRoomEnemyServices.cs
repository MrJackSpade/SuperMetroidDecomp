using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// Room enemy services for hosts that do not model every cartridge subsystem — focused fixtures and
/// tools. The RNG generator is required; each other service is installed explicitly, and an AI routine
/// that reaches an uninstalled one fails at that exact dependency instead of reading a false default.
/// </summary>
[ArchitectureReview("context-bag", "A verification host's implementation of the room enemy services contract, which is a set of service capabilities by definition; each capability is installed explicitly or fails at the dependency.")]
internal sealed class StandaloneRoomEnemyServices : IRoomEnemyServices
{
    /// <summary>Only RNG generation is mandatory; reading or seeding its current word needs explicit delegates.</summary>
    private readonly RandomNumbers random;

    /// <summary>Services whose RNG advances with <paramref name="generate"/>.</summary>
    public StandaloneRoomEnemyServices(Func<int> generate) => random = new RandomNumbers(Ensure.NotNull(generate));

    /// <inheritdoc />
    public IEnemyRandomNumbers Random => random;

    /// <inheritdoc />
    public IStoryEvents Events { get; init; } = Unavailable.Instance;

    /// <inheritdoc />
    public IZebetiteProgress ZebetiteProgress { get; init; } = Unavailable.Instance;

    /// <inheritdoc />
    public IDefeatedBossesRecorder Bosses { get; init; } = Unavailable.Instance;

    /// <inheritdoc />
    public IRoomObjectPresence RoomObjects { get; init; } = Unavailable.Instance;

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

    /// <summary>Routes generated values through a required fixture callback and current-word access through optional callbacks.</summary>
    private sealed class RandomNumbers(Func<int> generate) : IEnemyRandomNumbers
    {
        /// <summary>Optional source for the current RNG seed without advancing it.</summary>
        internal Func<int>? Read;
        /// <summary>Optional sink for writes to the current RNG seed.</summary>
        internal Action<int>? Write;

        /// <summary>Advances the host-supplied RNG generator exactly once.</summary>
        public int Generate() => generate();

        /// <summary>Advances the host-supplied RNG generator exactly once, its value unread.</summary>
        public void Advance() => _ = generate();

        /// <summary>Reads a configured seed source or fails at the missing RNG dependency.</summary>
        int IEnemyRandomNumbers.Read() => (Read ?? throw Unavailable.Missing("read the current random-number word"))();

        /// <summary>Writes a species' seed through a configured seed sink or fails at the missing RNG dependency.</summary>
        public void Reseed(SpeciesRandomReseed reseed) =>
            (Write ?? throw Unavailable.Missing("seed the random-number word"))(Ensure.NotNull(reseed).Seed.Value);
    }

    /// <summary>The services a standalone host does not provide: every call fails at the dependency.</summary>
    private sealed class Unavailable : IStoryEvents, IZebetiteProgress, IDefeatedBossesRecorder, IRoomObjectPresence,
        IRoomScrollZoneAccess, ISamusControlLock, IMotherBrainRoomEffects
    {
        /// <summary>Shared failure object for every cartridge-state capability absent from this standalone host.</summary>
        internal static readonly Unavailable Instance = new();

        /// <summary>Names the missing room-enemy service operation at the point an AI routine requests it.</summary>
        internal static InvalidOperationException Missing(string operation) => new(
            $"Room enemy AI attempted to {operation}, but this host's RoomEnemyLoadRequest did not install " +
            "the required cartridge-state service.");

        /// <summary>Fails when an actor queries event bits without an installed event service.</summary>
        public bool Has(StoryEvent storyEvent) => throw Missing("read story events");
        /// <summary>Fails when an actor tries to publish an event without an installed service.</summary>
        public void Set(StoryEvent storyEvent) => throw Missing("mark a story event");
        /// <summary>Fails when a Zebetite reads or advances the Zebetite count without an installed progress service.</summary>
        public int ZebetitesDestroyed
        {
            get => throw Missing("read the Zebetites destroyed");
            set => throw Missing("advance the Zebetites destroyed");
        }
        /// <summary>Fails when boss status is read without an installed area-boss service.</summary>
        public bool IsDefeated(DefeatedBosses bosses) => throw Missing($"read the area {bosses} flag");
        /// <summary>Fails when boss status is written without an installed area-boss service.</summary>
        public void SetDefeated(DefeatedBosses bosses) => throw Missing($"set the area {bosses} flag");
        /// <summary>Fails when AI asks after the statue's hand without an installed room-object service.</summary>
        public bool BombTorizoHandRemains => throw Missing("ask whether the statue's hand still stands");
        /// <summary>Fails when AI changes a screen's scroll state without an installed room-scroll service.</summary>
        public void Write(Position16<RoomScreens> screen, RoomScrollState state) => throw Missing("write a screen's scroll state");
        /// <summary>Fails when AI enables Samus's controls without an installed lock service.</summary>
        public void EnableControls() => throw Missing("enable Samus's controls");
        /// <summary>Fails when AI disables Samus's controls without an installed lock service.</summary>
        public void DisableControls() => throw Missing("disable Samus's controls");
        /// <summary>Fails when Mother Brain glass setup lacks its room-effects service.</summary>
        public void CountHeadShotOnGlass() => throw Missing("count a head shot on Mother Brain's glass");
    }
}

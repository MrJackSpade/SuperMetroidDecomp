namespace SuperMetroid.Core.Game;

/// <summary>
/// Native function pointers used by Mother Brain's body dispatcher in bank <c>$A9</c>.
/// Keeping the cartridge addresses as enum values makes a debugger watch line up with the
/// disassembly without pretending that unrelated phases are interchangeable host states.
/// </summary>
public enum MotherBrainBodyFunction : ushort
{
    FirstPhase = 0x87e1,
    FakeDeathDescentInitialPause = 0x881d,
    FakeDeathDescentPauseBeforeLock = 0x8829,
    FakeDeathDescentPauseBeforeMusic = 0x884d,
    FakeDeathDescentPauseBeforeUnlock = 0x886c,
    FakeDeathDescentPauseBeforeFlash = 0x8884,
    FakeDeathDescentFadeToGray = 0x88b2,
    FakeDeathDescentCollapseTubes = 0x88d3,
    FakeDeathAscentDrawRows2And3 = 0x8c87,
    FakeDeathAscentDrawRows4And5 = 0x8c9e,
    FakeDeathAscentDrawRows6And7 = 0x8cb5,
    FakeDeathAscentDrawRows8And9 = 0x8ccc,
    FakeDeathAscentDrawRowsAAndB = 0x8ce3,
    FakeDeathAscentDrawRowsCAndD = 0x8cfa,
    FakeDeathAscentSetupPhase2Graphics = 0x8d11,
    FakeDeathAscentSetupPhase2Brain = 0x8d49,
}

/// <summary>
/// Native function pointers stored in the head record's secondary Mother Brain dispatcher.
/// During fake death this independent state machine serializes the physical tube actors,
/// ceiling projectiles, and one-frame bank-$84 room mutations.
/// </summary>
public enum MotherBrainTubeCollapseFunction : ushort
{
    WaitForFourFreeProjectileSlots = 0x8949,
    ClearBottomLeftTube = 0x896e,
    SpawnTopRightTube = 0x8983,
    ClearCeilingColumn9 = 0x89a0,
    SpawnTopLeftTube = 0x89b5,
    ClearCeilingColumn6 = 0x89d2,
    SpawnBottomRightTube = 0x89e7,
    ClearBottomRightTube = 0x89fa,
    SpawnBottomMiddleLeftTube = 0x8a0f,
    ClearBottomMiddleLeftTube = 0x8a22,
    SpawnTopMiddleLeftTube = 0x8a37,
    ClearCeilingColumn7 = 0x8a54,
    SpawnTopMiddleRightTube = 0x8a69,
    ClearCeilingColumn8 = 0x8a86,
    SpawnBottomMiddleRightTube = 0x8a9b,
    ClearBottomMiddleRightTube = 0x8aae,
    SpawnMainTube = 0x8ac3,
    ClearBottomMiddleTubes = 0x8ad6,
    Finished = 0x8ae4,
}

/// <summary>Native function pointers used by Mother Brain's separate brain record.</summary>
public enum MotherBrainBrainFunction : ushort
{
    SetupBrainAndNeckToBeDrawn = 0x87a2,
    SetupBrainToBeDrawn = 0x87d0,
}

/// <summary>
/// Shared state behind retail enemy records <c>$EC7F</c> (body) and <c>$EC3F</c> (brain).
/// The SNES stores most of these words in named extended WRAM rather than in either common
/// <c>$40</c>-byte enemy slot. A dedicated object therefore preserves both the physical slot
/// boundary and the original one-owner/two-record relationship.
/// </summary>
public sealed class MotherBrainEnemyState
{
    internal MotherBrainEnemyState(RoomEnemySlot body) => Body = body;

    /// <summary>Physical slot zero, enemy definition <c>$EC7F</c>.</summary>
    public RoomEnemySlot Body { get; }

    /// <summary>Physical slot one, enemy definition <c>$EC3F</c>.</summary>
    public RoomEnemySlot? Head { get; internal set; }

    /// <summary>
    /// Exact <c>$7E:9000/$7E:9700</c> corpse graphics and rot-table producer initialized by
    /// the head record even though it is not consumed until the much later death sequence.
    /// </summary>
    public MotherBrainCorpseRottingState CorpseRotting { get; } = new();

    /// <summary>Mother Brain form word: zero is glass/first phase, one begins fake death.</summary>
    public ushort Form { get; internal set; }

    /// <summary>Native shared hitbox-enable word, initialized to two.</summary>
    public ushort HitboxesEnabled { get; internal set; }

    public MotherBrainBodyFunction Function { get; internal set; }
    public MotherBrainBrainFunction BrainFunction { get; internal set; }

    /// <summary>
    /// Native <c>mbn_var_F</c>. Every fake-death pause decrements this unsigned word and
    /// branches when bit 15 becomes set; zero therefore expires immediately to $FFFF.
    /// </summary>
    public ushort FunctionTimer { get; internal set; }

    /// <summary>Zero-based palette-step count stored in native <c>mbn_var_37</c>.</summary>
    public ushort GrayFadeIndex { get; internal set; }

    /// <summary>Eight-frame cadence and wrapping coordinate cursor for fake-death dust.</summary>
    public ushort FakeDeathExplosionTimer { get; internal set; }
    public ushort FakeDeathExplosionIndex { get; internal set; }

    /// <summary>
    /// Bank-$A9 room-palette bytecode pointer/timer. The timer counts upward, unlike enemy
    /// instruction timers, because handler $D192 compares elapsed frames with each duration.
    /// </summary>
    public ushort RoomPaletteInstructionPointer { get; internal set; }
    public ushort RoomPaletteInstructionTimer { get; internal set; }

    /// <summary>Head-record sub-dispatch and its independent underflow timer.</summary>
    public MotherBrainTubeCollapseFunction TubeCollapseFunction { get; internal set; }
    public ushort TubeCollapseTimer { get; internal set; }

    /// <summary>Exact delayed music writes made during the most recent enemy frame.</summary>
    public IReadOnlyList<MotherBrainMusicRequest> MusicRequests => _musicRequests;

    /// <summary>Hardcoded bank-$84 objects requested during the most recent enemy frame.</summary>
    public IReadOnlyList<MotherBrainPlmRequest> PlmRequests => _plmRequests;

    /// <summary>Last library-two fake-death/tube sound emitted during this enemy frame.</summary>
    public ushort? LastSoundEffect { get; internal set; }

    /// <summary>Count of dynamically spawned physical falling-tube enemy records.</summary>
    public int SpawnedFallingTubeCount { get; internal set; }

    /// <summary>FX table entry requested by the body initializer.</summary>
    public ushort FxEntry { get; internal set; }

    /// <summary>Whether the unpause hook must restore Mother Brain's BG2 image and beam SFX.</summary>
    public bool EnableUnpauseHook { get; internal set; }

    /// <summary>True after all <c>$800</c> enemy-BG2 words have been filled with tile <c>$0338</c>.</summary>
    public bool BackgroundTilemapPrepared { get; internal set; }

    /// <summary>Palette selector shared by the four neck segments.</summary>
    public ushort NeckPaletteIndex { get; internal set; }

    /// <summary>Palette selector applied by the custom brain drawing routine.</summary>
    public ushort BrainPaletteIndex { get; internal set; }

    /// <summary>Countdown used by the normal head-palette setup routine, initially ten.</summary>
    public ushort BrainPaletteTimer { get; internal set; }

    /// <summary>Earthquake timer copied into the head-shake word after the glass event.</summary>
    public ushort BrainMainShakeTimer { get; internal set; }

    /// <summary>Shared flag that asks Mother Brain Rinkas and turrets to remove themselves.</summary>
    public bool DeleteTurretsAndRinkas { get; internal set; }

    /// <summary>
    /// Set by the head main on frames where the cartridge's enemy-graphics-drawn hook points
    /// at <c>$A9:87DD</c>. It is deliberately rebuilt every frame rather than treated as
    /// ordinary visibility because both physical records carry property bit <c>$0100</c>.
    /// </summary>
    public bool DrawBrain { get; internal set; }

    /// <summary>
    /// Parameters passed to the twelve <c>$86</c> Mother Brain turret initializers during
    /// body load. The matching actors occupy the shared eighteen-slot projectile pool; this
    /// retained request list makes their native creation order explicit in debugger state.
    /// </summary>
    public IReadOnlyList<ushort> InitialTurretParameters => _initialTurretParameters;

    private readonly ushort[] _initialTurretParameters = new ushort[12];
    private readonly List<MotherBrainMusicRequest> _musicRequests = new();
    private readonly List<MotherBrainPlmRequest> _plmRequests = new();

    internal void RecordInitialTurretRequests()
    {
        for (ushort parameter = 0; parameter < _initialTurretParameters.Length; parameter++)
            _initialTurretParameters[parameter] = parameter;
    }

    internal void BeginFrame()
    {
        _musicRequests.Clear();
        _plmRequests.Clear();
        LastSoundEffect = null;
    }

    internal void RequestMusic(ushort rawTrack, byte delayFrames) =>
        _musicRequests.Add(new MotherBrainMusicRequest(rawTrack, delayFrames));

    internal void RequestPlm(byte blockX, byte blockY, ushort header) =>
        _plmRequests.Add(new MotherBrainPlmRequest(blockX, blockY, header));
}

/// <summary>
/// One raw <c>QueueMusic_Delayed8</c> call. Values such as $FF21 are retained whole because
/// their high byte is a command, not a host track number.
/// </summary>
public readonly record struct MotherBrainMusicRequest(ushort RawTrack, byte DelayFrames);

/// <summary>One literal <c>SpawnHardcodedPLM</c> call issued by Mother Brain's bank-$A9 AI.</summary>
public readonly record struct MotherBrainPlmRequest(byte BlockX, byte BlockY, ushort Header);

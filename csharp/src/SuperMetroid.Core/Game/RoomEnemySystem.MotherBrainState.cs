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
    /// body load. These are observable spawn requests, not a claim that their bank-$86
    /// movement pool has already been translated.
    /// </summary>
    public IReadOnlyList<ushort> InitialTurretParameters => _initialTurretParameters;

    private readonly ushort[] _initialTurretParameters = new ushort[12];

    internal void RecordInitialTurretRequests()
    {
        for (ushort parameter = 0; parameter < _initialTurretParameters.Length; parameter++)
            _initialTurretParameters[parameter] = parameter;
    }
}

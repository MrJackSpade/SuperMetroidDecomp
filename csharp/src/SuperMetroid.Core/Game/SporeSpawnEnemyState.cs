using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The five bank-$A5 functions stored in Spore Spawn's native variable A.
/// </summary>
public enum SporeSpawnFunction : ushort
{
    /// <summary><c>RTS_A5EB1A</c> at <c>$A5:EB1A</c>; performs no main-AI movement while instruction lists control the idle or open phase.</summary>
    Idle = 0xeb1a,
    /// <summary><c>Function_SporeSpawn_Descent</c> at <c>$A5:EB1B</c>; lowers the body one room pixel per AI update until Y reaches 624 and installs the fight-start list.</summary>
    Descending = 0xeb1b,
    /// <summary><c>Function_SporeSpawn_Moving</c> at <c>$A5:EB52</c>; computes the phase-driven body sway about its spawn center and updates stalk segment positions.</summary>
    Moving = 0xeb52,
    /// <summary><c>Function_SporeSpawn_SetupDeath</c> at <c>$A5:EB9B</c>; computes the fixed death-drift angle and velocity toward room position (128,624), without moving the body.</summary>
    SetupDeath = 0xeb9b,
    /// <summary><c>Function_SporeSpawn_Dying</c> at <c>$A5:EBEE</c>; applies death drift and ceiling dust, returning to idle when both axis distances from (128,624) are below eight pixels.</summary>
    Dying = 0xebee,
}

/// <summary>
/// Debugger-visible state that the cartridge stores beyond Spore Spawn's common enemy words.
/// Naming these fields keeps the boss's angle, stalk anchor, and death velocity from being
/// hidden in unrelated generic variables while retaining their native 16-bit arithmetic.
/// </summary>
public sealed class SporeSpawnEnemyState
{
    /// <summary>Staged CGRAM colors published by Spore Spawn's palette update.</summary>
    internal readonly Bgr555[] _targetPalette = new Bgr555[256];
    /// <summary>Marks staged palette slots so unrelated colors remain untouched.</summary>
    private readonly bool[] _targetPaletteWritten = new bool[256];

    /// <summary>Binds boss-specific state to its live common enemy slot.</summary>
    internal SporeSpawnEnemyState(RoomEnemySlot body) => Body = body;

    /// <summary>Live common enemy slot owned by the room enemy system; shared by movement, combat, instruction, and stalk-projectile owners.</summary>
    public RoomEnemySlot Body { get; }
    /// <summary>Current bank-$A5 main-AI pointer, corresponding to native variable A at WRAM $0FA8.</summary>
    public SporeSpawnFunction Function { get; internal set; }
    /// <summary>
    /// Fixed stalk interpolation anchor in room pixels: the spawn body's X and a Y 72 pixels
    /// above it; native <c>stalkXOrigin</c>/<c>stalkYOrigin</c> at $7E:7808/$7E:780A.
    /// </summary>
    public Position16<RoomPixels> StalkAnchor { get; internal set; }
    /// <summary>
    /// Original body position in room pixels, the sway centre, retained before the live spawn
    /// is raised 128 pixels; native <c>XOrigin</c>/<c>YOrigin</c> at WRAM $0FAC/$0FAE.
    /// </summary>
    public Position16<RoomPixels> MovementCenter { get; internal set; }
    /// <summary>Byte-wrapped sway phase stored in a word at $7E:7814, with 256 units per cycle; X samples cosine and Y samples the doubled, shifted phase.</summary>
    public ushort Angle { get; internal set; }
    /// <summary>Horizontal sway amplitude in room pixels, native $7E:7816; vertical amplitude is this value minus sixteen.</summary>
    public ushort MaximumXRadius { get; internal set; }
    /// <summary>Signed angular increment per movement update, native $7E:7818; hits may reverse its sign or increase its magnitude from one to two below 400 health, with movement using the low byte.</summary>
    public ushort AngleDelta { get; internal set; }
    /// <summary>Native $7E:9000 emitter gate: zero permits ceiling spawner countdowns, while nonzero suppresses generation during protected instruction phases.</summary>
    public ushort SporeGenerationFlag { get; internal set; }
    /// <summary>Native $7E:801E one-hit guard for an open phase; nonzero prevents repeated direction reversal and close-list installation until the clear-damaged opcode resets it.</summary>
    public ushort DamagedFlag { get; internal set; }
    /// <summary>Health value last recorded by the guarded damaging-hit palette update, native $7E:8800; used to avoid reloading an unchanged health palette.</summary>
    public ushort PreviousHealth { get; internal set; }
    /// <summary>Fixed byte-sized death-drift direction toward (128,624), stored at $7E:8806; zero is right, $40 up, $80 left, and $C0 down.</summary>
    public ushort DeathAngle { get; internal set; }
    /// <summary>
    /// Unsigned 16.16 pixels-per-update death-drift magnitude per axis, combining native death
    /// speed/subspeed words $7E:8010/$8012 (X) and $7E:8014/$8016 (Y); <see cref="DeathAngle"/>
    /// supplies each axis's movement sign.
    /// </summary>
    public FixedVector<RoomPixels> DeathVelocityMagnitude { get; internal set; }
    /// <summary>Whether the live fight's scrolling-finished hook still enforces the arena's minimum camera Y; cleared at death and never installed for an already defeated load.</summary>
    public bool ScrollClampHookActive { get; internal set; }
    /// <summary>Host guard recording that zero health has already triggered projectile cleanup, the defeated flag, death instructions, and the ceiling-crumble request.</summary>
    public bool DeathStarted { get; internal set; }
    /// <summary>Whether the death instruction has published its sixteen boss item-drop attempts; separate from individual destroyed-spore drops and actual drop-pool allocation success.</summary>
    public bool DeathDropRequested { get; internal set; }
    /// <summary>Defeated-area miniboss flag sampled at room initialization; selects the solid dead body and cleared ceiling instead of the live fight setup.</summary>
    public bool LoadedAsDefeated { get; internal set; }

    /// <summary>Stages one palette color for the next frame effect publication.</summary>
    internal void WriteTargetColor(int index, Bgr555 value)
    {
        _targetPalette[index] = value;
        _targetPaletteWritten[index] = true;
    }

    /// <summary>Publishes only staged colors and clears their pending markers afterward.</summary>
    internal void ConsumeTargetColors(Action<int, Bgr555> write)
    {
        for (int index = 0; index < _targetPalette.Length; index++)
        {
            if (!_targetPaletteWritten[index]) continue;
            write(index, _targetPalette[index]);
            _targetPaletteWritten[index] = false;
        }
    }
}

/// <summary>One hardcoded bank-$84 ceiling mutation published by Spore Spawn.</summary>
/// <param name="Block">Zero-based room block; the native ceiling request uses column seven, row thirty.</param>
/// <param name="Header">Bank-relative PLM header offset in bank $84, selecting the ceiling clear or crumble operation.</param>
public readonly record struct SporeSpawnPlmRequest(Position16<RoomBlocks> Block, PlmHeaderId Header);

/// <summary>Frame-local marker for one item-drop attempt from a destroyed spore or the boss's death instruction.</summary>
/// <remarks>Contains no position or table selector: the publisher also invokes drop spawning immediately, using stalk header $DF7F for a spore or body header $DF3F for one of the sixteen death drops.</remarks>
public readonly record struct SporeSpawnDropRequest();

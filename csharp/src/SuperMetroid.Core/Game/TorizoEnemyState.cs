using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Debugger-visible state that extends Bomb Torizo's ordinary $40-byte enemy slot. The
/// original actor aliases the bank-$AA boss variable arrays at $7800-$781F; spelling those
/// words out here keeps the state inspectable without pretending unrelated common slot
/// fields have the same meaning.
/// </summary>
public sealed class TorizoEnemyState
{
    /// <summary>Attaches variant-specific boss variables to the existing physical enemy slot.</summary>
    internal TorizoEnemyState(RoomEnemySlot slot, bool isGolden)
    {
        Slot = slot;
        IsGolden = isGolden;
    }

    /// <summary>The owning room enemy body slot, whose common position, health, properties, and instruction state this extension supplements.</summary>
    public RoomEnemySlot Slot { get; }
    /// <summary>Whether the owning definition is Golden Torizo ($EF7F), rather than Bomb Torizo ($EEFF); fixed at initialization.</summary>
    public bool IsGolden { get; }

    /// <summary>Persistent target colors and numerator shared by awakening and death fades.</summary>
    internal CartridgePaletteTransition? PaletteTransition { get; set; }

    /// <summary>Native <c>toriz_var_00</c>: instruction-list return/landing target.</summary>
    public ushort ReturnInstruction { get; internal set; }

    /// <summary>Native <c>toriz_var_01</c>: interrupted list restored after head damage.</summary>
    public ushort InterruptedInstruction { get; internal set; }

    /// <summary>Native <c>toriz_var_03</c>: delayed in-air transition counter.</summary>
    public ushort AirTransitionTimer { get; internal set; }

    /// <summary>
    /// Native <c>toriz_var_04</c>. Nonzero frames reject shots before common damage AI.
    /// The awakening list writes the cartridge sentinel $7777 and later clears it.
    /// </summary>
    public ushort ShotGuard { get; internal set; }

    /// <summary>Native <c>toriz_var_09</c>: animation/decision repetition counter.</summary>
    public ushort DecisionCounter { get; internal set; }

    /// <summary>Native <c>toriz_var_05</c>: last missile-family word seen by Golden Torizo.</summary>
    public ushort CapturedProjectileFamily { get; internal set; }

    /// <summary>Native <c>toriz_var_06</c>: Golden Torizo egg/attack-list flag word.</summary>
    public ushort AttackFlags { get; internal set; }

    /// <summary>Native <c>toriz_var_07</c>: consecutive frames Samus has space-jumped.</summary>
    public ushort SamusSpaceJumpFrames { get; internal set; }

    /// <summary>
    /// Signed 8.8 jump velocity words. The walking instructions reuse the native X word for a
    /// signed whole-pixel walk delta, which they read as whole pixels.
    /// </summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>Signed 8.8 vertical acceleration.</summary>
    public ushort VerticalAcceleration { get; internal set; }

    /// <summary>Native <c>toriz_var_E</c> main-function dispatcher pointer.</summary>
    public TorizoFunction Function { get; internal set; }

    /// <summary>Native <c>toriz_var_F</c> active-state pre-instruction pointer.</summary>
    public TorizoPreInstruction PreInstruction { get; internal set; }

    /// <summary>Host lifecycle latch set when $AA:C6C6 observes the $D6EA hand-trigger PLM gone and resumes Bomb Torizo's awakening instruction list.</summary>
    public bool AwakeningReleased { get; internal set; }
    /// <summary>Host latch preventing repeat entry into death list $AA:B1C8 after lethal damage; remains set for this state instance.</summary>
    public bool DeathStarted { get; internal set; }
    /// <summary>Whether the area's Torizo defeat bit ($04) was already set at initialization or was set by the translated $AA:B24D death instruction.</summary>
    public bool BossBitSet { get; internal set; }
    /// <summary>Host latch recording the death instruction's 16-pickup scatter request in the variant's arena; does not mean those pickups were collected.</summary>
    public bool ItemDropRequested { get; internal set; }
}

/// <summary>Music request produced by Bomb Torizo's ROM instruction stream.</summary>
/// <param name="Command">Track or music-data command to pass to the gameplay music queue.</param>
/// <param name="Delay">Authored queue delay, expressed by the music command's gameplay-frame delay identifier.</param>
public readonly record struct BombTorizoMusicRequest(MusicCommand Command, MusicCommandDelay Delay);

/// <summary>
/// One pickup request emitted after Samus shoots a Bomb/Golden Torizo Chozo orb. Opcode
/// <c>$86:AB8A</c> selects one of two enemy headers by area; the initialized Torizo variant
/// is the equivalent concrete authority in this room-scoped runtime.
/// </summary>
public readonly record struct TorizoOrbDropRequest();

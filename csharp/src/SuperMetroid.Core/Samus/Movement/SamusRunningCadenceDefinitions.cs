
namespace SuperMetroid.Core.Samus.Movement;

/// <summary>NTSC running cadence and boost-stage timing: gameplay rules, not editable art.</summary>
/// <remarks>
/// Independently reviewed for #1165 against the complete NTSC J/U v1.0 catalog and
/// pinned bank_91.asm, bank_90 stage advancement, and bank_80 Max6 queue accumulator.
/// Keep these prior conversions: ordinary frames use two ticks; boost stages use
/// 3, 2+(frame&amp;1), 2, 1+(frame&amp;1), and 1 tick, respectively. Every stream ends
/// at frame ten with command $FF. Pointers advance by eleven bytes; reset words are
/// one for stages0..3 and two for stage4. Separate named proofs cover each logical
/// mapping and byte view. This is NTSC policy, not a PAL interpolation rule.
/// Stage5 is a bounded adjacent-data case: pointer $0303 aliases the first two stage0
/// frames, and reset zero aliases pose0's direction/movement bytes. Its delay remains
/// mutable WRAM. Wrapped restored indices may reach other compiled catalog bytes or
/// the low bank; unrelated upper-bank code stays rejected. No stored table remains.
/// The cadences are read by offset into their region; a read leaving it finds the region's place in the bank's 16-bit
/// window, and the words naming each stream, in the installed <see cref="SamusAnimationWindows"/> (#1411).
/// </remarks>
internal static class SamusRunningCadenceDefinitions
{
    /// <summary>The animation byte that loops a stream back to its start.</summary>
    internal const byte LoopMarker = 0xff;
    /// <summary>The delay of each frame of the ordinary running stream.</summary>
    internal const byte OrdinaryDelay = 2;

    /// <summary>AnimationDelayTable_Running_NoSpeedBooster_pointer, the region's first word: the word naming the ordinary stream.</summary>
    internal const int OrdinaryStreamWord = 0;
    /// <summary>AnimationDelayTable_Running_NoSpeedBooster: eleven cadence/command bytes.</summary>
    internal const int OrdinaryDelays = 2;
    /// <summary>AnimationDelayTable_Running_SpeedBooster_pointers: the five words naming each stage's stream.</summary>
    internal const int StageStreamTable = 13;
    /// <summary>AnimationDelayTable_Running_SpeedBooster_0..4, five eleven-byte cadence streams.</summary>
    internal const int BoostDelays = 23;
    /// <summary>SpeedBoostTimerResetValues: stages zero through three reset to one, stage four to two.</summary>
    internal const int ResetWords = 78;
    /// <summary>
    /// The two zero bytes after the reset words, read when the native echo-sound call returns admitted queue occupancy five
    /// and the reset lookup crosses into pose zero.
    /// </summary>
    internal const int SoundQueueStageFiveReset = 88;
    /// <summary>The bytes the region fills, from the ordinary stream's word to the stage-five reset word.</summary>
    internal const int RegionByteCount = 90;
    /// <summary>Where stage five's stream starts: window position $0303, in the floor update residue the bank's low half aliases.</summary>
    internal const int SoundQueueClobberStart = 0x0303;
    /// <summary>Each native running cadence stream has ten frames followed by the $FF loop instruction.</summary>
    internal const int StreamLength = 11;
    /// <summary>
    /// The maximum selector the real Max6 echo queue can return: admitted
    /// occupancy zero through five, or the rejected selection.
    /// </summary>
    internal const byte MaximumSoundQueueSelection = 5;

    /// <summary>
    /// The selection a rejected echo leaves: the cartridge's queue call returns the rejected request itself where
    /// an admitted call returns the occupancy, and the Speed Booster echo is library three's third request.
    /// </summary>
    internal const byte RejectedEchoSelection = 3;
}

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One dead sidehopper's corpse state: its silhouette, rot table and schedule, hop, palette and contact progress.</summary>
public sealed class DeadSidehopperEnemyState
{
    /// <summary>Captures one corpse's silhouette, configuration identity and rot schedule.</summary>
    internal DeadSidehopperEnemyState(RoomEnemySlot slot, ushort graphicsVariant, ushort configurationPointer, CorpseRottingProgram rotting)
    {
        Slot = slot;
        GraphicsVariant = graphicsVariant;
        ConfigurationPointer = configurationPointer;
        YLimit = rotting.YLimit;
        LateMoveEntryIndex = rotting.LateMoveEntryIndex;
        WrapOffset = rotting.WrapOffset;
    }

    /// <summary>The physical enemy slot owning this corpse's positions, native AI words, collision properties, and instruction list.</summary>
    public RoomEnemySlot Slot { get; }
    /// <summary>Silhouette selected by the population parameter: 0 for the initially living Shitroid victim or 2 for the already dead Tourian corpse, selecting initial graphics and row-copy column layouts.</summary>
    public int GraphicsVariant { get; }
    /// <summary>Bank-$A9 rotting configuration identity: $DD68 for variant 0 or $DD78 for variant 2, describing table, callbacks, height, and uploads.</summary>
    public ushort ConfigurationPointer { get; }
    /// <summary>The corpse's (signed pixel Y, delay) entries in the shared rotting heap, one per corpse pixel row.</summary>
    internal CorpseRotTable RotTable { get; init; }
    /// <summary>The corpse tile uploads submitted after every rotting call, including the call that finishes the corpse.</summary>
    internal DeadMonsterRottingDefinitions.TransferRows VramTransfers { get; init; }
    /// <summary>Sprite height less one, 39: a moved pixel row completes when its next Y reaches this boundary; also identifies the final entry.</summary>
    public int YLimit { get; }
    /// <summary>Sprite height less two, 38: this entry and the final entry use destructive moves even during the last three delay calls.</summary>
    public int LateMoveEntryIndex { get; }
    /// <summary>Byte displacement, 148, added to the destination of pixel rows 6 and 7 so their two-pixel fall lands in the next tile row.</summary>
    public int WrapOffset { get; }

    /// <summary>Native variable A ($0FA8 plus the slot byte index), holding the current bank-$A9 dead-sidehopper AI function pointer.</summary>
    public DeadSidehopperAiFunction Function
    {
        get => (DeadSidehopperAiFunction)Slot.VariableA;
        internal set => Slot.VariableA = (ushort)value;
    }

    /// <summary>Position 0..3 in the four-hop launch cycle, selecting the next launch velocity; each landing advances it modulo four.</summary>
    public int JumpPhase { get; internal set; }

    /// <summary>Draining AI calls since the palette last advanced, 0..7; the eighth call advances the palette and resets it.</summary>
    public int PaletteFrameCounter { get; internal set; }

    /// <summary>
    /// Drained-palette progress: 0 before Shitroid feeds, 1..7 while draining, 8 once transformed, and
    /// <see cref="DeadSidehopperCorpseDefinitions.NoPaletteStage"/> for a corpse placed already dead. A stage of 8 or more
    /// lets a shot, touch or Power Bomb start rotting.
    /// </summary>
    public int PaletteStage { get; internal set; }

    /// <summary>Native signed 8.8 velocity: horizontal word 0A and vertical word 0B.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>
    /// Pre-rot AI calls counted in slot variable B, deliberately retained across repeated corpse contacts: the
    /// sixteenth call starts rotting. Every later contact counts on from the retained value, so the count can
    /// reach <see cref="LargestPreRotDelayCount"/> and wrap to zero, restoring the full sixteen-call delay.
    /// </summary>
    public int PreRotDelayCounter => Slot.VariableB;

    /// <summary>The largest pre-rot count; the next call counts zero.</summary>
    internal const int LargestPreRotDelayCount = 65535;

    /// <summary>Native AI variable F initialized to $0040 for the post-landing pause; expiry requires decrementing through zero to $FFFF, giving 65 delay-state calls.</summary>
    public NativeCountdown StateTimer
    {
        get => NativeCountdown.FromRaw(Slot.VariableF);
        internal set => Slot.VariableF = value.RawValue;
    }

    /// <summary>Host diagnostic count of shared rotting-processor calls for this slot since initialization, including the call that completes the final entry.</summary>
    public uint ProcessCallCount { get; internal set; }
    /// <summary>Host diagnostic count of entry-completion callbacks, including the final row entry before the scheduler returns completion.</summary>
    public uint FinishedEntryCount { get; internal set; }
    /// <summary>Host diagnostic count of dust effects requested by completed corpse entries; increments once per completion hook invocation.</summary>
    public uint DustSpawnCount { get; internal set; }

    /// <summary>Counts one pre-rot AI call, wrapping from <see cref="LargestPreRotDelayCount"/> to zero.</summary>
    internal void CountPreRotDelayCall() =>
        Slot.VariableB = checked((ushort)(PreRotDelayCounter == LargestPreRotDelayCount ? 0 : PreRotDelayCounter + 1));
}

namespace SuperMetroid.Core.Game;

/// <summary>Typed projection of one dead sidehopper's extended bank-$A9 WRAM.</summary>
public sealed class DeadSidehopperEnemyState
{
    /// <summary>Captures one corpse's native configuration and row-processor identities.</summary>
    internal DeadSidehopperEnemyState(RoomEnemySlot slot, ushort graphicsVariant, ushort configurationPointer, CorpseRottingProgram rotting)
    {
        Slot = slot;
        GraphicsVariant = graphicsVariant;
        ConfigurationPointer = configurationPointer;
        TablePointer = rotting.TablePointer;
        VramTablePointer = rotting.VramTablePointer;
        CopyFunction = rotting.CopyFunction;
        MoveFunction = rotting.MoveFunction;
        RotationTablePointer = rotting.RotationTablePointer;
        FinishFunction = rotting.FinishFunction;
        EntryCount = rotting.EntryCount;
        YLimit = rotting.YLimit;
        LateMoveEntryIndex = rotting.LateMoveEntryIndex;
        WrapOffset = rotting.WrapOffset;
    }

    /// <summary>The physical enemy slot owning this corpse's positions, native AI words, collision properties, and instruction list.</summary>
    public RoomEnemySlot Slot { get; }
    /// <summary>Native population variant 0 for the initially living Shitroid victim or 2 for the already dead Tourian corpse, selecting initial graphics and row-copy column layouts.</summary>
    public ushort GraphicsVariant { get; }
    /// <summary>Bank-$A9 rotting configuration identity: $DD68 for variant 0 or $DD78 for variant 2, describing table, callbacks, height, and uploads.</summary>
    public ushort ConfigurationPointer { get; }
    /// <summary>Bank-$7E offset of the mutable four-byte (signed pixel Y, delay) row entries: $9000 for variant 0 or $90A0 for variant 2.</summary>
    public ushort TablePointer { get; }
    /// <summary>Bank-$A9 VRAM-transfer definition identity: $E0E0 for variant 0 or $E10A for variant 2, submitting modified corpse tile data after each rotting call.</summary>
    public ushort VramTablePointer { get; }
    /// <summary>Native non-destructive pixel-row copy callback identity, $A9:E4F5 for variant 0 or $A9:E5F6 for variant 2; the host translates its work directly.</summary>
    public ushort CopyFunction { get; }
    /// <summary>Native destructive pixel-row move callback identity, $A9:E468 for variant 0 or $A9:E564 for variant 2, copying the row downward and clearing its source.</summary>
    public ushort MoveFunction { get; }
    /// <summary>Bank-$A9 tile-data row-offset table $E240, shared by both variants; despite the host name, it addresses 8-pixel tile rows rather than a rotation angle.</summary>
    public ushort RotationTablePointer { get; }
    /// <summary>Native completed-row callback $A9:DC08, the normal corpse dust-spawn and periodic library-two sound hook.</summary>
    public ushort FinishFunction { get; }
    /// <summary>Forty staggered rotting entries, matching the native corpse sprite height of $0028 pixels.</summary>
    public ushort EntryCount { get; }
    /// <summary>Native sprite-height-minus-one value, 39: a moved pixel row completes when its next Y reaches this boundary; also identifies the final entry.</summary>
    public ushort YLimit { get; }
    /// <summary>Native sprite-height-minus-two value, 38: this entry and the final entry use destructive moves even during the last three delay ticks.</summary>
    public ushort LateMoveEntryIndex { get; }
    /// <summary>Native inter-tile byte adjustment $0094 added to the pixel-row source offset for rows 6 and 7, wrapping their two-pixel downward destination into the next tile row.</summary>
    public ushort WrapOffset { get; }

    /// <summary>Native variable A ($0FA8 plus the slot byte index), holding the current bank-$A9 dead-sidehopper AI function pointer.</summary>
    public DeadSidehopperAiFunction Function
    {
        get => (DeadSidehopperAiFunction)Slot.VariableA;
        internal set => Slot.VariableA = (ushort)value;
    }

    /// <summary>Native extended word 06, selecting one of four jump velocity pairs.</summary>
    public ushort JumpPhase { get; internal set; }

    /// <summary>Native extended word 07, the eight-frame palette divider.</summary>
    public ushort PaletteFrameCounter { get; internal set; }

    /// <summary>Native extended word 08, written to one by Shitroid after feeding.</summary>
    public ushort PaletteStage { get; internal set; }

    /// <summary>Native signed 8.8 velocity: horizontal word 0A and vertical word 0B.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>Native AI variable B, deliberately retained across repeated corpse contacts.</summary>
    public ushort PreRotDelayCounter
    {
        get => Slot.VariableB;
        internal set => Slot.VariableB = value;
    }

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
    /// <summary>Most recently completed zero-based rotting entry index, or $FFFF before any entry has completed; retained as host diagnostic state.</summary>
    public ushort LastFinishedEntryIndex { get; internal set; } = ushort.MaxValue;
}

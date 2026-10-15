namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>A corpse's native rotting program: its tables, VRAM copy and move routines, and progress limits (#633).</summary>
internal sealed record CorpseRottingProgram
{
    /// <summary>Table pointer.</summary>
    public required ushort TablePointer { get; init; }

    /// <summary>Vram table pointer.</summary>
    public required ushort VramTablePointer { get; init; }

    /// <summary>Copy function.</summary>
    public required ushort CopyFunction { get; init; }

    /// <summary>Move function.</summary>
    public required ushort MoveFunction { get; init; }

    /// <summary>Rotation table pointer.</summary>
    public required ushort RotationTablePointer { get; init; }

    /// <summary>Finish function.</summary>
    public required ushort FinishFunction { get; init; }

    /// <summary>Entry count.</summary>
    public required ushort EntryCount { get; init; }

    /// <summary>Y limit.</summary>
    public required ushort YLimit { get; init; }

    /// <summary>Late move entry index.</summary>
    public required ushort LateMoveEntryIndex { get; init; }

    /// <summary>Wrap offset.</summary>
    public required ushort WrapOffset { get; init; }
}

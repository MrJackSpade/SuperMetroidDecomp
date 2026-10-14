/// <summary>A shielded Draygon cannon's seeded instruction lists, damage instruction and draws (#633).</summary>
internal readonly record struct ShieldedCannonLists
{
    /// <summary>List.</summary>
    public required ushort List { get; init; }

    /// <summary>Hit list.</summary>
    public required ushort HitList { get; init; }

    /// <summary>Destroyed list.</summary>
    public required ushort DestroyedList { get; init; }

    /// <summary>Damage instruction.</summary>
    public required ushort DamageInstruction { get; init; }

    /// <summary>Idle draw.</summary>
    public required ushort IdleDraw { get; init; }

    /// <summary>Hit draw.</summary>
    public required ushort HitDraw { get; init; }

    /// <summary>Destroyed draw.</summary>
    public required ushort DestroyedDraw { get; init; }
}

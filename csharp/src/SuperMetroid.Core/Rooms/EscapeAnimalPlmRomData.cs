namespace SuperMetroid.Core.Rooms;

/// <summary>Bank-$84 animal-rescue block definitions used by the Zebes escape.</summary>
public static class EscapeAnimalPlmRomData
{
    /// <summary>$8F:91B6 hardcoded spawn X coordinate for the animal rescue wall.</summary>
    public const byte WallX = 15;
    /// <summary>$8F:91B7 hardcoded spawn Y coordinate for the animal rescue wall.</summary>
    public const byte WallY = 10;
    /// <summary>$84:B9C5 writes type C / BTS 4F for the rescue wall origin.</summary>
    public const ushort OriginCollision = 0xc04f;
    /// <summary>$84:B9D7/B9E6 extend the origin downwards with type D / BTS FF.</summary>
    public const ushort ExtensionCollision = 0xd0ff;
    /// <summary>$94:9EA6 entry 4F selects critters escape reaction $84:B9C1.</summary>
    public const byte ReactionBts = 0x4f;
    /// <summary>$84:B9A2 plays the native wall-breaking animation before setting the event.</summary>
    public const ushort ReactionList = 0xb9a2;
    /// <summary>$84:B9B9 marks event 0F, allowing the animals to escape.</summary>
    public const ushort SetEscapedEventInstruction = 0xb9b9;
    /// <summary>$84:B994 replaces the visual block with index 9F during reaction setup.</summary>
    public const ushort ReactionVisualBlock = 0x009f;
}

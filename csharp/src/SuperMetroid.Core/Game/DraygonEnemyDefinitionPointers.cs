namespace SuperMetroid.Core.Game;

/// <summary>Bank-$A0 enemy headers composing the bank-$A5 Draygon encounter.</summary>
internal static class DraygonEnemyDefinitionPointers
{
    /// <summary>$A0:DE3F, the Draygon body and encounter owner.</summary>
    internal const ushort Body = 0xde3f;
    /// <summary>$A0:DE7F, Draygon's eye actor.</summary>
    internal const ushort Eye = 0xde7f;
    /// <summary>$A0:DEBF, Draygon's tail actor.</summary>
    internal const ushort Tail = 0xdebf;
    /// <summary>$A0:DEFF, Draygon's arms actor.</summary>
    internal const ushort Arms = 0xdeff;
}

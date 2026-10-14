namespace SuperMetroid.Core.Game;

/// <summary>Dragon's private bank-$A2 animation instruction.</summary>
internal enum DragonInstruction : ushort
{
    /// <summary><c>$A2:E5FB</c>, marks the current body attack animation complete.</summary>
    AttackFinishedCallback = 0xe5fb,
}

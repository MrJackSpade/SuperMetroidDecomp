namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's body-segment phases, each naming the bank-$86 routine it ports; the segment's state holds the member (#1413).</summary>
internal enum BotwoonBodyFunction : ushort
{
    /// <summary><c>Function_EnemyProjectile_BotwoonsBody_Main</c> at $86:EA98.</summary>
    Main = 1,
    /// <summary><c>Function_EnemyProjectile_BotwoonsBody_Dying_SetDelay</c> at $86:EAF4.</summary>
    BeginDying,
    /// <summary><c>Function_EnemyProjectile_BotwoonsBody_Dying_Delay</c> at $86:EB04.</summary>
    DyingDelay,
    /// <summary><c>Function_EnemyProjectile_BotwoonsBody_Dying_Falling</c> at $86:EB1F.</summary>
    Falling,
    /// <summary><c>RTS_86EB93</c>, the inert state after a body segment lands.</summary>
    Landed,
}

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's body-segment phases, each naming the bank-$86 routine it ports; the segment's state holds the member (#1413).</summary>
internal enum BotwoonBodyFunction
{
    /// <summary><c>Function_EnemyProjectile_BotwoonsBody_Main</c>.</summary>
    Main = 1,
    /// <summary><c>Function_EnemyProjectile_BotwoonsBody_Dying_SetDelay</c>.</summary>
    BeginDying,
    /// <summary><c>Function_EnemyProjectile_BotwoonsBody_Dying_Delay</c>.</summary>
    DyingDelay,
    /// <summary><c>Function_EnemyProjectile_BotwoonsBody_Dying_Falling</c>.</summary>
    Falling,
    /// <summary><c>RTS_86EB93</c>, the inert state after a body segment lands.</summary>
    Landed,
}

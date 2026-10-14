namespace SuperMetroid.Core.Game;

/// <summary>The literal bank-$A3 function pointer stored in a Skree's variable B.</summary>
public enum SkreeEnemyFunction : ushort
{
    /// <summary><c>$A3:C6D5</c>: waits until Samus is within 48 horizontal pixels.</summary>
    Idling = 0xc6d5,

    /// <summary><c>$A3:C6F7</c>: waits for the wind-up instruction to authorize the dive.</summary>
    PreparingAttack = 0xc6f7,

    /// <summary><c>$A3:C716</c>: descends six pixels and steers one pixel toward Samus until floor collision.</summary>
    Diving = 0xc716,

    /// <summary><c>$A3:C77F</c>: sinks for the retained 21-update lifetime, emits debris, then deletes the enemy.</summary>
    Burrowing = 0xc77f,
}

using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Game;

/// <summary>Validated decoding of native words that select a member of a closed enum.</summary>
public static class ClosedNativeWords
{
    /// <summary>Decodes <paramref name="word"/> or throws, naming <paramref name="meaning"/>.</summary>
    public static TEnum Decode<TEnum>(ushort word, string meaning)
        where TEnum : struct, Enum
    {
        TEnum value = Unsafe.BitCast<ushort, TEnum>(word);
        return Enum.IsDefined(value)
            ? value
            : throw new InvalidDataException($"${word:X4} is not a {meaning} ({typeof(TEnum).Name}).");
    }

    /// <summary>Decodes a native byte selector or throws, naming <paramref name="meaning"/>.</summary>
    public static TEnum Decode<TEnum>(byte value, string meaning)
        where TEnum : struct, Enum
    {
        TEnum decoded = Unsafe.BitCast<byte, TEnum>(value);
        return Enum.IsDefined(decoded)
            ? decoded
            : throw new InvalidDataException($"${value:X2} is not a {meaning} ({typeof(TEnum).Name}).");
    }
}

/// <summary>Bank-$AA main functions a Bomb or Golden Torizo installs in <c>toriz_var_E</c>.</summary>
public enum TorizoFunction : ushort
{
    /// <summary><c>RTS_AAC6AB</c>: dormant.</summary>
    Idle = 0xc6ab,

    /// <summary><c>Function_Torizo_SimpleMovement</c> at $AA:C6BF.</summary>
    Falling = 0xc6bf,

    /// <summary><c>Function_Torizo_WakeWhenBombTorizoChozoFinishesCrumbling</c> at $AA:C6C6.</summary>
    WaitForHandTrigger = 0xc6c6,

    /// <summary><c>Function_Torizo_NormalMovement</c> at $AA:C6FF.</summary>
    Active = 0xc6ff,

    /// <summary><c>Function_GoldenTorizo_WakeIfSamusIsBelowAndRightOfTargetPos</c> at $AA:D5C2.</summary>
    GoldenWaitForSamus = 0xd5c2,

    /// <summary><c>Function_GoldenTorizo_SimpleMovement</c> at $AA:D5DF.</summary>
    GoldenGravity = 0xd5df,

    /// <summary><c>Function_GoldenTorizo_NormalMovement</c> at $AA:D5E6.</summary>
    GoldenPreInstruction = 0xd5e6,
}

/// <summary>Bank-$AA movement pre-instructions a Torizo installs in <c>toriz_var_F</c>.</summary>
public enum TorizoPreInstruction : ushort
{
    /// <summary>$AA:C95E, the shared no-op movement step.</summary>
    Idle = 0xc95e,

    /// <summary><c>Function_Torizo_Movement_Walking</c> at $AA:C752.</summary>
    AirTransition = 0xc752,

    /// <summary><c>Function_Torizo_Movement_Attacking</c> at $AA:C828.</summary>
    Gravity = 0xc828,

    /// <summary><c>Function_Torizo_Movement_Jumping_Falling</c> at $AA:C82C.</summary>
    Jump = 0xc82c,

    /// <summary><c>Function_GoldenTorizo_Movement_Walking</c> at $AA:D5F1.</summary>
    GoldenAirTransition = 0xd5f1,

    /// <summary><c>Function_GoldenTorizo_Movement_Attacking</c> at $AA:D5ED.</summary>
    GoldenGravity = 0xd5ed,
}

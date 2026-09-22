using System.Numerics;
using System.Runtime.CompilerServices;

namespace SuperMetroid.Core;

/// <summary>Type-preserving numeric checks. Inclusive bounds include both endpoints.</summary>
public static partial class Ensure
{
    /// <summary>Checks GreaterThanZero for sbyte without changing its type.</summary>
    public static sbyte GreaterThanZero(sbyte value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThanZero(value, parameterName);

    /// <summary>Checks AtLeastZero for sbyte without changing its type.</summary>
    public static sbyte AtLeastZero(sbyte value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeastZero(value, parameterName);

    /// <summary>Checks LessThanZero for sbyte without changing its type.</summary>
    public static sbyte LessThanZero(sbyte value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThanZero(value, parameterName);

    /// <summary>Checks AtMostZero for sbyte without changing its type.</summary>
    public static sbyte AtMostZero(sbyte value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMostZero(value, parameterName);

    /// <summary>Checks AtLeast for sbyte without changing its type.</summary>
    public static sbyte AtLeast(sbyte value, sbyte minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeast(value, minimum, parameterName);

    /// <summary>Checks AtMost for sbyte without changing its type.</summary>
    public static sbyte AtMost(sbyte value, sbyte maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMost(value, maximum, parameterName);

    /// <summary>Requires a value between inclusive endpoints.</summary>
    public static sbyte Between(sbyte minimum, sbyte maximum, sbyte value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenInclusive for sbyte without changing its type.</summary>
    public static sbyte BetweenInclusive(sbyte minimum, sbyte maximum, sbyte value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenExclusive for sbyte without changing its type.</summary>
    public static sbyte BetweenExclusive(sbyte minimum, sbyte maximum, sbyte value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenExclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks GreaterThanZero for byte without changing its type.</summary>
    public static byte GreaterThanZero(byte value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThanZero(value, parameterName);

    /// <summary>Checks AtLeastZero for byte without changing its type.</summary>
    public static byte AtLeastZero(byte value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeastZero(value, parameterName);

    /// <summary>Checks LessThanZero for byte without changing its type.</summary>
    public static byte LessThanZero(byte value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThanZero(value, parameterName);

    /// <summary>Checks AtMostZero for byte without changing its type.</summary>
    public static byte AtMostZero(byte value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMostZero(value, parameterName);

    /// <summary>Checks AtLeast for byte without changing its type.</summary>
    public static byte AtLeast(byte value, byte minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeast(value, minimum, parameterName);

    /// <summary>Checks AtMost for byte without changing its type.</summary>
    public static byte AtMost(byte value, byte maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMost(value, maximum, parameterName);

    /// <summary>Requires a value between inclusive endpoints.</summary>
    public static byte Between(byte minimum, byte maximum, byte value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenInclusive for byte without changing its type.</summary>
    public static byte BetweenInclusive(byte minimum, byte maximum, byte value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenExclusive for byte without changing its type.</summary>
    public static byte BetweenExclusive(byte minimum, byte maximum, byte value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenExclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks GreaterThanZero for short without changing its type.</summary>
    public static short GreaterThanZero(short value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThanZero(value, parameterName);

    /// <summary>Checks AtLeastZero for short without changing its type.</summary>
    public static short AtLeastZero(short value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeastZero(value, parameterName);

    /// <summary>Checks LessThanZero for short without changing its type.</summary>
    public static short LessThanZero(short value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThanZero(value, parameterName);

    /// <summary>Checks AtMostZero for short without changing its type.</summary>
    public static short AtMostZero(short value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMostZero(value, parameterName);

    /// <summary>Checks AtLeast for short without changing its type.</summary>
    public static short AtLeast(short value, short minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeast(value, minimum, parameterName);

    /// <summary>Checks AtMost for short without changing its type.</summary>
    public static short AtMost(short value, short maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMost(value, maximum, parameterName);

    /// <summary>Requires a value between inclusive endpoints.</summary>
    public static short Between(short minimum, short maximum, short value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenInclusive for short without changing its type.</summary>
    public static short BetweenInclusive(short minimum, short maximum, short value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenExclusive for short without changing its type.</summary>
    public static short BetweenExclusive(short minimum, short maximum, short value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenExclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks GreaterThanZero for ushort without changing its type.</summary>
    public static ushort GreaterThanZero(ushort value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThanZero(value, parameterName);

    /// <summary>Checks AtLeastZero for ushort without changing its type.</summary>
    public static ushort AtLeastZero(ushort value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeastZero(value, parameterName);

    /// <summary>Checks LessThanZero for ushort without changing its type.</summary>
    public static ushort LessThanZero(ushort value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThanZero(value, parameterName);

    /// <summary>Checks AtMostZero for ushort without changing its type.</summary>
    public static ushort AtMostZero(ushort value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMostZero(value, parameterName);

    /// <summary>Checks AtLeast for ushort without changing its type.</summary>
    public static ushort AtLeast(ushort value, ushort minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeast(value, minimum, parameterName);

    /// <summary>Checks AtMost for ushort without changing its type.</summary>
    public static ushort AtMost(ushort value, ushort maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMost(value, maximum, parameterName);

    /// <summary>Requires a value between inclusive endpoints.</summary>
    public static ushort Between(ushort minimum, ushort maximum, ushort value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenInclusive for ushort without changing its type.</summary>
    public static ushort BetweenInclusive(ushort minimum, ushort maximum, ushort value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenExclusive for ushort without changing its type.</summary>
    public static ushort BetweenExclusive(ushort minimum, ushort maximum, ushort value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenExclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks GreaterThanZero for int without changing its type.</summary>
    public static int GreaterThanZero(int value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThanZero(value, parameterName);

    /// <summary>Checks AtLeastZero for int without changing its type.</summary>
    public static int AtLeastZero(int value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeastZero(value, parameterName);

    /// <summary>Checks LessThanZero for int without changing its type.</summary>
    public static int LessThanZero(int value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThanZero(value, parameterName);

    /// <summary>Checks AtMostZero for int without changing its type.</summary>
    public static int AtMostZero(int value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMostZero(value, parameterName);

    /// <summary>Checks AtLeast for int without changing its type.</summary>
    public static int AtLeast(int value, int minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeast(value, minimum, parameterName);

    /// <summary>Checks AtMost for int without changing its type.</summary>
    public static int AtMost(int value, int maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMost(value, maximum, parameterName);

    /// <summary>Requires a value between inclusive endpoints.</summary>
    public static int Between(int minimum, int maximum, int value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenInclusive for int without changing its type.</summary>
    public static int BetweenInclusive(int minimum, int maximum, int value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenExclusive for int without changing its type.</summary>
    public static int BetweenExclusive(int minimum, int maximum, int value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenExclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks GreaterThanZero for uint without changing its type.</summary>
    public static uint GreaterThanZero(uint value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThanZero(value, parameterName);

    /// <summary>Checks AtLeastZero for uint without changing its type.</summary>
    public static uint AtLeastZero(uint value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeastZero(value, parameterName);

    /// <summary>Checks LessThanZero for uint without changing its type.</summary>
    public static uint LessThanZero(uint value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThanZero(value, parameterName);

    /// <summary>Checks AtMostZero for uint without changing its type.</summary>
    public static uint AtMostZero(uint value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMostZero(value, parameterName);

    /// <summary>Checks AtLeast for uint without changing its type.</summary>
    public static uint AtLeast(uint value, uint minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeast(value, minimum, parameterName);

    /// <summary>Checks AtMost for uint without changing its type.</summary>
    public static uint AtMost(uint value, uint maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMost(value, maximum, parameterName);

    /// <summary>Requires a value between inclusive endpoints.</summary>
    public static uint Between(uint minimum, uint maximum, uint value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenInclusive for uint without changing its type.</summary>
    public static uint BetweenInclusive(uint minimum, uint maximum, uint value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenExclusive for uint without changing its type.</summary>
    public static uint BetweenExclusive(uint minimum, uint maximum, uint value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenExclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks GreaterThanZero for long without changing its type.</summary>
    public static long GreaterThanZero(long value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThanZero(value, parameterName);

    /// <summary>Checks AtLeastZero for long without changing its type.</summary>
    public static long AtLeastZero(long value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeastZero(value, parameterName);

    /// <summary>Checks LessThanZero for long without changing its type.</summary>
    public static long LessThanZero(long value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThanZero(value, parameterName);

    /// <summary>Checks AtMostZero for long without changing its type.</summary>
    public static long AtMostZero(long value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMostZero(value, parameterName);

    /// <summary>Checks AtLeast for long without changing its type.</summary>
    public static long AtLeast(long value, long minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeast(value, minimum, parameterName);

    /// <summary>Checks AtMost for long without changing its type.</summary>
    public static long AtMost(long value, long maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMost(value, maximum, parameterName);

    /// <summary>Requires a value between inclusive endpoints.</summary>
    public static long Between(long minimum, long maximum, long value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenInclusive for long without changing its type.</summary>
    public static long BetweenInclusive(long minimum, long maximum, long value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenExclusive for long without changing its type.</summary>
    public static long BetweenExclusive(long minimum, long maximum, long value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenExclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks GreaterThanZero for ulong without changing its type.</summary>
    public static ulong GreaterThanZero(ulong value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThanZero(value, parameterName);

    /// <summary>Checks AtLeastZero for ulong without changing its type.</summary>
    public static ulong AtLeastZero(ulong value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeastZero(value, parameterName);

    /// <summary>Checks LessThanZero for ulong without changing its type.</summary>
    public static ulong LessThanZero(ulong value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThanZero(value, parameterName);

    /// <summary>Checks AtMostZero for ulong without changing its type.</summary>
    public static ulong AtMostZero(ulong value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMostZero(value, parameterName);

    /// <summary>Checks AtLeast for ulong without changing its type.</summary>
    public static ulong AtLeast(ulong value, ulong minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeast(value, minimum, parameterName);

    /// <summary>Checks AtMost for ulong without changing its type.</summary>
    public static ulong AtMost(ulong value, ulong maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMost(value, maximum, parameterName);

    /// <summary>Requires a value between inclusive endpoints.</summary>
    public static ulong Between(ulong minimum, ulong maximum, ulong value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenInclusive for ulong without changing its type.</summary>
    public static ulong BetweenInclusive(ulong minimum, ulong maximum, ulong value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenExclusive for ulong without changing its type.</summary>
    public static ulong BetweenExclusive(ulong minimum, ulong maximum, ulong value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenExclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks GreaterThanZero for nint without changing its type.</summary>
    public static nint GreaterThanZero(nint value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThanZero(value, parameterName);

    /// <summary>Checks AtLeastZero for nint without changing its type.</summary>
    public static nint AtLeastZero(nint value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeastZero(value, parameterName);

    /// <summary>Checks LessThanZero for nint without changing its type.</summary>
    public static nint LessThanZero(nint value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThanZero(value, parameterName);

    /// <summary>Checks AtMostZero for nint without changing its type.</summary>
    public static nint AtMostZero(nint value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMostZero(value, parameterName);

    /// <summary>Checks AtLeast for nint without changing its type.</summary>
    public static nint AtLeast(nint value, nint minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeast(value, minimum, parameterName);

    /// <summary>Checks AtMost for nint without changing its type.</summary>
    public static nint AtMost(nint value, nint maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMost(value, maximum, parameterName);

    /// <summary>Requires a value between inclusive endpoints.</summary>
    public static nint Between(nint minimum, nint maximum, nint value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenInclusive for nint without changing its type.</summary>
    public static nint BetweenInclusive(nint minimum, nint maximum, nint value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenExclusive for nint without changing its type.</summary>
    public static nint BetweenExclusive(nint minimum, nint maximum, nint value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenExclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks GreaterThanZero for nuint without changing its type.</summary>
    public static nuint GreaterThanZero(nuint value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThanZero(value, parameterName);

    /// <summary>Checks AtLeastZero for nuint without changing its type.</summary>
    public static nuint AtLeastZero(nuint value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeastZero(value, parameterName);

    /// <summary>Checks LessThanZero for nuint without changing its type.</summary>
    public static nuint LessThanZero(nuint value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThanZero(value, parameterName);

    /// <summary>Checks AtMostZero for nuint without changing its type.</summary>
    public static nuint AtMostZero(nuint value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMostZero(value, parameterName);

    /// <summary>Checks AtLeast for nuint without changing its type.</summary>
    public static nuint AtLeast(nuint value, nuint minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeast(value, minimum, parameterName);

    /// <summary>Checks AtMost for nuint without changing its type.</summary>
    public static nuint AtMost(nuint value, nuint maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMost(value, maximum, parameterName);

    /// <summary>Requires a value between inclusive endpoints.</summary>
    public static nuint Between(nuint minimum, nuint maximum, nuint value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenInclusive for nuint without changing its type.</summary>
    public static nuint BetweenInclusive(nuint minimum, nuint maximum, nuint value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenExclusive for nuint without changing its type.</summary>
    public static nuint BetweenExclusive(nuint minimum, nuint maximum, nuint value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenExclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks GreaterThanZero for Half without changing its type.</summary>
    public static Half GreaterThanZero(Half value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThanZero(value, parameterName);

    /// <summary>Checks AtLeastZero for Half without changing its type.</summary>
    public static Half AtLeastZero(Half value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeastZero(value, parameterName);

    /// <summary>Checks LessThanZero for Half without changing its type.</summary>
    public static Half LessThanZero(Half value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThanZero(value, parameterName);

    /// <summary>Checks AtMostZero for Half without changing its type.</summary>
    public static Half AtMostZero(Half value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMostZero(value, parameterName);

    /// <summary>Checks AtLeast for Half without changing its type.</summary>
    public static Half AtLeast(Half value, Half minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeast(value, minimum, parameterName);

    /// <summary>Checks AtMost for Half without changing its type.</summary>
    public static Half AtMost(Half value, Half maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMost(value, maximum, parameterName);

    /// <summary>Requires a value between inclusive endpoints.</summary>
    public static Half Between(Half minimum, Half maximum, Half value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenInclusive for Half without changing its type.</summary>
    public static Half BetweenInclusive(Half minimum, Half maximum, Half value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenExclusive for Half without changing its type.</summary>
    public static Half BetweenExclusive(Half minimum, Half maximum, Half value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenExclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks GreaterThanZero for float without changing its type.</summary>
    public static float GreaterThanZero(float value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThanZero(value, parameterName);

    /// <summary>Checks AtLeastZero for float without changing its type.</summary>
    public static float AtLeastZero(float value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeastZero(value, parameterName);

    /// <summary>Checks LessThanZero for float without changing its type.</summary>
    public static float LessThanZero(float value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThanZero(value, parameterName);

    /// <summary>Checks AtMostZero for float without changing its type.</summary>
    public static float AtMostZero(float value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMostZero(value, parameterName);

    /// <summary>Checks AtLeast for float without changing its type.</summary>
    public static float AtLeast(float value, float minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeast(value, minimum, parameterName);

    /// <summary>Checks AtMost for float without changing its type.</summary>
    public static float AtMost(float value, float maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMost(value, maximum, parameterName);

    /// <summary>Requires a value between inclusive endpoints.</summary>
    public static float Between(float minimum, float maximum, float value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenInclusive for float without changing its type.</summary>
    public static float BetweenInclusive(float minimum, float maximum, float value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenExclusive for float without changing its type.</summary>
    public static float BetweenExclusive(float minimum, float maximum, float value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenExclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks GreaterThanZero for double without changing its type.</summary>
    public static double GreaterThanZero(double value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThanZero(value, parameterName);

    /// <summary>Checks AtLeastZero for double without changing its type.</summary>
    public static double AtLeastZero(double value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeastZero(value, parameterName);

    /// <summary>Checks LessThanZero for double without changing its type.</summary>
    public static double LessThanZero(double value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThanZero(value, parameterName);

    /// <summary>Checks AtMostZero for double without changing its type.</summary>
    public static double AtMostZero(double value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMostZero(value, parameterName);

    /// <summary>Checks AtLeast for double without changing its type.</summary>
    public static double AtLeast(double value, double minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeast(value, minimum, parameterName);

    /// <summary>Checks AtMost for double without changing its type.</summary>
    public static double AtMost(double value, double maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMost(value, maximum, parameterName);

    /// <summary>Requires a value between inclusive endpoints.</summary>
    public static double Between(double minimum, double maximum, double value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenInclusive for double without changing its type.</summary>
    public static double BetweenInclusive(double minimum, double maximum, double value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenExclusive for double without changing its type.</summary>
    public static double BetweenExclusive(double minimum, double maximum, double value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenExclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks GreaterThanZero for decimal without changing its type.</summary>
    public static decimal GreaterThanZero(decimal value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThanZero(value, parameterName);

    /// <summary>Checks AtLeastZero for decimal without changing its type.</summary>
    public static decimal AtLeastZero(decimal value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeastZero(value, parameterName);

    /// <summary>Checks LessThanZero for decimal without changing its type.</summary>
    public static decimal LessThanZero(decimal value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThanZero(value, parameterName);

    /// <summary>Checks AtMostZero for decimal without changing its type.</summary>
    public static decimal AtMostZero(decimal value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMostZero(value, parameterName);

    /// <summary>Checks AtLeast for decimal without changing its type.</summary>
    public static decimal AtLeast(decimal value, decimal minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeast(value, minimum, parameterName);

    /// <summary>Checks AtMost for decimal without changing its type.</summary>
    public static decimal AtMost(decimal value, decimal maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMost(value, maximum, parameterName);

    /// <summary>Requires a value between inclusive endpoints.</summary>
    public static decimal Between(decimal minimum, decimal maximum, decimal value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenInclusive for decimal without changing its type.</summary>
    public static decimal BetweenInclusive(decimal minimum, decimal maximum, decimal value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenExclusive for decimal without changing its type.</summary>
    public static decimal BetweenExclusive(decimal minimum, decimal maximum, decimal value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenExclusive(minimum, maximum, value, parameterName);

    /// <summary>Requires sbyte to be strictly greater than its bound.</summary>
    public static sbyte GreaterThan(sbyte value, sbyte minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThan(value, minimum, parameterName);

    /// <summary>Requires sbyte to be strictly less than its bound.</summary>
    public static sbyte LessThan(sbyte value, sbyte maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThan(value, maximum, parameterName);

    /// <summary>Requires byte to be strictly greater than its bound.</summary>
    public static byte GreaterThan(byte value, byte minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThan(value, minimum, parameterName);

    /// <summary>Requires byte to be strictly less than its bound.</summary>
    public static byte LessThan(byte value, byte maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThan(value, maximum, parameterName);

    /// <summary>Requires short to be strictly greater than its bound.</summary>
    public static short GreaterThan(short value, short minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThan(value, minimum, parameterName);

    /// <summary>Requires short to be strictly less than its bound.</summary>
    public static short LessThan(short value, short maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThan(value, maximum, parameterName);

    /// <summary>Requires ushort to be strictly greater than its bound.</summary>
    public static ushort GreaterThan(ushort value, ushort minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThan(value, minimum, parameterName);

    /// <summary>Requires ushort to be strictly less than its bound.</summary>
    public static ushort LessThan(ushort value, ushort maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThan(value, maximum, parameterName);

    /// <summary>Requires int to be strictly greater than its bound.</summary>
    public static int GreaterThan(int value, int minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThan(value, minimum, parameterName);

    /// <summary>Requires int to be strictly less than its bound.</summary>
    public static int LessThan(int value, int maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThan(value, maximum, parameterName);

    /// <summary>Requires uint to be strictly greater than its bound.</summary>
    public static uint GreaterThan(uint value, uint minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThan(value, minimum, parameterName);

    /// <summary>Requires uint to be strictly less than its bound.</summary>
    public static uint LessThan(uint value, uint maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThan(value, maximum, parameterName);

    /// <summary>Requires long to be strictly greater than its bound.</summary>
    public static long GreaterThan(long value, long minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThan(value, minimum, parameterName);

    /// <summary>Requires long to be strictly less than its bound.</summary>
    public static long LessThan(long value, long maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThan(value, maximum, parameterName);

    /// <summary>Requires ulong to be strictly greater than its bound.</summary>
    public static ulong GreaterThan(ulong value, ulong minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThan(value, minimum, parameterName);

    /// <summary>Requires ulong to be strictly less than its bound.</summary>
    public static ulong LessThan(ulong value, ulong maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThan(value, maximum, parameterName);

    /// <summary>Requires nint to be strictly greater than its bound.</summary>
    public static nint GreaterThan(nint value, nint minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThan(value, minimum, parameterName);

    /// <summary>Requires nint to be strictly less than its bound.</summary>
    public static nint LessThan(nint value, nint maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThan(value, maximum, parameterName);

    /// <summary>Requires nuint to be strictly greater than its bound.</summary>
    public static nuint GreaterThan(nuint value, nuint minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThan(value, minimum, parameterName);

    /// <summary>Requires nuint to be strictly less than its bound.</summary>
    public static nuint LessThan(nuint value, nuint maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThan(value, maximum, parameterName);

    /// <summary>Requires Half to be strictly greater than its bound.</summary>
    public static Half GreaterThan(Half value, Half minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThan(value, minimum, parameterName);

    /// <summary>Requires Half to be strictly less than its bound.</summary>
    public static Half LessThan(Half value, Half maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThan(value, maximum, parameterName);

    /// <summary>Requires float to be strictly greater than its bound.</summary>
    public static float GreaterThan(float value, float minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThan(value, minimum, parameterName);

    /// <summary>Requires float to be strictly less than its bound.</summary>
    public static float LessThan(float value, float maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThan(value, maximum, parameterName);

    /// <summary>Requires double to be strictly greater than its bound.</summary>
    public static double GreaterThan(double value, double minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThan(value, minimum, parameterName);

    /// <summary>Requires double to be strictly less than its bound.</summary>
    public static double LessThan(double value, double maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThan(value, maximum, parameterName);

    /// <summary>Requires decimal to be strictly greater than its bound.</summary>
    public static decimal GreaterThan(decimal value, decimal minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThan(value, minimum, parameterName);

    /// <summary>Requires decimal to be strictly less than its bound.</summary>
    public static decimal LessThan(decimal value, decimal maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThan(value, maximum, parameterName);

    /// <summary>Checks GreaterThanZero for Int128 without changing its type.</summary>
    public static Int128 GreaterThanZero(Int128 value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThanZero(value, parameterName);

    /// <summary>Checks AtLeastZero for Int128 without changing its type.</summary>
    public static Int128 AtLeastZero(Int128 value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeastZero(value, parameterName);

    /// <summary>Checks LessThanZero for Int128 without changing its type.</summary>
    public static Int128 LessThanZero(Int128 value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThanZero(value, parameterName);

    /// <summary>Checks AtMostZero for Int128 without changing its type.</summary>
    public static Int128 AtMostZero(Int128 value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMostZero(value, parameterName);

    /// <summary>Checks AtLeast for Int128 without changing its type.</summary>
    public static Int128 AtLeast(Int128 value, Int128 minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeast(value, minimum, parameterName);

    /// <summary>Checks AtMost for Int128 without changing its type.</summary>
    public static Int128 AtMost(Int128 value, Int128 maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMost(value, maximum, parameterName);

    /// <summary>Checks GreaterThan for Int128 without changing its type.</summary>
    public static Int128 GreaterThan(Int128 value, Int128 minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThan(value, minimum, parameterName);

    /// <summary>Checks LessThan for Int128 without changing its type.</summary>
    public static Int128 LessThan(Int128 value, Int128 maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThan(value, maximum, parameterName);

    /// <summary>Checks Between for Int128 without changing its type.</summary>
    public static Int128 Between(Int128 minimum, Int128 maximum, Int128 value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenInclusive for Int128 without changing its type.</summary>
    public static Int128 BetweenInclusive(Int128 minimum, Int128 maximum, Int128 value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenExclusive for Int128 without changing its type.</summary>
    public static Int128 BetweenExclusive(Int128 minimum, Int128 maximum, Int128 value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenExclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks GreaterThanZero for UInt128 without changing its type.</summary>
    public static UInt128 GreaterThanZero(UInt128 value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThanZero(value, parameterName);

    /// <summary>Checks AtLeastZero for UInt128 without changing its type.</summary>
    public static UInt128 AtLeastZero(UInt128 value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeastZero(value, parameterName);

    /// <summary>Checks LessThanZero for UInt128 without changing its type.</summary>
    public static UInt128 LessThanZero(UInt128 value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThanZero(value, parameterName);

    /// <summary>Checks AtMostZero for UInt128 without changing its type.</summary>
    public static UInt128 AtMostZero(UInt128 value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMostZero(value, parameterName);

    /// <summary>Checks AtLeast for UInt128 without changing its type.</summary>
    public static UInt128 AtLeast(UInt128 value, UInt128 minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeast(value, minimum, parameterName);

    /// <summary>Checks AtMost for UInt128 without changing its type.</summary>
    public static UInt128 AtMost(UInt128 value, UInt128 maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMost(value, maximum, parameterName);

    /// <summary>Checks GreaterThan for UInt128 without changing its type.</summary>
    public static UInt128 GreaterThan(UInt128 value, UInt128 minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThan(value, minimum, parameterName);

    /// <summary>Checks LessThan for UInt128 without changing its type.</summary>
    public static UInt128 LessThan(UInt128 value, UInt128 maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThan(value, maximum, parameterName);

    /// <summary>Checks Between for UInt128 without changing its type.</summary>
    public static UInt128 Between(UInt128 minimum, UInt128 maximum, UInt128 value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenInclusive for UInt128 without changing its type.</summary>
    public static UInt128 BetweenInclusive(UInt128 minimum, UInt128 maximum, UInt128 value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenExclusive for UInt128 without changing its type.</summary>
    public static UInt128 BetweenExclusive(UInt128 minimum, UInt128 maximum, UInt128 value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenExclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks GreaterThanZero for BigInteger without changing its type.</summary>
    public static BigInteger GreaterThanZero(BigInteger value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThanZero(value, parameterName);

    /// <summary>Checks AtLeastZero for BigInteger without changing its type.</summary>
    public static BigInteger AtLeastZero(BigInteger value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeastZero(value, parameterName);

    /// <summary>Checks LessThanZero for BigInteger without changing its type.</summary>
    public static BigInteger LessThanZero(BigInteger value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThanZero(value, parameterName);

    /// <summary>Checks AtMostZero for BigInteger without changing its type.</summary>
    public static BigInteger AtMostZero(BigInteger value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMostZero(value, parameterName);

    /// <summary>Checks AtLeast for BigInteger without changing its type.</summary>
    public static BigInteger AtLeast(BigInteger value, BigInteger minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtLeast(value, minimum, parameterName);

    /// <summary>Checks AtMost for BigInteger without changing its type.</summary>
    public static BigInteger AtMost(BigInteger value, BigInteger maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckAtMost(value, maximum, parameterName);

    /// <summary>Checks GreaterThan for BigInteger without changing its type.</summary>
    public static BigInteger GreaterThan(BigInteger value, BigInteger minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckGreaterThan(value, minimum, parameterName);

    /// <summary>Checks LessThan for BigInteger without changing its type.</summary>
    public static BigInteger LessThan(BigInteger value, BigInteger maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckLessThan(value, maximum, parameterName);

    /// <summary>Checks Between for BigInteger without changing its type.</summary>
    public static BigInteger Between(BigInteger minimum, BigInteger maximum, BigInteger value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenInclusive for BigInteger without changing its type.</summary>
    public static BigInteger BetweenInclusive(BigInteger minimum, BigInteger maximum, BigInteger value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenInclusive(minimum, maximum, value, parameterName);

    /// <summary>Checks BetweenExclusive for BigInteger without changing its type.</summary>
    public static BigInteger BetweenExclusive(BigInteger minimum, BigInteger maximum, BigInteger value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        CheckBetweenExclusive(minimum, maximum, value, parameterName);

    private static T CheckGreaterThan<T>(T value, T minimum, string? parameterName) where T : INumber<T>
    {
        if (!(value > minimum))
            throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be greater than {minimum}.");
        return value;
    }

    private static T CheckLessThan<T>(T value, T maximum, string? parameterName) where T : INumber<T>
    {
        if (!(value < maximum))
            throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be less than {maximum}.");
        return value;
    }


    private static T CheckGreaterThanZero<T>(T value, string? parameterName) where T : INumber<T>
    {
        if (!(value > T.Zero))
            throw new ArgumentOutOfRangeException(parameterName, value, "Value must be greater than zero.");
        return value;
    }

    private static T CheckAtLeastZero<T>(T value, string? parameterName) where T : INumber<T>
    {
        if (!(value >= T.Zero))
            throw new ArgumentOutOfRangeException(parameterName, value, "Value must be at least zero.");
        return value;
    }

    private static T CheckLessThanZero<T>(T value, string? parameterName) where T : INumber<T>
    {
        if (!(value < T.Zero))
            throw new ArgumentOutOfRangeException(parameterName, value, "Value must be less than zero.");
        return value;
    }

    private static T CheckAtMostZero<T>(T value, string? parameterName) where T : INumber<T>
    {
        if (!(value <= T.Zero))
            throw new ArgumentOutOfRangeException(parameterName, value, "Value must be at most zero.");
        return value;
    }

    private static T CheckAtLeast<T>(T value, T minimum, string? parameterName) where T : INumber<T>
    {
        if (!(value >= minimum))
            throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be at least {minimum}.");
        return value;
    }

    private static T CheckAtMost<T>(T value, T maximum, string? parameterName) where T : INumber<T>
    {
        if (!(value <= maximum))
            throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be at most {maximum}.");
        return value;
    }

    private static T CheckBetweenInclusive<T>(T minimum, T maximum, T value, string? parameterName) where T : INumber<T>
    {
        if (!(minimum <= maximum))
            throw new ArgumentException("Minimum must not exceed maximum.", nameof(minimum));
        if (!(value >= minimum && value <= maximum))
            throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be between {minimum} and {maximum}, inclusive.");
        return value;
    }

    private static T CheckBetweenExclusive<T>(T minimum, T maximum, T value, string? parameterName) where T : INumber<T>
    {
        if (!(minimum < maximum))
            throw new ArgumentException("Minimum must be less than maximum.", nameof(minimum));
        if (!(value > minimum && value < maximum))
            throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be between {minimum} and {maximum}, exclusive.");
        return value;
    }
}

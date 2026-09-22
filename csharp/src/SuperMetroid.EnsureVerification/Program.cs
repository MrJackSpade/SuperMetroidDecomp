using System.Numerics;
using System.Runtime.InteropServices;
using SuperMetroid.Core;

namespace SuperMetroid.EnsureVerification;

internal static partial class Program
{
    private static int Main()
    {
        try
        {
            if (OperatingSystem.IsWindows())
                NativeConsoleProcess.SetErrorMode(0x0001 | 0x0002 | 0x8000);
            VerifyEnsure();
            VerifyAnalyzer();
            Console.WriteLine("Ensure API and analyzer checks passed.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }

    private static void VerifyEnsure()
    {
        string? text = "okay";
        Equal("okay", Ensure.NotNull(text));
        Throws<ArgumentNullException>(() => Ensure.NotNull((string?)null), "(string?)null");
        int? optional = 7;
        Equal(7, Ensure.NotNull(optional));
        optional = null;
        Throws<ArgumentNullException>(() => Ensure.NotNull(optional), "optional");
        Throws<ArgumentException>(() => Ensure.NotNullOrEmpty(""), "\"\"");
        Throws<ArgumentException>(() => Ensure.NotNullOrWhiteSpace("  "), "\"  \"");
        Throws<ArgumentNullException>(() => Ensure.NotNullOrWhiteSpace(null), "null");
        Equal("a", Ensure.NotNullOrWhiteSpace("a"));

        sbyte signedByte = 1;
        byte unsignedByte = 1;
        short signedShort = 1;
        ushort unsignedShort = 1;
        int signedInt = 1;
        uint unsignedInt = 1;
        long signedLong = 1;
        ulong unsignedLong = 1;
        nint nativeInt = 1;
        nuint nativeUInt = 1;
        Half half = (Half)1;
        float single = 1;
        double doubled = 1;
        decimal money = 1;
        Int128 wideSigned = 1;
        UInt128 wideUnsigned = 1;
        BigInteger arbitrary = 1;
        Equal(signedByte, Ensure.GreaterThanZero(signedByte));
        Equal(unsignedByte, Ensure.GreaterThanZero(unsignedByte));
        Equal(signedShort, Ensure.GreaterThanZero(signedShort));
        Equal(unsignedShort, Ensure.GreaterThanZero(unsignedShort));
        Equal(signedInt, Ensure.GreaterThanZero(signedInt));
        Equal(unsignedInt, Ensure.GreaterThanZero(unsignedInt));
        Equal(signedLong, Ensure.GreaterThanZero(signedLong));
        Equal(unsignedLong, Ensure.GreaterThanZero(unsignedLong));
        Equal(nativeInt, Ensure.GreaterThanZero(nativeInt));
        Equal(nativeUInt, Ensure.GreaterThanZero(nativeUInt));
        Equal(half, Ensure.GreaterThanZero(half));
        Equal(single, Ensure.GreaterThanZero(single));
        Equal(doubled, Ensure.GreaterThanZero(doubled));
        Equal(money, Ensure.GreaterThanZero(money));
        Equal(wideSigned, Ensure.GreaterThanZero(wideSigned));
        Equal(wideUnsigned, Ensure.GreaterThanZero(wideUnsigned));
        Equal(arbitrary, Ensure.GreaterThanZero(arbitrary));
        Throws<ArgumentOutOfRangeException>(() => Ensure.GreaterThanZero(signedInt - 1), "signedInt - 1");
        Throws<ArgumentOutOfRangeException>(() => Ensure.AtLeastZero(float.NaN), "float.NaN");
        Throws<ArgumentOutOfRangeException>(() => Ensure.Between(0f, 1f, float.NaN), "float.NaN");
        Equal(0, Ensure.Between(0, 64, 0));
        Equal(64, Ensure.BetweenInclusive(0, 64, 64));
        Equal(32, Ensure.BetweenExclusive(0, 64, 32));
        Throws<ArgumentOutOfRangeException>(() => Ensure.BetweenExclusive(0, 64, 64), "64");
        Throws<ArgumentException>(() => Ensure.BetweenInclusive(5, 4, 4), "minimum");
        Equal(3, Ensure.AtLeast(3, 3));
        Equal(3, Ensure.AtMost(3, 3));
        Throws<ArgumentOutOfRangeException>(() => Ensure.GreaterThan(3, 3), "3");
        Throws<ArgumentOutOfRangeException>(() => Ensure.LessThan(3, 3), "3");

        int[] array = [1, 2];
        Equal(array, Ensure.LengthEqual(array, 2));
        Throws<ArgumentException>(() => Ensure.LengthEqual(array, 3), "array");
        Span<int> span = stackalloc int[2];
        Equal(2, Ensure.LengthEqual(span, 2).Length);
        ReadOnlySpan<int> readOnlySpan = span;
        Equal(2, Ensure.LengthEqual(readOnlySpan, 2).Length);
        try
        {
            Ensure.LengthEqual(span, 3);
            throw new InvalidOperationException("Expected span length rejection.");
        }
        catch (ArgumentException exception)
        {
            Equal("span", exception.ParamName);
        }
        IReadOnlyCollection<int> items = new List<int> { 1, 2 };
        Equal(items, Ensure.CountEqual(items, 2));
        Equal(32, Ensure.OneOf(32, 32, 64));
        Throws<ArgumentOutOfRangeException>(() => Ensure.OneOf(16, 32, 64), "16");
        Equal("x", Ensure.Equal("x", "x"));
        Throws<ArgumentException>(() => Ensure.Equal("x", "y"), "\"x\"");

        var options = new EnumOptions { Mode = ByteMode.High };
        Equal(ByteMode.High, Ensure.IsDefined(options.Mode));
        options.Mode = (ByteMode)17;
        Throws<ArgumentOutOfRangeException>(() => Ensure.IsDefined(options.Mode), "options.Mode", "17");
        options.Modes[0] = (ByteMode)18;
        Throws<ArgumentOutOfRangeException>(() => Ensure.IsDefined(options.Modes[0]), "options.Modes[0]", "18");
        Equal(SByteMode.Low, Ensure.IsDefined(SByteMode.Low));
        Equal(ShortMode.Low, Ensure.IsDefined(ShortMode.Low));
        Equal(UShortMode.High, Ensure.IsDefined(UShortMode.High));
        Equal(IntMode.Low, Ensure.IsDefined(IntMode.Low));
        Equal(UIntMode.High, Ensure.IsDefined(UIntMode.High));
        Equal(LongMode.Low, Ensure.IsDefined(LongMode.Low));
        Equal(ULongMode.High, Ensure.IsDefined(ULongMode.High));
        Equal(AliasMode.One, Ensure.IsDefined(AliasMode.AlsoOne));
        Throws<ArgumentOutOfRangeException>(() => Ensure.IsDefined((ByteMode)17), "(ByteMode)17", "17");
        Throws<ArgumentOutOfRangeException>(() => Ensure.IsDefined((SByteMode)(-1)), "(SByteMode)(-1)", "-1");
        Throws<ArgumentOutOfRangeException>(() => Ensure.IsDefined((ShortMode)(-1)), "(ShortMode)(-1)", "-1");
        Throws<ArgumentOutOfRangeException>(() => Ensure.IsDefined((UShortMode)17), "(UShortMode)17", "17");
        Throws<ArgumentOutOfRangeException>(() => Ensure.IsDefined((IntMode)(-1)), "(IntMode)(-1)", "-1");
        Throws<ArgumentOutOfRangeException>(() => Ensure.IsDefined((UIntMode)17), "(UIntMode)17", "17");
        Throws<ArgumentOutOfRangeException>(() => Ensure.IsDefined((LongMode)42), "(LongMode)42");
        Throws<ArgumentOutOfRangeException>(() => Ensure.IsDefined((ULongMode)42), "(ULongMode)42");
    }

    private static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new InvalidOperationException($"Expected {expected}, got {actual}.");
    }

    private static void Throws<T>(Action action, string parameterName, string? messageFragment = null) where T : ArgumentException
    {
        try
        {
            action();
        }
        catch (T exception) when (exception.GetType() == typeof(T))
        {
            Equal(parameterName, exception.ParamName);
            if (messageFragment is not null &&
                !exception.Message.Contains(messageFragment, StringComparison.Ordinal))
                throw new InvalidOperationException($"Expected message to contain '{messageFragment}': {exception.Message}");
            return;
        }
        throw new InvalidOperationException($"Expected {typeof(T).Name} for {parameterName}.");
    }

    private sealed class EnumOptions
    {
        public ByteMode Mode { get; set; }
        public ByteMode[] Modes { get; } = [ByteMode.Zero];
    }

    private enum ByteMode : byte { Zero = 0, High = 200 }
    private enum SByteMode : sbyte { Low = sbyte.MinValue }
    private enum ShortMode : short { Low = short.MinValue }
    private enum UShortMode : ushort { High = ushort.MaxValue }
    private enum IntMode : int { Low = int.MinValue }
    private enum UIntMode : uint { High = uint.MaxValue }
    private enum LongMode : long { Low = long.MinValue }
    private enum ULongMode : ulong { High = ulong.MaxValue }
    private enum AliasMode { One = 1, AlsoOne = 1 }
}

internal static partial class NativeConsoleProcess
{
    [LibraryImport("kernel32.dll")]
    internal static partial uint SetErrorMode(uint errorMode);
}

using System.Runtime.CompilerServices;

namespace Engine.Tests;

/// <summary>The assertions checks are written with. Each failure names the expression that was checked.</summary>
public static class Expect
{
    /// <summary>Fails unless <paramref name="actual"/> equals <paramref name="expected"/>.</summary>
    public static void Equal<T>(T expected, T actual, [CallerArgumentExpression(nameof(actual))] string expression = "")
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new CheckFailedException($"{expression}: expected {expected}, got {actual}.");
    }

    /// <summary>Fails unless <paramref name="condition"/> is true.</summary>
    public static void True(bool condition, [CallerArgumentExpression(nameof(condition))] string expression = "")
    {
        if (!condition)
            throw new CheckFailedException($"{expression}: expected true.");
    }

    /// <summary>Fails unless <paramref name="condition"/> is false.</summary>
    public static void False(bool condition, [CallerArgumentExpression(nameof(condition))] string expression = "")
    {
        if (condition)
            throw new CheckFailedException($"{expression}: expected false.");
    }

    /// <summary>Fails unless <paramref name="action"/> throws a <typeparamref name="TException"/>.</summary>
    public static void Throws<TException>(Action action, [CallerArgumentExpression(nameof(action))] string expression = "")
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }

        throw new CheckFailedException($"{expression}: expected {typeof(TException).Name} to be thrown.");
    }
}

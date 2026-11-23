namespace Engine.Tests;

/// <summary>A check found something other than what it expected.</summary>
public sealed class CheckFailedException(string message) : Exception(message)
{
}

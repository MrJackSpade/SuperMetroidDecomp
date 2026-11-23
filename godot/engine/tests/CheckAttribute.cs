namespace Engine.Tests;

/// <summary>Marks a public, parameterless method as one check. The test runner runs every such method.</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class CheckAttribute : Attribute
{
}

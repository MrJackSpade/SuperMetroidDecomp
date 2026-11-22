using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Generators;

/// <summary>Writes a greeting into every compilation it is handed; only the compiler constructs it.</summary>
[Generator(LanguageNames.CSharp)]
public sealed class GreetingGenerator : IIncrementalGenerator
{
    /// <summary>The source it writes.</summary>
    private const string Source = "namespace SuperMetroid.Generated; internal static class GeneratedGreeting { internal const string Text = \"Hello\"; }";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context) =>
        context.RegisterPostInitializationOutput(static output => output.AddSource("GeneratedGreeting.g.cs", Source));
}

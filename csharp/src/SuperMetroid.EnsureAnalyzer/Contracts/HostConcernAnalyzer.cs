using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Reports, in <c>SuperMetroid.Core</c>, every type or member that holds a host concern (SME6448, #1421): a Windows API
/// name or a platform-invoke declaration, an input device or driver identifier, a thread-synchronization primitive used
/// for presentation, a wall-clock read or deadline, a host-configuration file format, or a named renderer. Core publishes
/// a display packet and an audio frame and consumes typed input; everything that schedules, presents, times, configures
/// or reads a device is the host's (v0.9.0 manifesto, section 3, "Hosts"). See <c>docs/host-boundary.md</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class HostConcernAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A Core type or member holds a host concern.</summary>
    public const string HostConcernId = "SME6448";

    /// <summary>
    /// Windows API names, matched against one camel-case word or a run of adjacent words, ignoring case
    /// (<c>WinMM</c> is the words <c>Win</c> and <c>MM</c>). An identifier that begins with the word <c>Windows</c>
    /// followed by more words (<c>WindowsGamepadCodes</c>) also names the platform; <c>Windows</c> alone, or after another
    /// word (<c>GameplayWindows</c>), is the plural of a display window and is not reported.
    /// </summary>
    internal static readonly ImmutableArray<string> WindowsApiNames =
        ["Win32", "WinMM", "Hwnd", "JoyCaps", "XInput", "DirectInput", "RawInput"];

    /// <summary>Input device and driver words: a controller is typed buttons in Core, never the device that produced them.</summary>
    internal static readonly ImmutableArray<string> DeviceNames = ["Usb", "Hid", "Gamepad", "Joystick", "VendorId", "ProductId"];

    /// <summary>Renderer names. Core publishes a display packet; which backend draws it is the host's choice.</summary>
    internal static readonly ImmutableArray<string> RendererNames =
        ["Direct3D", "Direct3D11", "D3D11", "DirectX", "Dxgi", "OpenGL", "Vulkan", "SwapChain"];

    /// <summary>Words naming wall-clock scheduling: a deadline and the catch-up a host allows after missing one.</summary>
    internal static readonly ImmutableArray<string> WallClockNames = ["Deadline", "WallClock", "CatchUp"];

    /// <summary>Words naming a host-configuration file format.</summary>
    internal static readonly ImmutableArray<string> ConfigurationNames = ["Ini"];

    /// <summary>The word naming a frame hand-off between threads; a mailbox is presentation synchronization by construction.</summary>
    internal static readonly ImmutableArray<string> MailboxNames = ["Mailbox"];

    /// <summary>
    /// Words that put a type or namespace in presentation: a synchronization primitive used inside it serializes the
    /// picture's hand-off between threads.
    /// </summary>
    internal static readonly ImmutableHashSet<string> PresentationWords = ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase,
        "Render", "Rendering", "Renderer", "Present", "Presentation", "Presenter", "Mailbox", "Display", "Publication", "Publisher");

    /// <summary>Thread-synchronization types, by namespace-qualified name.</summary>
    private static readonly ImmutableHashSet<string> SynchronizationTypes =
    [
        "System.Threading.SemaphoreSlim", "System.Threading.Semaphore", "System.Threading.Mutex", "System.Threading.Monitor",
        "System.Threading.ManualResetEvent", "System.Threading.ManualResetEventSlim", "System.Threading.AutoResetEvent",
        "System.Threading.EventWaitHandle", "System.Threading.Barrier", "System.Threading.CountdownEvent",
        "System.Threading.SpinLock", "System.Threading.ReaderWriterLock", "System.Threading.ReaderWriterLockSlim",
        "System.Threading.Lock", "System.Threading.Interlocked", "System.Threading.Volatile",
    ];

    /// <summary>Wall-clock types, by namespace-qualified name.</summary>
    private static readonly ImmutableHashSet<string> WallClockTypes =
    [
        "System.Diagnostics.Stopwatch", "System.TimeProvider", "System.Threading.PeriodicTimer", "System.Threading.Timer",
        "System.Timers.Timer",
    ];

    /// <summary>Wall-clock members, by containing type's namespace-qualified name and member name.</summary>
    private static readonly ImmutableHashSet<string> WallClockMembers =
    [
        "System.DateTime.Now", "System.DateTime.UtcNow", "System.DateTime.Today", "System.DateTimeOffset.Now",
        "System.DateTimeOffset.UtcNow", "System.Environment.TickCount", "System.Environment.TickCount64",
        "System.Threading.Thread.Sleep", "System.Threading.Tasks.Task.Delay",
    ];

    /// <summary>The simple names worth resolving: the last segment of every type and member above.</summary>
    private static readonly ImmutableHashSet<string> ReferenceNames =
        SynchronizationTypes.Concat(WallClockTypes).Concat(WallClockMembers)
            .Select(name => name.Substring(name.LastIndexOf('.') + 1)).ToImmutableHashSet(StringComparer.Ordinal);

    /// <summary>
    /// Attribute types that declare a platform-invoke entry point. <c>DllImport</c> is a pseudo-attribute the compiler
    /// keeps as import data rather than an attribute, so it is read through <see cref="IMethodSymbol.GetDllImportData"/>.
    /// </summary>
    private static readonly ImmutableHashSet<string> PlatformInvokeAttributes = ["System.Runtime.InteropServices.LibraryImportAttribute"];

    private static readonly DiagnosticDescriptor Rule = new(
        HostConcernId,
        "Core holds a host concern",
        "'{0}' {1}; this is the host's, so it lives in the host and Core takes a typed value through a narrow interface",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Core publishes a display packet and an audio frame and consumes typed input. Windows APIs, device and " +
            "driver identifiers, presentation threading, wall-clock deadlines, host-configuration files and renderer choice " +
            "belong to the host that schedules, presents, times and configures the game (#1421, docs/host-boundary.md).");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != CoreLayerAnalyzer.CoreAssembly)
                return;
            start.RegisterSymbolAction(AnalyzeSymbol,
                SymbolKind.NamedType, SymbolKind.Field, SymbolKind.Property, SymbolKind.Method, SymbolKind.Event);
            start.RegisterSyntaxNodeAction(AnalyzeReference, SyntaxKind.IdentifierName, SyntaxKind.GenericName);
            start.RegisterSyntaxNodeAction(AnalyzeLock, SyntaxKind.LockStatement);
            start.RegisterSyntaxNodeAction(AnalyzeLiteral, SyntaxKind.StringLiteralExpression);
        });
    }

    /// <summary>What the identifier <paramref name="name"/> names that is the host's, or null.</summary>
    internal static string? NamedConcern(string name)
    {
        string[] words = CoreVocabularyAnalyzer.Words(name).ToArray();
        if (words.Length > 1 && string.Equals(words[0], "Windows", StringComparison.Ordinal))
            return "names the Windows API ('Windows')";
        if (Run(words, WindowsApiNames) is { } api)
            return $"names the Windows API ('{api}')";
        if (Run(words, DeviceNames) is { } device)
            return $"names an input device or driver ('{device}')";
        if (Run(words, RendererNames) is { } renderer)
            return $"names a renderer ('{renderer}')";
        if (Run(words, WallClockNames) is { } clock)
            return $"names wall-clock scheduling ('{clock}')";
        if (Run(words, ConfigurationNames) is { } format)
            return $"names a host-configuration file format ('{format}')";
        if (Run(words, MailboxNames) is { } mailbox)
            return $"names a presentation hand-off between threads ('{mailbox}')";
        return null;
    }

    /// <summary>
    /// The first of <paramref name="names"/> that one camel-case word, or a run of up to three adjacent words, spells
    /// without regard to case, allowing a plural <c>s</c>; or null.
    /// </summary>
    private static string? Run(string[] words, ImmutableArray<string> names)
    {
        for (int start = 0; start < words.Length; start++)
        {
            string spelled = "";
            for (int end = start; end < words.Length && end < start + 3; end++)
            {
                spelled += words[end];
                foreach (string name in names)
                {
                    if (string.Equals(spelled, name, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(spelled, name + "s", StringComparison.OrdinalIgnoreCase))
                    {
                        return name;
                    }
                }
            }
        }
        return null;
    }

    /// <summary>Reports a declared type or member that names a host concern or declares a platform-invoke entry point.</summary>
    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        ISymbol symbol = context.Symbol;
        if (symbol.IsImplicitlyDeclared || symbol.Locations.FirstOrDefault(location => location.IsInSource) is not { } location)
            return;
        if (symbol is IMethodSymbol { MethodKind: not (MethodKind.Ordinary or MethodKind.UserDefinedOperator or MethodKind.Conversion) })
            return;
        if (symbol is IMethodSymbol method && (method.GetDllImportData() is not null || method.GetAttributes().Any(attribute =>
                attribute.AttributeClass is { } type && PlatformInvokeAttributes.Contains(QualifiedName(type)))))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, symbol.Name, "declares a native platform API entry point"));
            return;
        }
        if (symbol.IsOverride)
            return;
        if (NamedConcern(symbol.Name) is { } concern)
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, symbol.Name, concern));
    }

    /// <summary>Reports a reference to the wall clock, or to a synchronization primitive inside presentation.</summary>
    private static void AnalyzeReference(SyntaxNodeAnalysisContext context)
    {
        var name = (SimpleNameSyntax)context.Node;
        string text = name.Identifier.ValueText;
        if (!ReferenceNames.Contains(text))
            return;
        if (context.SemanticModel.GetSymbolInfo(name, context.CancellationToken).Symbol is not { } symbol)
            return;
        if (symbol is IMethodSymbol { MethodKind: MethodKind.Constructor } constructor)
            symbol = constructor.ContainingType;
        string qualified = symbol is INamedTypeSymbol type
            ? QualifiedName(type)
            : symbol.ContainingType is { } owner ? QualifiedName(owner) + "." + symbol.Name : symbol.Name;
        if (WallClockTypes.Contains(qualified) || WallClockMembers.Contains(qualified))
            context.ReportDiagnostic(Diagnostic.Create(Rule, name.GetLocation(), text, "reads the wall clock"));
        else if (SynchronizationTypes.Contains(qualified) && InPresentation(context.ContainingSymbol))
            context.ReportDiagnostic(Diagnostic.Create(Rule, name.GetLocation(), text, "synchronizes presentation across threads"));
    }

    /// <summary>Reports a <c>lock</c> statement inside presentation.</summary>
    private static void AnalyzeLock(SyntaxNodeAnalysisContext context)
    {
        if (InPresentation(context.ContainingSymbol))
        {
            var statement = (LockStatementSyntax)context.Node;
            context.ReportDiagnostic(Diagnostic.Create(Rule, statement.LockKeyword.GetLocation(), "lock", "synchronizes presentation across threads"));
        }
    }

    /// <summary>Reports a string naming a host-configuration file.</summary>
    private static void AnalyzeLiteral(SyntaxNodeAnalysisContext context)
    {
        var literal = (LiteralExpressionSyntax)context.Node;
        string value = literal.Token.ValueText;
        if (value.EndsWith(".ini", StringComparison.OrdinalIgnoreCase))
            context.ReportDiagnostic(Diagnostic.Create(Rule, literal.GetLocation(), value, "names a host-configuration file"));
    }

    /// <summary>Whether <paramref name="symbol"/> sits in a type or namespace whose name puts it in presentation.</summary>
    private static bool InPresentation(ISymbol? symbol)
    {
        for (ISymbol? current = symbol; current is not null; current = current.ContainingSymbol)
        {
            if (current is INamedTypeSymbol or INamespaceSymbol &&
                CoreVocabularyAnalyzer.Words(current.Name).Any(PresentationWords.Contains))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>The namespace-qualified name of <paramref name="type"/>, without type arguments.</summary>
    private static string QualifiedName(INamedTypeSymbol type)
    {
        string prefix = type.ContainingType is { } outer
            ? QualifiedName(outer)
            : type.ContainingNamespace is { IsGlobalNamespace: false } space ? space.ToDisplayString() : "";
        return prefix.Length == 0 ? type.Name : prefix + "." + type.Name;
    }
}

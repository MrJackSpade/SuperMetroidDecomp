using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// An animation segment says what it shows (SME6459, #2309): in <c>SuperMetroid.Core</c>, a member of an enum named
/// <c>*Segment</c>, one run of an actor's animation, has a summary stating what that run shows in game terms. A summary
/// that only restates the member's name, as "The facing left shooting animation." on <c>FacingLeftShooting</c>, "The
/// idle." on <c>Idle</c>, or "The loop.", says nothing the name does not. Nor does one whose words are all the name's,
/// the actor's its enum is named for, an article or a possessive, or "animation" (#2465), however they are ordered or
/// punctuated: "Draygon body animation: facing left, reset." on <c>DraygonSegment.BodyFacingLeftReset</c>. The
/// comparison ignores case and spacing; a summary holding anything besides plain text, such as a <c>see</c> reference, is
/// not a restatement.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AnimationSegmentSummaryAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A Core animation segment's summary only restates its name.</summary>
    public const string RestatedSegmentNameId = "SME6459";

    private static readonly DiagnosticDescriptor Rule = new(
        RestatedSegmentNameId,
        "Animation segment summary only restates its name",
        "The summary of '{0}' only restates its name; say what this run of the animation shows",
        "Documentation",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A *Segment enum member names one run of an actor's animation. Its summary states what the run shows " +
            "in game terms (what the actor does, which way it faces, whether it repeats or holds, what it plays), read from " +
            "its pictures and its animation's steps; \"The <name> animation.\", \"The <name>.\" and \"The loop.\" say " +
            "nothing the name does not (#2309).");

    /// <summary>The places a member name's words meet: a capital after a lower-case letter or digit, the last capital of a run before a lower-case letter, and a digit after a letter.</summary>
    private static readonly Regex WordBoundary = new(
        "(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])|(?<=[A-Za-z])(?=[0-9])", RegexOptions.CultureInvariant);

    /// <summary>A run of spacing inside a summary.</summary>
    private static readonly Regex Spacing = new(@"\s+", RegexOptions.CultureInvariant);

    /// <summary>A word of a summary's plain text: a run of letters and digits.</summary>
    private static readonly Regex SummaryWord = new("[a-z0-9]+", RegexOptions.CultureInvariant);

    /// <summary>
    /// The words a restatement adds to the name's (#2465): articles, possessives (a possessive's s among them) and
    /// "animation", which say nothing of what the run shows.
    /// </summary>
    private static readonly ImmutableHashSet<string> RestatingWords =
        ["the", "a", "an", "its", "his", "her", "their", "s", "animation", "animations"];

    /// <summary>The suffix of an enum whose members are animation segments.</summary>
    private const string SegmentSuffix = "Segment";

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != CoreLayerAnalyzer.CoreAssembly)
                return;
            start.RegisterSyntaxNodeAction(AnalyzeMember, SyntaxKind.EnumMemberDeclaration);
        });
    }

    /// <summary>
    /// Reports a segment whose summary's plain text is only its name, as an animation or alone, "The loop.", or words all of
    /// which are the name's, the actor's or restating words (<see cref="RestatingWords"/>).
    /// </summary>
    private static void AnalyzeMember(SyntaxNodeAnalysisContext context)
    {
        var member = (EnumMemberDeclarationSyntax)context.Node;
        if (member.Parent is not EnumDeclarationSyntax { Identifier.ValueText: var enumName } ||
            !enumName.EndsWith(SegmentSuffix, StringComparison.Ordinal) || enumName.Length == SegmentSuffix.Length)
            return;
        if (SummaryText(member) is not { } summary)
            return;
        string name = member.Identifier.ValueText;
        string words = WordBoundary.Replace(name, " ").ToLowerInvariant();
        string text = summary.ToLowerInvariant();
        if (text == "the loop." || text == "the " + words + " animation." || text == "the " + words + "." ||
            OnlyNamesWords(text, words, enumName))
            context.ReportDiagnostic(Diagnostic.Create(Rule, member.Identifier.GetLocation(), name));
    }

    /// <summary>
    /// Whether every word of <paramref name="text"/> is one of the member name's <paramref name="words"/>, a word of the actor
    /// <paramref name="enumName"/> is named for, or a restating word, and at least one is the name's (#2465).
    /// </summary>
    private static bool OnlyNamesWords(string text, string words, string enumName)
    {
        HashSet<string> named = [.. words.Split(' ')];
        HashSet<string> actor = [.. WordBoundary.Replace(enumName.Substring(0, enumName.Length - SegmentSuffix.Length), " ").ToLowerInvariant().Split(' ')];
        string[] summaryWords = [.. SummaryWord.Matches(text).Cast<Match>().Select(match => match.Value)];
        return summaryWords.Any(named.Contains) &&
            summaryWords.All(word => named.Contains(word) || actor.Contains(word) || RestatingWords.Contains(word));
    }

    /// <summary>
    /// The member's summary as one line of plain text, its spacing collapsed, or null when it has no summary or the summary
    /// holds anything besides text.
    /// </summary>
    private static string? SummaryText(EnumMemberDeclarationSyntax member)
    {
        foreach (SyntaxTrivia trivia in member.GetLeadingTrivia())
        {
            if (trivia.GetStructure() is not DocumentationCommentTriviaSyntax documentation)
                continue;
            foreach (XmlElementSyntax element in documentation.Content.OfType<XmlElementSyntax>())
            {
                if (element.StartTag.Name.LocalName.ValueText != "summary")
                    continue;
                if (element.Content.Any(node => node is not XmlTextSyntax))
                    return null;
                IEnumerable<string> pieces = element.Content.OfType<XmlTextSyntax>()
                    .SelectMany(text => text.TextTokens)
                    .Where(token => token.IsKind(SyntaxKind.XmlTextLiteralToken))
                    .Select(token => token.ValueText);
                return Spacing.Replace(string.Join(" ", pieces), " ").Trim();
            }
        }
        return null;
    }
}

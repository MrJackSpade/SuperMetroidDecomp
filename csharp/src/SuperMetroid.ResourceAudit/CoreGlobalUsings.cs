using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SuperMetroid.ResourceAudit;

/// <summary>
/// The global usings Core compiles with: the SDK's implicit usings and Core's own <c>GlobalUsings.cs</c>. Contract
/// fixtures compile selected Core sources, which rely on those imports exactly as the Core project does.
/// </summary>
internal static class CoreGlobalUsings
{
    /// <summary>Repository-relative path of Core's declared global usings.</summary>
    private const string CoreGlobalUsingsPath = "csharp/src/SuperMetroid.Core/GlobalUsings.cs";

    /// <summary>The SDK implicit usings enabled for Core.</summary>
    private const string ImplicitUsings =
        "global using System; global using System.Collections.Generic; global using System.IO; global using System.Linq; " +
        "global using System.Net.Http; global using System.Threading; global using System.Threading.Tasks;";

    /// <summary>One tree holding every global using Core compiles with, read from the working directory's repository.</summary>
    internal static SyntaxTree Tree() =>
        CSharpSyntaxTree.ParseText(ImplicitUsings + Environment.NewLine + File.ReadAllText(CoreGlobalUsingsPath), path: "<core-global-usings>");
}

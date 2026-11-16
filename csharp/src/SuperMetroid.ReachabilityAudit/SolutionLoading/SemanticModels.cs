using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.ReachabilityAudit.SolutionLoading;

/// <summary>
/// The semantic models of a compilation's trees, shared by every walk of a loaded solution. A semantic model caches what
/// it binds, so the audits' walks answer from that cache once <see cref="Bind"/> has bound every tree's code on all
/// processors. Every answer is the compilation's own, whichever model gives it, so the sharing changes no result, only
/// how often the same code is bound.
/// </summary>
/// <remarks>
/// A model binds a name that has no bound node of its own (a method name in an invocation inside a collection
/// expression, a type name) by binding its surroundings again, one query at a time, and the large definition catalogs
/// hold tens of thousands of such names; one model of one of them takes longer to bind than the whole rest of its
/// project. A tree's nodes are therefore bound in runs of <see cref="NodesPerModel"/>, each through a model of its own,
/// and a walk asks <see cref="At"/> for the model that bound the node it is at.
/// </remarks>
internal static class SemanticModels
{
    /// <summary>The queried nodes one model binds; a tree with more is bound through several models at once.</summary>
    private const int NodesPerModel = 1000;

    /// <summary>The models of each live compilation, by tree; a compilation no longer used takes its models with it.</summary>
    private static readonly ConditionalWeakTable<Compilation, ConcurrentDictionary<SyntaxTree, TreeModels>> Models = new();

    /// <summary>The shared semantic model of <paramref name="tree"/> in <paramref name="compilation"/>.</summary>
    public static SemanticModel Of(Compilation compilation, SyntaxTree tree) => ModelsOf(compilation, tree).First;

    /// <summary>The model that bound <paramref name="node"/> of <paramref name="tree"/>, or the tree's shared model.</summary>
    public static SemanticModel At(Compilation compilation, SyntaxTree tree, SyntaxNode node) => ModelsOf(compilation, tree).At(node.SpanStart);

    /// <summary>
    /// Binds the code of every given tree, on all processors: the symbol of each expression, attribute, pattern and
    /// constructor initializer, which binds every member body the walks visit. A tree compiled by several projects is
    /// bound in each.
    /// </summary>
    public static void Bind(IEnumerable<(Compilation Compilation, SyntaxTree Tree)> trees)
    {
        var runs = new List<(SemanticModel Model, List<SyntaxNode> Nodes, int TreeNodes)>();
        foreach (var (compilation, tree) in trees)
        {
            var nodes = tree.GetRoot().DescendantNodes()
                .Where(node => node is ExpressionSyntax or AttributeSyntax or PatternSyntax or ConstructorInitializerSyntax).ToList();
            TreeModels models = ModelsOf(compilation, tree);
            for (int start = 0; start < nodes.Count; start += NodesPerModel)
            {
                var run = nodes.GetRange(start, Math.Min(NodesPerModel, nodes.Count - start));
                runs.Add((models.ForRunAt(compilation, run[0].SpanStart, start == 0), run, nodes.Count));
            }
        }
        // The largest trees' runs cost the most, so they start first, and each processor takes the next run when it is
        // free, so no processor is left with a long run at the end while the others wait.
        var ordered = runs.OrderByDescending(run => run.TreeNodes).ToList();
        Parallel.ForEach(Partitioner.Create(ordered, EnumerablePartitionerOptions.NoBuffering), run =>
        {
            foreach (SyntaxNode node in run.Nodes)
                _ = run.Model.GetSymbolInfo(node);
        });
    }

    /// <summary>The models of one tree, created on first use.</summary>
    private static TreeModels ModelsOf(Compilation compilation, SyntaxTree tree) =>
        Models.GetValue(compilation, _ => new ConcurrentDictionary<SyntaxTree, TreeModels>())
            .GetOrAdd(tree, key => new TreeModels(compilation.GetSemanticModel(key)));

    /// <summary>A tree's shared model and the further models that bound its later runs, by where each run starts.</summary>
    private sealed class TreeModels(SemanticModel first)
    {
        private readonly Lock _gate = new();
        private readonly SortedList<int, SemanticModel> _runs = new();

        /// <summary>The tree's shared model, which binds its first run.</summary>
        public SemanticModel First { get; } = first;

        /// <summary>The model for the run starting at <paramref name="position"/>: the shared one for the first run, a new
        /// one for each later run.</summary>
        public SemanticModel ForRunAt(Compilation compilation, int position, bool firstRun)
        {
            if (firstRun)
                return First;
            lock (_gate)
            {
                if (!_runs.TryGetValue(position, out SemanticModel? model))
                    _runs[position] = model = compilation.GetSemanticModel(First.SyntaxTree);
                return model;
            }
        }

        /// <summary>The model of the last run that starts at or before <paramref name="position"/>, or the shared one.</summary>
        public SemanticModel At(int position)
        {
            lock (_gate)
            {
                SemanticModel found = First;
                IList<int> starts = _runs.Keys;
                int low = 0, high = starts.Count - 1;
                while (low <= high)
                {
                    int middle = (low + high) / 2;
                    if (starts[middle] <= position)
                    {
                        found = _runs.Values[middle];
                        low = middle + 1;
                    }
                    else
                    {
                        high = middle - 1;
                    }
                }
                return found;
            }
        }
    }
}

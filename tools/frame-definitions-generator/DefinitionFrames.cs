using Microsoft.CodeAnalysis;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>The frames each definition of a behaviour shows, and the notes saying why frames are every definition's.</summary>
/// <param name="Frames">The frames each definition shows, by definition name.</param>
/// <param name="Notes">Each unit whose frames every definition shows because no definition's code alone runs it.</param>
internal sealed record DefinitionFrameSplit(IReadOnlyDictionary<string, IReadOnlySet<string>> Frames, IReadOnlyList<string> Notes);

/// <summary>
/// Splits the frames a behaviour serving several definitions shows between those definitions, from its code:
/// <list type="bullet">
/// <item>Each definition's dispatch is its entity factory and entity code evaluated for that definition (a switch over
/// its part, a test of its slot's definition): what that code runs or hands its slot to run is that definition's.</item>
/// <item>Code that draws a named definition's sheet frame (an explicit <c>EnemySheetDraw</c>), and the code that calls
/// it, is that definition's.</item>
/// <item>Code a definition's code runs is that definition's too. Code no definition's code runs and nothing calls (a
/// hook the room runs, a handler registered for any slot) is every definition's, and so is what it runs.</item>
/// </list>
/// Only calls carry a definition on: a method group handed on (registered as a handler) runs later, for whoever invokes it.
/// </summary>
/// <param name="model">The Core facts.</param>
/// <param name="scanner">The shared scanner.</param>
/// <param name="behaviour">The behaviour.</param>
/// <param name="walk">The behaviour's whole walk.</param>
internal sealed class DefinitionFrames(CoreModel model, UnitScanner scanner, Behaviour behaviour, Walk walk)
{
    /// <summary>The frames each definition shows, every definition also showing <paramref name="familyFrames"/>.</summary>
    public DefinitionFrameSplit Resolve(IReadOnlySet<string> familyFrames)
    {
        IReadOnlySet<Unit> units = walk.OwnUnits;
        var contexts = behaviour.Definitions.ToDictionary(
            name => name,
            name => new DefinitionContext(behaviour, model.DefinitionField(name), behaviour.Parts.GetValueOrDefault(name)),
            StringComparer.Ordinal);
        var code = units.Where(u => !scanner.IsDispatch(u, behaviour)).ToHashSet();

        // What each unit of the behaviour's own code runs, and what runs it.
        var runs = new Dictionary<Unit, List<Unit>>();
        var runBy = new Dictionary<Unit, List<Unit>>();
        foreach (Unit unit in code)
        {
            UnitScan scan = scanner.Scan(unit);
            var targets = scan.Targets.Where(t => !scan.Deferred.Contains(t) || scan.Runs.Contains(t))
                .Concat(unit.Kind == UnitKind.Member ? model.Implementations(unit.Symbol).Select(Unit.Member) : [])
                .Where(code.Contains).Distinct().ToList();
            runs[unit] = targets;
            foreach (Unit target in targets)
            {
                if (!runBy.TryGetValue(target, out List<Unit>? list))
                    runBy.Add(target, list = []);
                list.Add(unit);
            }
        }

        // Each definition's dispatch, and the dispatch units it evaluates.
        var owners = code.ToDictionary(unit => unit, _ => new HashSet<string>(StringComparer.Ordinal));
        var dispatchOf = new Dictionary<string, HashSet<Unit>>(StringComparer.Ordinal);
        foreach ((string name, DefinitionContext context) in contexts)
        {
            HashSet<Unit> dispatch = DispatchUnits(units, context);
            dispatchOf.Add(name, dispatch);
            var seeds = dispatch.SelectMany(unit => scanner.Scan(unit, context).Targets).Where(code.Contains);
            Spread(seeds, name, owners, runs);
        }
        // A named draw of a definition's sheet is that definition's, with the code calling it.
        foreach (string name in behaviour.Definitions)
        {
            IFieldSymbol definition = model.DefinitionField(name);
            var pending = new Queue<Unit>(code.Where(unit => scanner.Scan(unit).SheetDraws.Contains(definition)));
            var leading = new HashSet<Unit>();
            while (pending.TryDequeue(out Unit unit))
            {
                if (!leading.Add(unit))
                    continue;
                foreach (Unit caller in runBy.GetValueOrDefault(unit) ?? [])
                    pending.Enqueue(caller);
            }
            Spread(leading, name, owners, runs);
        }

        // Code nothing runs and no definition owns is the room's or any slot's: every definition's, with what it runs.
        var everyone = new HashSet<Unit>();
        var roots = code.Where(unit => owners[unit].Count == 0 && !runBy.ContainsKey(unit)).ToList();
        var spreading = new Queue<Unit>(roots);
        while (spreading.TryDequeue(out Unit unit))
        {
            if (!everyone.Add(unit))
                continue;
            foreach (Unit target in runs[unit])
                spreading.Enqueue(target);
        }
        // Code only a cycle of itself runs is every definition's too.
        everyone.UnionWith(code.Where(unit => owners[unit].Count == 0));

        var frames = behaviour.Definitions.ToDictionary(name => name, _ => new HashSet<string>(familyFrames, StringComparer.Ordinal), StringComparer.Ordinal);
        var notes = new List<string>();
        foreach (Unit unit in code)
        {
            List<string> named = [.. FramesOf(scanner.Scan(unit))];
            if (named.Count == 0)
                continue;
            if (everyone.Contains(unit))
                notes.Add($"every definition shows {named.Count} frames {model.Describe(unit)} names");
            foreach (string name in everyone.Contains(unit) ? behaviour.Definitions : owners[unit])
                frames[name].UnionWith(named);
        }
        foreach (Unit unit in units.Where(u => scanner.IsDispatch(u, behaviour)))
        {
            bool anyDispatch = false;
            foreach ((string name, HashSet<Unit> dispatch) in dispatchOf)
            {
                if (!dispatch.Contains(unit))
                    continue;
                anyDispatch = true;
                frames[name].UnionWith(FramesOf(scanner.Scan(unit, contexts[name])));
            }
            // Dispatch code no definition's factory constructs runs for whichever slot reaches it.
            if (!anyDispatch)
            {
                foreach (HashSet<string> shown in frames.Values)
                    shown.UnionWith(FramesOf(scanner.Scan(unit)));
            }
        }
        return new DefinitionFrameSplit(
            frames.ToDictionary(pair => pair.Key, pair => (IReadOnlySet<string>)pair.Value, StringComparer.Ordinal),
            notes);
    }

    /// <summary>
    /// The dispatch units a definition evaluates: its factory, the entity types the factory constructs for it, and the
    /// entity code (base types' included) those reach.
    /// </summary>
    private HashSet<Unit> DispatchUnits(IReadOnlySet<Unit> units, DefinitionContext context)
    {
        var dispatch = new HashSet<Unit>();
        var pending = new Queue<Unit>(units.Where(u => UnitScanner.IsCreate(u, behaviour)));
        while (pending.TryDequeue(out Unit unit))
        {
            if (!dispatch.Add(unit))
                continue;
            foreach (Unit target in scanner.Scan(unit, context).Targets.Where(t => scanner.IsDispatch(t, behaviour)))
            {
                pending.Enqueue(target);
                // A constructed entity's members are what the dispatcher calls for the definition's slot.
                if (target.IsConstructor && model.IsEntityType(target.Type))
                {
                    foreach (Unit member in model.UnitsOf(target.Type))
                        pending.Enqueue(member);
                }
            }
        }
        return dispatch;
    }

    /// <summary>Gives <paramref name="name"/> to <paramref name="seeds"/> and everything they run.</summary>
    private static void Spread(IEnumerable<Unit> seeds, string name, Dictionary<Unit, HashSet<string>> owners, Dictionary<Unit, List<Unit>> runs)
    {
        var pending = new Queue<Unit>(seeds);
        while (pending.TryDequeue(out Unit unit))
        {
            if (!owners[unit].Add(name))
                continue;
            foreach (Unit target in runs[unit])
                pending.Enqueue(target);
        }
    }

    /// <summary>The enemy frames <paramref name="scan"/>'s code names, the empty frames excepted.</summary>
    private IEnumerable<string> FramesOf(UnitScan scan) =>
        scan.EnumMembers
            .Where(member => SymbolEqualityComparer.Default.Equals(member.ContainingType, model.EnemyFrameId))
            .Select(member => member.Name)
            .Where(name => !Walk.ExcludedEnemyFrames.Contains(name))
            .Distinct(StringComparer.Ordinal);
}

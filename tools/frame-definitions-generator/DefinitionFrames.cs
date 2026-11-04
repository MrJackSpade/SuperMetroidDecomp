using Microsoft.CodeAnalysis;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>
/// Splits the frames a behaviour serving several definitions shows between those definitions, from its code:
/// <list type="bullet">
/// <item>Each definition's dispatch is walked with its entity factory and entity code evaluated for that definition
/// (a switch over its part, a test of its slot's definition), so the code that dispatch reaches is that definition's.</item>
/// <item>Code that draws a named definition's sheet frame (an explicit <c>EnemySheetDraw</c>), and the code leading to
/// that draw, is that definition's.</item>
/// <item>Code reached from code of some definitions is theirs too; code reached otherwise (a hook the room runs, a
/// handler any definition's slot may run) is every definition's.</item>
/// </list>
/// Each definition shows the frames its code names.
/// </summary>
/// <param name="model">The Core facts.</param>
/// <param name="scanner">The shared scanner.</param>
/// <param name="behaviour">The behaviour.</param>
/// <param name="walk">The behaviour's whole walk.</param>
internal sealed class DefinitionFrames(CoreModel model, UnitScanner scanner, Behaviour behaviour, Walk walk)
{
    /// <summary>The frames each definition shows, every definition also showing <paramref name="familyFrames"/>.</summary>
    public IReadOnlyDictionary<string, IReadOnlySet<string>> Resolve(IReadOnlySet<string> familyFrames)
    {
        IReadOnlySet<Unit> units = walk.OwnUnits;
        var contexts = behaviour.Definitions.ToDictionary(
            name => name,
            name => new DefinitionContext(behaviour, model.DefinitionField(name), behaviour.Parts.GetValueOrDefault(name)),
            StringComparer.Ordinal);

        // Each definition's dispatch: its factory and the entity types it constructs, evaluated for it.
        var dispatched = new Dictionary<string, Walk>(StringComparer.Ordinal);
        foreach ((string name, DefinitionContext context) in contexts)
        {
            var definitionWalk = new Walk(model, scanner, behaviour.Type, family: null, context);
            foreach (Unit unit in units.Where(u => UnitScanner.IsCreate(u, behaviour)))
            {
                definitionWalk.Root(unit);
                foreach (Unit target in scanner.Scan(unit, context).Targets.Where(t => t.IsConstructor && model.IsEntityType(t.Type)))
                {
                    foreach (Unit entityUnit in model.UnitsOf(target.Type))
                        definitionWalk.Root(entityUnit);
                }
            }
            definitionWalk.Run();
            dispatched.Add(name, definitionWalk);
        }

        // The call graph among the behaviour's own non-dispatch code.
        var code = units.Where(u => !scanner.IsDispatch(u, behaviour)).ToHashSet();
        var callees = new Dictionary<Unit, List<Unit>>();
        var callers = new Dictionary<Unit, List<Unit>>();
        foreach (Unit unit in code)
        {
            var targets = scanner.Scan(unit).Targets
                .Concat(unit.Kind == UnitKind.Member ? model.Implementations(unit.Symbol).Select(Unit.Member) : [])
                .Where(code.Contains).Distinct().ToList();
            callees[unit] = targets;
            foreach (Unit target in targets)
            {
                if (!callers.TryGetValue(target, out List<Unit>? list))
                    callers.Add(target, list = []);
                list.Add(unit);
            }
        }

        // Seeds: the code each definition's dispatch reaches, and the code leading to a draw of its sheet by name.
        var owners = code.ToDictionary(unit => unit, _ => new HashSet<string>(StringComparer.Ordinal));
        foreach ((string name, Walk definitionWalk) in dispatched)
        {
            foreach (Unit unit in definitionWalk.OwnUnits.Where(code.Contains))
                owners[unit].Add(name);
        }
        foreach (string name in behaviour.Definitions)
        {
            IFieldSymbol definition = model.DefinitionField(name);
            var pending = new Queue<Unit>(code.Where(unit => scanner.Scan(unit).SheetDraws.Contains(definition)));
            var leading = new HashSet<Unit>();
            while (pending.TryDequeue(out Unit unit))
            {
                if (!leading.Add(unit))
                    continue;
                owners[unit].Add(name);
                foreach (Unit caller in callers.GetValueOrDefault(unit) ?? [])
                    pending.Enqueue(caller);
            }
        }

        // Code with no definition of its own that nothing calls is the room's or any slot's: every definition's.
        var everyone = new HashSet<Unit>(code.Where(unit => owners[unit].Count == 0 && !callers.ContainsKey(unit)));
        bool changed = true;
        while (changed)
        {
            changed = false;
            foreach (Unit unit in code)
            {
                foreach (Unit callee in callees[unit])
                {
                    if (everyone.Contains(unit) && everyone.Add(callee))
                        changed = true;
                    int before = owners[callee].Count;
                    owners[callee].UnionWith(owners[unit]);
                    changed |= owners[callee].Count != before;
                }
            }
        }
        // Code no seed reaches (a cycle only itself calls) is every definition's.
        everyone.UnionWith(code.Where(unit => owners[unit].Count == 0));

        var frames = behaviour.Definitions.ToDictionary(name => name, _ => new HashSet<string>(familyFrames, StringComparer.Ordinal), StringComparer.Ordinal);
        foreach (Unit unit in code)
        {
            IEnumerable<string> shownBy = everyone.Contains(unit) ? behaviour.Definitions : owners[unit];
            foreach (string name in shownBy)
                frames[name].UnionWith(FramesOf(scanner.Scan(unit)));
        }
        foreach (Unit unit in units.Where(u => scanner.IsDispatch(u, behaviour)))
        {
            bool anyDispatch = false;
            foreach ((string name, Walk definitionWalk) in dispatched)
            {
                if (!definitionWalk.OwnUnits.Contains(unit))
                    continue;
                anyDispatch = true;
                frames[name].UnionWith(FramesOf(scanner.Scan(unit, contexts[name])));
            }
            // Dispatch code no definition's dispatch reaches runs for whichever slot reaches it.
            if (!anyDispatch)
            {
                foreach (HashSet<string> shown in frames.Values)
                    shown.UnionWith(FramesOf(scanner.Scan(unit)));
            }
        }
        return frames.ToDictionary(pair => pair.Key, pair => (IReadOnlySet<string>)pair.Value, StringComparer.Ordinal);
    }

    /// <summary>The enemy frames <paramref name="scan"/>'s code names, the empty frames excepted.</summary>
    private IEnumerable<string> FramesOf(UnitScan scan) =>
        scan.EnumMembers
            .Where(member => SymbolEqualityComparer.Default.Equals(member.ContainingType, model.EnemyFrameId))
            .Select(member => member.Name)
            .Where(name => !Walk.ExcludedEnemyFrames.Contains(name));
}

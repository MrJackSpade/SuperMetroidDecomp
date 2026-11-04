"""Removes the using directives a build log reports unnecessary (IDE0005), so a build never fails twice on them (#1530).

A file compiled into several projects (test-support) loses a line only when every project that compiles it reported that
line, since a using one project does not need can be one another needs; a project's own file loses every reported line.
Every line to remove must be a using directive, and nothing is written unless all are. A project whose build also
reported a compiler error is left alone: its semantic model is incomplete, so a using whose types failed to resolve
reads as unnecessary, and removing it would break the file further.

Usage: remove-unused-usings.py <build log>. Prints how many it removed; exit 0 either way."""
import collections
import pathlib
import re
import sys

root = pathlib.Path(__file__).resolve().parent.parent / "csharp"
hit = re.compile(r"^([A-Za-z]:\\[^(]+\.cs)\((\d+),\d+\): (?:error|warning|info) IDE0005.*\[([^\]]+\.csproj)\]")
using = re.compile(r"^﻿?(global )?using (static )?[\w.]+( = [\w.<>, ]+)?;\s*$")

compile_error = re.compile(r": error CS\d+:.*\[([^\]]+\.csproj)\]")
flagged = collections.defaultdict(set)
broken = set()
for line in open(sys.argv[1], encoding="utf-8", errors="replace"):
    match = hit.match(line.strip())
    if match:
        flagged[(pathlib.Path(match.group(1)).resolve(), int(match.group(2)))].add(pathlib.Path(match.group(3)).name)
    elif error := compile_error.search(line):
        broken.add(pathlib.Path(error.group(1)).name)
# Only projects that compiled apart from their unused usings have trustworthy reports.
flagged = {key: projects - broken for key, projects in flagged.items() if projects - broken}

# The projects that compile each test-support folder: those importing that folder's projitems.
importers = collections.defaultdict(set)
for project in (root / "src").glob("*/*.csproj"):
    for imported in re.findall(r"test-support/([\w.]+)\.projitems", project.read_text(encoding="utf-8")):
        importers[imported].add(project.name)
support = (root / "test-support").resolve()


def compilers(path):
    """The projects compiling a test-support file, or None for a project's own file."""
    if support not in path.parents:
        return None
    folder = path.relative_to(support).parts[0]
    projects = set()
    for imported, names in importers.items():
        if imported == folder or imported.startswith(folder + "."):
            projects |= names
    return projects


drops = collections.defaultdict(set)
kept = 0
for (path, line), projects in flagged.items():
    needed = compilers(path)
    if needed is not None and not needed <= projects:
        kept += 1
        continue
    drops[path].add(line)
texts = {}
for path, lines in drops.items():
    # Read and written untranslated, so every line keeps the ending it had.
    source = path.read_bytes().decode("utf-8").split("\n")
    for line in lines:
        if not using.match(source[line - 1].rstrip("\r")):
            sys.exit(f"{path}:{line} is not a using directive: {source[line - 1]!r}")
    bom = source[0].startswith("﻿")
    remaining = [text for index, text in enumerate(source, 1) if index not in lines]
    # A removed first line takes the byte-order mark with it; the new first line keeps it.
    if bom and remaining and not remaining[0].startswith("﻿"):
        remaining[0] = "﻿" + remaining[0]
    texts[path] = "\n".join(remaining)
for path, text in texts.items():
    path.write_bytes(text.encode("utf-8"))
print(f"removed {sum(len(lines) for lines in drops.values())} unused usings in {len(drops)} files; "
      f"kept {kept} in shared files another project may still need"
      + (f"; skipped projects with compiler errors: {', '.join(sorted(broken))}" if broken else ""))

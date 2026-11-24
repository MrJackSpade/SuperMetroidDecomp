"""Checks, as text, that the values the extractor's writers give are of the kind the Godot types read.

    python tools/godot-rooms/check_value_kinds.py

A Godot type reads each value marked DataValue from its data file by one rule for its kind: a whole number must be
written as a whole number, text as text, a flag as true or false, a list as a list. A value of another kind is refused
when the file is read. Nothing here runs the extractor: this reads the Godot source for each value's kind, and the
writers' source for what each `.Value("name", ...)` is given where that can be told from its text.

It also checks that every value a type reads by this rule is of a kind the rule knows.
"""
from __future__ import annotations

import glob
import os
import re
import sys

REPO = os.path.normpath(os.path.join(os.path.dirname(__file__), '..', '..'))
GODOT = os.path.join(REPO, 'godot')
ENGINE = os.path.join(REPO, 'csharp', 'src', 'SuperMetroid.AssetExtraction', 'Engine')

VALUE = re.compile(r'\[[^\]\n]*DataValue\("(\w+)"([^\]\n]*)\)[^\]\n]*\]\s*(?:///[^\n]*\n\s*)*public\s+([\w.<>\[\]?]+)\s+(\w+)\s*\{')

# The kinds DataValues.Convert reads, by the C# type of the property.
SIMPLE = {'int': 'whole', 'bool': 'flag', 'string': 'text', 'double': 'number', 'Pixels': 'number',
          'string[]': 'list', 'int[]': 'list'}


def godot_sources() -> list[str]:
    found: list[str] = []
    for folder, _, names in os.walk(GODOT):
        if '.godot' in folder or os.sep + 'tests' in folder:
            continue
        found += [os.path.join(folder, name) for name in names if name.endswith('.cs')]
    return sorted(found)


# The kind of each constant of the old game and of the writers, by its name, where every constant of that name is of
# one kind.
CONSTANTS: dict[str, set[str]] = {}


def read_constants() -> None:
    declared = re.compile(r'\b(?:const|static readonly)\s+(int|bool|string|double|decimal)\s+(\w+)\s*=')
    kinds = {'int': 'whole', 'bool': 'flag', 'string': 'text', 'double': 'fraction', 'decimal': 'fraction'}
    for root in (os.path.join(REPO, 'csharp', 'src', 'SuperMetroid.Core'), os.path.join(REPO, 'csharp', 'src', 'SuperMetroid.AssetExtraction')):
        for folder, _, names in os.walk(root):
            if os.sep + 'bin' in folder or os.sep + 'obj' in folder:
                continue
            for name in names:
                if name.endswith('.cs'):
                    for kind, constant in declared.findall(open(os.path.join(folder, name), encoding='utf-8-sig').read()):
                        CONSTANTS.setdefault(constant, set()).add(kinds[kind])


def kind_given(expression: str) -> str | None:
    """The kind of value a writer's expression is, where its text says so beyond doubt."""
    text = expression.strip()
    if text.startswith('(int)'):
        return 'whole'
    if re.fullmatch(r'[A-Z]\w*(\.[A-Z]\w*)+', text) or re.fullmatch(r'[A-Z]\w*', text):
        found = CONSTANTS.get(text.split('.')[-1])
        if found and len(found) == 1:
            return next(iter(found))
    if re.fullmatch(r'-?\d+', text):
        return 'whole'
    if re.fullmatch(r'-?\d+\.\d+m?', text):
        return 'fraction'
    if text in ('true', 'false'):
        return 'flag'
    if re.match(r'\$?@?"', text) and not re.search(r'"\s*\)?\s*[?:]', text):
        return 'text'
    if re.match(r'EngineNames\.(Sound|SoundOrStop|SoundCue|Side)\(', text) or re.match(r'(MusicName|MessageName|EffectFile|DustFile|DeathFile|PickupFile)\(', text):
        return 'text'
    if re.match(r'EngineNames\.Number\(', text) or re.search(r'\.Pixels\)*$', text):
        return 'fraction'
    if re.match(r'new JsonArray\b', text) or text.startswith('['):
        return 'list'
    return None


def split_arguments(text: str, start: int) -> tuple[list[str], int]:
    """The arguments of a call whose opening bracket is at `start`, and where the call ends."""
    depth, parts, piece, at, quote = 0, [], '', start, ''
    while at < len(text):
        letter = text[at]
        if quote:
            piece += letter
            if letter == '\\':
                piece += text[at + 1]
                at += 1
            elif letter == quote:
                quote = ''
        elif letter in '"\'':
            quote = letter
            piece += letter
        elif letter in '([{':
            depth += 1
            if depth > 1:
                piece += letter
        elif letter in ')]}':
            depth -= 1
            if depth == 0:
                parts.append(piece)
                return parts, at
            piece += letter
        elif letter == ',' and depth == 1:
            parts.append(piece)
            piece = ''
        else:
            piece += letter
        at += 1
    raise ValueError('a call is never closed')


def main() -> None:
    read_constants()
    enums: set[str] = set()
    kinds: dict[str, set[str]] = {}
    problems = 0
    for path in godot_sources():
        text = open(path, encoding='utf-8-sig').read()
        enums |= set(re.findall(r'\benum (\w+)', text))
    for path in godot_sources():
        text = open(path, encoding='utf-8-sig').read()
        for name, options, kind, prop in VALUE.findall(text):
            if 'ReadByOwner' in options:
                continue
            plain = kind.rstrip('?')
            if plain in SIMPLE:
                kinds.setdefault(name, set()).add(SIMPLE[plain])
            elif plain.split('.')[-1] in enums:
                kinds.setdefault(name, set()).add('text')
            else:
                print(f'{os.path.relpath(path, REPO)}: the value "{name}" ({prop}) is a {kind}, which a data file cannot give as a simple value; its owner must read it')
                problems += 1

    checked = 0
    for path in sorted(glob.glob(os.path.join(ENGINE, '*.cs'))):
        text = open(path, encoding='utf-8-sig').read()
        for call in re.finditer(r'\.Value\(', text):
            arguments, _ = split_arguments(text, call.end() - 1)
            if len(arguments) != 2:
                continue
            named = re.fullmatch(r'\s*"(\w+)"\s*', arguments[0])
            if not named:
                continue
            name = named.group(1)
            given = kind_given(arguments[1])
            wanted = kinds.get(name)
            if given is None or wanted is None or len(wanted) != 1:
                continue
            checked += 1
            (kind,) = wanted
            fine = {
                'whole': given == 'whole',
                'number': given in ('whole', 'fraction'),
                'text': given == 'text',
                'flag': given == 'flag',
                'list': given == 'list',
            }[kind]
            if not fine:
                line = text.count('\n', 0, call.start()) + 1
                print(f'{os.path.basename(path)}:{line}: "{name}" is read as a {kind} and given {arguments[1].strip()[:80]} ({given})')
                problems += 1
    print(f'{problems} problems; {checked} values whose kind could be told were checked, of {len(kinds)} names of values')
    sys.exit(1 if problems else 0)


if __name__ == '__main__':
    main()

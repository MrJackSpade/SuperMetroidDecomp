"""ROM-free packaging, GitHub release publication, and receipted Discord announcements."""
import base64
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys
import tempfile
import urllib.error
import urllib.parse
import urllib.request
import zipfile

TAG = re.compile(r"v(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-([0-9A-Za-z]+(?:[.-][0-9A-Za-z]+)*))?\Z")
RECEIPT = re.compile(r"\n?<!-- discord-release: (\{[^\n]*\}) -->")
GUILD_ID = "1547270817312809112"
DEFAULT_INI_NAME = "SuperMetroid.defaults.ini"
DEFAULT_INI = Path(__file__).resolve().parents[1] / "csharp/src/SuperMetroid.Diagnostics/Configuration" / DEFAULT_INI_NAME


def check_default_ini(contents):
    # The only distributable INI is the tracked runtime template, never a player's
    # active configuration. Accommodate Git's platform-specific line endings only.
    if contents.decode("utf-8").replace("\r\n", "\n") != DEFAULT_INI.read_text(encoding="utf-8"):
        raise ValueError("Release default INI differs from the canonical runtime defaults.")


def version(tag):
    if not TAG.fullmatch(tag):
        raise ValueError("Release tags must be vMAJOR.MINOR.PATCH, optionally followed by -prerelease.")
    return tag[1:]


def check_names(names):
    for name in names:
        normalized = name.replace("\\", "/").lower()
        parts = normalized.split("/")
        if (any(p in {"..", "standalone-assets", "debug-states", "input-recordings", "logs"} for p in parts)
                or normalized.startswith("/")
                or normalized.endswith((".smc", ".sfc", ".spcu", ".wav", ".smstate", ".smrec", ".srm", ".keystore", ".jks"))
                or parts[-1] in {"audio-manifest.json", "installation.json", "supermetroid.save.json", "supermetroid.ini"}):
            raise ValueError(f"Release contains private game data or signing material: {name}")


def check_archive(path):
    with zipfile.ZipFile(path) as archive:
        check_names(archive.namelist())
        for name in archive.namelist():
            if name.replace("\\", "/").split("/")[-1].lower() == DEFAULT_INI_NAME.lower():
                if name != DEFAULT_INI_NAME:
                    raise ValueError("Release default INI must be at the archive root.")
                check_default_ini(archive.read(name))
        if "SuperMetroid.Game.exe" in archive.namelist() and DEFAULT_INI_NAME not in archive.namelist():
            raise ValueError("Desktop release is missing its documented default INI.")
        if archive.testzip() is not None:
            raise ValueError(f"Corrupt release archive: {path.name}")


# The two games a release carries: Classic, the C# port under csharp/, and Rebirth, the Godot game under godot/.
ARTIFACTS = {
    "classic-windows": "SuperMetroid-Classic-windows-x64.zip",
    "rebirth-windows": "SuperMetroid-Rebirth-windows-x64.zip",
    "rebirth-android": "SuperMetroid-Rebirth-android-arm64.apk",
}
REBIRTH_WINDOWS_FILES = ("SuperMetroidRebirth.exe", "SuperMetroidRebirthAssets.exe", "REBIRTH-SETUP.md")


def zip_folder(source, target, files):
    with zipfile.ZipFile(target, "w", zipfile.ZIP_DEFLATED) as archive:
        for path in sorted(files):
            archive.write(path, path.relative_to(source))


def package(kind, source, output):
    if kind not in ARTIFACTS:
        raise ValueError(f"Unknown artifact: {kind}")
    source, output = Path(source), Path(output)
    output.mkdir(parents=True, exist_ok=True)
    target = output / ARTIFACTS[kind]
    if kind == "classic-windows":
        files = [p for p in source.rglob("*") if p.is_file()]
        check_names(str(p.relative_to(source)) for p in files)
        if not (source / "SuperMetroid.Game.exe").is_file():
            raise ValueError("Desktop publish is missing SuperMetroid.Game.exe.")
        if not (source / DEFAULT_INI_NAME).is_file():
            raise ValueError("Desktop publish is missing its documented default INI.")
        check_default_ini((source / DEFAULT_INI_NAME).read_bytes())
        zip_folder(source, target, files)
    elif kind == "rebirth-windows":
        files = [p for p in source.rglob("*") if p.is_file()]
        check_names(str(p.relative_to(source)) for p in files)
        for name in REBIRTH_WINDOWS_FILES:
            if not (source / name).is_file():
                raise ValueError(f"Rebirth export is missing {name}.")
        if (source / "assets").exists():
            raise ValueError("Rebirth export carries an assets folder; the game's assets never ship.")
        zip_folder(source, target, files)
    else:
        check_archive(source)
        shutil.copyfile(source, target)
    check_archive(target)
    print(f"Verified game-data-free artifact: {target.name}")


def stamp_android(presets, version_name, code):
    # The Android package's version is the release's: Android takes an update only with a higher code.
    if not TAG.fullmatch("v" + version_name) or not re.fullmatch(r"[1-9]\d*", code):
        raise ValueError("The Android version must be the release's version and a positive whole code.")
    path = Path(presets)
    text = path.read_text(encoding="utf-8")
    for key, value in (("version/code", code), ("version/name", f'"{version_name}"')):
        text, count = re.subn(rf"(?m)^{re.escape(key)}=.*$", f"{key}={value}", text)
        if count != 1:
            raise ValueError(f"The export presets set {key} {count} times; exactly one Android preset is expected.")
    path.write_text(text, encoding="utf-8")
    print(f"Android version {version_name} ({code})")


def android_tool(name):
    # The newest build tools of the SDK the runner names; apksigner is a script, a batch file on Windows.
    builds = sorted((Path(os.environ["ANDROID_HOME"]) / "build-tools").iterdir(),
                    key=lambda p: [int(n) if n.isdigit() else 0 for n in re.split(r"[.-]", p.name)])
    if not builds:
        raise ValueError("The Android SDK has no build tools.")
    for candidate in (builds[-1] / name, builds[-1] / f"{name}.bat", builds[-1] / f"{name}.exe"):
        if candidate.is_file():
            return str(candidate)
    raise ValueError(f"The Android SDK's build tools have no {name}.")


def run_tool(*args):
    # A batch file is not something Windows starts by itself.
    command = ["cmd", "/c", *args] if args[0].endswith(".bat") else list(args)
    result = subprocess.run(command, text=True, capture_output=True)
    if result.returncode:
        raise RuntimeError(f"{Path(args[0]).name} failed: {(result.stderr or result.stdout).strip()}")
    return result.stdout


def sign_android(unsigned, signed):
    # The release key is held in the repository's Actions secrets and reaches this process only through its
    # environment. It is written to a file of its own for the signing and removed again whatever happens; the
    # passwords are handed to apksigner by the names of their variables, never on a command line.
    for name in ("ANDROID_KEYSTORE_BASE64", "ANDROID_KEY_ALIAS", "ANDROID_KEY_PASSWORD", "ANDROID_STORE_PASSWORD"):
        if not os.environ.get(name):
            raise ValueError(f"The Android signing secret {name} is not set.")
    unsigned, signed = Path(unsigned), Path(signed)
    aligned = signed.with_name(signed.stem + ".aligned.apk")
    keystore = Path(tempfile.mkdtemp(prefix="release-key-")) / "release.jks"
    try:
        keystore.write_bytes(base64.b64decode(os.environ["ANDROID_KEYSTORE_BASE64"], validate=True))
        run_tool(android_tool("zipalign"), "-f", "-p", "4", str(unsigned), str(aligned))
        run_tool(android_tool("apksigner"), "sign", "--ks", str(keystore), "--ks-key-alias", os.environ["ANDROID_KEY_ALIAS"],
                 "--ks-pass", "env:ANDROID_STORE_PASSWORD", "--key-pass", "env:ANDROID_KEY_PASSWORD",
                 "--out", str(signed), str(aligned))
    finally:
        shutil.rmtree(keystore.parent, ignore_errors=False)
        if aligned.exists():
            aligned.unlink()
    report = run_tool(android_tool("apksigner"), "verify", "--verbose", str(signed))
    if "Verifies" not in report:
        raise ValueError("The signed Android package does not verify.")
    print(f"Signed and verified: {signed.name}")


def gh(*args, data=None, optional=False):
    result = subprocess.run(["gh", *args], input=data, text=True, capture_output=True)
    if result.returncode:
        if optional and "not found" in result.stderr.lower():
            return None
        raise RuntimeError(f"GitHub command failed ({args[0]}): {result.stderr.strip()}")
    return result.stdout


def release_info(tag):
    result = gh("release", "view", tag, "--json", "databaseId,tagName,body,url,isDraft,assets", optional=True)
    return json.loads(result) if result else None


def set_body(release, body):
    repo = os.environ["GH_REPO"]
    gh("api", "--method", "PATCH", f"repos/{repo}/releases/{release['databaseId']}", "--input", "-",
       data=json.dumps({"tag_name": release["tagName"], "body": body}))
    release["body"] = body


def publish(directory):
    tag = os.environ["RELEASE_TAG"]
    version(tag)
    root = Path(directory)
    # A release carries both games: Classic for Windows, and Rebirth for Windows and for Android (#3258).
    assets = [root / name for name in ARTIFACTS.values()]
    for asset in assets:
        check_archive(asset)
    checksums = root / "SHA256SUMS.txt"
    checksums.write_text("".join(f"{hashlib.sha256(p.read_bytes()).hexdigest()}  {p.name}\n" for p in assets), encoding="utf-8")
    assets.append(checksums)
    release = release_info(tag)
    if release and not release["isDraft"]:
        # Published versions are immutable. A rerun can retry the announcement without replacing binaries.
        required = {p.name for p in assets}
        if not required.issubset({a["name"] for a in release["assets"]}):
            raise ValueError("Published release is missing required assets; inspect it before retrying.")
        print(f"Release already published: {release['url']}")
        return
    if not release:
        gh("release", "create", tag, "--verify-tag", "--draft", "--title", tag, "--generate-notes")
        release = release_info(tag)
        set_body(release, (release["body"] or "") + "\n\nDownloads: Classic for Windows x64 (self-contained ZIP); "
                 "Rebirth for Windows x64 (ZIP) and for Android arm64 (APK). No game assets are bundled: each is set up "
                 "from your own ROM, as the setup notes in its download say.\n")
    gh("release", "upload", tag, *(str(p) for p in assets), "--clobber")
    args = ["release", "edit", tag, "--tag", tag, "--draft=false"]
    args.append("--prerelease" if "-" in tag else "--latest")
    gh(*args)
    print(f"Published {tag} with the verified Classic and Rebirth builds and SHA-256 checksums.")


def webhook_request(url, method="GET", payload=None):
    request = urllib.request.Request(url, method=method, headers={"Content-Type": "application/json", "User-Agent": "SuperMetroid-Release/1.0"},
                                     data=json.dumps(payload).encode() if payload is not None else None)
    try:
        with urllib.request.urlopen(request, timeout=30) as response:
            return json.load(response)
    except urllib.error.HTTPError as error:
        # URLs contain the credential. Never print the exception, request URL, or response body.
        raise RuntimeError(f"Discord request failed with HTTP {error.code}; inspect the release receipt before retrying.") from None
    except (urllib.error.URLError, TimeoutError):
        raise RuntimeError("Discord delivery could not be confirmed; inspect the release receipt before retrying.") from None


def receipt_body(body, receipt):
    return RECEIPT.sub("", body).rstrip() + "\n\n<!-- discord-release: " + json.dumps(receipt, separators=(",", ":")) + " -->\n"


def notification_payload(tag, url):
    version(tag)
    return {"content": f"**Super Metroid {tag} is available**\nWindows x64 download:\n{url}",
            "allowed_mentions": {"parse": []}, "flags": 4}


def notify():
    url = os.environ.get("DISCORD_RELEASES_WEBHOOK_URL", "").strip()
    channel = os.environ.get("DISCORD_RELEASES_CHANNEL_ID", "").strip()
    if not url or not channel:
        raise ValueError("Configure DISCORD_RELEASES_WEBHOOK_URL secret and DISCORD_RELEASES_CHANNEL_ID variable. The GitHub release is already published.")
    parsed = urllib.parse.urlsplit(url)
    if (parsed.scheme != "https" or parsed.netloc != "discord.com" or parsed.query or parsed.fragment
            or not re.fullmatch(r"/api/webhooks/\d+/[A-Za-z0-9_-]+", parsed.path)):
        raise ValueError("Expected a standard discord.com webhook URL.")
    info = webhook_request(url)
    if str(info.get("guild_id")) != GUILD_ID or str(info.get("channel_id")) != channel:
        raise ValueError("Release webhook destination does not match the configured server/channel.")
    tag = os.environ["RELEASE_TAG"]
    release = release_info(tag)
    if not release or release["isDraft"]:
        raise ValueError("Only a published release can be announced.")
    match = RECEIPT.search(release["body"] or "")
    receipt = json.loads(match.group(1)) if match else None
    payload = notification_payload(tag, release["url"])
    if receipt:
        if receipt.get("status") != "sent":
            raise ValueError("An earlier Discord attempt has uncertain delivery. Reconcile its release-body receipt before retrying; see docs/releases.md.")
        message_id = str(receipt.get("message_id", ""))
        if not message_id.isdigit() or receipt.get("webhook_id") != str(info["id"]):
            raise ValueError("Discord receipt does not match this webhook.")
        message = webhook_request(url + "/messages/" + message_id, "PATCH", payload)
    else:
        receipt = {"status": "pending", "webhook_id": str(info["id"]), "run_id": os.environ.get("GITHUB_RUN_ID", "local")}
        set_body(release, receipt_body(release["body"] or "", receipt))
        message = webhook_request(url + "?wait=true", "POST", payload)
    if str(message.get("channel_id")) != channel or not str(message.get("id", "")).isdigit():
        raise ValueError("Discord response did not confirm delivery in the releases channel.")
    receipt.update(status="sent", message_id=str(message["id"]))
    set_body(release, receipt_body(release["body"] or "", receipt))
    print(f"Discord release announcement confirmed: message {message['id']}.")


def main():
    command = sys.argv[1]
    if command == "version":
        run = int(os.environ["RELEASE_RUN"])
        tag = os.environ["RELEASE_REF"].removeprefix("refs/tags/") if os.environ["RELEASE_EVENT"] == "push" else f"v0.0.0-ci.{run}"
        values = f"tag={tag}\nversion={version(tag)}\ncode={run + 1}\n"
        with open(os.environ["GITHUB_OUTPUT"], "a", encoding="utf-8") as output:
            output.write(values)
    elif command.startswith("package-"):
        package(command.removeprefix("package-"), *sys.argv[2:])
    elif command == "stamp-android":
        stamp_android(*sys.argv[2:])
    elif command == "sign-android":
        sign_android(*sys.argv[2:])
    elif command == "publish":
        publish(sys.argv[2])
    elif command == "notify":
        notify()
    else:
        raise ValueError("Unknown release command.")


if __name__ == "__main__":
    try:
        main()
    except Exception as error:
        print(f"Release error: {error}", file=sys.stderr)
        sys.exit(1)

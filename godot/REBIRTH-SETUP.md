# Super Metroid Rebirth: setting it up

Rebirth is the game written new in Godot. Like Classic, it ships with nothing of the game's own: no ROM, no
pictures, no sound. Its assets are made on your computer from your own ROM, once.

## Windows

1. Unpack the ZIP into a folder of its own.
2. Make the assets, from a command prompt in that folder:

   ```
   SuperMetroidRebirthAssets.exe "C:\path\to\your\Super Metroid.smc"
   ```

   It reads the ROM, never changes it, and writes a folder named `assets` beside itself, saying each step as it
   starts.
3. Start `SuperMetroidRebirth.exe`.

The game looks for `assets` beside `SuperMetroidRebirth.exe` and nowhere else. If it finds none it says so, with
the folder it looked in, and starts nothing.

## Android

The game makes its own assets on the device, the first time it starts.

1. Copy your ROM to the device, anywhere its file chooser can reach: `Download`, say.
2. Install the APK and start the game. It says its assets are not installed.
3. Press any button, or touch the screen. The device's file chooser opens: choose the ROM.
4. Wait while the assets are made. A bar shows how far the making has come, with the step under way and a count
   that moves with the work. Leave the game open until it is done: if it is closed part-way, nothing half-made is
   kept, and the making starts again the next time.
5. The game begins.

Your ROM is read and never changed. The game keeps a copy of it in its own folder on the device, beside its
assets (`Android/data/org.supermetroid.rebirth/files/Documents`), so that a new version can make its assets again
without asking. Both go with the game when it is uninstalled.

## After an update

A new version may read assets the old one did not write.

- **Windows:** make them again with the `SuperMetroidRebirthAssets.exe` that came with the new version. It replaces
  the `assets` folder.
- **Android:** nothing to do. A new version sees that the assets were made by another, and makes them again from
  the ROM it kept, the first time it starts. To have it ask for a ROM again, clear the game's storage in the
  device's settings.

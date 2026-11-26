# Super Metroid Rebirth: setting it up

Rebirth is the game written new in Godot. Like Classic, it ships with nothing of the game's own: no ROM, no
pictures, no sound. Its assets are made on your computer from your own ROM, once.

## Windows

1. Unpack the ZIP into a folder of its own.
2. Make the assets, from a command prompt in that folder:

   ```
   SuperMetroidRebirthAssets.exe "C:\path\to\your\Super Metroid.smc"
   ```

   It reads the ROM, never changes it, and writes a folder named `assets` beside itself. That takes about a
   minute.
3. Start `SuperMetroidRebirth.exe`.

The game looks for `assets` beside `SuperMetroidRebirth.exe` and nowhere else. If it finds none it says so, with
the folder it looked in, and starts nothing.

## Android

The assets are made on a computer, as above, and copied to the device.

1. Install the APK.
2. Start the game once. It shows the folder it reads its assets from, which on most devices is

   ```
   Android/data/org.supermetroid.rebirth/files/Documents/assets
   ```

   on the device's own storage.
3. On a computer, make the assets with `SuperMetroidRebirthAssets.exe` from the Windows download, as above.
4. Copy the `assets` folder it wrote to the folder the game showed, so that the device has
   `.../files/Documents/assets/rooms`, `.../files/Documents/assets/samus` and the rest. The folder holds some
   fifty thousand small files, which a file browser copies slowly. With the device plugged in and USB debugging
   on, `adb` is much quicker:

   ```
   adb push assets /sdcard/Android/data/org.supermetroid.rebirth/files/Documents/
   ```
5. Start the game again.

## After an update

A new version may read assets the old one did not write. Make them again with the `SuperMetroidRebirthAssets.exe`
that came with the new version, and replace the `assets` folder.

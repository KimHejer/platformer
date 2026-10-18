Kix normalized PNG frames

All 28 PNGs are 192x96 RGBA with transparent backgrounds. Original artwork scaled uniformly to 50% with nearest-neighbor, and placed on consistent body/feet anchors. No per-frame stretching.

Replace Content/Sprites/Kix folders with these. Your LoadFrames code and filenames stay unchanged.

Game1.Draw(): draw frame in a 192x96 rectangle, at X = player center - 62, Y = player bottom - 90. This aligns the player body approximately to the existing collision rectangle. Do not draw at 64x64 (it would distort).

Note: these are visually aligned approximate anchors from source images, not artist-authored per-pose skeletal alignment. Hair, scarf, muzzle flash and dust remain outside collision bounds.

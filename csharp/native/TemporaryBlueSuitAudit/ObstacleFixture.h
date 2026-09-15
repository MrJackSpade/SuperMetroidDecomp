/* Native movement contacts the constructed solid wall/ceiling; no collision
   result or boost word is injected. */
static void install_blue_obstacle(int left, int mode) {
  int column = (samus_x_pos + (left ? -40 : 40)) >> 4;
  int row = (samus_y_pos - 64) >> 4;
  if (mode & 1)
    for (int y = 0; y < room_height_in_blocks; y++) level_data[y * room_width_in_blocks + column] = 0x8000;
  if (mode & 2)
    for (int x = 0; x < room_width_in_blocks; x++) level_data[row * room_width_in_blocks + x] = 0x8000;
}

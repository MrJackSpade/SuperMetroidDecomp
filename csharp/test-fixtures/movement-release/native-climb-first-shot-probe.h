#include "native-bounded-cpu.h"

// #1159: only the two reported Climb shot states, not a parameter search.
// Execute the original cartridge's beam producer, including its zero-speed
// bank-$94 collision call. Synthetic terrain has the reported cap/door layout;
// no cartridge bytes or player recording are stored in this fixture.
int DiagnosticClimbFirstShot(const char *rom) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  for (int stopped = 0; stopped < 2; stopped++) {
    cpu_reset(g_snes->cpu);
    memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false;
    g_snes->cpu->sp = 0x1ff0;
    g_snes->cpu->dp = 0;
    room_width_in_blocks = 32;
    room_height_in_blocks = 144;
    room_width_in_scrolls = 2;
    room_height_in_scrolls = 9;
    room_size_in_blocks = 32 * 144 * 2;
    level_data[134 * 32 + 30] = 0xc044;
    BTS[134 * 32 + 30] = 0x40;
    for (int row = 135; row <= 137; row++) {
      level_data[row * 32 + 30] = 0xd044;
      BTS[row * 32 + 30] = (uint8)(134 - row);
    }
    // All four cells behind the cap are solid door-transition blocks in Climb.
    for (int row = 134; row <= 137; row++) level_data[row * 32 + 31] = 0x9000;
    samus_pose = stopped ? 0x89 : 0x09;
    samus_movement_type = stopped ? 0x15 : 1;
    samus_pose_x_dir = 8;
    samus_x_pos = 475;
    samus_y_pos = 2187;
    samus_x_radius = 5;
    samus_y_radius = 21;
    button_config_shoot_x = joypad1_newkeys = 0x40;
    joypad1_lastkeys = 0x140;
    hud_item_index = 0;
    ProbeRunBounded(0x90b80d);
    int actors = 0;
    for (int slot = 0; slot < 40; slot++) actors += plm_header_ptr[slot] != 0;
    printf("CLIMB_FIRST_SHOT stopped=%d pose=%02X x=%d y=%d type=%04X actors=%d cap=%04X radius=%d/%d direction=%d list=%04X\n",
      stopped, samus_pose, projectile_x_pos[0], projectile_y_pos[0],
      projectile_type[0], actors, level_data[134 * 32 + 30],
      projectile_x_radius[0], projectile_y_radius[0], projectile_dir[0], projectile_bomb_instruction_ptr[0]);
    if (projectile_type[0] != 0x8700 || actors != stopped ||
        projectile_x_pos[0] != (stopped ? 494 : 498) ||
        projectile_y_pos[0] != (stopped ? 2182 : 2179)) return 10;
  }
  return 0;
}

#include "native-bounded-cpu.h"

// #411: execute retail fire dispatch and projectile owner, with controlled camera.
int DiagnosticHeroShot(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "follow,frame,camera,x,subx,y,suby,vx,vy,type,instruction\n");
  for (int follow = 0; follow < 2; follow++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_width_in_blocks = 64; room_height_in_blocks = 16;
    room_width_in_scrolls = 4; room_height_in_scrolls = 1;
    room_size_in_blocks = 64 * 16 * 2; // Native LevelDataSize is a byte count.
    for (int row = 0; row < 16; row++) level_data[row * 64 + 32] = 0x8000;
    samus_pose = 1; samus_pose_x_dir = 8; samus_x_pos = samus_y_pos = 128;
    button_config_shoot_x = 0x40;
    for (int frame = 0; frame < 120; frame++) {
      layer1_x_pos = follow && projectile_bomb_instruction_ptr[0] && projectile_x_pos[0] > 128
        ? projectile_x_pos[0] - 128 : 0;
      joypad1_lastkeys = joypad1_newkeys = frame == 0 ? 0x40 : 0;
      ProbeRunBounded(0x90b80d);
      projectile_index = 0;
      if (projectile_bomb_instruction_ptr[0]) ProbeRunBoundedRegisters(0x90aece, 0, 0, 0);
      fprintf(f, "%d,%d,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X\n", follow, frame,
        layer1_x_pos, projectile_x_pos[0], projectile_bomb_x_subpos[0], projectile_y_pos[0],
        projectile_bomb_y_subpos[0], projectile_bomb_x_speed[0], projectile_bomb_y_speed[0],
        projectile_type[0], projectile_bomb_instruction_ptr[0]);
      if (!projectile_bomb_instruction_ptr[0] || (projectile_type[0] & 0xf00)) break;
    }
  }
  fclose(f); return 0;
}

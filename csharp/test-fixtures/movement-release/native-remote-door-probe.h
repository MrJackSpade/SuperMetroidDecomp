#include "native-bounded-cpu.h"

// #463: real type-nine side effects through the observational wall-check entry.
int DiagnosticRemoteDoor(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "left,gap,blocker,fraction,distance,x,xsub,y,ysub,collision,amount,door,state\n");
  for (int left=0;left<2;left++)
  for (int gap=0;gap<17;gap++)
  for (int blocker=0;blocker<3;blocker++)
  for (int fraction=0;fraction<3;fraction++)
  for (int distance=1;distance<=9;distance++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    room_width_in_blocks=16; room_height_in_blocks=16;
    room_width_in_scrolls=1; room_height_in_scrolls=1; room_size_in_blocks=512;
    samus_x_pos=left ? 117+gap : 139-gap; samus_y_pos=136;
    samus_x_subpos=fraction==0 ? 0 : fraction==1 ? 0x8000 : 0xffff;
    samus_y_subpos=0x3456; samus_x_radius=5; samus_y_radius=12;
    samus_pose=left ? 0x1a : 0x19; door_list_pointer=0x927b;
    game_state=8;
    int column=left ? 6 : 9;
    level_data[8*16+column]=0x9000;
    if (blocker) level_data[(blocker==1 ? 7 : 9)*16+column]=0x8000;
    // DP $12/$14 are the native signed whole/fractional requested displacement.
    *(uint16 *)(g_ram+0x12)=left ? -distance : distance;
    *(uint16 *)(g_ram+0x14)=0;
    ProbeRunBounded(0x94967f);
    fprintf(f,"%d,%d,%d,%d,%d,%04X,%04X,%04X,%04X,%04X,%04X%04X,%04X,%04X\n",
      left,gap,blocker,fraction,distance,samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,
      samus_collision_flag,*(uint16 *)(g_ram+0x12),*(uint16 *)(g_ram+0x14),door_def_ptr,game_state);
  }
  fclose(f); return 0;
}

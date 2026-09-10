#include "native-bounded-cpu.h"

// #463: down-aim expansion observes, but must not activate, directional crumble blocks.
int DiagnosticPoseCrumble(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  fprintf(f,"gap,parity,bts,x,y,pose,block,active\n");
  for(int gap=0;gap<13;gap++) for(int parity=0;parity<2;parity++) for(int bts=0;bts<8;bts++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    room_width_in_blocks=16; room_height_in_blocks=16; room_size_in_blocks=512;
    interactive_enemy_indexes[0]=0xffff;
    samus_x_pos=136; samus_y_pos=150-gap; samus_y_subpos=0x3456;
    samus_x_radius=5; samus_y_radius=10;
    samus_prev_pose=0x2d; samus_pose=0x29;
    samus_pose_x_dir=samus_prev_pose_x_dir=8;
    samus_movement_type=samus_prev_movement_type2=6;
    samus_x_base_speed=1; samus_x_base_subspeed=0x4000;
    samus_y_dir=2; nmi_frame_counter_word=parity;
    level_data[10*16+8]=0xb000; BTS[10*16+8]=bts;
    ProbeRunBounded(0x91f404);
    int active=0; for(int i=0;i<40;i++) if(plm_header_ptr[i]) active++;
    fprintf(f,"%d,%d,%d,%04X%04X,%04X%04X,%02X,%04X,%d\n",gap,parity,bts,
      samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,samus_pose,level_data[10*16+8],active);
  }
  fclose(f); return 0;
}

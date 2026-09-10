#include "native-bounded-cpu.h"

// Native station access must distinguish ordinary contact from direction-$F probes.
int DiagnosticStationProbe(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  fprintf(f,"kind,left,probe,gap,collision,activated\n");
  const uint16 headers[]={0xb6d3,0xb6df,0xb6eb};
  for(int kind=0;kind<3;kind++)
  for(int left=0;left<2;left++)
  for(int probe=0;probe<2;probe++)
  for(int gap=0;gap<10;gap++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    room_width_in_blocks=16; room_height_in_blocks=16; room_size_in_blocks=512;
    room_width_in_scrolls=room_height_in_scrolls=1;
    samus_x_pos=left ? 117+gap : 139-gap; samus_y_pos=139;
    samus_x_radius=5; samus_y_radius=12;
    samus_pose=left ? 0x8a : 0x89; samus_pose_x_dir=left ? 4 : 8;
    samus_health=50; samus_max_health=99; samus_max_missiles=5;
    samus_collision_direction=left ? 0 : 1;
    int column=left ? 6 : 9;
    level_data[8*16+column]=0xb000; BTS[8*16+column]=0x47+kind*2+(left ? 0 : 1);
    int parent=8*16+column+(left ? -1 : kind==0 ? 2 : 1);
    plm_header_ptr[39]=headers[kind]; plm_block_indices[39]=2*parent;
    plm_instruction_timer[39]=9; plm_instruction_list_link_reg[39]=0xad62;
    *(uint16 *)(g_ram+0x12)=left ? -8 : 8; *(uint16 *)(g_ram+0x14)=0;
    ProbeRunBounded(probe ? 0x94967f : 0x949543);
    fprintf(f,"%d,%d,%d,%d,%d,%d\n",kind,left,probe,gap,probe ? samus_collision_flag : g_snes->cpu->c,
      plm_instruction_timer[39]==1);
  }
  fclose(f);return 0;
}

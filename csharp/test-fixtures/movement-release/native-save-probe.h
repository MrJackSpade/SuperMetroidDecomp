#include "native-bounded-cpu.h"

// Save triggers accept downward movement, never direction-$F pose observations.
int DiagnosticSaveProbe(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  fprintf(f,"probe,center,gap,parity,collision,activated\n");
  for(int probe=0;probe<2;probe++) for(int center=0;center<2;center++)
  for(int gap=0;gap<10;gap++) for(int parity=0;parity<2;parity++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    room_width_in_blocks=16; room_height_in_blocks=16; room_size_in_blocks=512;
    samus_x_pos=center ? 136 : 132; samus_y_pos=149-gap;
    samus_x_radius=5; samus_y_radius=12; samus_pose=1;
    samus_collision_direction=3; nmi_frame_counter_word=parity;
    int block=10*16+8;
    level_data[block]=0xb000; BTS[block]=0x4d;
    plm_header_ptr[39]=0xb76f; plm_block_indices[39]=2*block;
    plm_instruction_timer[39]=9; plm_instr_list_ptrs[39]=0xafb0;
    *(uint16 *)(g_ram+0x12)=7; *(uint16 *)(g_ram+0x14)=0;
    ProbeRunBounded(probe ? 0x9496ab : 0x949763);
    fprintf(f,"%d,%d,%d,%d,%d,%d\n",probe,center,gap,parity,samus_collision_flag,plm_instruction_timer[39]==1);
  }
  fclose(f);return 0;
}

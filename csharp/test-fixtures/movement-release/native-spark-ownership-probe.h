#include "native-bounded-cpu.h"

static void SparkOwnershipRow(FILE *f, int beam, int order, int stage) {
  fprintf(f,"%d,%d,%d,%04X",beam,order,stage,projectile_counter);
  for(int slot=3;slot<=4;slot++) fprintf(f,",%04X,%04X,%04X,%04X,%04X",
    projectile_type[slot],projectile_bomb_pre_instructions[slot],
    speed_echo_xspeed[slot-1],speed_echo_xpos[slot-1],speed_echo_ypos[slot-1]);
  fprintf(f,"\n");
}

// Boundary order matrix: combo then crash finish, or finish then combo.
// No claim of controller reachability; captures distinct native storage owners.
int DiagnosticSparkOwnership(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  const int beams[]={1,2,4,8};
  fprintf(f,"beam,order,stage,count,type3,pre3,draw3,x3,y3,type4,pre4,draw4,x4,y4\n");
  for(int beam=0;beam<4;beam++) for(int order=0;order<2;order++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    samus_x_pos=samus_y_pos=128; samus_pose=0xc9; samus_pose_x_dir=8;
    equipped_beams=0x1000|beams[beam]; samus_power_bombs=2; hud_item_index=3;
    ProbeRunBounded(order?0x90d40d:0x90ccc0);
    SparkOwnershipRow(f,beams[beam],order,0);
    ProbeRunBounded(order?0x90ccc0:0x90d40d);
    SparkOwnershipRow(f,beams[beam],order,1);
    // Advance only still-installed departing echo handlers. A replaced slot must
    // not keep running its old handler, even when its drawing word remains set.
    for(int frame=0;frame<20;frame++) {
      for(int slot=4;slot>=3;slot--)
        if(projectile_bomb_pre_instructions[slot]==0xd4d2)
          ProbeRunBoundedRegisters(0x90d4d2,0,slot*2,0);
      SparkOwnershipRow(f,beams[beam],order,frame+2);
    }
  }
  fclose(f); return 0;
}

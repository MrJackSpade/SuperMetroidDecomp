#include "native-bounded-cpu.h"

// Original crash-finish and released-echo handlers, not the host C translation.
int DiagnosticSparkDeparture(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  fprintf(f,"pose,frame,slot,active,radius,x,y\n");
  for(int pose=0xc9;pose<=0xce;pose++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    samus_x_pos=samus_y_pos=128; samus_pose=pose;
    samus_pose_x_dir=(pose&1)?8:4;
    ProbeRunBounded(0x90d40d);
    for(int frame=0;frame<40;frame++) {
      if(frame) {
        // Descending native projectile order; no controller/movement is synthesized.
        for(int slot=4;slot>=3;slot--) if(projectile_type[slot])
          ProbeRunBoundedRegisters(0x90d4d2,0,slot*2,0);
      }
      for(int slot=3;slot<=4;slot++)
        fprintf(f,"%d,%d,%d,%d,%04X,%04X,%04X\n",pose,frame,slot,
          projectile_type[slot]!=0,projectile_bomb_x_speed[slot],
          speed_echo_xpos[slot-1],speed_echo_ypos[slot-1]);
    }
  }
  fclose(f); return 0;
}

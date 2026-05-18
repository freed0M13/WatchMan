<script>
  // Mock data: { lat, lng, intensity }
  export let data = [
    { id: 1, x: 25, y: 35, intensity: 0.8 }, // US
    { id: 2, x: 75, y: 45, intensity: 0.4 }, // China
    { id: 3, x: 50, y: 30, intensity: 0.9 }, // Europe
    { id: 4, x: 85, y: 60, intensity: 0.2 }  // Australia
  ];
</script>

<div class="geo-map">
  <!-- Simplified SVG World Map Base -->
  <svg viewBox="0 0 100 60" class="map-svg" preserveAspectRatio="none">
    <path class="land" d="M10,15 Q15,10 20,12 T30,20 Q35,15 40,25 T30,40 Q20,35 15,30 Z" />
    <path class="land" d="M45,20 Q50,15 55,20 T60,30 Q55,40 45,35 T40,25 Z" />
    <path class="land" d="M65,10 Q75,5 85,15 T90,30 Q80,40 70,35 T60,20 Z" />
    <path class="land" d="M75,45 Q80,40 85,45 T90,55 Q80,60 75,55 Z" />
    <path class="land" d="M25,35 Q30,40 35,50 T30,60 Q20,55 25,45 Z" />
    
    <!-- Plot Points -->
    {#each data as point}
      <circle 
        cx={point.x} 
        cy={point.y} 
        r={point.intensity * 2 + 0.5} 
        class="point" 
        style="opacity: {point.intensity}"
      />
      <!-- Pulse effect for high intensity -->
      {#if point.intensity > 0.7}
        <circle 
          cx={point.x} 
          cy={point.y} 
          r={point.intensity * 2 + 0.5} 
          class="point-pulse" 
        />
      {/if}
    {/each}
  </svg>
</div>

<style>
  .geo-map {
    width: 100%;
    height: 100%;
    min-height: 250px;
    background: var(--surface-container-lowest);
    border-radius: var(--radius-md);
    display: flex;
    align-items: center;
    justify-content: center;
    overflow: hidden;
    position: relative;
  }

  .map-svg {
    width: 100%;
    height: 100%;
    stroke-linejoin: round;
  }

  .land {
    fill: var(--surface-container-highest);
    stroke: var(--panel-border);
    stroke-width: 0.2;
    transition: fill var(--transition-slow);
  }

  .point {
    fill: var(--error);
  }

  .point-pulse {
    fill: none;
    stroke: var(--error);
    stroke-width: 0.5;
    animation: ping 2s cubic-bezier(0, 0, 0.2, 1) infinite;
  }

  @keyframes ping {
    75%, 100% {
      transform: scale(2);
      opacity: 0;
    }
  }
</style>

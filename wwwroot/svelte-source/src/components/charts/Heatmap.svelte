<script>
  /** @type {Array<{day: string, hour: number, intensity: number}>} */
  export let data = [];
  
  const days = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
  const hours = Array.from({ length: 24 }, (_, i) => i);

  // Generate mock data if not provided
  if (!data || data.length === 0) {
    for (let d of days) {
      for (let h of hours) {
        data.push({
          day: d,
          hour: h,
          intensity: Math.random() > 0.6 ? Math.random() : Math.random() * 0.3
        });
      }
    }
  }

  /** @param {number} intensity */
  function getColor(intensity) {
    if (intensity < 0.2) return 'var(--surface-container-high)';
    if (intensity < 0.4) return 'var(--primary-container)';
    if (intensity < 0.7) return 'var(--primary)';
    return 'var(--error)';
  }
</script>

<div class="heatmap-container">
  <div class="heatmap-grid">
    <div class="header-corner"></div>
    {#each hours as hour}
      <div class="hour-label">{hour}h</div>
    {/each}
    
    {#each days as day}
      <div class="day-label">{day}</div>
      {#each hours as hour}
        {@const point = data.find(d => d.day === day && d.hour === hour)}
        <div 
          class="heatmap-cell" 
          style="background: {getColor(point?.intensity || 0)}"
          title="{day} {hour}:00 - Intensity: {Math.round((point?.intensity || 0) * 100)}%"
        ></div>
      {/each}
    {/each}
  </div>
</div>

<style>
  .heatmap-container {
    width: 100%;
    overflow-x: auto;
    background: var(--surface-container-lowest);
    padding: var(--space-md);
    border-radius: var(--radius-md);
  }

  .heatmap-grid {
    display: grid;
    grid-template-columns: 40px repeat(24, minmax(15px, 1fr));
    gap: 2px;
    min-width: 500px;
  }

  .hour-label, .day-label {
    font: var(--text-mono-label);
    color: var(--outline);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 9px;
  }

  .day-label {
    justify-content: flex-end;
    padding-right: var(--space-sm);
  }

  .heatmap-cell {
    aspect-ratio: 1;
    border-radius: 2px;
    transition: transform var(--transition-fast), opacity var(--transition-fast);
    cursor: crosshair;
  }

  .heatmap-cell:hover {
    transform: scale(1.2);
    box-shadow: 0 0 5px rgba(0,0,0,0.5);
    z-index: 10;
    position: relative;
  }
</style>

<!--
  ConnectionFlow.svelte
  Connection Flow Topology visualization for Network Analysis page.
  Props: sources, destinations
-->
<script>
  /**
   * @typedef {{ label: string }} FlowNode
   */

  /** @type {FlowNode[]} */
  export let sources = [];
  /** @type {FlowNode[]} */
  export let destinations = [];
</script>

<div class="flow-container">
  <div class="flow-column">
    {#each sources as src}
      <div class="flow-node source">{src.label}</div>
    {/each}
  </div>

  <div class="flow-lines">
    <svg viewBox="0 0 120 200" preserveAspectRatio="none">
      {#each sources as _, si}
        {#each destinations as _, di}
          <line
            x1="0" y1={30 + si * 60}
            x2="120" y2={30 + di * 60}
            stroke="var(--outline-variant)"
            stroke-width="1"
            opacity="0.4"
          />
        {/each}
      {/each}
    </svg>
  </div>

  <div class="flow-column">
    {#each destinations as dst}
      <div class="flow-node destination">{dst.label}</div>
    {/each}
  </div>
</div>

<style>
  .flow-container {
    display: flex;
    align-items: stretch;
    gap: 0;
    width: 100%;
  }

  .flow-column {
    display: flex;
    flex-direction: column;
    gap: var(--space-md);
    flex: 1;
  }

  .flow-lines {
    width: 120px;
    flex-shrink: 0;
  }

  .flow-lines svg {
    width: 100%;
    height: 100%;
  }

  .flow-node {
    padding: var(--space-sm) var(--space-md);
    border: 1px solid var(--panel-border);
    border-radius: var(--radius-default);
    font: var(--text-mono-code);
    color: var(--on-surface-variant);
    text-align: center;
    background: var(--surface-container-low);
    transition: border-color var(--transition-fast);
  }

  .flow-node:hover {
    border-color: var(--primary);
  }
</style>

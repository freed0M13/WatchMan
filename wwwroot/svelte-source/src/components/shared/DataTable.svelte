<!--
  DataTable.svelte
  Reusable data table with alternating rows, used in Logs, Users, Whitelist pages.
  Props: columns, rows, selectable, selectedIndex
-->
<script>
  import { createEventDispatcher } from 'svelte';

  const dispatch = createEventDispatcher();

  /**
   * @typedef {{ key: string, label: string, mono?: boolean, width?: string }} Column
   */

  /** @type {Column[]} */
  export let columns = [];
  /** @type {Record<string, any>[]} */
  export let rows = [];
  /** @type {boolean} */
  export let selectable = false;
  /** @type {number} */
  export let selectedIndex = -1;

  /** @param {number} index */
  function handleRowClick(index) {
    if (selectable) {
      selectedIndex = index;
      dispatch('select', { index, row: rows[index] });
    }
  }
</script>

<div class="table-container">
  <table>
    <thead>
      <tr>
        {#if selectable}
          <th class="col-check" style="width: 40px;"></th>
        {/if}
        {#each columns as col}
          <th style={col.width ? `width: ${col.width}` : ''}>
            {col.label}
          </th>
        {/each}
      </tr>
    </thead>
    <tbody>
      {#each rows as row, i}
        <tr
          class:selected={selectedIndex === i}
          on:click={() => handleRowClick(i)}
        >
          {#if selectable}
            <td class="col-check">
              <span class="checkbox" class:checked={selectedIndex === i}>
                {#if selectedIndex === i}✓{/if}
              </span>
            </td>
          {/if}
          {#each columns as col}
            <td class:mono={col.mono}>
              {#if $$slots.default}
                <slot {row} col={col} index={i} />
              {:else}
                {row[col.key] ?? ''}
              {/if}
            </td>
          {/each}
        </tr>
      {/each}
    </tbody>
  </table>
</div>

<style>
  .table-container {
    overflow-x: auto;
    width: 100%;
  }

  table {
    width: 100%;
    border-collapse: collapse;
  }

  thead th {
    font: var(--text-mono-label);
    text-transform: uppercase;
    letter-spacing: 0.05em;
    color: var(--outline);
    text-align: left;
    padding: var(--space-sm) var(--space-md);
    border-bottom: 1px solid var(--panel-border);
    white-space: nowrap;
  }

  tbody tr {
    cursor: default;
    transition: background var(--transition-fast);
  }

  tbody tr:nth-child(even) {
    background: rgba(255, 255, 255, 0.02);
  }

  tbody tr:hover {
    background: rgba(255, 255, 255, 0.04);
  }

  tbody tr.selected {
    background: rgba(192, 193, 255, 0.08);
  }

  td {
    padding: var(--space-sm) var(--space-md);
    font: var(--text-body-md);
    color: var(--on-surface);
    border-bottom: 1px solid var(--panel-border-light);
    vertical-align: middle;
  }

  td.mono {
    font: var(--text-mono-code);
  }

  .col-check {
    width: 40px;
    text-align: center;
  }

  .checkbox {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 18px;
    height: 18px;
    border: 1px solid var(--outline-variant);
    border-radius: 3px;
    font-size: 11px;
    color: var(--primary);
    transition: border-color var(--transition-fast), background var(--transition-fast);
  }

  .checkbox.checked {
    background: rgba(192, 193, 255, 0.15);
    border-color: var(--primary);
  }
</style>

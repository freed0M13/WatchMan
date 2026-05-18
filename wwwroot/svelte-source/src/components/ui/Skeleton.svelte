<script>
  export let width = '100%';
  export let height = '20px';
  export let borderRadius = 'var(--radius-md)';
</script>

<div class="skeleton" style="width: {width}; height: {height}; border-radius: {borderRadius}"></div>

<style>
  .skeleton {
    background: var(--surface-container-highest);
    position: relative;
    overflow: hidden;
  }

  .skeleton::after {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: linear-gradient(
      90deg,
      transparent,
      rgba(255, 255, 255, 0.05),
      transparent
    );
    animation: shimmer 1.5s infinite;
  }

  @keyframes shimmer {
    0% { transform: translateX(-100%); }
    100% { transform: translateX(100%); }
  }
</style>

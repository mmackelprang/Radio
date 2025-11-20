// Audio Visualizer Canvas Renderer
let visualizerCanvas = null;
let visualizerContext = null;
let currentFFTData = [];
let animationFrameId = null;

// Initialize the visualizer canvas
window.initializeVisualizer = function (canvasElement) {
  if (!canvasElement) {
    console.error('Canvas element not provided');
    return;
  }

  visualizerCanvas = canvasElement;
  visualizerContext = visualizerCanvas.getContext('2d');

  // Set canvas size to match container
  resizeCanvas();
  window.addEventListener('resize', resizeCanvas);

  // Start animation loop
  startAnimation();

  console.log('Visualizer initialized');
};

// Resize canvas to match container size
function resizeCanvas() {
  if (!visualizerCanvas) return;

  const container = visualizerCanvas.parentElement;
  visualizerCanvas.width = container.clientWidth;
  visualizerCanvas.height = container.clientHeight;
}

// Update FFT data from SignalR
window.updateVisualizerData = function (fftData) {
  if (!fftData || fftData.length === 0) return;
  currentFFTData = fftData;
};

// Animation loop
function startAnimation() {
  function animate() {
    drawVisualization();
    animationFrameId = requestAnimationFrame(animate);
  }
  animate();
}

// Draw the visualization bars
function drawVisualization() {
  if (!visualizerContext || !visualizerCanvas) return;

  const ctx = visualizerContext;
  const width = visualizerCanvas.width;
  const height = visualizerCanvas.height;

  // Clear canvas with gradient background
  const gradient = ctx.createLinearGradient(0, 0, 0, height);
  gradient.addColorStop(0, '#0a0a0a');
  gradient.addColorStop(1, '#1a1a1a');
  ctx.fillStyle = gradient;
  ctx.fillRect(0, 0, width, height);

  // If no FFT data, draw placeholder
  if (!currentFFTData || currentFFTData.length === 0) {
    drawPlaceholder(ctx, width, height);
    return;
  }

  // Calculate bar dimensions
  const barCount = Math.min(currentFFTData.length, 64); // Limit to 64 bars for clean display
  const barWidth = (width / barCount) * 0.8;
  const barGap = (width / barCount) * 0.2;

  // Draw bars
  for (let i = 0; i < barCount; i++) {
    const dataIndex = Math.floor((i / barCount) * currentFFTData.length);
    const value = currentFFTData[dataIndex] || 0;
    
    // Normalize value (assuming FFT data is 0-1 range)
    const normalizedValue = Math.max(0, Math.min(1, value));
    const barHeight = normalizedValue * height * 0.9;

    // Calculate bar position
    const x = i * (barWidth + barGap);
    const y = height - barHeight;

    // Create gradient for bar color (from bottom to top)
    const barGradient = ctx.createLinearGradient(x, height, x, y);
    
    // Color based on intensity: green -> yellow -> red
    if (normalizedValue < 0.5) {
      barGradient.addColorStop(0, '#4caf50'); // Green
      barGradient.addColorStop(1, '#8bc34a'); // Light green
    } else if (normalizedValue < 0.75) {
      barGradient.addColorStop(0, '#ffc107'); // Yellow
      barGradient.addColorStop(1, '#ffeb3b'); // Light yellow
    } else {
      barGradient.addColorStop(0, '#f44336'); // Red
      barGradient.addColorStop(1, '#ff5722'); // Light red
    }

    ctx.fillStyle = barGradient;
    ctx.fillRect(x, y, barWidth, barHeight);

    // Add glow effect for higher values
    if (normalizedValue > 0.6) {
      ctx.shadowColor = normalizedValue > 0.8 ? '#ff5722' : '#ffc107';
      ctx.shadowBlur = 10;
      ctx.fillRect(x, y, barWidth, barHeight);
      ctx.shadowBlur = 0;
    }
  }

  // Draw center line
  ctx.strokeStyle = '#424242';
  ctx.lineWidth = 1;
  ctx.beginPath();
  ctx.moveTo(0, height / 2);
  ctx.lineTo(width, height / 2);
  ctx.stroke();
}

// Draw placeholder when no data
function drawPlaceholder(ctx, width, height) {
  ctx.fillStyle = '#424242';
  ctx.font = '16px Roboto, sans-serif';
  ctx.textAlign = 'center';
  ctx.textBaseline = 'middle';
  ctx.fillText('Waiting for audio data...', width / 2, height / 2);

  // Draw some static bars as placeholder
  const barCount = 32;
  const barWidth = (width / barCount) * 0.8;
  const barGap = (width / barCount) * 0.2;

  for (let i = 0; i < barCount; i++) {
    const x = i * (barWidth + barGap);
    const randomHeight = Math.random() * height * 0.3;
    const y = height - randomHeight;

    ctx.fillStyle = '#2c2c2c';
    ctx.fillRect(x, y, barWidth, randomHeight);
  }
}

// Cleanup
window.disposeVisualizer = function () {
  if (animationFrameId) {
    cancelAnimationFrame(animationFrameId);
    animationFrameId = null;
  }
  window.removeEventListener('resize', resizeCanvas);
  visualizerCanvas = null;
  visualizerContext = null;
  currentFFTData = [];
};

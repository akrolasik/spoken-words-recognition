import { FrequenciesChunk } from '../services/api-client/FrequenciesChunk';
import { WordOccurence } from '../services/api-client/WordOccurence';
import { ValidationState } from './ValidationState';

export class StaticSoundChart {

  canvas: HTMLCanvasElement;
  canvasContext: CanvasRenderingContext2D;
  barWidth = 4;
  barSpace = 2;

  initDrawing(canvas: HTMLCanvasElement) {
    this.canvas = canvas;
    this.canvasContext = this.canvas.getContext('2d');
  }

  onResize() {
    if (this.canvas == null) { return; }
    const rect = this.canvas.getBoundingClientRect();
    this.canvas.width = rect.width;
    this.canvas.height = rect.height;
  }

  draw(state: ValidationState) {
    const canvasCenter = Math.floor(this.canvas.width / 2);
    const drawingAxis = Math.floor(this.canvas.height * 2 / 3);

    this.clearCanvas();

    let x = canvasCenter - state.frequenciesChunks.length / 2 * (this.barWidth + this.barSpace);

    state.frequenciesChunks.forEach(frequency => {
      this.setBarColor(frequency, state.wordOccurence);
      const h = Math.max(Math.floor(frequency.average / 2), this.barSpace);
      this.canvasContext.fillRect(x, drawingAxis - h, this.barWidth, h * 2);
      x += this.barWidth + this.barSpace;
    });
  }

  setBarColor(frequency: FrequenciesChunk, currentWord: WordOccurence) {
    if (frequency.milliseconds > currentWord.start && frequency.milliseconds < currentWord.end) {
      this.canvasContext.fillStyle = '#000000';
    } else if (currentWord.validated == null) {
      this.canvasContext.fillStyle = '#42aaff';
    } else if (currentWord.validated) {
      this.canvasContext.fillStyle = '#2ce69b';
    } else {
      this.canvasContext.fillStyle = '#ff708d';
    }
  }

  clearCanvas() {
    this.canvasContext.fillStyle = '#fff';
    this.canvasContext.fillRect(0, 0, this.canvas.width, this.canvas.height);
  }
}

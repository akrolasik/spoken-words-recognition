import { SoundRecognition } from './SoundRecognition';
import { AudioManager } from './AudioManager';
import { Word } from './Word';
import { WordOccurence } from '../services/api-client/WordOccurence';
import { Modification } from '../services/api-client/Modification';
import { FrequenciesChunk } from '../services/api-client/FrequenciesChunk';
import { ModificationColors } from './ModificationColors';

export class DynamicSoundChart {

  drawTooQuieteInfo: any;

  private visibleDurationMs = 3000;
  private transitionTimeMs = 500;
  private wordHalfDurationMs = 500;
  private minimalBarSize = 3;

  private canvas: HTMLCanvasElement;
  private canvasContext: CanvasRenderingContext2D;

  private animationStartTime: number;
  private chartColor: string;
  private interval: NodeJS.Timer;

  private recognition: SoundRecognition;
  private audio: AudioManager;
  private word: Word;
  private wordOccurence: WordOccurence;

  constructor(canvas: HTMLCanvasElement, audio: AudioManager, recognition: SoundRecognition) {
      this.audio = audio;
      this.recognition = recognition;

      this.canvas = canvas;
      this.canvasContext = this.canvas.getContext('2d');
  }

  start(ms: number = 0): void {
    this.interval = setInterval(() => this.draw(), ms);
  }

  stop(): void {
    clearInterval(this.interval);
  }

  private draw(): void {
    window.requestAnimationFrame(() => {

      this.clear();
      this.drawRecording();

      if (this.word) {
        this.drawWord(this.word);
      }

      if (this.wordOccurence != null) {
        this.drawWord(this.wordOccurence.word, this.wordOccurence.milliseconds);
      }
    });
  }

  private aggregateFrequencies(frequencies: FrequenciesChunk[]): any[] {
    const aggregated: any[] = [];
    const num = 4;

    for (let i = 0 ; i < frequencies.length - num - 1; i += num) {
      let sum = 0;
      for (let f = i; f < i + num; f++) {
        sum += frequencies[f].average;
      }
      aggregated.push({
        milliseconds: frequencies[i].milliseconds,
        value: sum / num
      });
    }

    return aggregated;
  }

  private drawRecording() {

    if (this.audio.frequencyChunks.length === 0) { return; }

    const now = performance.now() - this.audio.recordingStartTime;
    const frequencies: any[] = [];

    this.aggregateFrequencies(this.audio.frequencyChunks).forEach(chunk => {
      if (chunk.milliseconds > now - this.visibleDurationMs) {
        frequencies.push(chunk);
      }
    });

    if (frequencies.length === 0) { return; }

    let x0 = (frequencies[0].milliseconds - now + this.visibleDurationMs) /
      this.visibleDurationMs * this.canvas.width;

    const x1 = (frequencies[frequencies.length - 1].milliseconds - now + this.visibleDurationMs) /
      this.visibleDurationMs * this.canvas.width;

    const width = (x1 - x0) / frequencies.length;

    this.recognition.updateWordOccurence(frequencies);

    this.displayToQuietMessage();

    frequencies.forEach(freq => {

      this.canvasContext.fillStyle = this.getBarColor(freq.milliseconds, freq.value);

      const height = Math.max(this.minimalBarSize, freq.value * this.canvas.height / 50);
      const y = this.canvas.height * 3 / 4 - height / 2;

      this.canvasContext.fillRect(x0, y, width - this.minimalBarSize, height);
      x0 += width;

    });
  }

  private displayToQuietMessage() {
    if (this.recognition.itsTooQuiete) {
      const x = this.canvas.width / 2;
      const y = this.canvas.height / 2;
      this.drawText('Microphone is off or you are reading too quiet', { x, y }, '#ff708d', '20px Arial');
    }
  }

  private getBarColor(timespan: number, value: number) {
    let result = this.chartColor;
    this.recognition.wordOccurrences.forEach(occurence => {
      const nearCenter = Math.abs(timespan - occurence.milliseconds) < this.wordHalfDurationMs;
      if (nearCenter && value > 0.5) {
        result = '#000000';
      }
    });
    return result;
  }

  private clear() {
    this.canvasContext.fillStyle = '#ffffff';
    this.canvasContext.fillRect(0, 0, this.canvas.width, this.canvas.height);
  }

  private drawWord(word: Word, occuranceTimeMs?: number) {

    const animationTime = performance.now() - this.animationStartTime;
    let posAlpha = this.getWordAppearing(animationTime);

    if (occuranceTimeMs != null) {
      posAlpha = this.getWordDisappearing(posAlpha, animationTime, occuranceTimeMs);
    }

    const alphaColor = this.numberToHex(posAlpha.alpha * 255);
    const color1 = '#000000' + alphaColor;
    const color2 = occuranceTimeMs == null ? color1 : '#000000';
    const color3 = '#888888' + alphaColor;

    this.drawText(word.value, { x: posAlpha.x, y: posAlpha.y }, color1, '60px Arial');
    this.drawText(word.inWords, { x: posAlpha.x, y: posAlpha.y + 30 }, color2, '20px Arial');
    this.drawText(word.pronounce, { x: posAlpha.x, y: posAlpha.y + 60 }, color3, '20px Arial');
  }

  private getWordAppearing(animationTime: number) {
    const alpha = Math.min(animationTime / this.transitionTimeMs, 1);
    const x = this.canvas.width / 2;
    const y = this.canvas.height / 6;

    return { x, y, alpha };
  }

  private getWordDisappearing(positionAlpha: any, animationTime: number, occuranceTimeMs: number) {
    const targetY = this.canvas.height / 2.4;
    const now = performance.now() - this.audio.recordingStartTime;
    const targetX = (occuranceTimeMs - now + this.visibleDurationMs) /
      this.visibleDurationMs * this.canvas.width;

    const alpha = Math.max((this.transitionTimeMs - animationTime) / this.transitionTimeMs, 0);
    const y = positionAlpha.y * alpha + targetY * (1 - alpha);
    const x = positionAlpha.x * alpha + targetX * (1 - alpha);

    return { x, y, alpha };
  }

  private drawText(text: string, center: any, color: string, font: string) {
    this.canvasContext.fillStyle = color;
    this.canvasContext.font = font;
    const size = this.canvasContext.measureText(text);
    this.canvasContext.fillText(text, center.x - size.width / 2, center.y);
  }

  private numberToHex(num: number) {
    let hex = Math.floor(num).toString(16);
    if (hex.length < 2) { hex = '0' + hex; }
    return hex;
  }

  showWord(wordOccurence: WordOccurence, word: Word) {
    this.animationStartTime = performance.now();
    this.wordOccurence = wordOccurence;
    this.word = word;
  }

  setChartColor(modification: Modification) {
    this.chartColor = ModificationColors.get(modification);
  }
}

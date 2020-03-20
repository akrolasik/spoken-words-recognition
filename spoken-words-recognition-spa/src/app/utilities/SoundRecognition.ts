import { WordOccurence } from '../services/api-client/WordOccurence';
import { Word } from './Word';
import { AudioManager } from './AudioManager';

export class SoundRecognition {

    wordOccurrences: WordOccurence[];
    itsTooQuiete: boolean;
    currentWord: Word;

    private minimalRecordingTimeMs = 2000;
    private targetReliability = 100;
    private maximumWaitingTimeMs = 1000;
    private minimumWaitingTimeMs = 500;
    private frequenciesRange = 8;
    private detectionLevel = 32;

    private wordPool: Word[];
    private audio: AudioManager;
    private wordShowTime: number;
    private quietTime: number;
    private reliability: any = 0;
    private lastOccurence: any;

    wordRecognized: (occurence: WordOccurence, nextWord: Word) => void;

    constructor(audio: AudioManager, wordPool: Word[]) {
      this.wordPool = [];
      wordPool.forEach(element => {
        this.wordPool.push(element);
      });
      this.audio = audio;
      this.wordOccurrences = [];
      this.quietTime = this.minimalRecordingTimeMs + this.maximumWaitingTimeMs;
    }

    start() {
        this.currentWord = this.getCurrentWord();
      }

    updateWordOccurence(frequencies: any[]) {

      if (performance.now() - this.audio.recordingStartTime < this.minimalRecordingTimeMs) { return; }

      const occurenceTime = this.findWordOccurenceTime(frequencies);
      this.updateTooQuiete(occurenceTime);

      if (occurenceTime == null) { return; }

      this.updateReliability(occurenceTime);

      if (this.reliability > this.targetReliability &&
        this.wordOccurrences.findIndex(x => x.milliseconds === occurenceTime) === -1 &&
        occurenceTime > this.wordShowTime + this.minimumWaitingTimeMs) {

        const occurence: WordOccurence = {
          word: this.currentWord,
          milliseconds: occurenceTime
        };

        this.wordOccurrences.push(occurence);
        this.currentWord = this.getCurrentWord();
        this.wordRecognized(occurence, this.currentWord);
      }
    }

    private updateTooQuiete(occurence: number) {

      this.itsTooQuiete = false;

      const now = performance.now() - this.audio.recordingStartTime;

      if (occurence != null) {
        this.quietTime = now;
      }

      if (now - this.quietTime > this.maximumWaitingTimeMs) {
        this.itsTooQuiete = true;
      }
    }

    private getCurrentWord() {
      if (this.wordPool.length === 0) { return null; }

      const random = Math.floor(Math.random() * this.wordPool.length);
      const currentWord = this.wordPool[random];
      this.wordPool.splice(random, 1);
      this.wordShowTime = performance.now() - this.audio.recordingStartTime;

      return currentWord;
    }

    findWordOccurenceTime(frequencies: any[]): number {

      let occurence: number;
      let maximumSum = 0;

      for (let f = this.frequenciesRange; f < frequencies.length - this.frequenciesRange; f++) {

        let sum = 0;
        let weight = 0;

        for (let s = f - this.frequenciesRange; s < f + this.frequenciesRange; s++) {
          sum += frequencies[s].value;
          weight += frequencies[s].value * frequencies[s].milliseconds;
        }

        if (sum < this.detectionLevel) {
          maximumSum = 0;
        }

        if (sum > this.detectionLevel && sum > maximumSum) {
          maximumSum = sum;
          occurence = weight / sum;
        }
      }

      return occurence;
    }

    private updateReliability(occurence: number) {
      if (occurence === this.lastOccurence) {
        this.reliability++;
      } else {
        this.reliability = 0;
      }

      this.lastOccurence = occurence;
    }
  }

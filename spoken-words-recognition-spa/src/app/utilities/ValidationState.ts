import { RawRecording } from '../services/api-client/RawRecording';
import { FrequenciesChunk } from '../services/api-client/FrequenciesChunk';
import { Frequencies } from '../services/api-client/Frequencies';
import { WordOccurence } from '../services/api-client/WordOccurence';
import { RawDataLoader } from './RawDataLoader';
import { StaticSoundChart } from './StaticSoundChart';

export class ValidationState {

    public rawRecording: RawRecording;
    public audio: HTMLAudioElement;
    public frequencies: Frequencies;
    public wordOccurence: WordOccurence;
    public frequenciesChunks: FrequenciesChunk[];
    public audioInterval: NodeJS.Timer;
    public playFrom: number;
    public playTo: number;

    prepareData(data: RawDataLoader) {
      if (this.audio != null) {
        this.audio.pause();
      }

      this.rawRecording = data.rawData[0].rawRecording;
      this.audio = data.rawData[0].audio;
      this.frequencies = data.rawData[0].frequencies;

      this.frequencies.frequenciesChunks.forEach(chunk => {
        chunk.average = Frequencies.getAverage(chunk.data);
      });
    }

    getWord(index: number) {
      const currentIndex = this.rawRecording.words.findIndex(x => x.word === this.wordOccurence.word);
      const newIndex = currentIndex + index;

      if (newIndex < 0 || newIndex === this.rawRecording.words.length) { return null; }

      return this.rawRecording.words[newIndex].word.value;
    }

    play() {
      if (this.audio == null) { return; }

      this.audio.play();
      this.audio.currentTime = this.playFrom / 1000;

      clearInterval(this.audioInterval);

      this.audioInterval = setInterval(() => {
        if (this.audio.currentTime > this.playTo / 1000) {
          this.audio.pause();
          clearInterval(this.audioInterval);
        }
      }, 0);
    }

    firstWord() {
      return this.rawRecording.words[0].word;
    }

    lastWord() {
      return this.rawRecording.words[this.rawRecording.words.length - 1].word;
    }

    findBorders() {

      if (this.wordOccurence == null) { return; }
      if (this.wordOccurence.start != null) { return; }

      const middleIndex = Math.floor(this.frequenciesChunks.length / 2);
      const lowPass = 8;

      this.wordOccurence.start = 0;
      this.wordOccurence.end = 0;

      for (let i = 0; i < middleIndex; i++) {
        if (this.frequenciesChunks[i].average < lowPass) {
        this.wordOccurence.start = this.frequenciesChunks[i].milliseconds;
        }
      }

      for (let i = this.frequenciesChunks.length - 1; i > middleIndex; i--) {
        if (this.frequenciesChunks[i].average < lowPass) {
        this.wordOccurence.end = this.frequenciesChunks[i].milliseconds;
      }
        }
    }

    setCurrentWord(word: string) {
      this.wordOccurence = this.rawRecording.words.find(x => x.word.value === word);
      const occurence = this.rawRecording.words.find(x => x.word.value === word);

      const halfDuration = 750;
      this.playFrom = occurence.milliseconds - halfDuration;
      this.playTo = occurence.milliseconds + halfDuration;

      this.getFrequencies();
      this.findBorders();
      this.play();
    }

    updateBorders(drawing: StaticSoundChart, mouseDownPosition: number, mouseUpPosition: number) {
        const canvasCenter = Math.floor(drawing.canvas.width / 2);
        let x = canvasCenter - this.frequenciesChunks.length / 2 * (drawing.barWidth + drawing.barSpace);
        this.frequenciesChunks.forEach(frequency => {
          if (x < mouseDownPosition - drawing.barWidth) { this.wordOccurence.start = frequency.milliseconds; }
          if (x < mouseUpPosition + drawing.barWidth) { this.wordOccurence.end = frequency.milliseconds; }
          x += drawing.barWidth + drawing.barSpace;
        });
      }

    getFrequencies() {
      this.frequenciesChunks = this.frequencies.frequenciesChunks.filter(e =>
        e.milliseconds >= this.playFrom &&
        e.milliseconds <= this.playTo);
    }
  }

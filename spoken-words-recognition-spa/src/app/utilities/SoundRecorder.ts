import { WordOccurence } from '../services/api-client/WordOccurence';
import { DynamicSoundChart } from './DynamicSoundChart';
import { SoundRecognition } from './SoundRecognition';
import { AudioManager } from './AudioManager';
import { AudioSettings } from './AudioSettings';
import { AnalyzerSettings } from './AnalyzerSettings';
import { Word } from './Word';
import { Modification } from '../services/api-client/Modification';
import { SoundRecorderResult } from './SoundRecorderResult';

export class SoundRecorder {

  private drawing: DynamicSoundChart;
  private audio: AudioManager;
  private recognition: SoundRecognition;

  private onRecordingEnd: (recorderResult: SoundRecorderResult) => void;

  constructor(canvas: HTMLCanvasElement, wordPool: Word[], audioSettings: AudioSettings, analyzerSettings: AnalyzerSettings) {

    this.audio = new AudioManager(audioSettings, analyzerSettings);
    this.recognition = new SoundRecognition(this.audio, wordPool);
    this.drawing = new DynamicSoundChart(canvas, this.audio, this.recognition);

    setTimeout(() => {
      this.recognition.start();
      this.drawing.showWord(null, this.recognition.currentWord);
    }, 2000);

    this.recognition.wordRecognized = (occurence: WordOccurence, nextWord: Word) => {
      this.drawing.showWord(occurence, nextWord);
      if (nextWord == null) {
        setTimeout(() => {
          this.stop();
          this.getResult(this.onRecordingEnd);
        }, 1000);
      }
    };
  }

  setChartColor(modification: Modification) {
    this.drawing.setChartColor(modification);
  }

  start(onRecordingEnd: (recorderResult: SoundRecorderResult) => void): void {
    this.onRecordingEnd = onRecordingEnd;
    this.drawing.start();
    this.audio.start();
  }

  stop() {
    this.drawing.stop();
    this.audio.stop();
  }

  private getResult(func: (recorderResult: SoundRecorderResult) => void) {

    const fileReader = new FileReader();

    fileReader.onload = e => {
      func({
        rawAudio: fileReader.result as string,
        FrequenciesChunks: this.audio.frequencyChunks,
        wordOccurrences: this.recognition.wordOccurrences
      });
    };

    const audioBlob = new Blob(this.audio.audioChunks, { type: 'audio/webm' });
    fileReader.readAsBinaryString(audioBlob);
  }
}

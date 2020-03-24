import { FrequenciesChunk } from '../services/api-client/FrequenciesChunk';
import { AudioSettings } from './AudioSettings';
import { AnalyzerSettings } from './AnalyzerSettings';

export class AudioManager {

  audioChunks: Blob[];
  frequencyChunks: FrequenciesChunk[];
  recordingStartTime: number;

  private audioSettings: AudioSettings;
  private analyzerSettings: AnalyzerSettings;

  private stream: MediaStream;
  private audioContext: AudioContext;
  private recorder: MediaRecorder;

  constructor(audioSettings: AudioSettings, analyzerSettings: AnalyzerSettings) {
    this.audioSettings = audioSettings;
    this.analyzerSettings = analyzerSettings;
    this.audioChunks = [];
    this.frequencyChunks = [];
  }

  start() {

    this.recordingStartTime = performance.now();

    navigator.mediaDevices.getUserMedia({
      video: false,
      audio: this.audioSettings,
    }).then(stream => {

        this.stream = stream;
        this.audioContext = new AudioContext();

        this.startAnalyzer(stream);
        this.startRecorder(stream);

      }).catch(error => {
        console.error(error);
      });
  }

  stop() {
    this.stream.getAudioTracks().forEach(track => {
      track.stop();
    });

    if (this.audioContext.state !== 'closed') {
      this.audioContext.close();
    }

    if (this.recorder.state !== 'inactive') {
      this.recorder.stop();
    }
  }

  private startAnalyzer(stream: MediaStream) {

    const input = this.audioContext.createMediaStreamSource(stream);
    const analyser = this.audioContext.createAnalyser();
    const scriptProcessor = this.audioContext.createScriptProcessor(this.analyzerSettings.bufferSize);

    analyser.smoothingTimeConstant = this.analyzerSettings.smoothingTimeConstant;
    analyser.fftSize = this.analyzerSettings.fftSize;
    analyser.minDecibels = this.analyzerSettings.minDecibels;

    input.connect(analyser);
    analyser.connect(scriptProcessor);
    scriptProcessor.connect(this.audioContext.destination);

    scriptProcessor.onaudioprocess = _ => {

      let buffer = new Uint8Array(this.analyzerSettings.fftSize);
      analyser.getByteFrequencyData(buffer);

      let lowFrequencyLimit = Math.floor(buffer.length / 6);
      let lowFrequencyBuffer = buffer.slice(0, lowFrequencyLimit);
      
      let average = this.getAverage(lowFrequencyBuffer);

      this.frequencyChunks.push({
        milliseconds: (performance.now() - this.recordingStartTime),
        buffer: average == 0 ? new Uint8Array(0) : lowFrequencyBuffer,
        average: average,
        size: lowFrequencyLimit
      });
    };
  }

  private getAverage(array: Uint8Array) {
    let sum = 0;

    for (const element of array) {
        sum += element;
    }

    return sum / array.length;
  }

  private startRecorder(stream: MediaStream) {
    this.recorder = new MediaRecorder(stream, {
      mimeType: this.audioSettings.mimeType,
    });

    this.recorder.ondataavailable = async event => {
      this.audioChunks.push(event.data);
    };

    this.recorder.start(25);
  }
}

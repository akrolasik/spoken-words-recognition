import { Component, OnInit, AfterViewInit } from '@angular/core';
import { EventManager } from '@angular/platform-browser';
import { NbDialogService } from '@nebular/theme';
import { ThanksComponent } from './thanks/thanks.component';
import { Word } from '../utilities/Word';
import { AnalyticsService } from '../services/analytics.service';
import { WORDS } from '../utilities/Words';
import { SoundAnimation } from '../utilities/SoundAnimation';
import { Guid } from 'guid-typescript';
import { ApiClientService } from '../services/api-client/ApiClientService';
import { RawRecording } from '../services/api-client/RawRecording';
import { Modification } from '../services/api-client/Modification';
import { SoundRecorder } from '../utilities/SoundRecorder';
import { SoundRecorderResult } from "../utilities/SoundRecorderResult";
import { AudioSettings } from '../utilities/AudioSettings';
import { AnalyzerSettings } from '../utilities/AnalyzerSettings';
import { CollectiongState } from '../utilities/CollectiongState';
import { LocalStorageService } from 'ngx-webstorage';
import { RecordingState } from '../utilities/RecordingState';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-data-collector',
  templateUrl: './data-collector.component.html',
  styleUrls: ['./data-collector.component.scss'],
})
export class DataCollectorComponent implements OnInit, AfterViewInit {

  private collectingState: CollectiongState;
  private recorder: SoundRecorder;
  private wordPool: Word[] = [];
  private canvas: HTMLCanvasElement;
  private animation: SoundAnimation;
  private recorderResult: SoundRecorderResult;
  private headerText: string;
  private localization: string;

  constructor(
    private http: HttpClient,
    private analytics: AnalyticsService,
    private storage: LocalStorageService,
    private apiClient: ApiClientService,
    private eventManager: EventManager,
    private dialogService: NbDialogService) {

    this.collectingState = new CollectiongState(storage);

    this.eventManager.addGlobalEventListener('window', 'resize', this.onResize.bind(this));

    this.http.get('http://ip-api.com/json').subscribe(json => {
      this.localization = json['country'];
    });

    WORDS.forEach(word => {
      this.wordPool.push({
        value: word.value,
        inWords: word.inWords,
        pronounce: word.pronounce,
      });
    });

    this.updateHeaderText();
  }

  ngOnInit(): void {
    this.canvas = document.getElementById('canvas') as HTMLCanvasElement;
    this.animation = new SoundAnimation(this.canvas);
    this.animation.start();
  }

  ngAfterViewInit(): void {
    this.onResize();
  }

  onResize() {
    const rect = this.canvas.getBoundingClientRect();
    this.canvas.width = rect.width;
    this.canvas.height = rect.height;
  }

  uploadCallback(accept: boolean) {
    if (accept) this.uploadRecodring();
    else this.updateCollectingState();
  }

  updateHeaderText() {
    this.headerText = "";
    if(this.collectingState.state === RecordingState.Recording) {
      this.headerText =  "Please read the following words";
    } else if (this.collectingState.state === RecordingState.Uploading) {
      this.headerText =  "Please upload the recording";
    } else {
      if(!this.collectingState.modifications.get(Modification.None) && this.collectingState.currentModification === null) {
        this.headerText =  "Hello, I'm looking for your voice!";
      } else if(this.collectingState.modifications.get(Modification.None) && this.collectingState.currentModification !== Modification.Noise) {
        this.headerText =  "Thank You!";
      } else if(this.collectingState.currentModification === Modification.Noise || this.collectingState.currentModification === Modification.None) {
        this.headerText =  "Get ready!";
      }
    }
  }

  uploadRecodring() {
    let rawRecording = this.getRawRecording();

    this.recorderResult.FrequenciesChunks.forEach(chunk => {
      chunk.data = String.fromCharCode(...chunk.buffer)
    });

    this.apiClient.putRawRecording(rawRecording);
    this.apiClient.putRawRecordingAudio(rawRecording.id, this.recorderResult.rawAudio);
    this.apiClient.putRawRecordingFrequencies(rawRecording.id, this.recorderResult.FrequenciesChunks);

    this.collectingState.modifications.set(this.collectingState.currentModification, true);
    this.storage.store(this.collectingState.currentModification, true);
    this.showThanksPopup();
    this.updateCollectingState();
  }

  getRawRecording(): RawRecording {
    return {
      id: Guid.create().toString(),
      speakerId: this.analytics.getUser(),
      accent: this.storage.retrieve('accent') as string,
      localization: this.localization,
      modification: this.collectingState.currentModification.toString(),
      words: this.recorderResult.wordOccurrences
    };
  }

  onRecordingButtonClick(button: string) {
    if(button == 'cancel') {
      this.stopRecording();
    } else if(button == 'start') {
      this.updateCollectingState(this.collectingState.state, Modification.None)
    } else if(button == 'start recording') {
      this.startRecording(this.collectingState.currentModification);
    }
  }

  showThanksPopup() {
    if (this.collectingState.currentModification === Modification.None) return;
    this.dialogService.open(ThanksComponent, {
      context: {
        modification: this.collectingState.currentModification,
      },
    });
  }

  startRecording(modification: Modification) {
    this.animation.stop();
    this.updateCollectingState(RecordingState.Recording, modification);
    this.blurActiveElement();

    this.recorder = new SoundRecorder(this.canvas, this.wordPool, new AudioSettings(), new AnalyzerSettings());
    this.recorder.setChartColor(modification);
    this.recorder.start((recorderResult) => {
      this.processRecorderResult(recorderResult);
    });
  }

  processRecorderResult(recorderResult: SoundRecorderResult) {
    this.recorderResult = recorderResult;
    this.animation.start();
    this.blurActiveElement();
    this.updateCollectingState(RecordingState.Uploading, this.collectingState.currentModification);
  }

  updateCollectingState(state: RecordingState = RecordingState.Ready, modification: Modification = null) {
    this.collectingState.state = state;
    this.collectingState.currentModification = modification;
    this.updateHeaderText();
  }

  stopRecording() {
    this.animation.start();
    this.recorder.stop(); 
    this.blurActiveElement();
    this.updateCollectingState();
  }

  blurActiveElement() {
    (document.activeElement as HTMLElement).blur();
  }
}

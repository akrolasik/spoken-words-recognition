import { Component, OnInit, AfterViewInit, HostListener } from '@angular/core';
import { ValidationState } from '../utilities/ValidationState';
import { MouseEventsHandler } from '../utilities/MouseEventsHandler';
import { StaticSoundChart } from '../utilities/StaticSoundChart';
import { RawDataLoader } from '../utilities/RawDataLoader';
import { EventManager } from '@angular/platform-browser';
import { ApiClientService } from '../services/api-client/ApiClientService';

@Component({
  selector: 'app-data-validator',
  templateUrl: './data-validator.component.html',
  styleUrls: ['./data-validator.component.scss']
})
export class DataValidatorComponent implements OnInit, AfterViewInit {

  state: ValidationState;
  mouseEventsHandler: MouseEventsHandler;
  drawing: StaticSoundChart;
  data: RawDataLoader;

  constructor(
    private apiClient: ApiClientService,
    private eventManager: EventManager) {

      this.drawing = new StaticSoundChart();
      this.state = new ValidationState();

      this.data = new RawDataLoader(apiClient, () => {
        this.prepareData();
      });

      this.mouseEventsHandler = new MouseEventsHandler((mouseDownPosition, mouseUpPosition) => {
        this.state.updateBorders(this.drawing, mouseDownPosition, mouseUpPosition);
      });
    }

  @HostListener('window:keydown', ['$event'])
  handleKeyDown(event: KeyboardEvent) {
    if (event.key === 'ArrowUp') { this.accept(); }
    if (event.key === 'ArrowDown') { this.decline(); }
    if (event.key === 'ArrowLeft') { this.move(-1); }
    if (event.key === 'ArrowRight') { this.move(1); }
  }

  ngOnInit() {
    const layer = document.getElementById('layer') as HTMLDivElement;
    const canvas = document.getElementById('canvas') as HTMLCanvasElement;

    this.mouseEventsHandler.setMouseEvents(layer, canvas);
    this.eventManager.addGlobalEventListener('window', 'resize', this.ngAfterViewInit.bind(this));
    this.drawing.initDrawing(canvas);

    setInterval(() => {
      if (this.state.rawRecording == null || this.data.dataLeftToValidate === 0) { return; }
      this.drawing.draw(this.state);
    }, 0);
  }

  ngAfterViewInit(): void {
    this.drawing.onResize();
  }

  removeFocus() {
    (document.activeElement as HTMLElement).blur();
  }

  accept() {
    this.state.wordOccurence.validated = true;
    if (!this.checkIfAllAreVerified()) {
      this.move(1);
    }
  }

  decline() {
    this.state.wordOccurence.validated = false;
    if (!this.checkIfAllAreVerified()) {
      this.move(1);
    }
  }

  checkIfAllAreVerified() {
    const allVerified = this.state.rawRecording.words.filter(x => x.validated != null).length === this.state.rawRecording.words.length;
    if (allVerified) {
      this.uploadProcessedData();
    }
    return allVerified;
  }

  async uploadProcessedData() {
    this.apiClient.putWords(this.state.rawRecording.id, this.state.rawRecording.words.filter(x => x.validated != null));
    this.next();
  }

  delete() {
    this.apiClient.deleteRawRecording(this.state.rawRecording.id);
    this.next();
  }

  next() {
    this.data.removeFirst();
    if (this.data.rawData.length > 0) {
      this.prepareData();
    }
  }

  move(index: number) {
    const newWord = this.state.getWord(index);
    this.setCurrentWord(newWord);
    this.removeFocus();
  }

  setCurrentWord(word: string) {
    if (this.state.rawRecording == null) { return; }
    this.state.setCurrentWord(word);
    this.removeFocus();
  }

  prepareData() {
    this.state.prepareData(this.data);
    this.setCurrentWord(this.state.rawRecording.words[0].word.value);
  }
}

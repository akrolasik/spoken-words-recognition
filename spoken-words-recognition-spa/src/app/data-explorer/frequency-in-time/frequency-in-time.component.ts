import { Component, OnInit, Input } from '@angular/core';
import { Recording } from 'src/app/services/api-client/Recording';
import { NbDialogRef } from '@nebular/theme';
import { DataService } from 'src/app/services/data.service';
import { FrequenciesChunk } from 'src/app/services/api-client/FrequenciesChunk';

@Component({
  selector: 'app-frequency-in-time',
  templateUrl: './frequency-in-time.component.html',
  styleUrls: ['./frequency-in-time.component.scss']
})
export class FrequencyInTimeComponent implements OnInit {

  @Input() recording: Recording;

  blur: boolean = true;
  canvas: HTMLCanvasElement;
  canvasContext: CanvasRenderingContext2D;
  
  constructor(
    private ref: NbDialogRef<FrequencyInTimeComponent>,
    private dataService: DataService) {}

  async ngOnInit() {
    this.canvas = document.getElementById('canvas') as HTMLCanvasElement;
    this.canvasContext = this.canvas.getContext('2d');

    let frequencies = await this.dataService.getRecordingFrequencies(this.recording.id);

    let w = Math.ceil(this.canvas.width / frequencies.length);

    for(let x = 0; x < frequencies.length; x++) {

      let limit = frequencies[x].data.length;
      let h = Math.ceil(this.canvas.height / limit);

      for(let y = 0; y < limit; y++) {
        let value = this.blur ? this.getValue(frequencies, x, y, 1) : frequencies[x].data.charCodeAt(y);
        this.canvasContext.fillStyle = this.getColorAccordingToByteValue(value);
        this.canvasContext.fillRect(x * w, this.canvas.height - y * h, w, h);
      }
    }
  }

  onBlurChange(event: any) {
    this.blur = event.target.checked;
    this.ngOnInit();
  }

  getValue(frequencies: FrequenciesChunk[], x0: number, y0: number, halfSize: number): number {
    let sum = 0.0;
    let weight = 0.0;

    for(let x = x0 - halfSize; x <= x0 + halfSize; x++) {
      for(let y = y0 - halfSize; y <= y0 + halfSize; y++) {
        if(x >= 0 && x < frequencies.length && y >= 0 && y < frequencies[x].data.length) {
          let w = 1.0 / (1.0 + Math.abs(x - x0)) / (1.0 + Math.abs(y - y0));
          sum += frequencies[x].data.charCodeAt(y) * w;
          weight += w;
        }
      }
    }

    return Math.floor(sum / weight);
  }

  getColorAccordingToByteValue(value: number): string {
    var hex = Number(value).toString(16);
    if (hex.length < 2) {
        hex = "0" + hex;
    }
    return `#${hex}0000`;
  }

  dismiss() {
    this.ref.close();
  }
  
}

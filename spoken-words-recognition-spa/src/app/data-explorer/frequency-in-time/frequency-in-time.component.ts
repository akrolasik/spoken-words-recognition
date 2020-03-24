import { Component, OnInit, Input } from '@angular/core';
import { Recording } from 'src/app/services/api-client/Recording';
import { NbDialogRef } from '@nebular/theme';
import { ApiClientService } from 'src/app/services/api-client/ApiClientService';

@Component({
  selector: 'app-frequency-in-time',
  templateUrl: './frequency-in-time.component.html',
  styleUrls: ['./frequency-in-time.component.scss']
})
export class FrequencyInTimeComponent implements OnInit {

  @Input() recording: Recording;

  canvas: HTMLCanvasElement;
  canvasContext: CanvasRenderingContext2D;
  
  constructor(
    private ref: NbDialogRef<FrequencyInTimeComponent>,
    private apiClientService: ApiClientService) {}

  async ngOnInit() {
    this.canvas = document.getElementById('canvas') as HTMLCanvasElement;
    this.canvasContext = this.canvas.getContext('2d');

    let frequencies = await this.apiClientService.getRecordingFrequencies(this.recording.id);

    let w = Math.ceil(this.canvas.width / frequencies.length);

      for(let x = 0; x < frequencies.length; x++) {

        let limit = frequencies[x].data.length;
        let h = Math.ceil(this.canvas.height / limit);

        for(let y = 0; y < limit; y++) {
          let value = frequencies[x].data.charCodeAt(y);
          this.canvasContext.fillStyle = this.getColorAccordingToByteValue(value);
          this.canvasContext.fillRect(x * w, this.canvas.height - y * h, w, h);
        }
      }
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

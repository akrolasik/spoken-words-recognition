import { Component, Input, OnInit } from '@angular/core';
import { Recording } from 'src/app/services/api-client/Recording';
import { DataService } from 'src/app/services/data.service';
import { NbDialogService } from '@nebular/theme';
import { FrequencyInTimeComponent } from '../frequency-in-time/frequency-in-time.component';

@Component({
  selector: 'app-item',
  templateUrl: './item.component.html',
  styleUrls: ['./item.component.scss']
})
export class ItemComponent implements OnInit {

  @Input() recording: Recording;

  audio: HTMLAudioElement;
  duration: string;

  constructor(private dataService: DataService, private dialogService: NbDialogService) {}

  ngOnInit() {}

  display() {
    this.dialogService.open(FrequencyInTimeComponent, {
      context: {
        recording: this.recording,
      },
    });
  }

  play() {
    if(this.audio == null){
      this.dataService.getData(this.recording.zipEntry).data.subscribe(blob => {
        let blobURL = window.URL.createObjectURL(blob);
        this.audio = new Audio(blobURL);
        this.audio.play();
      });
    } else {
      this.audio.play();
    }
  }
}

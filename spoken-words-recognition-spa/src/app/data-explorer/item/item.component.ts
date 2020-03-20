import { Component, Input, OnInit } from '@angular/core';
import { Recording } from 'src/app/services/api-client/Recording';
import { DataService } from 'src/app/services/data.service';

@Component({
  selector: 'app-item',
  templateUrl: './item.component.html',
  styleUrls: ['./item.component.scss']
})
export class ItemComponent implements OnInit {

  @Input() recording: Recording;

  audio: HTMLAudioElement;
  duration: string;

  constructor(private dataService: DataService) {}

  ngOnInit() {}

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

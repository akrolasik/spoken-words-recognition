import { Component } from '@angular/core';
import { Recording } from '../services/api-client/Recording';

@Component({
  selector: 'app-data-explorer',
  templateUrl: './data-explorer.component.html',
  styleUrls: ['./data-explorer.component.scss']
})
export class DataExplorerComponent {

  recordings: Recording[];
  filteredRecordings: Recording[];
  loadedRecordings: Recording[];
  visibleRecordings: Recording[];
  pageToLoadNext: number = 0;
  pageSize: number = 30;
  treshold: number = 300;

  words: string[];
  accents: string[];

  selectedWord: string = "All";
  selecteAccent: string = "All";

  constructor() {}

  onRecordingsLoaded(recordings: Recording[]) {
    this.recordings = recordings;
    this.words = this.unique(this.recordings.map(x => x.word));
    this.accents = this.unique(this.recordings.map(x => x.accent));
    this.getFiltered();
  }

  loadNext(pageToLoad: number) {
    if(pageToLoad < 0 || 
      pageToLoad > this.filteredRecordings.length / this.pageSize ||
      this.loadedRecordings.length >= this.treshold) return;

    let start = pageToLoad * this.pageSize;
    let end = Math.min((pageToLoad + 1) * this.pageSize, this.filteredRecordings.length);
    this.loadedRecordings.push(...this.filteredRecordings.slice(start, end));
    this.pageToLoadNext = pageToLoad;

    this.putGroups();
  }

  onScroll(event: any) {
    if (event.target.offsetHeight + event.target.scrollTop >= event.target.scrollHeight) {
      this.loadNext(this.pageToLoadNext + 1);
    }
  }

  putGroups() {

    this.visibleRecordings = [];
    let lastWord: string;

    this.loadedRecordings.forEach(recording => {
      if(lastWord == null || lastWord != recording.word) {
        let group = new Recording();
        group.word = recording.word;
        this.visibleRecordings.push(group);
      }
      this.visibleRecordings.push(recording);
      lastWord = recording.word;
    });

  }

  onWordSelected(word: string) {
    this.selectedWord = word;
    this.getFiltered();
  }

  onAccentSelected(accent: string) {
    this.selecteAccent = accent;
    this.getFiltered();
  }

  getFiltered() {
    this.filteredRecordings = this.recordings.filter(x => 
      (this.selectedWord == "All" || x.word == this.selectedWord) && 
      (this.selecteAccent == "All" || x.accent == this.selecteAccent));

    this.loadedRecordings = [];
    this.loadNext(0);
  }

  private unique(array: any[]): any[] {
    return array.filter((x, i, a) => a.indexOf(x) === i);
  }

}

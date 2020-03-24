import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { DataService, ZipEntry } from 'src/app/services/data.service';
import { Recording } from 'src/app/services/api-client/Recording';
import { Modification } from 'src/app/services/api-client/Modification';

@Component({
  selector: 'app-package',
  templateUrl: './package.component.html',
  styleUrls: ['./package.component.scss']
})
export class PckageComponent implements OnInit {

  files: ZipEntry[];
  isDownloadButtonDisabled = true;
  downloadButtonText = 'Package version undefined';

  @Output() onRecordingsLoaded = new EventEmitter<Recording[]>();

  constructor(private dataService: DataService) {
    this.IsPackageLatest();
  }

  async IsPackageLatest() {
    this.isDownloadButtonDisabled = !await this.dataService.IsNewPackageAvailable();
    if (this.isDownloadButtonDisabled) {
      this.downloadButtonText = 'Package up-to-date';
    } else {
      this.downloadButtonText = 'Download latest package';
    }
  }

  async DownloadPackage() {
    // set kręcioł
    await this.dataService.DownloadLatestPackage();
    this.IsPackageLatest();
  }

  fileChanged(event: any) {
    const file = event.target.files[0];
    this.dataService.getEntries(file).subscribe(result => {
      this.files = result as Array<ZipEntry>;
      let index = this.files.find(x => x.filename == "index.tsv")
      this.readIndex(index);
    })
  }

  readIndex(index: ZipEntry) {
    this.dataService.getData(index).data.subscribe(blob => {
      var reader = new FileReader();
      reader.onload = () => {
        let rows = reader.result.toString().split('\n');
        let recordings = rows.map(row => this.getRecording(row));
        this.onRecordingsLoaded.emit(recordings);
      }
      reader.readAsText(blob);
    })
  }

  getRecording(row: string) {
    let properties = row.split('\t');
    return {
      id: properties[0],
      speakerId: properties[1],
      word: properties[2],
      accent: properties[3],
      localization: properties[4],
      modification: properties[5] as Modification,
      chunksCount: Number.parseInt(properties[6]),
      zipEntry: this.files.find(x => x.filename == `${properties[0]}.mp3`),
    };
  }

  ngOnInit() {
  }

}

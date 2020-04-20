import { Component, Input } from '@angular/core';
import { DataService } from 'src/app/services/data.service';
import { browser } from 'protractor';

@Component({
  selector: 'app-package',
  templateUrl: './package.component.html',
  styleUrls: ['./package.component.scss']
})
export class PckageComponent {

  @Input() size: string = "medium";

  isDownloadButtonDisabled = true;
  fileName: string;
  downloadButtonText = 'Package version undefined';

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
    await this.dataService.DownloadLatestPackage();
    this.IsPackageLatest();
  }

  fileChanged(event: any) {
    const file = event.target.files[0];
    this.fileName = file.name;
    this.dataService.load(file);
  }
}

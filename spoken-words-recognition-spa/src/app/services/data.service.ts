import { Injectable } from '@angular/core';
import { LocalStorageService } from 'ngx-webstorage';
import { HttpClient, HttpResponse, HttpRequest, HttpHeaders } from '@angular/common/http';
import * as FileSaver from 'file-saver';
import { Observable, Subject } from 'rxjs';

declare const zip: any;

export interface ZipEntry {
  version: number;
  bitFlag: number;
  compressionMethod: number;
  lastModDateRaw: number;
  lastModDate: string;
  crc32: number;
  compressedSize: number;
  uncompressedSize: number;
  filenameLength: number;
  extraFieldLength: number;
  commentLength: number;
  directory: boolean;
  offset: 0;
  filename: string;
  comment: string;
}

export interface ZipTaskProgress {
  active: boolean;
  current?: number;
  total?: number;
}

export interface ZipTask {
  progress: Observable<ZipTaskProgress>;
  data: Observable<Blob>;
}

@Injectable({
  providedIn: 'root'
})
export class DataService {
  
  packageVersion = 'package-version';
  publicBlobContainerUrl = 'https://akrolasikstorage.blob.core.windows.net/public/';
  packageFileName = 'package.zip';
  indexFileName = 'index.tsv';
  packageUrl = this.publicBlobContainerUrl + this.packageFileName;

  constructor(private http: HttpClient, private storage: LocalStorageService) { 
    zip.workerScriptsPath = 'assets/zip/';
  }

  async IsNewPackageAvailable() {
    const serverVersion = await this.GetServerPackageLastUpdateTime();

    if (serverVersion == null) { return false; }

    const currentVersion = this.storage.retrieve(this.packageVersion) as number;
    return currentVersion == null || currentVersion < serverVersion;
  }

  public async GetServerPackageLastUpdateTime(): Promise<number> {

    const httpRequest = new HttpRequest('HEAD', this.packageUrl, { headers: new HttpHeaders({
      'Cache-Control': 'no-cache',
      Pragma: 'no-cache'
    })});
    const response = await this.http.request(httpRequest).toPromise() as HttpResponse<any>;

    if (response.headers != null) {
      const lastModified = response.headers.get('last-modified');
      return Date.parse(lastModified);
    }

    return null;
  }

  async DownloadLatestPackage() {
    const blob = await this.http.get(this.packageUrl, { responseType: 'blob', headers: new HttpHeaders({
      'Cache-Control': 'no-cache',
      Pragma: 'no-cache'
    })}).toPromise() as Blob;

    console.log(blob);
    FileSaver.saveAs(blob, 'package.zip');

    const serverVersion = await this.GetServerPackageLastUpdateTime();
    this.storage.store(this.packageVersion, serverVersion);
  }

  getEntries(file): Observable<Array<ZipEntry>> {
    return new Observable(subscriber => {
      const reader = new zip.BlobReader(file);
      zip.createReader(reader, zipReader => {
        zipReader.getEntries(entries => {
          subscriber.next(entries);
          subscriber.complete();
        });
      }, message => {
        subscriber.error({ message });
      });
    });
  }

  getData(entry: ZipEntry): ZipTask {
    const progress = new Subject<ZipTaskProgress>();
    const data = new Observable<Blob>(subscriber => {
      const writer = new zip.BlobWriter();

      (entry as any).getData(writer, blob => {
        subscriber.next(blob);
        subscriber.complete();
        progress.next(null);
      }, (current, total) => {
        progress.next({ active: true, current, total });
      });
    });
    return { progress, data };
  }

}

import { Injectable } from '@angular/core';
import { LocalStorageService } from 'ngx-webstorage';
import { HttpClient, HttpResponse, HttpRequest, HttpHeaders } from '@angular/common/http';
import * as FileSaver from 'file-saver';
import { Observable, Subject, BehaviorSubject } from 'rxjs';
import { Modification } from './api-client/Modification';
import { Recording } from './api-client/Recording';
import { ApiClientService } from './api-client/ApiClientService';
import { FrequenciesChunk } from './api-client/FrequenciesChunk';
import { ZipEntry } from './ZipEntry';

declare const zip: any;

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
  loadedFileName: string;

  files: ZipEntry[];

  public recordings = new BehaviorSubject<Recording[]>([]);

  constructor(
    private http: HttpClient, 
    private localStorage: LocalStorageService,
    private storage: LocalStorageService, 
    private apiClientService: ApiClientService) { 
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

    FileSaver.saveAs(blob, 'package.zip');

    const serverVersion = await this.GetServerPackageLastUpdateTime();
    this.storage.store(this.packageVersion, serverVersion);
  }

  async getRecordingFrequencies(id: string): Promise<FrequenciesChunk[]> {

    let key = `frequencies/${id}`;
    let frequencies = this.localStorage.retrieve(key);
    
    if(frequencies == null) {
      frequencies = await this.apiClientService.getRecordingFrequencies(id).toPromise();
      this.localStorage.store(key, frequencies);
    }

    return frequencies;
  }

  load(file: any) {
    this.loadedFileName = file.name;
    console.log(this.loadedFileName);
    this.getEntries(file).subscribe(result => {
      this.files = result as Array<ZipEntry>;
      let index = this.files.find(x => x.filename == "index.tsv")
      this.readIndex(index);
    })
  }

  readIndex(index: ZipEntry) {
    this.getData(index).data.subscribe(blob => {
      var reader = new FileReader();
      reader.onload = () => {
        let rows = reader.result.toString().split('\n');
        let recordings = rows.map(row => this.getRecording(row));
        this.recordings.next(recordings);
        this.storage.store('recordings', recordings);
      }
      reader.readAsText(blob);
    })
  }

  isLoaded() {
    return this.recordings.value != null && this.recordings.value.length > 0;
  }

  getIndex(): Recording[] {

    if(this.recordings.value == null || this.recordings.value.length == 0) {
      let recordings = this.storage.retrieve('recordings') as Recording[];

      if(recordings == null) 
        throw "Package was newer loaded";

      this.recordings.next(recordings);
    }
    return this.recordings.value;
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

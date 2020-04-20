import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Image } from './Image';
import { ReportedIssue } from './ReportedIssue';
import { RawRecording } from './RawRecording';
import { SessionStorageService } from 'ngx-webstorage';
import { Observable } from 'rxjs';
import { WordOccurence } from './WordOccurence';
import { FrequenciesChunk } from './FrequenciesChunk';

@Injectable({
  providedIn: 'root',
})
export class ApiClientService {

  baseUrl = 'https://localhost:5001';//'https://spoken-words-recognition-api.azurewebsites.net';

  token: string;
  headers: HttpHeaders = new HttpHeaders({
    'Content-Type': 'application/json',
  });

  constructor(
    private http: HttpClient, 
    private sessionStorage: SessionStorageService) { }

  public putReportedIssue(reportedIssue: ReportedIssue) {
    const url = `${this.baseUrl}/analytics/issue`;
    this.http.put(url, reportedIssue, this.getOptions()).toPromise();
  }

  public putReportedIssueImage(issueId: string, image: Image) {
    const url = `${this.baseUrl}/analytics/issue/${issueId}/image`;
    this.http.put(url, image, this.getOptions()).toPromise();
  }

  public getReportedIssues(): Observable<ReportedIssue[]> {
    const url = `${this.baseUrl}/analytics/issues`;
    return this.http.get<ReportedIssue[]>(url, this.getOptions());
  }

  public getReportedIssueImage(issueId: string): Observable<Image> {
    const url = `${this.baseUrl}/analytics/issue/${issueId}/image`;
    return this.http.get<Image>(url, this.getOptions());
  }

  public deleteReportedIssue(issueId: string) {
    const url = `${this.baseUrl}/analytics/issue/${issueId}`;
    this.http.delete(url, this.getOptions()).toPromise();
  }

  public putRawRecording(rawRecording: RawRecording) {
    const url = `${this.baseUrl}/data/raw`;
    this.http.put(url, rawRecording, this.getOptions()).toPromise();
  }

  public putRawRecordingAudio(rawRecordingId: string, data: string) {
    const url = `${this.baseUrl}/data/raw/${rawRecordingId}/audio`;
    this.http.put(url, data).toPromise();
  }

  public async putRawRecordingFrequencies(rawRecordingId: string, chunks: FrequenciesChunk[]) {
    let url = `${this.baseUrl}/data/raw/${rawRecordingId}/frequencies`;

    while(chunks.length > 0) {

      let end = 100;

      if(end >= chunks.length) {
        end = chunks.length;
        url += "?end=true";
      }

      let part = chunks.slice(0, end);
      await this.http.put(url, part, this.getOptions()).toPromise();

      chunks = chunks.slice(end, chunks.length);
    }
  }

  public async getRawRecordingAudio(rawRecordingId: string): Promise<HTMLAudioElement> {
    const url = `${this.baseUrl}/data/raw/${rawRecordingId}/audio`;

    const options = this.getOptions('arraybuffer');

    const response = await this.http.get(url, options).toPromise();
    const blob = new Blob([response as any], { type: 'audio/webm' });
    const audioUrl = URL.createObjectURL(blob);
    return new Audio(audioUrl);
  }

  public getRawRecordingFrequencies(rawRecordingId: string): Observable<FrequenciesChunk[]> {
    const url = `${this.baseUrl}/data/raw/${rawRecordingId}/frequencies`;
    return this.http.get<FrequenciesChunk[]>(url, this.getOptions());
  }

  public getRecordingFrequencies(recordingId: string): Observable<FrequenciesChunk[]> {
    const url = `${this.baseUrl}/data/${recordingId}/frequencies`;
    return this.http.get<FrequenciesChunk[]>(url, this.getOptions());
  }

  public deleteRawRecording(rawRecordingId: string) {
    const url = `${this.baseUrl}/data/raw/${rawRecordingId}`;
    this.http.delete(url, ).toPromise();
  }

  public getRawRecordings(): Observable<RawRecording[]> {
    const url = `${this.baseUrl}/data/raws`;
    return this.http.get<RawRecording[]>(url, this.getOptions());
  }

  public putWords(rawRecordingId: string, words: WordOccurence[]) {
    const url = `${this.baseUrl}/data/raw/${rawRecordingId}/words`;
    return this.http.put(url, words, this.getOptions()).toPromise();
  }

  private getOptions(responseType: any = null) {

    if (this.token == null) {
        this.token = this.sessionStorage.retrieve('id_token');
    }

    if (this.token != null) {
        this.headers = new HttpHeaders({
          'Content-Type': 'application/json',
          Authorization: 'Bearer ' + this.token,
        });
    }

    return {
      headers: this.headers,
      responseType
    };
  }
}

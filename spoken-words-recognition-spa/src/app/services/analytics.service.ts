import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { LocalStorageService } from 'ngx-webstorage';
import { Guid } from 'guid-typescript';

const eventUrl = 'http://localhost:5000/api/event';
const mostSearchedTagsUrl = eventUrl + '/mostSearchedTags';
const STORAGE_KEY = 'user';

export class AnalyticsEvent {
  User: string;
  Session: string;
  Timestamp: string;
  Key: string;
  Value: string;
}

@Injectable({
  providedIn: 'root',
})
export class AnalyticsService {

  user: string;

  constructor(private http: HttpClient, private storage: LocalStorageService) {}

  getUser() {
    if (this.user == null) {
      this.user = this.storage.retrieve(STORAGE_KEY);
      if (this.user == null) this.user = Guid.create().toString();
      this.storage.store(STORAGE_KEY, this.user);
    }
    return this.user;
  }

  getValue<T>(key: string) {
    return this.storage.retrieve(key) as T;
  }

  setValue(key: string, value: any) {
    return this.storage.store(key, value);
  }

  mostSearchedTags(scope: string) {

    let url = mostSearchedTagsUrl;
    if (scope !== 'all') url += '/' + this.getUser();

    return this.http.get(url);
  }

  sendEvent(key: string, value: any) {

    // const event: AnalyticsEvent = {
    //   User: this.getUser(),
    //   Session: this.getSession(),
    //   Timestamp: new Date().toISOString(),
    //   Key: key,
    //   Value: value,
    // };

    // this.http.post(eventUrl, event, {
    //   headers: new HttpHeaders({
    //     'Content-Type': 'application/json',
    //   }),
    // }).subscribe(response => {
    //   // nothing
    // }, error => {
    //   console.error(error);
    // });
  }

  // map
  public sendEventCountry(country: string) {
    this.sendEvent('country', country);
  }

  // bar
  public sendEventLanguage(language: string) {
    this.sendEvent('language', language);
  }

  // bar
  public sendEventTheme(theme: string) {
    this.sendEvent('theme', theme);
  }

  // bar
  public sendEventDevice(device: string) {
    this.sendEvent('device', device);
  }

  // bar
  public sendEventSystem(system: string) {
    this.sendEvent('system', system);
  }

  // bubble
  public sendEventResolution(resolution: string) {
    this.sendEvent('resolution', resolution);
  }

  // ???
  public sendEventRoute(route: string) {
    this.sendEvent('route', route);
  }

  // counters
  public sendEventLink(link: string) {
    this.sendEvent('link', link);
  }

  // circular
  public sendEventTag(tag: string) {
    this.sendEvent('tag', tag);
  }

  // ???
  public sendEventIdea(idea: string, agree: boolean) {
    this.sendEvent(agree ? 'agree' : 'disagree', idea);
  }

  // time on site - bar
  // entry in time - line
}

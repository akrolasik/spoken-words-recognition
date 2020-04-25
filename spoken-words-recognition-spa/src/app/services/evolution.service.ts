import { Injectable } from '@angular/core';
import { HttpHeaders, HttpClient } from '@angular/common/http';
import { EvolutionConfig, EvolutionStatistics } from '../neural-network-tool/evolution-tile/evolution-tile.component';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class EvolutionService {

  baseUrl = 'https://localhost:44381/evolution';

  token: string;
  headers: HttpHeaders = new HttpHeaders({
    'Content-Type': 'application/json',
  });

  constructor(private http: HttpClient) { }

  public addEvolution(evolution: EvolutionConfig) {
    return this.http.put(this.baseUrl, evolution, { headers: new HttpHeaders({
      'Content-Type': 'application/json',
    })}).toPromise();
  }

  public getEvolutions(): Observable<EvolutionConfig[]> {
    return this.http.get<EvolutionConfig[]>(this.baseUrl, { headers: new HttpHeaders({
      'Content-Type': 'application/json',
    })});
  }

  public getStatistics(id: string): Observable<EvolutionStatistics[]> {
    return this.http.get<EvolutionStatistics[]>(`${this.baseUrl}/statistics/${id}`, { headers: new HttpHeaders({
      'Content-Type': 'application/json',
    })});
  }

  public deleteEvolution(id: string): Promise<any> {
    return this.http.delete(`${this.baseUrl}/delete/${id}`, { headers: new HttpHeaders({
      'Content-Type': 'application/json',
    })}).toPromise();
  }

  public startEvolution(id: string): Promise<any> {
    return this.http.post(`${this.baseUrl}/start/${id}`, { headers: new HttpHeaders({
      'Content-Type': 'application/json',
    })}).toPromise();
  }

  public stopEvolution(): Promise<any> {
    return this.http.delete(`${this.baseUrl}/stop`, { headers: new HttpHeaders({
      'Content-Type': 'application/json',
    })}).toPromise();
  }
}

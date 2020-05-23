import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { RandomNamesService } from 'src/app/services/random-names.service';
import { Guid } from 'guid-typescript';
import { DataService } from 'src/app/services/data.service';
import { Recording } from 'src/app/services/api-client/Recording';
import { EvolutionConfig, EvolutionState } from '../evolution-tile/evolution-tile.component';
import { EvolutionService } from 'src/app/services/evolution.service';

@Component({
  selector: 'app-add-new-evolution',
  templateUrl: './add-new-evolution.component.html',
  styleUrls: ['./add-new-evolution.component.scss']
})
export class AddNewEvolutionComponent implements OnInit {
  
  words: string[];
  accents: string[];
  modifications: string[];

  evolution: EvolutionConfig;
  wordsIncluded: any[] = [];
  accentsIncluded: any[] = [];
  modificationsIncluded: any[] = [];
  recordings: Recording[];

  constructor(
    private evolutionService: EvolutionService, 
    private randomNamesService: RandomNamesService, 
    private dataService: DataService) {
    this.dataService.recordings.asObservable().subscribe(recordings => {
      this.onRecordingsLoaded(recordings);
    });
  }

  onRecordingsLoaded(recordings: Recording[]) {
    this.recordings = recordings;
    this.words = this.unique(this.recordings.map(x => x.word));
    this.accents = this.unique(this.recordings.map(x => x.accent));
    this.modifications = this.unique(this.recordings.map(x => x.modification));
    this.initData();
  }

  private unique(array: any[]): any[] {
    return array.filter((x, i, a) => a.indexOf(x) === i);
  }

  ngOnInit(): void {
    this.init();
  }

  init() {
    this.evolution = {
      id: Guid.create().toString(),
      name: this.randomNamesService.getRandomName(),
      state: EvolutionState.Idle,
      networkConfig: {
        inputResolution: { width: 50, height: 170 },
        hiddenLayersNeuronCount: [400, 100],
        outputCount: 25
      },
      inputConfig: {
        packageFileName: null,
        wordsIncluded: [],
        accentsIncluded: [],
        modificationsIncluded: []
      }
    };

    this.initData();
  }

  initData() {
    this.wordsIncluded = [];
    this.words.forEach(word => {
      this.wordsIncluded.push({
        word, 
        selected: true
      })
    });

    this.accentsIncluded = [];
    this.accents.forEach(accent => {
      this.accentsIncluded.push({
        accent, 
        selected: true
      })
    });

    this.modificationsIncluded = [];
    this.modifications.forEach(modification => {
      this.modificationsIncluded.push({
        modification, 
        selected: true
      })
    });
  }

  onNumberOfHiddenLayersChange(event: any) {
    let newNumber = event.target.valueAsNumber;
    this.resize(this.evolution.networkConfig.hiddenLayersNeuronCount, newNumber, null);
  }

  onNumberOfNeuronsInHiddenLayersChange(index: number, event: any) {
    let newNumber = event.target.valueAsNumber;
    this.evolution.networkConfig.hiddenLayersNeuronCount[index] = newNumber;
  }

  trackArray(index, item) {
    return index;
  }

  display() {
    let nums = this.evolution.networkConfig.hiddenLayersNeuronCount.map(x => x == null ? '?' : x.toString());
    return nums;
  }

  async addNewEvolution() {
    this.evolution.inputConfig.wordsIncluded = this.wordsIncluded.filter(x => x.selected).map(x => x.word);
    this.evolution.inputConfig.accentsIncluded = this.accentsIncluded.filter(x => x.selected).map(x => x.accent);
    this.evolution.inputConfig.modificationsIncluded = this.modificationsIncluded.filter(x => x.selected).map(x => x.modification);
    this.evolution.inputConfig.packageFileName = this.dataService.loadedFileName;

    this.evolution.networkConfig.outputCount = this.evolution.inputConfig.wordsIncluded.length;

    await this.evolutionService.addEvolution(this.evolution);

    this.init();
  }

  selectedWordsCount() {
    let selected = this.wordsIncluded.filter(x => x.selected).length;
    return selected == this.wordsIncluded.length ? 'all' : selected;
  }

  selectedAccentsCount() {
    let selected = this.accentsIncluded.filter(x => x.selected).length;
    return selected == this.accentsIncluded.length ? 'all' : selected;
  }

  selectedModificationsCount() {
    let selected = this.modificationsIncluded.filter(x => x.selected).length;
    return selected == this.modificationsIncluded.length ? 'all' : selected;
  } 

  // retrieve() {
  //   this.numberOfHiddenLayers = this.storage.retrieve("numberOfHiddenLayers") ?? 3;
  //   this.numberOfNeuronsInHiddenLayers = this.storage.retrieve("numberOfNeuronsInHiddenLayers") ?? [100,100,100];
  // }

  // store() {
  //   this.storage.store("numberOfHiddenLayers", this.numberOfHiddenLayers);
  //   this.storage.store("numberOfNeuronsInHiddenLayers", this.numberOfNeuronsInHiddenLayers);
  // }

  blurActiveElement() {
    (document.activeElement as HTMLElement).blur();
  }

  resize(arr: any[], size: number, defaultValue: any) {
    while (arr.length > size) { arr.pop(); }
    while (arr.length < size) { arr.push(defaultValue); }
  }
}

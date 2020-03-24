import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { RandomNamesService } from 'src/app/services/random-names.service';
import { Guid } from 'guid-typescript';
import { Evolution } from '../../utilities/Evolution';

@Component({
  selector: 'app-add-new-evolution',
  templateUrl: './add-new-evolution.component.html',
  styleUrls: ['./add-new-evolution.component.scss']
})
export class AddNewEvolutionComponent implements OnInit {
  
  @Input() words: string[];
  @Input() accents: string[];
  @Input() modifications: string[];
  @Output() newEvolution: EventEmitter<Evolution> = new EventEmitter();

  evolution: Evolution;
  wordsIncluded: any[] = [];
  accentsIncluded: any[] = [];
  modificationsIncluded: any[] = [];

  constructor(private randomNamesService: RandomNamesService) { }

  ngOnInit(): void {
    this.init();
  }

  init() {
    this.evolution = {
      id: Guid.create(),
      name: this.randomNamesService.getRandomName(),
      numberOfHiddenLayers: 3,
      numberOfNeuronsInHiddenLayers: [100, 100, 100],
      wordsIncluded: [],
      accentsIncluded: [],
      modificationsIncluded: [],
      populationCount: 100,
      survivalFactor: 10,
      mutationFactor: 10,
      mutationForce: 10,
      isProcessing: false,
      totalComputingTimeInSeconds: 0
    };

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
    this.evolution.numberOfHiddenLayers = newNumber;
    this.resize(this.evolution.numberOfNeuronsInHiddenLayers, newNumber, null);
  }

  onNumberOfNeuronsInHiddenLayersChange(index: number, event: any) {
    let newNumber = event.target.valueAsNumber;
    this.evolution.numberOfNeuronsInHiddenLayers[index] = newNumber;
  }

  trackArray(index, item) {
    return index;
  }

  display() {
    let nums = this.evolution.numberOfNeuronsInHiddenLayers.map(x => x == null ? '?' : x.toString());
    return nums;
  }

  addNewEvolution() {
    this.evolution.wordsIncluded = this.wordsIncluded.filter(x => x.selected).map(x => x.word);
    this.evolution.accentsIncluded = this.accentsIncluded.filter(x => x.selected).map(x => x.accent);
    this.evolution.modificationsIncluded = this.modificationsIncluded.filter(x => x.selected).map(x => x.modification);
    this.newEvolution.emit(this.evolution);
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

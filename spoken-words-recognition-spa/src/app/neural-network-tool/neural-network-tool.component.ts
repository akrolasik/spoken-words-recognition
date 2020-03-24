import { Component, OnInit } from '@angular/core';
import { Evolution } from "../utilities/Evolution";
import { LocalStorageService } from 'ngx-webstorage';

@Component({
  selector: 'app-neural-network-tool',
  templateUrl: './neural-network-tool.component.html',
  styleUrls: ['./neural-network-tool.component.scss']
})
export class NeuralNetworkToolComponent implements OnInit {

  storageKey = "neural-network-training";

  words = [
    "one",
    "two",
    "three"
  ];

  accents = [
    "one",
    "two",
    "three"
  ];

  modifications = [
    "one",
    "two",
    "three"
  ];

  evolutions: Evolution[] = [];

  constructor(private storage: LocalStorageService) { }

  ngOnInit() {
    this.evolutions = this.storage.retrieve(this.storageKey) ?? [];
  }

  onNewEvolution(evolution: Evolution) {
    this.evolutions.push(evolution);
    this.store();
  }

  onIsProcessingChange(isProcessing: boolean) {
    console.log(isProcessing);
    this.store();
  }

  delete(evolution: Evolution) {
    this.evolutions = this.evolutions.filter(x => x != evolution);
    this.store();
  }

  store() {
    this.storage.store(this.storageKey, this.evolutions);
  }

}

import { FrequenciesChunk } from './FrequenciesChunk';

export class Frequencies {
  frequenciesChunks: FrequenciesChunk[] = [];

  // REDUCE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!11
  public static getAverage(values: string): number {

    if(values.length == 0) return 0;

    let sum = 0;
    for (let i = 0; i < values.length; i++) {
      sum += values.charCodeAt(i);
    }
    return sum / values.length;
  }
}

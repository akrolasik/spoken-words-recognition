import { Guid } from 'guid-typescript';

export class Evolution {
  public id: Guid;
  public name: string;
  public numberOfHiddenLayers: number;
  public numberOfNeuronsInHiddenLayers: number[];
  public wordsIncluded: string[];
  public accentsIncluded: string[];
  public modificationsIncluded: string[];

  public populationCount: number;
  public survivalFactor: number;
  public mutationFactor: number;
  public mutationForce: number;

  public totalComputingTimeInSeconds: number;
  public isProcessing: boolean;
}

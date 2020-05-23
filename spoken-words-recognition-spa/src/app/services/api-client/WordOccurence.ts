import { Word } from '../../utilities/Word';

export class WordOccurence {
  milliseconds: number;
  word: Word;
  start?: number;
  end?: number;
  validated?: boolean;
  notGoodForTraining: boolean;
}

import { WordOccurence } from './WordOccurence';

export class RawRecording {

  id: string;
  speakerId: string;
  accent: string;
  localization: string;
  modification: string;
  words: WordOccurence[];

  public static prepare(raw: RawRecording) {
    raw.words.sort((x, y) => x.word > y.word ? 1 : -1);
  }
}

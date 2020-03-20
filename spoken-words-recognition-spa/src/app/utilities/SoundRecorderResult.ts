import { FrequenciesChunk } from '../services/api-client/FrequenciesChunk';
import { WordOccurence } from '../services/api-client/WordOccurence';

export class SoundRecorderResult {
  rawAudio: string;
  FrequenciesChunks: FrequenciesChunk[];
  wordOccurrences: WordOccurence[];
}

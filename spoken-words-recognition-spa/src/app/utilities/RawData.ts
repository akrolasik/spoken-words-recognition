import { RawRecording } from '../services/api-client/RawRecording';
import { Frequencies } from '../services/api-client/Frequencies';

export class RawData {
  rawRecording: RawRecording;
  frequencies?: Frequencies;
  audio?: HTMLAudioElement;
}

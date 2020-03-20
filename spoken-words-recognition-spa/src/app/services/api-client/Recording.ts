import { Modification } from './Modification';
import { ZipEntry } from '../data.service';

export class Recording {
  id: string;
  speakerId: string;
  accent: string;
  localization: string;
  modification: Modification;
  word: string;
  zipEntry: ZipEntry;
}

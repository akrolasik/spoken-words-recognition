import { Modification } from '../services/api-client/Modification';
import { LocalStorageService } from 'ngx-webstorage';
import { RecordingState } from './RecordingState';

export class CollectiongState {

  public state: RecordingState = RecordingState.Ready;
  public modifications: Map<Modification, boolean>;
  public currentModification: Modification = null;

  constructor(private storage: LocalStorageService) {
    this.modifications = new Map<Modification, boolean>();
    Object.keys(Modification).forEach(key => {
      const value = this.storage.retrieve(key) as boolean;
      this.modifications.set(key as Modification, value);
    });
  }
}

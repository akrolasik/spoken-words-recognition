import { TestBed } from '@angular/core/testing';

import { RandomNamesService } from './random-names.service';

describe('RandomNamesService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: RandomNamesService = TestBed.get(RandomNamesService);
    expect(service).toBeTruthy();
  });
});

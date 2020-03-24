import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FrequencyInTimeComponent } from './frequency-in-time.component';

describe('FrequencyInTimeComponent', () => {
  let component: FrequencyInTimeComponent;
  let fixture: ComponentFixture<FrequencyInTimeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FrequencyInTimeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FrequencyInTimeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

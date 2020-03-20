import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RecordingInfoComponent } from './recording-info.component';

describe('RecordingInfoComponent', () => {
  let component: RecordingInfoComponent;
  let fixture: ComponentFixture<RecordingInfoComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RecordingInfoComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RecordingInfoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

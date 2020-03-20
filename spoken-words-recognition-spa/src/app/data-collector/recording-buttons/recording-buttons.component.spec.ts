import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RecordingButtonsComponent } from './recording-buttons.component';

describe('RecordingButtonsComponent', () => {
  let component: RecordingButtonsComponent;
  let fixture: ComponentFixture<RecordingButtonsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RecordingButtonsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RecordingButtonsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DataValidatorComponent } from './data-validator.component';

describe('DataValidatorComponent', () => {
  let component: DataValidatorComponent;
  let fixture: ComponentFixture<DataValidatorComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DataValidatorComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DataValidatorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

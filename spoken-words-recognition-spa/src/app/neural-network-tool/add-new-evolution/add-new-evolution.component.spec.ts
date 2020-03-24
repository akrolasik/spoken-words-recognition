import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AddNewEvolutionComponent } from './add-new-evolution.component';

describe('NetworkSettingsComponent', () => {
  let component: AddNewEvolutionComponent;
  let fixture: ComponentFixture<AddNewEvolutionComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AddNewEvolutionComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AddNewEvolutionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PckageComponent } from './package.component';

describe('PathComponent', () => {
  let component: PckageComponent;
  let fixture: ComponentFixture<PckageComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PckageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PckageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

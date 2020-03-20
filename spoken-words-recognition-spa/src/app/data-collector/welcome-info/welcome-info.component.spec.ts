import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { WelcomeInfoComponent } from './welcome-info.component';

describe('WelcomeInfoComponent', () => {
  let component: WelcomeInfoComponent;
  let fixture: ComponentFixture<WelcomeInfoComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ WelcomeInfoComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(WelcomeInfoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

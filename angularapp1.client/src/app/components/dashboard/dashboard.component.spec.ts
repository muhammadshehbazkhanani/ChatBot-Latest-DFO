import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DashboardComponent } from './dashboard.component';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { ConfigComponent } from '../config/config.component';
import { ChatComponent } from '../../chat/chat.component';
import { SidebarComponent } from '../sidebar/sidebar.component';

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
        MatSnackBarModule,
        DashboardComponent,
        ChatComponent,
        ConfigComponent,
        SidebarComponent
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have a default selected value of 0', () => {
    expect(component.selected).toBe(0);
  });

  it('should change the selected value when setSelected is called', () => {
    component.setSelected(1);
    expect(component.selected).toBe(1);
    component.setSelected(2);
    expect(component.selected).toBe(2);
  });
});

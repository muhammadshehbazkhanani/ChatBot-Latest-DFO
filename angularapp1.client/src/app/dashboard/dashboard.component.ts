import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, OnDestroy } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { ConfigComponent } from '../config/config.component';
import { ChatComponent } from '../chat/chat.component';
import { SidebarComponent } from '../sidebar/sidebar.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [FormsModule, CommonModule, ReactiveFormsModule, MatSnackBarModule,  ChatComponent, ConfigComponent, SidebarComponent],
  animations: [],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent  {

  selected = 0;

  setSelected(index: number): void {
    this.selected = index;
  }
}

import { CommonModule, NgOptimizedImage } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [FormsModule, CommonModule, ReactiveFormsModule, MatSnackBarModule, MatIconModule, NgOptimizedImage],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css'
})
export class SidebarComponent {
  @Input() selected: number = 0;
  @Output() select = new EventEmitter<number>();
  
  icons = ['settings', 'chat'];

  constructor(private router: Router, private snackBar: MatSnackBar) { }

  handleSelection(index: number) {
    this.select.emit(index); 
  }

  logout() {
    localStorage.removeItem('auth_token');
    this.snackBar.open('Logged out successfully', 'Close', {
      duration: 3000,
      panelClass: ['success-snackbar'],
      horizontalPosition: 'center',
      verticalPosition: 'top',
    });
    this.router.navigate(['/login']);
  }

}

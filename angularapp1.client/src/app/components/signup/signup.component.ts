import { Component } from '@angular/core';
import {
  trigger,
  transition,
  style,
  animate,
  query,
  stagger,
} from '@angular/animations';
import { FormsModule, NgForm, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-Signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.css'],
  standalone: true,
  imports: [FormsModule, CommonModule, HttpClientModule, ReactiveFormsModule, MatSnackBarModule, NgOptimizedImage],
  providers: [AuthService],
  animations: [
    trigger('staggeredFadeIn', [
      transition(':enter', [
        query('.fade-item', [
          style({ opacity: 0, transform: 'translateY(10px)' }),
          stagger(50, [
            animate(
              '400ms ease-out',
              style({ opacity: 1, transform: 'translateY(0)' })
            ),
          ]),
        ]),
      ]),
    ]),
  ],
})
export class SignupComponent {
  constructor(
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  SignupObj = {
    Email: '',
    Password: '',
  };

  onSignup() {
    if (!this.SignupObj.Email || !this.SignupObj.Password) {
      this.snackBar.open('Please fill all the fields', 'Close', {
        duration: 3000,
        panelClass: ['error-snackbar'],
        horizontalPosition: 'center',  
              verticalPosition: 'top'
      });
      return;
    }

    this.authService
      .register(this.SignupObj.Email, this.SignupObj.Password)
      .subscribe(
        (response: any) => {
          if (response.result) {
            this.snackBar.open('User created successfully!', 'Close', {
              duration: 3000,
              panelClass: ['success-snackbar'],
              horizontalPosition: 'center',  
              verticalPosition: 'top'
            });
            this.router.navigate(['/login']);
          } else {
            this.snackBar.open('Signup failed. Try again.', 'Close', {
              duration: 3000,
              panelClass: ['error-snackbar'],
              horizontalPosition: 'center',  
              verticalPosition: 'top'
            });
          }
        },
        (error) => {
          console.error('Signup error:', error);
        
          const errorMessage = error?.error?.message || 'An error occurred during Signup.';
          this.snackBar.open(errorMessage, 'Close', {
            duration: 3000,
            panelClass: ['error-snackbar'],
            horizontalPosition: 'center',  
              verticalPosition: 'top'
          });
        }        
      );
  }
}

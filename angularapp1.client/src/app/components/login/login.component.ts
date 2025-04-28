import { Component } from '@angular/core';
import {
  trigger,
  transition,
  style,
  animate,
  query,
  stagger,
} from '@angular/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  standalone: true,
  imports: [FormsModule, CommonModule, ReactiveFormsModule, MatSnackBarModule],
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
export class LoginComponent {
  constructor(
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) { }

  loginObj = {
    Email: '',
    Password: '',
  };

  onLogin() {

    if (!this.loginObj.Email || !this.loginObj.Password) {
      this.snackBar.open('Please fill in all fields.', 'Close', {
        duration: 3000,
        panelClass: ['error-snackbar'],
        horizontalPosition: 'center',
        verticalPosition: 'top',
      });
      return;
    }

    this.authService.login(this.loginObj.Email, this.loginObj.Password).subscribe(
      (response: any) => {
        if (response && response.Token) {
          localStorage.setItem('auth_token', response.Token);
          this.snackBar.open('Login Successful', 'Close', {
            duration: 3000,
            panelClass: ['success-snackbar'],
            horizontalPosition: 'center',
            verticalPosition: 'top',
          });
          this.router.navigate(['/dashboard/config']);
        } else {
          this.snackBar.open(response?.message || 'Login failed!', 'Close', {
            duration: 3000,
            panelClass: ['error-snackbar'],
            horizontalPosition: 'center',
            verticalPosition: 'top',
          });
        }
      },
      (error) => {
        console.error('Login error:', error);
        const message = error?.error?.message || 'An error occurred during login.';

        if (error.status === 400) {
          this.snackBar.open(message, 'Close', {
            duration: 3000,
            panelClass: ['error-snackbar'],
            horizontalPosition: 'center',
            verticalPosition: 'top',
          });
        } else if (error.status === 500) {
          this.snackBar.open('An unexpected error occurred. Please try again later.', 'Close', {
            duration: 3000,
            panelClass: ['error-snackbar'],
            horizontalPosition: 'center',
            verticalPosition: 'top',
          });
        } else {
          this.snackBar.open('An error occurred during login.', 'Close', {
            duration: 3000,
            panelClass: ['error-snackbar'],
            horizontalPosition: 'center',
            verticalPosition: 'top',
          });
        }
      }
    );
  }
}

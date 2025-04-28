import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { LoginComponent } from './login.component';
import { AuthService } from '../../services/auth.service';
import { of, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let routerSpy: jasmine.SpyObj<Router>;
  let snackBarSpy: jasmine.SpyObj<MatSnackBar>;

  beforeEach(async () => {
    // create our spies
    authServiceSpy = jasmine.createSpyObj('AuthService', ['login']);
    routerSpy = jasmine.createSpyObj('Router', ['navigate']);
    snackBarSpy = jasmine.createSpyObj('MatSnackBar', ['open']);

    await TestBed.configureTestingModule({
      imports: [
        NoopAnimationsModule,
        LoginComponent
      ],
      // HTTP providers are fine at module level
      providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
      ]
    })
      // override the component's own providers: now AuthService, Router, MatSnackBar inside the component
      .overrideComponent(LoginComponent, {
        set: {
          providers: [
            { provide: AuthService, useValue: authServiceSpy },
            { provide: Router, useValue: routerSpy },
            { provide: MatSnackBar, useValue: snackBarSpy }
          ]
        }
      })
      .compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    localStorage.removeItem('auth_token');
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should show alert if fields are empty', () => {
    component.loginObj = { Email: '', Password: '' };
    component.onLogin();

    expect(snackBarSpy.open).toHaveBeenCalledWith(
      'Please fill in all fields.',
      'Close',
      jasmine.objectContaining({
        duration: 3000,
        panelClass: ['error-snackbar'],
        horizontalPosition: 'center',
        verticalPosition: 'top'
      })
    );
  });

  it('should call AuthService.login and handle success', fakeAsync(() => {
    authServiceSpy.login.and.returnValue(of({ Token: 'abc123' }));

    component.loginObj = { Email: 'test@example.com', Password: 'password123' };
    component.onLogin();
    tick();

    expect(authServiceSpy.login).toHaveBeenCalledWith('test@example.com', 'password123');
    expect(localStorage.getItem('auth_token')).toBe('abc123');
    expect(snackBarSpy.open).toHaveBeenCalledWith(
      'Login Successful',
      'Close',
      jasmine.objectContaining({
        duration: 3000,
        panelClass: ['success-snackbar'],
        horizontalPosition: 'center',
        verticalPosition: 'top'
      })
    );
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/dashboard/config']);
  }));

  it('should handle 400 error', fakeAsync(() => {
    authServiceSpy.login.and.returnValue(
      throwError(() => ({ status: 400, error: { message: 'Invalid credentials' } }))
    );

    component.loginObj = { Email: 'test@example.com', Password: 'wrongpassword' };
    component.onLogin();
    tick();

    expect(snackBarSpy.open).toHaveBeenCalledWith(
      'Invalid credentials',
      'Close',
      jasmine.objectContaining({
        duration: 3000,
        panelClass: ['error-snackbar'],
        horizontalPosition: 'center',
        verticalPosition: 'top'
      })
    );
  }));

  it('should handle 500 error', fakeAsync(() => {
    authServiceSpy.login.and.returnValue(
      throwError(() => ({ status: 500, error: { message: 'Server error' } }))
    );

    component.loginObj = { Email: 'test@example.com', Password: 'pass' };
    component.onLogin();
    tick();

    expect(snackBarSpy.open).toHaveBeenCalledWith(
      'An unexpected error occurred. Please try again later.',
      'Close',
      jasmine.objectContaining({
        duration: 3000,
        panelClass: ['error-snackbar'],
        horizontalPosition: 'center',
        verticalPosition: 'top'
      })
    );
  }));

  it('should handle generic error', fakeAsync(() => {
    authServiceSpy.login.and.returnValue(
      throwError(() => ({ status: 0, error: { message: 'Network error' } }))
    );

    component.loginObj = { Email: 'test@example.com', Password: 'pass' };
    component.onLogin();
    tick();

    expect(snackBarSpy.open).toHaveBeenCalledWith(
      'An error occurred during login.',
      'Close',
      jasmine.objectContaining({
        duration: 3000,
        panelClass: ['error-snackbar'],
        horizontalPosition: 'center',
        verticalPosition: 'top'
      })
    );
  }));
});

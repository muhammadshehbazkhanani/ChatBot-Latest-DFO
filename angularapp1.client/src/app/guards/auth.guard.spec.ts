import { TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { Router } from '@angular/router';
import { AuthGuard } from './auth.guard';
import { of } from 'rxjs';

describe('AuthGuard', () => {
  let authGuard: AuthGuard;
  let router: Router;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [RouterTestingModule],
      providers: [AuthGuard]
    });

    authGuard = TestBed.inject(AuthGuard);
    router = TestBed.inject(Router);
  });

  it('should allow access when token exists', () => {
    spyOn(localStorage, 'getItem').and.returnValue('some-token'); // Mocking the localStorage
    spyOn(router, 'navigate'); // Mocking the router navigate method

    const result = authGuard.canActivate();
    expect(result).toBe(true); // should allow access
  });

  it('should redirect to login when no token exists', () => {
    spyOn(localStorage, 'getItem').and.returnValue(null); // Mocking the absence of a token
    spyOn(router, 'navigate'); // Mocking the router navigate method

    const result = authGuard.canActivate();
    expect(result).toBe(false); // should deny access
    expect(router.navigate).toHaveBeenCalledWith(['/login']); // should navigate to login
  });
});

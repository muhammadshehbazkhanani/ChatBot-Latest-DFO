import { TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { Router } from '@angular/router';
import { LoginGuard } from './login.guard';

describe('LoginGuard', () => {
  let loginGuard: LoginGuard;
  let router: Router;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [RouterTestingModule],
      providers: [LoginGuard]
    });

    loginGuard = TestBed.inject(LoginGuard);
    router = TestBed.inject(Router);
  });

  it('should navigate to dashboard if token exists', () => {
    spyOn(localStorage, 'getItem').and.returnValue('some-token'); // Mocking the localStorage
    spyOn(router, 'navigate'); // Mocking the router navigate method

    const result = loginGuard.canActivate();
    expect(result).toBe(false); // should deny access
    expect(router.navigate).toHaveBeenCalledWith(['/dashboard/config']); // should navigate to dashboard
  });

  it('should allow access to login if no token exists', () => {
    spyOn(localStorage, 'getItem').and.returnValue(null); // Mocking the absence of a token

    const result = loginGuard.canActivate();
    expect(result).toBe(true); // should allow access
  });
});

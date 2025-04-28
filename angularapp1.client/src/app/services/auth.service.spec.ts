// src/app/services/auth.service.spec.ts
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  const baseUrl = 'http://localhost:8080/api/auth';

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService]
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('#login', () => {
    it('should POST to /login with correct body and headers', () => {
      const mockResponse = { token: 'abc123' };
      service.login('user@example.com', 'password').subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${baseUrl}/login`);
      expect(req.request.method).toBe('POST');
      expect(req.request.headers.get('Content-Type')).toBe('application/json');
      expect(req.request.body).toEqual({
        email: 'user@example.com',
        password: 'password'
      });

      req.flush(mockResponse);
    });

    it('should propagate error response', () => {
      const mockError = { status: 401, statusText: 'Unauthorized' };

      service.login('user@example.com', 'wrongpass').subscribe({
        next: () => fail('expected an error'),
        error: (error) => {
          expect(error.status).toBe(401);
        }
      });

      const req = httpMock.expectOne(`${baseUrl}/login`);
      req.flush({}, mockError);
    });
  });

  describe('#register', () => {
    it('should POST to /register without role when not provided', () => {
      const mockResponse = { id: 1, email: 'new@example.com' };

      service.register('new@example.com', 'pass123').subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${baseUrl}/register`);
      expect(req.request.method).toBe('POST');
      expect(req.request.headers.get('Content-Type')).toBe('application/json');
      expect(req.request.body).toEqual({
        email: 'new@example.com',
        password: 'pass123'
      });

      req.flush(mockResponse);
    });

    it('should include role when provided', () => {
      const mockResponse = { id: 2, email: 'admin@example.com', role: 'admin' };

      service.register('admin@example.com', 'adminpass', 'admin').subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${baseUrl}/register`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({
        email: 'admin@example.com',
        password: 'adminpass',
        role: 'admin'
      });

      req.flush(mockResponse);
    });

    it('should propagate error response on register failure', () => {
      const mockError = { status: 500, statusText: 'Server Error' };

      service.register('user@example.com', 'pass123').subscribe({
        next: () => fail('expected an error'),
        error: (error) => {
          expect(error.status).toBe(500);
        }
      });

      const req = httpMock.expectOne(`${baseUrl}/register`);
      req.flush({}, mockError);
    });
  });
});

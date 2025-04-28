import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private baseUrl = 'http://localhost:8080/api/auth';
  
  constructor(private http: HttpClient) {}

  // Login method 
  login(email: string, password: string): Observable<any> {
    const body = {
      email: email,
      password: password
    };
    return this.http.post(`${this.baseUrl}/login`, body, {
      headers: {
        'Content-Type': 'application/json'
      }
    });
  }

  // New register method
  register(email: string, password: string, role?: string): Observable<any> {
    const body = {
      email: email,
      password: password,
      ...(role && { role: role }) 
    };
    return this.http.post(`${this.baseUrl}/register`, body, {
      headers: {
        'Content-Type': 'application/json'
      }
    });
  }
}

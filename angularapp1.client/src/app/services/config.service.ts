import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ConfigService {
  private baseUrl = 'http://localhost:8080/api/BotConfigs'; 

  constructor(private http: HttpClient) {}

  // Get all configurations
  getConfigs(): Observable<any> {
    return this.http.get(`${this.baseUrl}`);
  }

  // Get a specific configuration by ID
  getConfig(id: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/${id}`);
  }

  // Create a new configuration
  createConfig(config: any): Observable<any> {
    return this.http.post(`${this.baseUrl}`, config, {
      headers: {
        'Content-Type': 'application/json',
      },
    });
  }

  // Update an existing configuration
  updateConfig(id: string, config: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/${id}`, config, {
      headers: {
        'Content-Type': 'application/json',
      },
    });
  }

  // Delete a configuration
  deleteConfig(id: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }
}

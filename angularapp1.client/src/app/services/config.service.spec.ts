import { TestBed } from '@angular/core/testing';
import { ConfigService } from './config.service';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

describe('ConfigService', () => {
  let service: ConfigService;
  let httpMock: HttpTestingController;
  const baseUrl = 'http://localhost:8080/api/BotConfigs';

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ConfigService]
    });

    service = TestBed.inject(ConfigService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should fetch all configs', () => {
    const mockConfigs = [{ id: '1' }, { id: '2' }];

    service.getConfigs().subscribe(configs => {
      expect(configs.length).toBe(2);
      expect(configs).toEqual(mockConfigs);
    });

    const req = httpMock.expectOne(`${baseUrl}`);
    expect(req.request.method).toBe('GET');
    req.flush(mockConfigs);
  });

  it('should fetch a config by id', () => {
    const mockConfig = { id: '1', name: 'Test Config' };

    service.getConfig('1').subscribe(config => {
      expect(config).toEqual(mockConfig);
    });

    const req = httpMock.expectOne(`${baseUrl}/1`);
    expect(req.request.method).toBe('GET');
    req.flush(mockConfig);
  });

  it('should create a config', () => {
    const newConfig = { name: 'New Config' };

    service.createConfig(newConfig).subscribe(response => {
      expect(response).toEqual(newConfig);
    });

    const req = httpMock.expectOne(`${baseUrl}`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(newConfig);
    req.flush(newConfig);
  });

  it('should update a config', () => {
    const updatedConfig = { id: '1', name: 'Updated Config' };

    service.updateConfig('1', updatedConfig).subscribe(response => {
      expect(response).toEqual(updatedConfig);
    });

    const req = httpMock.expectOne(`${baseUrl}/1`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(updatedConfig);
    req.flush(updatedConfig);
  });

  it('should delete a config', () => {
    service.deleteConfig('1').subscribe(response => {
      expect(response).toBeNull();
    });

    const req = httpMock.expectOne(`${baseUrl}/1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});

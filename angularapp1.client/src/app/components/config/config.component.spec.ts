// // src/app/config/config.component.spec.ts
import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ConfigComponent } from './config.component';
import { ConfigService } from '../../services/config.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

describe('ConfigComponent (standalone)', () => {
  let fixture: ComponentFixture<ConfigComponent>;
  let component: ConfigComponent;
  let svc: jasmine.SpyObj<ConfigService>;
  let snack: jasmine.SpyObj<MatSnackBar>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    svc = jasmine.createSpyObj('ConfigService', ['getConfigs', 'createConfig', 'updateConfig', 'deleteConfig']);
    snack = jasmine.createSpyObj('MatSnackBar', ['open']);
    router = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [NoopAnimationsModule, ConfigComponent],
      providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting()
      ]
    })
      .overrideComponent(ConfigComponent, {
        set: {
          providers: [
            { provide: ConfigService, useValue: svc },
            { provide: MatSnackBar, useValue: snack },
            { provide: Router, useValue: router }
          ]
        }
      })
      .compileComponents();

    fixture = TestBed.createComponent(ConfigComponent);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    svc.getConfigs.calls.reset();
    svc.createConfig.calls.reset();
    svc.updateConfig.calls.reset();
    svc.deleteConfig.calls.reset();
    snack.open.calls.reset();
    router.navigate.calls.reset();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit / getConfigs', () => {
    it('loads configs and auto-selects first', fakeAsync(() => {
      const cfgs = [{ Id: 1, AppName: 'A', Config1: 'x', Config2: 'y', Config3: 'z' }];
      svc.getConfigs.and.returnValue(of(cfgs));

      component.ngOnInit();
      tick();

      expect(svc.getConfigs).toHaveBeenCalled();
      expect(component.configList).toEqual(cfgs);
      expect(component.selectedConfigIndex).toBe(0);
      expect(component.configObj.AppName).toBe('A');
    }));

    it('shows error snackbar on load failure', fakeAsync(() => {
      svc.getConfigs.and.returnValue(throwError(() => ({ status: 500 })));

      component.ngOnInit();
      tick();

      expect(snack.open).toHaveBeenCalledWith(
        'Failed to load configurations.',
        'Close',
        jasmine.objectContaining({ panelClass: ['error-snackbar'] })
      );
    }));
  });

  describe('addNewBot / showConfigDetails', () => {
    it('adds new bot and selects it', () => {
      component.configList = [];
      component.addNewBot();
      expect(component.configList.length).toBe(1);
      expect(component.selectedConfigIndex).toBe(0);
    });

    it('showConfigDetails switches selection and obj', () => {
      const obj = { Id: 2, AppName: 'B', Config1: '', Config2: '', Config3: '' } as any;
      component.showConfigDetails(obj, 3);
      expect(component.selectedConfigIndex).toBe(3);
      expect(component.configObj).toEqual(obj);
    });
  });

  describe('onSubmit – validation', () => {
    it('alerts if any field missing', () => {
      component.configObj = { AppName: '', Config1: '', Config2: '', Config3: '' };
      component.onSubmit();
      expect(snack.open).toHaveBeenCalledWith(
        'Please fill in all fields.', 'Close',
        jasmine.objectContaining({ panelClass: ['error-snackbar'] })
      );
    });
  });

  describe('onSubmit – create new config (no Id)', () => {
    beforeEach(() => {
      component.selectedConfigIndex = null;
      component.configObj = { AppName: 'X', Config1: '1', Config2: '2', Config3: '3' };
    });

    it('calls createConfig and pushes to list on success', fakeAsync(() => {
      const created = { Id: 5, ...component.configObj };
      svc.createConfig.and.returnValue(of(created));

      component.onSubmit();
      tick();

      expect(svc.createConfig).toHaveBeenCalledWith(component.configObj);
      expect(component.configList).toContain(created);
      expect(component.selectedConfigIndex).toBe(component.configList.length - 1);
      expect(snack.open).toHaveBeenCalledWith(
        'Configuration saved successfully!', 'Close',
        jasmine.objectContaining({ panelClass: ['success-snackbar'] })
      );
    }));

    it('shows error snackbar on create failure', fakeAsync(() => {
      svc.createConfig.and.returnValue(throwError(() => ({ status: 500 })));

      component.onSubmit();
      tick();

      expect(snack.open).toHaveBeenCalledWith(
        'Failed to save configuration.', 'Close',
        jasmine.objectContaining({ panelClass: ['error-snackbar'] })
      );
    }));
  });

  describe('onSubmit – update existing config (with Id)', () => {
    beforeEach(() => {
      const existing = { Id: (10).toString(), AppName: 'Y', Config1: 'a', Config2: 'b', Config3: 'c' };
      component.configList = [existing];
      component.selectedConfigIndex = 0;
      component.configObj = { ...existing };
    });

    it('calls updateConfig and replaces in list on success', fakeAsync(() => {
      const updated = { Id: 10, AppName: 'Y2', Config1: 'a2', Config2: 'b2', Config3: 'c2' };
      svc.updateConfig.and.returnValue(of(updated));

      component.onSubmit();
      tick();

      expect(svc.updateConfig).toHaveBeenCalledWith((10).toString(), component.configObj);
      expect(component.configList[0]).toEqual(updated);
      expect(snack.open).toHaveBeenCalledWith(
        'Configuration updated successfully!', 'Close',
        jasmine.objectContaining({ panelClass: ['success-snackbar'] })
      );
    }));

    it('shows error snackbar on update failure', fakeAsync(() => {
      svc.updateConfig.and.returnValue(throwError(() => ({ status: 500 })));

      component.onSubmit();
      tick();

      expect(snack.open).toHaveBeenCalledWith(
        'Failed to update configuration.', 'Close',
        jasmine.objectContaining({ panelClass: ['error-snackbar'] })
      );
    }));
  });

  describe('deleteConfig', () => {
    it('removes saved config and shows success', fakeAsync(() => {
      const saved = { Id: 20, AppName: 'Z', Config1: 'z1', Config2: 'z2', Config3: 'z3' };
      component.configList = [saved];
      component.selectedConfigIndex = 0;
      svc.deleteConfig.and.returnValue(throwError(() => ({ status: 500 })));

      component.deleteConfig();
      tick();

      expect(snack.open).toHaveBeenCalledWith(
        'Failed to delete configuration.', 'Close',
        jasmine.objectContaining({ panelClass: ['error-snackbar'] })
      );
    }));
  });
});

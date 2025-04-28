import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { ConfigService } from '../services/config.service';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-config',
  standalone: true,
  imports: [FormsModule, CommonModule, ReactiveFormsModule, MatSnackBarModule, MatIconModule],
  templateUrl: './config.component.html',
  styleUrl: './config.component.css'
})
export class ConfigComponent {
  configList: any[] = [];
  selectedConfigIndex: number | null = null;
  
  configObj = {
    AppName: '',
    Config1: '',
    Config2: '',
    Config3: ''
  };

  constructor(
    private configService: ConfigService,
    private snackBar: MatSnackBar,
    private router: Router
  ) {}

  ngOnInit() {
    this.getConfigs();
  }

  getConfigs() {
    this.configService.getConfigs().subscribe(
      (response) => {
        this.configList = response;
        // Select the first config by default if available
        if (this.configList.length > 0) {
          this.showConfigDetails(this.configList[0], 0);
        }
      },
      (error) => {
        console.error('Error fetching configs:', error);
        this.snackBar.open('Failed to load configurations.', 'Close', {
          duration: 3000,
          panelClass: ['error-snackbar'],
           horizontalPosition: 'center',  
              verticalPosition: 'top'
        });
      }
    );
  }

  addNewBot() {
    const newBot = {
      AppName: '',
      Config1: '',
      Config2: '',
      Config3: ''
    };
    this.configList.push(newBot);
    this.selectedConfigIndex = this.configList.length - 1;
    this.configObj = { ...newBot };
  }

  showConfigDetails(config: any, index: number) {
    this.selectedConfigIndex = index;
    this.configObj = { ...config };
  }

  isActive(index: number): boolean {
    return this.selectedConfigIndex === index;
  }

  onSubmit() {
    if (!this.configObj.AppName || !this.configObj.Config1 || !this.configObj.Config2 || !this.configObj.Config3) {
      this.snackBar.open('Please fill in all fields.', 'Close', {
        duration: 3000,
        panelClass: ['error-snackbar'],
        horizontalPosition: 'center',  
              verticalPosition: 'top'
      });
      return;
    }
  
    if (this.selectedConfigIndex !== null) {
      const config = this.configList[this.selectedConfigIndex];
      if (config.Id) {
        // Update existing config - pass both id and configObj
        this.configService.updateConfig(config.Id, this.configObj).subscribe(
          (response) => {
            if (this.selectedConfigIndex !== null) {
              this.configList[this.selectedConfigIndex] = response;
            }
            this.snackBar.open('Configuration updated successfully!', 'Close', {
              duration: 3000,
              panelClass: ['success-snackbar'],
              horizontalPosition: 'center',  
              verticalPosition: 'top'
            });
          },
          (error) => {
            console.error('Error updating config:', error);
            this.snackBar.open('Failed to update configuration.', 'Close', {
              duration: 3000,
              panelClass: ['error-snackbar'],
              horizontalPosition: 'center',  
              verticalPosition: 'top'
            });
          }
        );
      } else {
        // Create new config
        this.configService.createConfig(this.configObj).subscribe(
          (response) => {
            if (this.selectedConfigIndex !== null) {
            this.configList[this.selectedConfigIndex] = response;
            }
            this.snackBar.open('Configuration saved successfully!', 'Close', {
              duration: 3000,
              panelClass: ['success-snackbar'],
              horizontalPosition: 'center',  
              verticalPosition: 'top'
            });
          },
          (error) => {
            console.error('Error creating config:', error);
            this.snackBar.open('Failed to save configuration.', 'Close', {
              duration: 3000,
              panelClass: ['error-snackbar'],
              horizontalPosition: 'center',  
              verticalPosition: 'top'
            });
          }
        );
      }
    } else {
      // Create new config (when no selection exists)
      this.configService.createConfig(this.configObj).subscribe(
        (response) => {
          this.configList.push(response);
          this.selectedConfigIndex = this.configList.length - 1;
          this.snackBar.open('Configuration saved successfully!', 'Close', {
            duration: 3000,
            panelClass: ['success-snackbar'],
            horizontalPosition: 'center',  
              verticalPosition: 'top'
          });
        },
        (error) => {
          console.error('Error creating config:', error);
          this.snackBar.open('Failed to save configuration.', 'Close', {
            duration: 3000,
            panelClass: ['error-snackbar'],
            horizontalPosition: 'center',  
              verticalPosition: 'top'
          });
        }
      );
    }
  }

  isUpdating(): boolean {
    return this.selectedConfigIndex !== null && !!this.configList[this.selectedConfigIndex]?.Id;
  }

  deleteConfig() {
    if (this.selectedConfigIndex !== null) {
      const config = this.configList[this.selectedConfigIndex];
      if (config.Id) {
        this.configService.deleteConfig(config.Id).subscribe(
          () => {
            this.configList.splice(this.selectedConfigIndex!, 1);
            this.handleAfterDelete();
            this.snackBar.open('Configuration deleted successfully!', 'Close', {
              duration: 3000,
              panelClass: ['success-snackbar'],
              horizontalPosition: 'center',  
              verticalPosition: 'top'
            });
          },
          (error) => {
            console.error('Error deleting config:', error);
            this.snackBar.open('Failed to delete configuration.', 'Close', {
              duration: 3000,
              panelClass: ['error-snackbar'],
              horizontalPosition: 'center',  
              verticalPosition: 'top'
            });
          }
        );
      } else {
        // For new unsaved configs (without ID)
        this.configList.splice(this.selectedConfigIndex!, 1);
        this.handleAfterDelete();
      }
    }
  }

  private handleAfterDelete() {
    if (this.configList.length > 0) {
      this.selectedConfigIndex = Math.min(this.selectedConfigIndex!, this.configList.length - 1);
      this.configObj = { ...this.configList[this.selectedConfigIndex] };
    } else {
      this.selectedConfigIndex = null;
      this.resetForm();
    }
  }

  resetForm() {
    this.configObj = {
      AppName: '',
      Config1: '',
      Config2: '',
      Config3: ''
    };
  }
}

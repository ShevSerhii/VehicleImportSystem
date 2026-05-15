import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { VehicleApiService } from '../../services/vehicle-api';
import { CurrencyRates } from '../../models/vehicle.models';
import { ThemeService } from '../../services/theme.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    CommonModule, 
    MatToolbarModule, 
    MatIconModule, 
    MatButtonModule, 
    MatTooltipModule
  ],
  templateUrl: './header.html',
  styleUrl: './header.scss'
})
export class Header implements OnInit {
  private readonly api = inject(VehicleApiService);
  protected readonly themeService = inject(ThemeService);

  rates?: CurrencyRates;
  isLoadingRates = true;
  errorLoadingRates = false;

  ngOnInit(): void {
    this.loadCurrencyRates();
  }

  loadCurrencyRates(): void {
    this.isLoadingRates = true;
    this.errorLoadingRates = false;
    
    this.api.getCurrencyRates().subscribe({
      next: (data) => {
        this.rates = data;
        this.isLoadingRates = false;
      },
      error: (err) => {
        console.error('Failed to load currency:', err);
        this.errorLoadingRates = true;
        this.isLoadingRates = false;
      }
    });
  }
}
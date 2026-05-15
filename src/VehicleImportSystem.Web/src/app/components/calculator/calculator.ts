import { Component, OnInit, inject, DestroyRef, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';

import { BaseChartDirective } from 'ng2-charts';
import { Chart, ChartConfiguration, ChartData, registerables } from 'chart.js';

import { Brand, Model } from '../../models/vehicle.models';
import { FuelType } from '../../models/fuel-type.enum';
import { HybridExciseScheme } from '../../models/hybrid-excise-scheme.enum';
import { CalculationRequest, CalculationResult } from '../../models/calculation.models';
import { VehicleApiService } from '../../services/vehicle-api';
import { ThemeService } from '../../services/theme.service';

Chart.register(...registerables);

@Component({
  selector: 'app-calculator',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatIconModule,
    BaseChartDirective,
  ],
  templateUrl: './calculator.html',
  styleUrl: './calculator.scss',
})
export class Calculator implements OnInit {
  form: FormGroup;

  protected readonly FuelType = FuelType;
  protected readonly HybridExciseScheme = HybridExciseScheme;
  private readonly themeService = inject(ThemeService);
  private readonly destroyRef = inject(DestroyRef);

  brands: Brand[] = [];
  filteredBrands: Brand[] = [];
  models: Model[] = [];
  filteredModels: Model[] = [];

  fuelTypes = [
    { value: FuelType.Petrol, label: 'Petrol (Бензин)' },
    { value: FuelType.Diesel, label: 'Diesel (Дизель)' },
    { value: FuelType.Gas, label: 'Gas (Газ)' },
    { value: FuelType.GasPetrol, label: 'Gas/Petrol (ГБО)' },
    { value: FuelType.Hybrid, label: 'Hybrid (Гібрид)' },
    { value: FuelType.Electric, label: 'Electric (Електро)' },
  ];

  hybridSchemes = [
    { value: HybridExciseScheme.FixedRate, label: '100 €' },
    { value: HybridExciseScheme.ByIceEngine, label: 'За об\'ємом двигуна' },
  ];

  hybridIceFuelTypes = [
    { value: FuelType.Petrol, label: 'Бензин' },
    { value: FuelType.Diesel, label: 'Дизель' },
  ];

  isLoading = false;
  isLoadingBrands = false;
  isLoadingModels = false;
  isPriceLoading = false;
  
  result: CalculationResult | null = null;
  private lastCalculationResult: CalculationResult | null = null;

  public pieChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: true,
        position: 'right',
      },
    },
  };

  public pieChartData: ChartData<'pie', number[], string | string[]> = {
    labels: ['Ввізне мито', 'Акциз', 'ПДВ', 'Пенсійний фонд'],
    datasets: [{ data: [0, 0, 0, 0] }],
  };

  constructor(
    private fb: FormBuilder,
    private api: VehicleApiService,
  ) {
    effect(() => {
      const isDark = this.themeService.isDarkMode();

      if (this.lastCalculationResult) {
        setTimeout(() => {
          this.updateChartData(this.lastCalculationResult!);
        }, 50);
      }
    });

    this.form = this.fb.group({
      brandInput: [''],
      markId: [null, Validators.required],
      modelInput: [{ value: '', disabled: true }],
      modelId: [{ value: null, disabled: true }, Validators.required],
      year: [2018, [Validators.required, Validators.min(1900), Validators.max(2026)]],
      fuelType: [null, Validators.required],
      engineCapacity: [2000, [Validators.required, Validators.min(1)]],
      priceInEur: [15000, [Validators.required, Validators.min(1)]],
      hybridExciseScheme: [HybridExciseScheme.FixedRate],
      hybridIceFuelType: [FuelType.Petrol],
      evVatExemptPercent: [0, [Validators.min(0), Validators.max(100)]],
    });
  }

  get isHybrid(): boolean {
    return this.form.get('fuelType')?.value === FuelType.Hybrid;
  }

  get isHybridByIce(): boolean {
    return this.isHybrid && this.form.get('hybridExciseScheme')?.value === HybridExciseScheme.ByIceEngine;
  }

  get isElectric(): boolean {
    return this.form.get('fuelType')?.value === FuelType.Electric;
  }

  ngOnInit(): void {
    this.isLoadingBrands = true;
    this.api.getBrands().subscribe({
      next: (data) => {
        this.brands = data;
        this.filteredBrands = data;
        this.isLoadingBrands = false;
      },
      error: (err) => {
        console.error(err);
        this.isLoadingBrands = false;
      },
    });

    this.form.get('brandInput')?.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        if (typeof value === 'string') {
          this.filterBrands(value);
          const matchingBrand = this.brands.find(
            (b) => b.name.toLowerCase() === value.toLowerCase(),
          );

          if (!matchingBrand) {
            this.form.get('markId')?.setValue(null, { emitEvent: false });
          } else {
            this.form.get('markId')?.setValue(matchingBrand.id, { emitEvent: false });
          }
        }
      });

    this.form.get('markId')?.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((brandId) => {
        if (brandId) {
          const selectedBrand = this.brands.find((b) => b.id === brandId);
          if (selectedBrand) {
            this.form.get('brandInput')?.setValue(selectedBrand.name, { emitEvent: false });
          }

          this.form.get('modelInput')?.enable();
          this.isLoadingModels = true;
          this.models = [];
          this.filteredModels = [];
          
          this.form.get('modelInput')?.setValue('');
          this.form.get('modelId')?.setValue(null);
          this.form.get('modelId')?.disable();

          this.api.getModels(brandId).subscribe({
            next: (data) => {
              this.models = data;
              this.filteredModels = data;
              this.form.get('modelId')?.enable();
              this.isLoadingModels = false;
            },
            error: (err) => {
              console.error(err);
              this.isLoadingModels = false;
            },
          });
        } else {
          this.form.get('modelInput')?.disable();
          this.models = [];
          this.filteredModels = [];
          this.form.get('modelInput')?.setValue('');
          this.form.get('modelId')?.setValue(null);
          this.form.get('modelId')?.disable();
        }
      });

    this.form.get('modelInput')?.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        if (typeof value === 'string') {
          this.filterModels(value);
          const matchingModel = this.models.find(
            (m) => m.name.toLowerCase() === value.toLowerCase(),
          );

          if (!matchingModel) {
            this.form.get('modelId')?.setValue(null, { emitEvent: false });
          } else {
            this.form.get('modelId')?.setValue(matchingModel.id, { emitEvent: false });
          }
        }
      });

    this.form.get('fuelType')?.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        const scheme = this.form.get('hybridExciseScheme');
        if (this.isHybrid) {
          scheme?.setValidators(Validators.required);
        } else {
          scheme?.clearValidators();
        }
        scheme?.updateValueAndValidity({ emitEvent: false });
      });
  }

  private filterBrands(value: string): void {
    const filterValue = value.toLowerCase();
    this.filteredBrands = this.brands.filter((brand) =>
      brand.name.toLowerCase().startsWith(filterValue),
    );
  }

  private filterModels(value: string): void {
    const filterValue = value.toLowerCase();
    this.filteredModels = this.models.filter((model) =>
      model.name.toLowerCase().startsWith(filterValue),
    );
  }

  displayBrandFn(brand: Brand): string {
    return brand && brand.name ? brand.name : '';
  }

  displayModelFn(model: Model): string {
    return model && model.name ? model.name : '';
  }

  onBrandSelected(brandName: string): void {
    const brand = this.brands.find((b) => b.name === brandName);
    if (brand) {
      this.form.get('brandInput')?.setValue(brand.name, { emitEvent: false });
      this.form.get('markId')?.setValue(brand.id);
    }
  }

  onModelSelected(modelName: string): void {
    const model = this.models.find((m) => m.name === modelName);
    if (model) {
      this.form.get('modelInput')?.setValue(model.name, { emitEvent: false });
      this.form.get('modelId')?.setValue(model.id);
    }
  }

  getMarketPrice(): void {
    const { markId, modelId, year, fuelType, engineCapacity } = this.form.value;

    if (!markId || !modelId || !year || !fuelType) {
      this.form.markAllAsTouched();
      return;
    }

    this.isPriceLoading = true;
    
    const volumeForApi = (fuelType === this.FuelType.Electric) ? 0 : (engineCapacity || 0) / 1000;

    this.api.getAveragePrice(markId, modelId, year, fuelType, volumeForApi).subscribe({
      next: (res) => {
        if (res && res.priceEur > 0) {
          this.form.patchValue({ priceInEur: res.priceEur });
        } else {
          console.warn('Ціну не знайдено');
        }
        this.isPriceLoading = false;
      },
      error: (err) => {
        console.error(err);
        this.isPriceLoading = false;
      },
    });
  }

  onSubmit() {
    if (this.form.invalid) return;

    this.isLoading = true;
    this.result = null;

    this.api.calculate(this.buildCalculationRequest()).subscribe({
      next: (res) => {
        this.result = res;
        this.lastCalculationResult = res;
        this.api.triggerRefreshHistory();
        this.updateChartData(res);
        this.isLoading = false;
      },
      error: (err) => {
        console.error(err);
        this.isLoading = false;
      },
    });
  }

  private buildCalculationRequest(): CalculationRequest {
    const v = this.form.getRawValue();
    const request: CalculationRequest = {
      markId: v.markId,
      modelId: v.modelId,
      year: v.year,
      fuelType: v.fuelType,
      engineCapacity: v.engineCapacity,
      priceInEur: v.priceInEur,
      evVatExemptShare: this.isElectric ? (v.evVatExemptPercent ?? 0) / 100 : 0,
    };

    if (v.fuelType === FuelType.Hybrid) {
      request.hybridExciseScheme = v.hybridExciseScheme;
      if (v.hybridExciseScheme === HybridExciseScheme.ByIceEngine) {
        request.hybridIceFuelType = v.hybridIceFuelType;
      }
    }

    return request;
  }

  private updateChartData(res: CalculationResult) {
    const style = getComputedStyle(document.documentElement);
    
    const getVar = (name: string, fallback: string) => 
      style.getPropertyValue(name).trim() || fallback;

    const colors = [
      getVar('--chart-duty', '#FF6384'),
      getVar('--chart-excise', '#36A2EB'),
      getVar('--chart-vat', '#FFCE56'),
      getVar('--chart-pension', '#4BC0C0')
    ];
    
    const borderColor = getVar('--chart-border', '#ffffff');

    this.pieChartData = {
      labels: ['Ввізне мито', 'Акциз', 'ПДВ', 'Пенсійний фонд'],
      datasets: [
        {
          data: [res.importDuty, res.exciseTax, res.vat, res.pensionFund],
          backgroundColor: colors,
          borderColor: borderColor,
          borderWidth: 2,
          hoverOffset: 4,
        },
      ],
    };
  }
}
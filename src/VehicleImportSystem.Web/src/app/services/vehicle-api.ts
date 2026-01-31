import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { DeviceService } from './device.service';
import { environment } from '../../environments/environment';
import { USE_DEVICE_ID } from '../core/http/api.context';
import { FuelType } from '../models/fuel-type.enum';
import { Brand, CurrencyRates, Model } from '../models/vehicle.models';
import {
  AveragePriceDto,
  CalculationRequest,
  CalculationResult,
  HistoryRecord,
} from '../models/calculation.models';

@Injectable({
  providedIn: 'root',
})
export class VehicleApiService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  private refreshHistorySource = new Subject<void>();
  
  refreshHistory$ = this.refreshHistorySource.asObservable();

  triggerRefreshHistory() {
    this.refreshHistorySource.next();
  }

  getCurrencyRates(): Observable<CurrencyRates> {
    return this.http.get<CurrencyRates>(`${this.apiUrl}/currency/rates`);
  }

  getBrands(): Observable<Brand[]> {
    return this.http.get<Brand[]>(`${this.apiUrl}/brands`);
  }

  getModels(brandId: number): Observable<Model[]> {
    return this.http.get<Model[]>(`${this.apiUrl}/market/brands/${brandId}/models`);
  }

  getAveragePrice(
    markId: number,
    modelId: number,
    year: number,
    fuelType: FuelType,
    engineVolume: number,
  ): Observable<AveragePriceDto> {
    let params = new HttpParams()
      .set('markId', markId)
      .set('modelId', modelId)
      .set('year', year)
      .set('fuelType', fuelType)
      .set('engineVolume', engineVolume);

    return this.http.get<AveragePriceDto>(`${this.apiUrl}/market/average-price`, { params });
  }

  getHistory(): Observable<HistoryRecord[]> {
    return this.http.get<HistoryRecord[]>(`${this.apiUrl}/history/`, {
      context: new HttpContext().set(USE_DEVICE_ID, true),
    });
  }

  calculate(data: CalculationRequest): Observable<CalculationResult> {
    return this.http.post<CalculationResult>(`${this.apiUrl}/calculator/calculate`, data, {
      context: new HttpContext().set(USE_DEVICE_ID, true),
    });
  }

  deleteRecord(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/history/${id}`);
  }

  clearHistory(): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/history/clear`, {
      context: new HttpContext().set(USE_DEVICE_ID, true),
    });
  }
}

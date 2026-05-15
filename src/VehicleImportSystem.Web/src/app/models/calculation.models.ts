import { FuelType } from './fuel-type.enum';
import { HybridExciseScheme } from './hybrid-excise-scheme.enum';

export interface CalculationRequest {
  markId?: number;
  modelId?: number;
  year: number;
  fuelType: FuelType;
  engineCapacity: number;
  priceInEur: number;
  hybridExciseScheme?: HybridExciseScheme | null;
  hybridIceFuelType?: FuelType | null;
  evVatExemptShare?: number;
}

export interface CalculationResult {
  importDuty: number;
  exciseTax: number;
  vat: number;
  pensionFund: number;
  totalCustomsClearance: number;
  totalVehicleCost: number;
  marketPrice: number;
  potentialProfit: number;
  isProfitable: boolean;
  currencyRateUsed: number;
}

export interface HistoryRecord {
  id: number;
  brandName: string;
  modelName: string;
  year: number;
  priceInEur: number;
  totalTurnkeyPrice: number;
  potentialProfit: number;
  date: string;
}

export interface AveragePriceDto {
  priceUsd: number;
  priceEur: number;
}

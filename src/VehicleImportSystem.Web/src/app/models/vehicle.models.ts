export interface Brand {
  id: number;
  name: string;
}

export interface Model {
  id: number;
  name: string;
  brandId: number;
}

export interface CurrencyRates {
  usd: number;
  eur: number;
  date: string;
}
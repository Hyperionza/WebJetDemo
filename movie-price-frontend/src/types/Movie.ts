export interface PriceInfo {
  provider: string;
  price: number;
  freshness: 'Fresh' | 'Cached' | 'Stale';
  lastUpdated: string;
  freshnessIndicator: string;
}

export interface MovieComparison {
  id: number;
  title: string;
  year?: string;
  genre?: string;
  director?: string;
  poster?: string;
  rating?: string;
  prices: PriceInfo[];
  bestPrice?: PriceInfo;
}

export interface MovieDetail extends MovieComparison {
  rated?: string;
  released?: string;
  runtime?: string;
  writer?: string;
  actors?: string;
  plot?: string;
  language?: string;
  country?: string;
  awards?: string;
  metascore?: string;
  votes?: string;
}

export interface ApiHealth {
  provider: string;
  isHealthy: boolean;
  lastChecked: string;
  errorMessage?: string;
}

export interface PriceInfo {
  providerId: string;
  provider: string;
  movieId: string;
  price: number;
  lastUpdated: string;
}

export interface MovieComparison {
  id: string;
  title: string;
  year?: string;
  genre?: string;
  director?: string;
  actors?: string;
  plot?: string;
  poster?: string;
  rating?: string;
  prices: PriceInfo[];
  cheapestPrice?: PriceInfo;
}

export interface MovieDetail {
  title: string;
  year?: string;
  type?: string;
  rated?: string;
  released?: string;
  runtime?: string;
  genre?: string;
  director?: string;
  writer?: string;
  actors?: string;
  plot?: string;
  language?: string;
  country?: string;
  awards?: string;
  poster?: string;
  metascore?: string;
  rating?: string;
  votes?: string;
  prices: PriceInfo[];
  cheapestPrice?: PriceInfo;
  updatedAt: string;
}

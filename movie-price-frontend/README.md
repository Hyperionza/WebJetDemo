# Movie Price Comparison Frontend

A modern React application built with TypeScript that provides a beautiful, responsive interface for comparing movie prices from multiple providers. The frontend communicates with a Clean Architecture backend API to deliver real-time price comparisons.

## 🎬 Features

- **💰 Price Comparison**: View prices from Cinemaworld and Filmworld side-by-side
- **🏆 Best Deal Highlighting**: Automatically highlights the cheapest price
- **📱 Responsive Design**: Works seamlessly on desktop, tablet, and mobile
- **🖼️ Image Handling**: Poster image loading with fallbacks and placeholders
- **⚡ Data Refresh**: Manual refresh functionality for latest price data
- **🎨 Clean UI**: Simple, professional interface with movie cards
- **⚠️ Error Handling**: Basic error states and loading indicators

## 🏗️ Architecture

### **Technology Stack**
- **React 18**: React with concurrent features
- **TypeScript**: Type-safe development with IntelliSense
- **CSS Modules**: Scoped styling with CSS-in-JS benefits
- **Axios**: HTTP client for API communication
- **React Hooks**: Modern functional component patterns
- **Error Boundaries**: Graceful error handling and recovery

### **Project Structure**
```
movie-price-frontend/
├── public/                    # Static assets
│   ├── index.html            # HTML template
│   ├── favicon.ico           # App icon
│   └── manifest.json         # PWA manifest
├── src/                      # Source code
│   ├── components/           # React components
│   │   ├── MovieCard.tsx     # Movie display component
│   │   ├── MovieCard.css     # MovieCard styles
│   │   └── __tests__/        # Component tests
│   │       └── MovieCard.test.tsx
│   ├── services/             # API services
│   │   ├── movieApi.ts       # Movie API service
│   │   └── __tests__/        # Service tests
│   │       └── movieApi.test.ts
│   ├── types/               # TypeScript definitions
│   │   └── Movie.ts         # Movie type definitions
│   ├── __tests__/           # App tests
│   │   └── App.test.tsx     # Main app tests
│   ├── App.tsx              # Main application component
│   ├── App.css              # App-level styles
│   ├── index.tsx            # Application entry point
│   ├── index.css            # Base styles
│   └── setupTests.ts        # Test configuration
├── package.json             # Dependencies and scripts
├── tsconfig.json           # TypeScript configuration
├── .env.development         # Development environment variables
└── Dockerfile              # Container configuration
```

## 🎯 Component Architecture

### **Component Hierarchy**
```
App
├── Header (Movie Price Comparison title)
├── Controls (Refresh button)
├── Loading/Error states (conditional)
└── MovieGrid
    └── MovieCard (multiple)
        ├── Movie poster with fallback
        ├── Movie info (title, year, genre, rating)
        └── Price section (best price + all prices)
```

### **Key Components**

#### **App Component**
```tsx
function App() {
  const [movies, setMovies] = useState<MovieComparison[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [refreshing, setRefreshing] = useState(false);

  const loadMovies = async () => {
    try {
      setLoading(true);
      setError(null);
      const moviesData = await movieApi.getMovies();
      setMovies(moviesData);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load movies');
    } finally {
      setLoading(false);
    }
  };

  const handleRefresh = async () => {
    try {
      setRefreshing(true);
      await movieApi.refreshMovieData();
      await loadMovies();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to refresh data');
    } finally {
      setRefreshing(false);
    }
  };

  const handleMovieClick = (movie: MovieComparison) => {
    // Shows movie details in alert dialog
    const priceText = movie.prices.map(p => `${p.provider}: $${p.price.toFixed(2)}`).join('\n');
    const cheapestText = movie.cheapestPrice ? `\nBest Price: ${movie.cheapestPrice.provider} - $${movie.cheapestPrice.price.toFixed(2)}` : '';
    alert(`Movie Details:\n\nTitle: ${movie.title}\nYear: ${movie.year || 'N/A'}\nGenre: ${movie.genre || 'N/A'}\nDirector: ${movie.director || 'N/A'}\nRating: ${movie.rating || 'N/A'}\n\nPrices:\n${priceText}${cheapestText}`);
  };

  return (
    <div className="App">
      <header className="App-header">
        <h1>🎬 Movie Price Comparison</h1>
        <div className="controls">
          <button onClick={handleRefresh} disabled={refreshing} className="refresh-button">
            {refreshing ? '🔄 Refreshing...' : '🔄 Refresh Data'}
          </button>
        </div>
      </header>

      <main className="App-main">
        {loading && <div className="loading">Loading movies...</div>}
        {error && (
          <div className="error">
            <p>Error: {error}</p>
            <button onClick={loadMovies}>Try Again</button>
          </div>
        )}
        {!loading && !error && (
          <>
            <div className="movies-count">
              Found {movies.length} movie{movies.length !== 1 ? 's' : ''}
            </div>
            <div className="movies-grid">
              {movies.map((movie) => (
                <MovieCard key={movie.id} movie={movie} onClick={handleMovieClick} />
              ))}
            </div>
            {movies.length === 0 && (
              <div className="no-movies">
                No movies found. Try refreshing the data or check your search query.
              </div>
            )}
          </>
        )}
      </main>
    </div>
  );
}
```

#### **MovieCard Component**
```tsx
interface MovieCardProps {
  movie: MovieComparison;
  onClick: (movie: MovieComparison) => void;
}

const MovieCard: React.FC<MovieCardProps> = ({ movie, onClick }) => {
  const [imageError, setImageError] = useState(false);
  const [imageLoading, setImageLoading] = useState(true);

  const formatPrice = (price: number) => `$${price.toFixed(2)}`;

  const handleImageError = () => {
    setImageError(true);
    setImageLoading(false);
  };

  const handleImageLoad = () => {
    setImageLoading(false);
  };

  const renderPosterContent = () => {
    if (!movie.poster || imageError) {
      return (
        <div className="poster-placeholder">
          <div className="poster-icon">🎬</div>
          <div className="poster-text">{movie.title}</div>
        </div>
      );
    }

    return (
      <>
        {imageLoading && (
          <div className="poster-loading" data-testid="poster-loading">
            <div className="loading-spinner"></div>
          </div>
        )}
        <img
          src={movie.poster}
          alt={movie.title}
          onError={handleImageError}
          onLoad={handleImageLoad}
          style={{ display: imageLoading ? 'none' : 'block' }}
        />
      </>
    );
  };

  return (
    <button className="movie-card" onClick={() => onClick(movie)} aria-label={movie.title} type="button">
      <div className="movie-poster">{renderPosterContent()}</div>
      <div className="movie-info">
        <h3 className="movie-title">{movie.title}</h3>
        <p className="movie-year">{movie.year}</p>
        <p className="movie-genre">{movie.genre}</p>
        {movie.rating && <p className="movie-rating">⭐ {movie.rating}</p>}

        <div className="price-section">
          {movie.cheapestPrice && (
            <div className="best-price">
              <span className="best-price-label">Best Price:</span>
              <span className="best-price-value">{formatPrice(movie.cheapestPrice.price)}</span>
            </div>
          )}
          <div className="all-prices">
            {movie.prices.map((price, index) => (
              <div key={index} className="price-item">
                <span className="provider">{price.provider}:</span>
                <span className="price">{formatPrice(price.price)}</span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </button>
  );
};
```

## 🔗 API Integration

### **Movie API Service**
```typescript
// services/movieApi.ts
import { MovieComparison, MovieDetail } from '../types/Movie';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://127.0.0.1:5091';

class MovieApiService {
  async getMovies(): Promise<MovieComparison[]> {
    const response = await fetch(`${API_BASE_URL}/api/movies`);
    if (!response.ok) {
      throw new Error('Failed to fetch movies');
    }
    return response.json();
  }

  async getMovieDetail(id: number): Promise<MovieDetail> {
    const response = await fetch(`${API_BASE_URL}/api/movies/${id}`);
    if (!response.ok) {
      throw new Error('Failed to fetch movie detail');
    }
    return response.json();
  }

  async refreshMovieData(): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/api/refresh`, {
      method: 'POST',
    });
    if (!response.ok) {
      throw new Error('Failed to refresh movie data');
    }
  }
}

export const movieApi = new MovieApiService();
```

### **TypeScript Types**
```typescript
// types/Movie.ts
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
```

## 🎨 Styling & Design

### **CSS Modules**
```css
/* MovieCard.module.css */
.movieCard {
  background: var(--card-background);
  border-radius: var(--border-radius-lg);
  box-shadow: var(--shadow-md);
  transition: all 0.3s ease;
  cursor: pointer;
  overflow: hidden;
}

.movieCard:hover {
  transform: translateY(-4px);
  box-shadow: var(--shadow-lg);
}

.movieInfo {
  padding: var(--spacing-md);
}

.title {
  font-size: var(--font-size-lg);
  font-weight: var(--font-weight-bold);
  color: var(--text-primary);
  margin-bottom: var(--spacing-sm);
}

.priceComparison {
  display: flex;
  justify-content: space-between;
  margin: var(--spacing-md) 0;
}

.bestDeal {
  background: var(--success-color);
  color: white;
  padding: var(--spacing-xs) var(--spacing-sm);
  border-radius: var(--border-radius-sm);
  font-weight: var(--font-weight-bold);
  font-size: var(--font-size-sm);
}
```

### **CSS Variables**
```css
/* styles/globals.css */
:root {
  /* Colors */
  --primary-color: #2563eb;
  --secondary-color: #64748b;
  --success-color: #10b981;
  --warning-color: #f59e0b;
  --error-color: #ef4444;
  
  /* Text Colors */
  --text-primary: #1f2937;
  --text-secondary: #6b7280;
  --text-muted: #9ca3af;
  
  /* Background Colors */
  --background-primary: #ffffff;
  --background-secondary: #f9fafb;
  --card-background: #ffffff;
  
  /* Spacing */
  --spacing-xs: 0.25rem;
  --spacing-sm: 0.5rem;
  --spacing-md: 1rem;
  --spacing-lg: 1.5rem;
  --spacing-xl: 2rem;
  
  /* Typography */
  --font-size-sm: 0.875rem;
  --font-size-base: 1rem;
  --font-size-lg: 1.125rem;
  --font-size-xl: 1.25rem;
  --font-size-2xl: 1.5rem;
  
  --font-weight-normal: 400;
  --font-weight-medium: 500;
  --font-weight-bold: 700;
  
  /* Borders */
  --border-radius-sm: 0.25rem;
  --border-radius-md: 0.375rem;
  --border-radius-lg: 0.5rem;
  
  /* Shadows */
  --shadow-sm: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
  --shadow-md: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  --shadow-lg: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
}
```

## 📱 Responsive Design

### **Breakpoints**
```css
/* Mobile First Approach */
.movieGrid {
  display: grid;
  grid-template-columns: 1fr;
  gap: var(--spacing-md);
  padding: var(--spacing-md);
}

/* Tablet */
@media (min-width: 768px) {
  .movieGrid {
    grid-template-columns: repeat(2, 1fr);
    gap: var(--spacing-lg);
    padding: var(--spacing-lg);
  }
}

/* Desktop */
@media (min-width: 1024px) {
  .movieGrid {
    grid-template-columns: repeat(3, 1fr);
  }
}

/* Large Desktop */
@media (min-width: 1280px) {
  .movieGrid {
    grid-template-columns: repeat(4, 1fr);
  }
}
```

## 🔧 Configuration

### **Environment Variables**
```bash
# .env.local
REACT_APP_API_URL=http://localhost:5091
REACT_APP_APP_NAME=Movie Price Comparison
REACT_APP_VERSION=1.0.0
REACT_APP_ENABLE_ANALYTICS=false
```

### **TypeScript Configuration**
```json
{
  "compilerOptions": {
    "target": "es5",
    "lib": ["dom", "dom.iterable", "es6"],
    "allowJs": true,
    "skipLibCheck": true,
    "esModuleInterop": true,
    "allowSyntheticDefaultImports": true,
    "strict": true,
    "forceConsistentCasingInFileNames": true,
    "noFallthroughCasesInSwitch": true,
    "module": "esnext",
    "moduleResolution": "node",
    "resolveJsonModule": true,
    "isolatedModules": true,
    "noEmit": true,
    "jsx": "react-jsx"
  },
  "include": ["src"],
  "exclude": ["node_modules"]
}
```

## 🧪 Testing

### **Testing Strategy**
```typescript
// __tests__/MovieCard.test.tsx
import { render, screen, fireEvent } from '@testing-library/react';
import { MovieCard } from '../components/MovieCard/MovieCard';
import { mockMovie } from '../__mocks__/movieData';

describe('MovieCard', () => {
  it('renders movie information correctly', () => {
    render(<MovieCard movie={mockMovie} />);
    
    expect(screen.getByText(mockMovie.title)).toBeInTheDocument();
    expect(screen.getByText(mockMovie.year)).toBeInTheDocument();
    expect(screen.getByText(mockMovie.genre)).toBeInTheDocument();
  });
  
  it('displays cheapest price badge when available', () => {
    render(<MovieCard movie={mockMovie} />);
    
    const bestDealBadge = screen.getByText(/best deal/i);
    expect(bestDealBadge).toBeInTheDocument();
  });
  
  it('calls onMovieClick when card is clicked', () => {
    const onMovieClick = jest.fn();
    render(<MovieCard movie={mockMovie} onMovieClick={onMovieClick} />);
    
    fireEvent.click(screen.getByRole('button'));
    expect(onMovieClick).toHaveBeenCalledWith(mockMovie);
  });
});
```

### **Running Tests**
```bash
# Run all tests
npm test

# Run tests in watch mode
npm test -- --watch

# Run tests with coverage
npm test -- --coverage

# Run specific test file
npm test MovieCard.test.tsx
```

## 🚀 Development

### **Getting Started**
```bash
# Install dependencies
npm install

# Start development server
npm start

# Build for production
npm run build

# Run tests
npm test

# Lint code
npm run lint

# Format code
npm run format
```

### **Development Server**
- **URL**: http://localhost:3000
- **Hot Reload**: Enabled
- **Source Maps**: Enabled for debugging
- **Error Overlay**: Shows compilation errors in browser

### **Build Process**
```bash
# Production build
npm run build

# Analyze bundle size
npm run analyze

# Serve production build locally
npm run serve
```

## 📦 Deployment

### **Docker**
```dockerfile
# Multi-stage build for optimized production image
FROM node:18-alpine as build
WORKDIR /app
COPY package*.json ./
RUN npm ci --only=production
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/build /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

### **Environment-Specific Builds**
```bash
# Development
npm run build:dev

# Staging
npm run build:staging

# Production
npm run build:prod
```

## 🔍 Performance Optimization

### **Code Splitting**
```typescript
// Lazy loading components
const MovieDetail = lazy(() => import('./components/MovieDetail/MovieDetail'));

// Route-based code splitting
const App = () => (
  <Router>
    <Suspense fallback={<LoadingSpinner />}>
      <Routes>
        <Route path="/" element={<MovieList />} />
        <Route path="/movie/:id" element={<MovieDetail />} />
      </Routes>
    </Suspense>
  </Router>
);
```

### **Image Optimization**
```typescript
// Optimized image component with lazy loading
const MovieImage: React.FC<MovieImageProps> = ({ src, alt, fallback }) => {
  const [imageSrc, setImageSrc] = useState(fallback);
  const [isLoading, setIsLoading] = useState(true);
  
  useEffect(() => {
    const img = new Image();
    img.onload = () => {
      setImageSrc(src);
      setIsLoading(false);
    };
    img.onerror = () => {
      setImageSrc(fallback);
      setIsLoading(false);
    };
    img.src = src;
  }, [src, fallback]);
  
  return (
    <div className={styles.imageContainer}>
      {isLoading && <div className={styles.imagePlaceholder} />}
      <img 
        src={imageSrc} 
        alt={alt}
        loading="lazy"
        className={styles.movieImage}
      />
    </div>
  );
};
```

## 🛡️ Error Handling

### **Error Boundary**
```typescript
class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false, error: null };
  }
  
  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error };
  }
  
  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('Error caught by boundary:', error, errorInfo);
    // Log to error reporting service
  }
  
  render() {
    if (this.state.hasError) {
      return (
        <div className={styles.errorContainer}>
          <h2>Something went wrong</h2>
          <p>We're sorry, but something unexpected happened.</p>
          <button onClick={() => window.location.reload()}>
            Reload Page
          </button>
        </div>
      );
    }
    
    return this.props.children;
  }
}
```

## 🎯 Features Implemented

✅ **Simple React Architecture** with hooks and functional components  
✅ **TypeScript Integration** for type safety and better developer experience  
✅ **Basic Responsive Design** with CSS grid layout  
✅ **Price Comparison** with visual highlighting of best deals  
✅ **Error Handling** with basic error states and loading indicators  
✅ **Image Handling** with fallback placeholders for missing posters  
✅ **Manual Data Refresh** functionality  
✅ **Movie Details** displayed via alert dialogs  
✅ **Clean API Integration** with the backend using fetch  
✅ **Comprehensive Testing** with Jest and React Testing Library  

This frontend provides a functional, clean interface for comparing movie prices with modern React patterns and TypeScript safety.

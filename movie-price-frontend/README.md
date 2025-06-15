# Movie Price Comparison Frontend

A modern React application built with TypeScript that provides a beautiful, responsive interface for comparing movie prices from multiple providers. The frontend communicates with a Clean Architecture backend API to deliver real-time price comparisons.

## 🎬 Features

- **🔍 Movie Search**: Search movies by title, genre, director, or actors
- **💰 Price Comparison**: View prices from Cinemaworld and Filmworld side-by-side
- **🏆 Best Deal Highlighting**: Automatically highlights the cheapest price
- **📱 Responsive Design**: Works seamlessly on desktop, tablet, and mobile
- **🖼️ Enhanced Images**: Optimized image loading with fallbacks and placeholders
- **⚡ Real-time Updates**: Live price data with intelligent caching
- **🎨 Modern UI**: Clean, professional interface with smooth animations
- **♿ Accessibility**: WCAG compliant with keyboard navigation support

## 🏗️ Architecture

### **Technology Stack**
- **React 18**: Latest React with concurrent features
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
│   │   ├── MovieCard/        # Movie display component
│   │   │   ├── MovieCard.tsx
│   │   │   └── MovieCard.module.css
│   │   ├── MovieList/        # Movie list container
│   │   │   ├── MovieList.tsx
│   │   │   └── MovieList.module.css
│   │   ├── SearchBar/        # Search functionality
│   │   │   ├── SearchBar.tsx
│   │   │   └── SearchBar.module.css
│   │   ├── LoadingSpinner/   # Loading states
│   │   │   ├── LoadingSpinner.tsx
│   │   │   └── LoadingSpinner.module.css
│   │   └── ErrorBoundary/    # Error handling
│   │       ├── ErrorBoundary.tsx
│   │       └── ErrorBoundary.module.css
│   ├── services/             # API services
│   │   ├── api.ts           # API client configuration
│   │   └── movieService.ts  # Movie-specific API calls
│   ├── types/               # TypeScript definitions
│   │   ├── Movie.ts         # Movie type definitions
│   │   └── ApiResponse.ts   # API response types
│   ├── hooks/               # Custom React hooks
│   │   ├── useMovies.ts     # Movie data management
│   │   ├── useSearch.ts     # Search functionality
│   │   └── useDebounce.ts   # Debounced search
│   ├── utils/               # Utility functions
│   │   ├── formatters.ts    # Data formatting utilities
│   │   └── constants.ts     # App constants
│   ├── styles/              # Global styles
│   │   ├── globals.css      # Global CSS variables
│   │   └── components.css   # Shared component styles
│   ├── App.tsx              # Main application component
│   ├── App.css              # App-level styles
│   ├── index.tsx            # Application entry point
│   └── index.css            # Base styles
├── package.json             # Dependencies and scripts
├── tsconfig.json           # TypeScript configuration
├── .env.example            # Environment variables template
└── Dockerfile              # Container configuration
```

## 🎯 Component Architecture

### **Component Hierarchy**
```
App
├── ErrorBoundary
├── Header
├── SearchBar
├── LoadingSpinner (conditional)
├── MovieList
│   └── MovieCard (multiple)
│       ├── MovieImage
│       ├── MovieInfo
│       ├── PriceComparison
│       └── BestDealBadge
└── Footer
```

### **Key Components**

#### **MovieCard Component**
```tsx
interface MovieCardProps {
  movie: Movie;
  onMovieClick?: (movie: Movie) => void;
}

const MovieCard: React.FC<MovieCardProps> = ({ movie, onMovieClick }) => {
  const cheapestPrice = movie.cheapestPrice;
  
  return (
    <div className={styles.movieCard} onClick={() => onMovieClick?.(movie)}>
      <MovieImage 
        src={movie.poster} 
        alt={movie.title}
        fallback="/images/movie-placeholder.jpg"
      />
      <div className={styles.movieInfo}>
        <h3 className={styles.title}>{movie.title}</h3>
        <p className={styles.year}>{movie.year}</p>
        <p className={styles.genre}>{movie.genre}</p>
        
        <PriceComparison prices={movie.prices} />
        
        {cheapestPrice && (
          <BestDealBadge 
            provider={cheapestPrice.provider}
            price={cheapestPrice.price}
          />
        )}
      </div>
    </div>
  );
};
```

#### **SearchBar Component**
```tsx
const SearchBar: React.FC = () => {
  const [query, setQuery] = useState('');
  const debouncedQuery = useDebounce(query, 300);
  const { searchMovies, isSearching } = useSearch();
  
  useEffect(() => {
    if (debouncedQuery) {
      searchMovies(debouncedQuery);
    }
  }, [debouncedQuery, searchMovies]);
  
  return (
    <div className={styles.searchContainer}>
      <input
        type="text"
        placeholder="Search movies by title, genre, director, or actors..."
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        className={styles.searchInput}
      />
      {isSearching && <LoadingSpinner size="small" />}
    </div>
  );
};
```

## 🔗 API Integration

### **Movie Service**
```typescript
// services/movieService.ts
export class MovieService {
  private api: AxiosInstance;
  
  constructor() {
    this.api = axios.create({
      baseURL: process.env.REACT_APP_API_URL || 'http://localhost:5091',
      timeout: 10000,
      headers: {
        'Content-Type': 'application/json',
      },
    });
    
    this.setupInterceptors();
  }
  
  async getMovies(): Promise<Movie[]> {
    try {
      const response = await this.api.get<Movie[]>('/api/movies');
      return response.data;
    } catch (error) {
      throw new ApiError('Failed to fetch movies', error);
    }
  }
  
  async getMovieById(id: number): Promise<MovieDetail> {
    try {
      const response = await this.api.get<MovieDetail>(`/api/movies/${id}`);
      return response.data;
    } catch (error) {
      throw new ApiError(`Failed to fetch movie ${id}`, error);
    }
  }
  
  async searchMovies(query: string): Promise<Movie[]> {
    try {
      const response = await this.api.get<Movie[]>('/api/movies/search', {
        params: { query }
      });
      return response.data;
    } catch (error) {
      throw new ApiError('Failed to search movies', error);
    }
  }
}
```

### **Custom Hooks**

#### **useMovies Hook**
```typescript
// hooks/useMovies.ts
export const useMovies = () => {
  const [movies, setMovies] = useState<Movie[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  const movieService = useMemo(() => new MovieService(), []);
  
  const fetchMovies = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await movieService.getMovies();
      setMovies(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch movies');
    } finally {
      setLoading(false);
    }
  }, [movieService]);
  
  useEffect(() => {
    fetchMovies();
  }, [fetchMovies]);
  
  return {
    movies,
    loading,
    error,
    refetch: fetchMovies
  };
};
```

#### **useDebounce Hook**
```typescript
// hooks/useDebounce.ts
export const useDebounce = <T>(value: T, delay: number): T => {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);
  
  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);
    
    return () => {
      clearTimeout(handler);
    };
  }, [value, delay]);
  
  return debouncedValue;
};
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

✅ **Modern React Architecture** with hooks and functional components  
✅ **TypeScript Integration** for type safety and better developer experience  
✅ **Responsive Design** that works on all device sizes  
✅ **Real-time Search** with debounced input for better performance  
✅ **Price Comparison** with visual highlighting of best deals  
✅ **Error Handling** with graceful fallbacks and user feedback  
✅ **Performance Optimization** with lazy loading and code splitting  
✅ **Accessibility** with WCAG compliance and keyboard navigation  
✅ **Professional UI** with modern design patterns and animations  
✅ **Clean Architecture** integration with the backend API  

This frontend provides a polished, professional user experience for comparing movie prices with modern React best practices and Clean Architecture principles.

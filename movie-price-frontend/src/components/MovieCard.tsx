import React, { useState } from 'react';
import { MovieComparison } from '../types/Movie';
import './MovieCard.css';

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
    <button
      className="movie-card"
      onClick={() => onClick(movie)}
      aria-label={movie.title}
      type="button"
    >
      <div className="movie-poster">
        {renderPosterContent()}
      </div>

      <div className="movie-info">
        <h3 className="movie-title">{movie.title}</h3>
        <p className="movie-year">{movie.year}</p>
        <p className="movie-genre">{movie.genre}</p>
        {movie.rating && <p className="movie-rating">⭐ {movie.rating}</p>}

        <div className="price-section">
          {movie.cheapestPrice && (
            <div className="best-price">
              <span className="best-price-label">Best Price:</span>
              <span className="best-price-value">
                {formatPrice(movie.cheapestPrice.price)}
              </span>
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

export default MovieCard;

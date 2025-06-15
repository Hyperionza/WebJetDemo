# Dynamic API Provider Configuration

This document explains the implementation of dynamic API provider configuration in the Movie Price Comparison application.

## Overview

The application now supports dynamic configuration of 3rd party API providers through a mock configuration service, replacing the previous hardcoded approach. This provides greater flexibility, easier maintenance, and the ability to add/remove/configure providers without code changes.

## Architecture

### Components

1. **ApiProvider Model** (`Models/ApiProvider.cs`)
   - Represents an API provider configuration
   - Contains endpoint URLs, authentication tokens, headers, timeouts, and status

2. **IApiProviderService** (`Services/IApiProviderService.cs`)
   - Interface for managing API provider configurations
   - Provides methods to get, refresh, and check provider status

3. **ApiProviderService** (`Services/ApiProviderService.cs`)
   - Implementation that fetches provider configurations from a service
   - Includes caching and fallback mechanisms

4. **MockConfigurationController** (`Controllers/MockConfigurationController.cs`)
   - Mock configuration service endpoint
   - Simulates a real configuration management service

5. **Updated ExternalApiService** (`Services/ExternalApiService.cs`)
   - Modified to use dynamic provider configurations
   - Maintains fallback support for legacy configurations

## Key Features

### 🔄 Dynamic Configuration
- API providers are fetched from a configuration service
- No hardcoded provider details in the application
- Real-time updates without application restart

### 💾 Intelligent Caching
- Provider configurations are cached for 15 minutes
- Reduces external service calls
- Automatic cache refresh on errors

### 🛡️ Fallback Mechanisms
- Falls back to hardcoded configuration if service is unavailable
- Graceful degradation ensures application continues working
- Multiple fallback layers for maximum reliability

### 🎛️ Provider Management
- Enable/disable providers dynamically
- Configure timeouts, priorities, and headers per provider
- Support for multiple authentication methods

### 📊 Monitoring & Logging
- Comprehensive logging for troubleshooting
- Health check endpoints for each provider
- Configuration service health monitoring

## Configuration Structure

### ApiProvider Model
```csharp
public class ApiProvider
{
    public string Id { get; set; }                    // Unique identifier
    public string Name { get; set; }                  // Internal name
    public string DisplayName { get; set; }           // User-friendly name
    public string BaseUrl { get; set; }               // API base URL
    public string ApiToken { get; set; }              // Authentication token
    public bool IsEnabled { get; set; }               // Enable/disable flag
    public int Priority { get; set; }                 // Provider priority
    public int TimeoutSeconds { get; set; }           // Request timeout
    public Dictionary<string, string> Headers { get; set; }  // Custom headers
    public ApiEndpoints Endpoints { get; set; }       // Endpoint paths
    public DateTime LastUpdated { get; set; }         // Last update timestamp
}
```

### Example Configuration Response
```json
{
  "providers": [
    {
      "id": "cinemaworld",
      "name": "cinemaworld",
      "displayName": "Cinemaworld",
      "baseUrl": "https://webjetapitest.azurewebsites.net/api/cinemaworld",
      "apiToken": "your-api-token",
      "isEnabled": true,
      "priority": 1,
      "timeoutSeconds": 30,
      "headers": {
        "x-access-token": "your-api-token",
        "Content-Type": "application/json",
        "User-Agent": "MoviePriceComparison/1.0"
      },
      "endpoints": {
        "movies": "/movies",
        "movieDetail": "/movie/{id}",
        "health": "/health"
      },
      "lastUpdated": "2023-12-01T10:00:00Z"
    }
  ],
  "lastUpdated": "2023-12-01T10:00:00Z",
  "version": "1.0"
}
```

## API Endpoints

### Configuration Service Endpoints

#### Get API Providers
```http
GET /api/MockConfiguration/api-providers
```
Returns the current list of API provider configurations.

#### Update Provider Status
```http
PATCH /api/MockConfiguration/api-providers/{providerId}/status
Content-Type: application/json

true  // or false to enable/disable
```

#### Add New Provider
```http
POST /api/MockConfiguration/api-providers
Content-Type: application/json

{
  "id": "newprovider",
  "name": "newprovider",
  "displayName": "New Provider",
  "baseUrl": "https://api.newprovider.com",
  "apiToken": "token",
  "isEnabled": true,
  "timeoutSeconds": 30
}
```

#### Configuration Service Health
```http
GET /api/MockConfiguration/health
```

### Application Management Endpoints

#### Get Current Providers
```http
GET /api/providers
```
Returns the currently loaded provider configurations (from cache or service).

#### Refresh Provider Cache
```http
POST /api/providers/refresh
```
Forces a refresh of the provider configuration cache.

## Usage Examples

### Adding a New Provider

1. **Call the configuration service:**
```bash
curl -X POST http://localhost:5091/api/MockConfiguration/api-providers \
  -H "Content-Type: application/json" \
  -d '{
    "id": "movieworld",
    "name": "movieworld", 
    "displayName": "MovieWorld",
    "baseUrl": "https://api.movieworld.com",
    "apiToken": "mw-token-123",
    "isEnabled": true,
    "priority": 3,
    "timeoutSeconds": 25,
    "headers": {
      "Authorization": "Bearer mw-token-123",
      "Content-Type": "application/json"
    },
    "endpoints": {
      "movies": "/api/v1/movies",
      "movieDetail": "/api/v1/movies/{id}",
      "health": "/api/v1/status"
    }
  }'
```

2. **Refresh the application cache:**
```bash
curl -X POST http://localhost:5091/api/providers/refresh
```

3. **The new provider is now available for movie price comparisons**

### Disabling a Provider

```bash
curl -X PATCH http://localhost:5091/api/MockConfiguration/api-providers/filmworld/status \
  -H "Content-Type: application/json" \
  -d 'false'
```

### Checking Current Providers

```bash
curl http://localhost:5091/api/providers
```

## Configuration

### appsettings.json
```json
{
  "ApiProviderService": {
    "ConfigurationServiceUrl": "http://localhost:5091/api/MockConfiguration/api-providers",
    "CacheDurationMinutes": 15,
    "TimeoutSeconds": 30
  }
}
```

### Environment Variables
- `CINEMAWORLD_API_TOKEN`: Fallback token for Cinemaworld
- `FILMWORLD_API_TOKEN`: Fallback token for Filmworld

## Error Handling

### Service Unavailable
If the configuration service is unavailable, the application:
1. Logs a warning
2. Falls back to hardcoded provider configurations
3. Continues normal operation
4. Retries on next cache refresh

### Invalid Configuration
If invalid configuration is received:
1. Logs an error with details
2. Uses the last known good configuration
3. Falls back to defaults if no previous configuration exists

### Provider Failures
If an individual provider fails:
1. Logs the failure
2. Attempts fallback configuration for that provider
3. Continues with other providers
4. Returns partial results

## Benefits

### 🚀 **Operational Flexibility**
- Add new providers without code deployment
- Enable/disable providers for maintenance
- Update configurations in real-time

### 🔧 **Easier Maintenance**
- Centralized provider configuration
- No hardcoded values in application code
- Version-controlled configuration changes

### 📈 **Scalability**
- Support for unlimited number of providers
- Priority-based provider selection
- Load balancing through provider rotation

### 🛡️ **Reliability**
- Multiple fallback mechanisms
- Graceful degradation on failures
- Comprehensive error handling

### 🔍 **Observability**
- Detailed logging for troubleshooting
- Health monitoring for all providers
- Configuration change tracking

## Future Enhancements

### Planned Features
1. **Database-backed Configuration**: Store configurations in database instead of mock service
2. **Provider Load Balancing**: Distribute requests across multiple providers
3. **Circuit Breaker Pattern**: Automatically disable failing providers
4. **Configuration Versioning**: Track and rollback configuration changes
5. **A/B Testing**: Test new providers with subset of traffic
6. **Rate Limiting**: Per-provider rate limiting configuration
7. **Metrics Collection**: Detailed performance metrics per provider

### Integration Possibilities
1. **Azure App Configuration**: Use Azure's configuration service
2. **Consul**: Integrate with HashiCorp Consul for configuration
3. **Kubernetes ConfigMaps**: Use K8s native configuration
4. **External APIs**: Integrate with third-party configuration services

## Testing

### Unit Tests
The implementation includes comprehensive unit tests for:
- ApiProviderService caching behavior
- Fallback mechanisms
- Error handling scenarios
- Configuration parsing

### Integration Tests
- End-to-end provider configuration flow
- Mock configuration service integration
- Cache refresh scenarios

### Load Tests
- Performance under high load
- Cache efficiency testing
- Concurrent access scenarios

## Security Considerations

### Token Management
- API tokens are stored securely
- Environment variable fallbacks
- Azure Key Vault integration for production

### Configuration Validation
- Input validation on all configuration endpoints
- Schema validation for provider configurations
- Sanitization of user inputs

### Access Control
- Configuration endpoints should be secured in production
- Role-based access for provider management
- Audit logging for configuration changes

## Monitoring

### Key Metrics
- Provider response times
- Success/failure rates per provider
- Configuration service availability
- Cache hit/miss ratios

### Alerts
- Provider failures
- Configuration service downtime
- Unusual response times
- Cache refresh failures

### Dashboards
- Real-time provider status
- Performance metrics
- Configuration change history
- Error rate trends

This dynamic API provider system provides a robust, flexible foundation for managing third-party integrations while maintaining high availability and operational simplicity.

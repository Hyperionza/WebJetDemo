# Development Container Setup

This directory contains the configuration for a complete development environment using VS Code Dev Containers. The setup provides a consistent, reproducible development environment for the Movie Price Comparison application.

## 🚀 Quick Start

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [VS Code](https://code.visualstudio.com/)
- [Dev Containers Extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)

### Getting Started
1. Clone the repository
2. Open VS Code in the project root
3. When prompted, click "Reopen in Container" or use `Ctrl+Shift+P` → "Dev Containers: Reopen in Container"
4. Wait for the container to build and setup to complete
5. Start developing!

## 🏗️ Container Architecture

### Services
- **app**: Main development container with .NET 9, Node.js 18, and development tools
- **db**: SQL Server 2022 Developer Edition for database operations

### Included Tools
- **.NET 9 SDK**: For backend development
- **Node.js 18**: For frontend development
- **Entity Framework Core Tools**: For database migrations
- **SQL Server Tools**: For database management
- **Docker CLI**: For container operations
- **Azure CLI**: For cloud deployments
- **GitHub CLI**: For repository operations
- **PowerShell**: For cross-platform scripting

### VS Code Extensions
The container automatically installs essential extensions:
- C# development tools
- TypeScript/React development tools
- Database management tools
- Docker tools
- Git tools
- Testing tools

## 🔧 Configuration Files

### `devcontainer.json`
Main configuration file that defines:
- Container settings and features
- VS Code extensions to install
- Port forwarding configuration
- Environment variables
- Post-creation and post-start commands

### `docker-compose.yml`
Defines the multi-container setup:
- Development container with mounted workspace
- SQL Server database container
- Network configuration
- Volume management

### `Dockerfile`
Custom container image with:
- Ubuntu base with development tools
- .NET 9 SDK installation
- Node.js 18 installation
- SQL Server command-line tools
- Development utilities

### Setup Scripts
- **`post-create.sh`**: Runs once when container is created
  - Restores .NET packages
  - Installs npm dependencies
  - Sets up database
  - Creates configuration files
  - Sets up Git hooks
  - Creates development aliases

- **`post-start.sh`**: Runs every time container starts
  - Checks SQL Server connectivity
  - Runs database migrations
  - Displays helpful information

## 🌐 Port Configuration

| Port | Service | Description |
|------|---------|-------------|
| 3000 | React Frontend | Development server |
| 7001 | .NET API (HTTPS) | Backend API |
| 5000 | .NET API (HTTP) | Backend API |
| 1433 | SQL Server | Database server |

## 📋 Development Commands

The container includes helpful aliases for common tasks:

### Application Commands
```bash
api          # Start the .NET API server
frontend     # Start the React development server
build-all    # Build both backend and frontend
```

### Testing Commands
```bash
test-api     # Run backend unit tests
test-frontend # Run frontend unit tests
```

### Database Commands
```bash
db-update    # Apply database migrations
db-reset     # Reset database (drop and recreate)
```

### Utility Commands
```bash
workspace    # Navigate to workspace root
ll           # List files with details
```

## 🗄️ Database Configuration

The container includes SQL Server 2022 Developer Edition with:
- **Server**: localhost,1433
- **Username**: sa
- **Password**: YourStrong@Passw0rd
- **Database**: MoviePriceDb (created automatically)

### Connection String
```
Server=localhost,1433;Database=MoviePriceDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;
```

## 🔒 Security Considerations

### Development Only
- The SQL Server password is hardcoded for development convenience
- TrustServerCertificate is enabled for local development
- These settings should NEVER be used in production

### Production Deployment
- Use Azure Key Vault or similar for secrets management
- Enable proper SSL certificate validation
- Use managed database services with proper authentication

## 🛠️ Customization

### Adding Extensions
Edit the `extensions` array in `devcontainer.json`:
```json
"extensions": [
    "existing.extension",
    "new.extension.id"
]
```

### Adding Tools
Modify the `Dockerfile` to install additional tools:
```dockerfile
RUN apt-get update && apt-get install -y your-tool
```

### Environment Variables
Add to the `containerEnv` section in `devcontainer.json`:
```json
"containerEnv": {
    "YOUR_VARIABLE": "value"
}
```

## 🐛 Troubleshooting

### Container Won't Start
1. Ensure Docker Desktop is running
2. Check Docker Desktop has sufficient resources allocated
3. Try rebuilding the container: `Ctrl+Shift+P` → "Dev Containers: Rebuild Container"

### SQL Server Connection Issues
1. Wait for the database to fully initialize (can take 30-60 seconds)
2. Check container logs for SQL Server startup messages
3. Try restarting the container

### Port Conflicts
1. Check if ports 3000, 5000, 7001, or 1433 are in use locally
2. Stop conflicting services or modify port configuration

### Performance Issues
1. Increase Docker Desktop memory allocation (recommended: 8GB+)
2. Enable WSL 2 backend on Windows
3. Consider using volume mounts instead of bind mounts for better performance

## 📚 Additional Resources

- [VS Code Dev Containers Documentation](https://code.visualstudio.com/docs/remote/containers)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [.NET in Docker](https://docs.microsoft.com/en-us/dotnet/core/docker/)
- [SQL Server in Docker](https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-docker-container-deployment)

## 🤝 Contributing to Dev Container

When modifying the dev container configuration:
1. Test changes thoroughly
2. Update this documentation
3. Consider backward compatibility
4. Test on different platforms (Windows, macOS, Linux)

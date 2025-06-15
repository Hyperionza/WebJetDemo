# CI/CD Pipeline Documentation

This repository uses GitHub Actions for continuous integration and deployment. The pipeline is designed to ensure code quality, run comprehensive tests, and deploy to multiple environments safely.

## Pipeline Overview

### 1. CI Pipeline (`ci.yml`)
**Triggers:**
- Push to `main` branch
- Pull requests to `main` branch
- Manual trigger (`workflow_dispatch`) from any branch

**Jobs:**
- **Backend Tests**: Runs .NET tests with code coverage
- **Frontend Tests**: Runs React tests with linting and code coverage
- **Backend Build**: Builds Docker image for the API
- **Frontend Build**: Builds React application for production
- **Security Scan**: Runs Trivy vulnerability scanner
- **Quality Gate**: Ensures all checks pass before allowing merge

### 2. Staging Deployment (`deploy-staging.yml`)
**Triggers:**
- Push to `staging` branch
- Manual trigger

**Process:**
- Builds both backend and frontend
- Deploys to Azure staging environment
- Runs smoke tests
- Sends Slack notifications

### 3. Production Deployment (`deploy-production.yml`)
**Triggers:**
- Push to `production` branch
- Manual trigger

**Process:**
- Blue-green deployment using Azure deployment slots
- Comprehensive health checks
- Automatic rollback on failure
- Slack notifications

## Branch Strategy

```
main ──────────────────────────────────────────
 │                                              
 ├── feature/branch ──┐                         
 │                    │                         
 └── staging ─────────┴─────────────────────────
     │                                          
     └── production ────────────────────────────
```

### Branch Purposes:
- **`main`**: Primary development branch, all PRs merge here
- **`staging`**: Deploys to staging environment for testing
- **`production`**: Deploys to production environment

## Environment Configuration

### Required Secrets:
```
AZURE_CREDENTIALS          # Azure service principal credentials
STAGING_SWA_TOKEN          # Azure Static Web Apps token for staging
PRODUCTION_SWA_TOKEN       # Azure Static Web Apps token for production
SLACK_WEBHOOK              # Slack webhook for notifications
```

### Required Variables:
```
# Staging Environment
STAGING_API_URL            # https://staging-api.example.com
STAGING_FRONTEND_URL       # https://staging.example.com
STAGING_BACKEND_APP_NAME   # Azure App Service name for staging

# Production Environment
PRODUCTION_API_URL         # https://api.example.com
PRODUCTION_FRONTEND_URL    # https://example.com
PRODUCTION_BACKEND_APP_NAME # Azure App Service name for production
AZURE_RESOURCE_GROUP       # Azure resource group name
```

## Deployment Process

### To Staging:
1. Merge changes to `staging` branch
2. Pipeline automatically builds and deploys
3. Smoke tests verify deployment
4. Team receives Slack notification

### To Production:
1. Merge `staging` to `production` branch
2. Pipeline creates deployment slot
3. Deploys to staging slot first
4. Runs comprehensive health checks
5. Swaps slots for zero-downtime deployment
6. Verifies production health
7. Cleans up staging slot

## Manual Pipeline Triggers

Developers can manually trigger pipelines from any branch:

1. Go to **Actions** tab in GitHub
2. Select the desired workflow
3. Click **Run workflow**
4. Choose the branch and environment

## Quality Gates

All code must pass these checks before deployment:

### Backend:
- ✅ Unit tests pass
- ✅ Code coverage meets threshold
- ✅ Docker image builds successfully
- ✅ Security scan passes

### Frontend:
- ✅ Unit tests pass
- ✅ Linting passes
- ✅ Build succeeds
- ✅ Code coverage meets threshold

## Monitoring and Notifications

### Slack Integration:
- Deployment status notifications
- Failure alerts with commit details
- Success confirmations with environment URLs

### Health Checks:
- API endpoint availability
- Frontend application loading
- Database connectivity (via health endpoint)

## Rollback Strategy

### Automatic Rollback:
- Production deployments automatically rollback on health check failure
- Staging slot is cleaned up on failure

### Manual Rollback:
1. Identify last known good commit
2. Create hotfix branch from that commit
3. Deploy through normal process
4. Or use Azure portal to swap slots back

## Security Considerations

- All secrets are stored in GitHub Secrets
- Azure credentials use service principal with minimal permissions
- Vulnerability scanning on every build
- Environment isolation between staging and production
- HTTPS enforced on all endpoints

## Troubleshooting

### Common Issues:

**Build Failures:**
- Check test results in Actions tab
- Verify dependencies are up to date
- Check for linting errors

**Deployment Failures:**
- Verify Azure credentials are valid
- Check resource group and app service names
- Ensure environment variables are set correctly

**Health Check Failures:**
- Check application logs in Azure
- Verify database connectivity
- Check external API dependencies

### Getting Help:
- Check the Actions tab for detailed logs
- Review Azure App Service logs
- Contact DevOps team via Slack #devops channel

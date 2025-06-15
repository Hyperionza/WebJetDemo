# CI/CD Pipeline Documentation

This repository uses GitHub Actions for continuous integration. The pipeline is designed to ensure code quality, run comprehensive tests.

Deployment was considered beyond the scope of this demo.

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

## Deployment Process (well, this is what it could be)

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

## Monitoring and Notifications (hypothetical, deployment pipelines not included)

### Slack Integration:
- Deployment status notifications
- Failure alerts with commit details
- Success confirmations with environment URLs

### Health Checks:
- API endpoint availability
- Frontend application loading

## Rollback Strategy

### Automatic Rollback ():
- Production deployments automatically rollback on health check failure
- Staging slot is cleaned up on failure

### Manual Rollback:
1. Identify last known good commit
2. Create hotfix branch from that commit
3. Deploy through normal process
4. Or use Cloud portal to roll back

## Security Considerations (hypothetical)

- All secrets are stored in GitHub Secrets 
- Cloud credentials use service principal with minimal permissions
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
- Verify Cloud credentials are valid
- Check resource group and app service names (Azure nomenclature used, but the principle applies to AWS and GCP too)
- Ensure environment variables are set correctly

**Health Check Failures:**
- Check application logs on cloud platform
- Check external API dependencies
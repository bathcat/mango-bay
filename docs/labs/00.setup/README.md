# Lab 00: Environment Setup

## Summary

Before exploring security vulnerabilities, you need a working development environment with both the API and web application running. This lab walks you through getting Mango Bay Cargo up and running using your preferred development approach.

## Requirements

1. Choose your setup method based on available tools:

   - **GitHub Codespaces**: Zero installation, runs in browser (requires GitHub account)
   - **Docker + VS Code**: Consistent containerized environment (requires Docker Desktop)
   - **Local Installation**: Direct installation on your machine (requires .NET 9, Node 18+, Git)

2. Complete the setup process for your chosen method

   - Follow the detailed instructions in the root `SETUP.md` file
   - For local installations: run `npm install` in `.\projects\web`
   - Be patient on first run: containers download dependencies, database seeding takes time

3. Verify both services are running
   - API should be accessible at http://localhost:5110
   - Web app should be accessible at http://localhost:4200

## Acceptance Criteria

- Navigate to http://localhost:4200 and see the Mango Bay Cargo home page
- Create a new user account through the web interface
- Successfully log in with your new credentials
- Browse the pilot list or other pages without errors

## Tips

- **First run is slow**: Container builds and npm installs can take 5-15 minutes. This is normal.
- **Ports already in use**: If you see port conflicts, check for other services running on 5110 or 4200
- **Container setup recommended**: Unless you already have .NET 9 and Node 18+ installed, using Codespaces or Docker will save time


## Learning Resources

1. [Complete Setup Guide](../../../SETUP.md) - Detailed instructions for all three setup options
2. [Dev Containers Documentation](https://code.visualstudio.com/docs/devcontainers/containers) - Understanding VS Code + Docker workflow
3. [GitHub Codespaces Docs](https://docs.github.com/en/codespaces) - Browser-based development environment

FROM mcr.microsoft.com/devcontainers/base:ubuntu

# Install .NET 9 SDK
RUN wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && rm packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y dotnet-sdk-9.0 || \
    (echo "Trying alternative installation method..." && \
    curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0 --install-dir /usr/share/dotnet && \
    ln -s /usr/share/dotnet/dotnet /usr/local/bin/dotnet)

# Install Node.js 22
RUN curl -fsSL https://deb.nodesource.com/setup_22.x | bash - \
    && apt-get install -y nodejs \
    && npm install -g npm@latest

# Install additional tools
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends \
    git \
    curl \
    wget \
    unzip \
    vim \
    nano \
    jq \
    tree \
    htop \
    ca-certificates \
    gnupg \
    lsb-release

# Install Docker CLI (for building containers)
RUN mkdir -p /etc/apt/keyrings \
    && curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg \
    && echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null \
    && apt-get update \
    && apt-get install -y docker-ce-cli

# Install Entity Framework Core tools
RUN dotnet tool install --global dotnet-ef

# Clean up
RUN apt-get autoremove -y && apt-get clean -y && rm -rf /var/lib/apt/lists/*

# Configure non-root user (vscode user already exists in base image)
ARG USERNAME=vscode
ARG USER_UID=1000
ARG USER_GID=$USER_UID

# Update package list and install sudo if not already installed
RUN apt-get update && apt-get install -y sudo \
    && apt-get clean && rm -rf /var/lib/apt/lists/*

# Ensure vscode user has sudo privileges (may already be configured)
RUN echo $USERNAME ALL=\(root\) NOPASSWD:ALL > /etc/sudoers.d/$USERNAME \
    && chmod 0440 /etc/sudoers.d/$USERNAME

# Set up workspace with proper permissions
WORKDIR /workspace
RUN chown -R $USERNAME:$USERNAME /workspace

# Switch to non-root user
USER $USERNAME

# Add dotnet tools to PATH for the user
ENV PATH="$PATH:/home/$USERNAME/.dotnet/tools"

# Create a script to fix permissions on startup
RUN echo '#!/bin/bash\n\
    # Fix workspace permissions\n\
    sudo chown -R vscode:vscode /workspace\n\
    # Clean any existing build artifacts that might have wrong permissions\n\
    find /workspace -name "bin" -type d -exec sudo rm -rf {} + 2>/dev/null || true\n\
    find /workspace -name "obj" -type d -exec sudo rm -rf {} + 2>/dev/null || true\n\
    echo "Workspace permissions fixed"\n\
    ' > /home/$USERNAME/fix-permissions.sh && chmod +x /home/$USERNAME/fix-permissions.sh

FROM mcr.microsoft.com/devcontainers/dotnet:1-9.0-bookworm

COPY installSQLtools.sh installSQLtools.sh
RUN bash ./installSQLtools.sh \
     && apt-get clean -y && rm -rf /var/lib/apt/lists/* /tmp/library-scripts


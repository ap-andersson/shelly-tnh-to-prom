# Build on the build machine's own architecture and cross-publish for the
# target, so multi-arch builds need no emulation.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
WORKDIR /src

COPY ShellyTnhToProm.csproj .
RUN dotnet restore ShellyTnhToProm.csproj -a $TARGETARCH

COPY . .
RUN dotnet publish ShellyTnhToProm.csproj -c Release -a $TARGETARCH --no-restore -o /app /p:UseAppHost=false

# Chiseled: minimal Ubuntu image with just the .NET runtime, runs as non-root.
FROM mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled
WORKDIR /app
COPY --from=build /app .

ARG APP_VERSION=dev
ENV APP_VERSION=${APP_VERSION}

EXPOSE 1234
ENTRYPOINT ["dotnet", "ShellyTnhToProm.dll"]

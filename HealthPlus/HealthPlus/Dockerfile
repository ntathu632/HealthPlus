# Build context is the solution folder (HealthPlus/HealthPlus/), so this Dockerfile
# can see and restore all sibling projects. Run from there:
#   docker build -f HealthPlus/Dockerfile -t healthplus-api .
# (docker-compose.yml at the repo root already sets this up correctly.)

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the .csproj files first so `dotnet restore` is cached across builds
# and doesn't re-run just because a .cs file changed.
COPY HealthPlus/HealthPlus.csproj HealthPlus/
COPY HealthPlus.Domain/HealthPlus.Domain.csproj HealthPlus.Domain/
COPY HealthPlus.Application/HealthPlus.Application.csproj HealthPlus.Application/
COPY HealthPlus.Persistence/HealthPlus.Persistence.csproj HealthPlus.Persistence/
COPY HealthPlus.Infrastructure/HealthPlus.Infrastructure.csproj HealthPlus.Infrastructure/

RUN dotnet restore HealthPlus/HealthPlus.csproj

COPY HealthPlus/ HealthPlus/
COPY HealthPlus.Domain/ HealthPlus.Domain/
COPY HealthPlus.Application/ HealthPlus.Application/
COPY HealthPlus.Persistence/ HealthPlus.Persistence/
COPY HealthPlus.Infrastructure/ HealthPlus.Infrastructure/

# RuntimeIdentifier is intentionally left unset here — the win-x64 override in
# HealthPlus.csproj only applies when $(OS)==Windows_NT (the dev machine), not
# inside this Linux build container. Publish is framework-dependent linux-x64,
# matching the aspnet:8.0 runtime image below.
RUN dotnet publish HealthPlus/HealthPlus.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Populated by a volume mount in docker-compose so files survive container recreation.
RUN mkdir -p /app/uploads /app/logs

EXPOSE 8080
ENTRYPOINT ["dotnet", "HealthPlus.dll"]

# NOTE — OCR (Tesseract) does not work in this image: the Tesseract NuGet package only
# ships Windows x86/x64 native binaries, and this is a Linux container. TesseractOcrService
# already catches that failure and degrades gracefully (empty OCR result, no crash) — same
# code path as "not configured". Everything else in the app is unaffected.

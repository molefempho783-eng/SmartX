# Multi-stage Docker build for .NET 10 API (Docker Docs, 2026).


#  STAGE 1: BUILD
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj first so Docker can cache the restore layer
# (Docker Docs, 2026).
COPY ["SmartX.Api.csproj", "./"]
RUN dotnet restore "SmartX.Api.csproj"

# Copy remaining source and publish in Release mode (Microsoft Docs, 2026).
COPY . .
RUN dotnet publish "SmartX.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

#  STAGE 2: RUNTIME 
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy published output from build stage (Docker Docs, 2026).
COPY --from=build /app/publish .

# Create the uploads directory and hand ownership to the non-root account
# before dropping privileges (OWASP, 2021).
RUN mkdir -p /app/Uploads && chown -R $APP_UID /app

# The .NET runtime images already ship a non-root account, exposed as the
# $APP_UID environment variable, so no user needs to be created here - the
# image has no adduser binary (Microsoft Docs, 2026; OWASP, 2021).
USER $APP_UID

# Expose the standard ASP.NET Core port (Microsoft Docs, 2026).
EXPOSE 8080

# Set environment so Kestrel listens on 8080 (Microsoft Docs, 2026).
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "SmartX.Api.dll"]

# Reference List
# Docker Docs, 2026. Multi-stage builds. [online] Available at: <https://docs.docker.com/build/building/multi-stage/> [Accessed 13 September 2026].
# Microsoft Docs, 2026. Containerize a .NET app. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/core/docker/build-container> [Accessed 13 September 2026].
# OWASP, 2021. Docker security cheatsheet. [online] Available at: <https://cheatsheetseries.owasp.org/cheatsheets/Docker_Security_Cheat_Sheet.html> [Accessed 13 September 2026].
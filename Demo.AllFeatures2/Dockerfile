FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

COPY --from=build-env /app/out .

# Configure Kestral Envrionment
ENV ASPNETCORE_URLS http://+:5002
EXPOSE 5002

ENTRYPOINT ["dotnet", "Demo.AllFeatures2.dll"]

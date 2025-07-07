# Use the official .NET 6 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Set the working directory
WORKDIR /app

# Copy the project file and restore dependencies
COPY PHIRedactionAPI.csproj .
RUN dotnet restore

# Copy the source code
COPY . .

# Build the application
RUN dotnet publish -c Release -o out

# Use the official .NET 6 runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime

# Set the working directory
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/out .

# Create directory for processed files
RUN mkdir -p ProcessedFiles

# Expose the port the app runs on
EXPOSE 80
EXPOSE 443

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "PHIRedactionAPI.dll"]

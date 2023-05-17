# syntax=docker/dockerfile:1

# Use the official dotnet sdk version 7.0 as base image for building the app
FROM mcr.microsoft.com/dotnet/sdk:7.0 as build-env

# Set workdir to /RareDiseasePredictor/RareDiseasePredictor
WORKDIR /RareDiseasePredicter/RareDiseasePredicter

# Copies all files that ends in .csproj over to workdir
COPY RareDiseasePredicter/RareDiseasePredicter/*.csproj .

# Runs dotnet restore to restore the dependencies and tools required
RUN dotnet restore

# Copies the remaining files to the workdir
COPY RareDiseasePredicter/RareDiseasePredicter .

# Builds the production version of the backend
RUN dotnet publish -c Release -o /publish



# Use the official dotnet aspnet for serving the backend
FROM mcr.microsoft.com/dotnet/aspnet:7.0 as runtime

# Sets workdir to /publish
WORKDIR /publish

# Copies the dotnet build files from the previous stage to the publish folder
COPY --from=build-env /publish .

# Copies the preset database data provided to the backend
COPY RareDiseasePredicter/RareDiseasePredicter/Database.db .

# Expose port 57693 for API calls
EXPOSE 57693

# Start the aspnet service
ENTRYPOINT ["dotnet", "RareDiseasePredicter.dll", "--urls", "http://*:57693"]
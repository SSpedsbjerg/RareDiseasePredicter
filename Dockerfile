# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:7.0 as build-env
WORKDIR /RareDiseasePredicter/RareDiseasePredicter
COPY RareDiseasePredicter/RareDiseasePredicter/*.csproj .
RUN dotnet restore
COPY RareDiseasePredicter/RareDiseasePredicter .
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0 as runtime
WORKDIR /publish
COPY --from=build-env /publish .
EXPOSE 57693
ENTRYPOINT ["dotnet", "RareDiseasePredicter.dll", "--urls", "http://*:57693"]
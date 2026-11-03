FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY MedicalServices.Domain/MedicalServices.Domain.csproj MedicalServices.Domain/
COPY MedicalServices.Infrastructure/MedicalServices.Infrastructure.csproj MedicalServices.Infrastructure/
COPY MedicalServices.Web/MedicalServices.Web.csproj MedicalServices.Web/
RUN dotnet restore MedicalServices.Web/MedicalServices.Web.csproj
COPY MedicalServices.Domain/ MedicalServices.Domain/
COPY MedicalServices.Infrastructure/ MedicalServices.Infrastructure/
COPY MedicalServices.Web/ MedicalServices.Web/
RUN dotnet publish MedicalServices.Web/MedicalServices.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MedicalServices.Web.dll"]

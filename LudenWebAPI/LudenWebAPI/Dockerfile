FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["LudenWebAPI/LudenWebAPI.csproj", "LudenWebAPI/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Entities/Entities.csproj", "Entities/"]

RUN dotnet restore "LudenWebAPI/LudenWebAPI.csproj"

COPY . .
WORKDIR "/src/LudenWebAPI"
RUN dotnet publish "LudenWebAPI.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "LudenWebAPI.dll"]

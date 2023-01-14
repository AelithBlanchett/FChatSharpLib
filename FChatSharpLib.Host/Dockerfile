#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["FChatSharpLib.Host/FChatSharp.Host.csproj", "FChatSharpLib.Host/"]
COPY ["FChatSharpLib/FChatSharpLib.csproj", "FChatSharpLib/"]
RUN dotnet restore "FChatSharpLib.Host/FChatSharp.Host.csproj"
COPY . .
WORKDIR "/src/FChatSharpLib.Host"
RUN dotnet build "FChatSharp.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FChatSharp.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FChatSharp.Host.dll"]
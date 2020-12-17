#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM dotnet-sdk-3-1-cryptopro-4r4 AS base
WORKDIR /app

FROM dotnet-sdk-3-1-cryptopro-4r4 AS build
WORKDIR /src
COPY ["Smev3Client.Test/Smev3Client.Test.csproj", "Smev3Client.Test/"]
RUN dotnet restore "Smev3Client.Test/Smev3Client.Test.csproj"
COPY . .
WORKDIR "/src/Smev3Client.Test"
RUN dotnet build "Smev3Client.Test.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Smev3Client.Test.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Smev3Client.Test.dll"]
FROM microsoft/aspnetcore-build:2.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore MovieVotingCoreMySQL.csproj

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out MovieVotingCoreMySQL.csproj

# Build runtime image
FROM microsoft/aspnetcore:2.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "MovieVotingCoreMySQL.dll"]
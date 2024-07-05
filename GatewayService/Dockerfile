FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
WORKDIR /app
EXPOSE 80
COPY Carsties.sln /Carsties/Carsties.sln
COPY AuctionService/AuctionService.csproj /Carsties/AuctionService/AuctionService.csproj
COPY SearchService/SearchService.csproj /Carsties/SearchService/SearchService.csproj
COPY GatewayService/GatewayService.csproj /Carsties/GatewayService/GatewayService.csproj
COPY Contracts/Contracts.csproj /Carsties/Contracts/Contracts.csproj
COPY IdentityService/IdentityService.csproj /Carsties/IdentityService/IdentityService.csproj
# Step1: All nuget packages and other depencies are restored by looking at the proj files
RUN dotnet restore /Carsties/Carsties.sln

# Step2: copy other files, folders from the project
COPY GatewayService /Carsties/GatewayService

# Step3: time to publish/run your app, so for easy access set your current working directory on the
# docker file system to, to your project location
WORKDIR /Carsties/GatewayService

RUN dotnet publish -c Release -o /app/Carsties/out

# build runtime image (a lighter version as we dont need the sdk image anymore)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 
WORKDIR /app
COPY --from=build /app/Carsties/out .
ENTRYPOINT [ "dotnet", "GatewayService.dll" ]
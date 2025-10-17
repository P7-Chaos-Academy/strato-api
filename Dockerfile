# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /App

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Use the official .NET 8 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /App/out .

# Create directory for certificates
RUN mkdir -p /https

## Copy SSL certificates
#COPY aspnetapp.crt /https/aspnetapp.crt
#COPY aspnetapp.key /https/aspnetapp.key
#
## Set certificate permissions
#RUN chmod 600 /https/aspnetapp.key
#RUN chmod 644 /https/aspnetapp.crt

## Set environment variables for HTTPS
#ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.crt
#ENV ASPNETCORE_Kestrel__Certificates__Default__KeyPath=/https/aspnetapp.key

# Expose both HTTP and HTTPS ports
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "strato-api.dll"]

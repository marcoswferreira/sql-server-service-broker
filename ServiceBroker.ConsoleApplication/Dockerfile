FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory in the container
WORKDIR /app

# Copy the project files into the container
COPY . ./

# Publish the application
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory in the final container
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/out ./

# Start the application
ENTRYPOINT ["dotnet", "ServiceBroker.Console.dll"]

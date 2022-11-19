FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source
RUN curl -sL https://deb.nodesource.com/setup_18.x |  bash -
RUN apt-get install -y nodejs

# copy csproj and restore as distinct layers
COPY *.sln .
COPY SlackGoogleReviewBot/*.csproj ./SlackGoogleReviewBot/
RUN dotnet restore

# copy everything else and build app
COPY SlackGoogleReviewBot/. ./SlackGoogleReviewBot/
WORKDIR /source/SlackGoogleReviewBot
RUN dotnet publish -c release -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app ./
EXPOSE 80
ENTRYPOINT ["dotnet", "SlackGoogleReviewBot.dll"]

#
# STAGE 1 - BUILD
#
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# copy sources
WORKDIR /src
COPY . ./

# build
RUN dotnet restore -r linux-x64
RUN dotnet build -c Release
WORKDIR /src/TicketBurst.ReservationService
RUN dotnet publish -c Release -r linux-x64

#
# STAGE 2 - RUN
#

#TODO
# FROM alpine:3.14
# install .NET Core deps
# RUN apk add bash icu-libs krb5-libs libgcc libintl libssl1.1 libstdc++ zlib
# RUN apk add libgdiplus --repository https://dl-3.alpinelinux.org/alpine/edge/testing/

FROM mcr.microsoft.com/dotnet/aspnet:6.0
#FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine3.14

RUN apt-get -qq update
RUN apt-get -qq install curl

# copy executable
WORKDIR /app
COPY --from=build /src/TicketBurst.ReservationService/bin/Release/net6.0/linux-x64/publish ./
RUN chmod +x ./reservationd

#EXPOSE 3001
#ENV ASPNETCORE_URLS=http://+:3001

EXPOSE 80
ENTRYPOINT ["/app/reservationd"]

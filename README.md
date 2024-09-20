
Overview
========================

This project simulates a long running server task.

The project is split up into 2 folders - Front and Bank representing the front end UI (Angular + Node) and back end services (.NET Core + Postgres)

The client is expected to stay updated realtime using sockets, and as the task progresses are allow to cancel the task on the server gracefully.

If the client is disconnected, then they should be informed and the app should attempt to reconnect when the server is available.

The server jobs are persisted in and run in hangfire. 


# Run this project in containers

========================

Ensure you have docker running 


## Bank end
========================

Bring the back end containers online


<Project Root>/>cd Back
<Project Root>/Back/>docker-compose up

This should bring up the web service on 
https://localhost:8001/swagger/index.html

... and a supportng postgres SQL database on
port 5432


Ensure both containers are running


## Front end

========================
Bring the front end container online


<Project Root>/>cd Front

<Project Root>/Front/>docker-compose up

This should bring up the front end UI on
http://localhost:4200/

Ensure that the Connection status is green (connected)


# Or run locally

========================

## Bank end

In Visual Studio open the backend Solution file

/Back/OI.Web.sln

Use VS Debug configuration

Uncomment the ConnectionStrings in appsettings.development.json (issue with docker compose picking up incorrect config)

Taget the OI.Web.Services Project as startup

Use the https launch settings

this will launch the default swagger UI at
https://localhost:8001/swagger/index.html

The app is expecting a PostGres SQL instance to be listening on 5432, so either
use the instance provided by running the 'docker-compose up' in the parent directory to bring up
a PostGres instance, or provide your own PostGres and modify the connection string in
appsettings.json

## Front end

Open the FE directory with VS Code

<Project Root>/>cd Front

Ensure that all packages are installed

<Project Root>/Front/>npm i

Run the project

<Project Root>/Front/>ng serve -o

This should bring up the front end UI on
http://localhost:4200/


## Back end tests
========================
Run NUnit tests from VS Test Explorer





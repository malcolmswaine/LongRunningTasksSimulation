  

# Overview

This project simulates a long running server task.

The client is expected to stay updated of the task real-time, and while the task is active is allow to cancel, which cancels the task on the server gracefully.

If the client is disconnected, then they should be informed, and the app should attempt to reconnect when the server is available.

Jobs on the server should provide resilient to server failure and persistence for recovery and audit.


###Example of the app functioning:
input = "Hello, World!". Generated base64="SGVsbG8sIFdvcmxkIQ=="\
What web client receives from the server:\
Random pause… "S"\
Random pause… "G"\
Random pause… "V"\
Random pause… "s"\
Etc.\
What does user see in the result text field on web UI:\
Random pause… "S"\
Random pause… "SG"\
Random pause… "SGV"\
Random pause… "SGVs"\
Etc.

# Running the project
  
Clone the project source from github
https://github.com/malcolmswaine/LongRunningTasksSimulation.git

The project is structured into 3 top level folders representing the Angular Front End, .NET Core 8 Back End, and Postgres Database

- Back
- Front
- DB

You can either run the project completely in containers (requires docker), or run it locally in Visual Studio.\ 
To run locally you will need to supply a local Postgres DB (see Run the project locally -> Database).\
Recommended to run the back end from Full Visual Studio, and front end from VS Code.

## Running the project in containers

If youre hosting on a Windows machine, sure you have Docker Desktop and Windows Subsystem for Linux 2 (WSL2) installed and running.

NOTE!!
On windows docker manages a DNS lookup **host.docker.internal** which **needs to present
in your hosts file**, or have some name resolution in your network. This is to
facilitate container to container communication where localhost maps to the 
resolution of the local container instance

Do these steps in order!

## Database

With a command prompt drop into the project DB folder\
  $\<Project Root\>cd DB

Run the docker compose up command\
$\<Project Root\DB\>docker-compose up

This should bring up a Postgres instance in a container with port 5432 exposed.
 
## Bank end .NET

With a command prompt drop into the project Back folder\
  $\<Project Root\>cd Back
  
Run the docker compose up command\
$\<Project Root\Back\>docker-compose up

This will run an initial data migration, so make sure the database is online and accepting connections.

The new container instance should create a web server exposing http on port 8000, and https on 8001

Open the swagger gen to check everything is working 

https://localhost:8001/swagger/index.html

## Front end

With a command prompt drop into the project Back folder\
  $\<Project Root\>cd Front
  
Run the docker compose up command\
$\<Project Root\Front\>docker-compose up
  
This should spin up a container that hosts the front end UI.

Open the default web page to check everything is working.

http://localhost:4200/

Ensure that the connection status icon is green (connected). This means that a socket between the client
and server has been created

  
  

# Run the project locally


## Database
If you don't want to use the containerised Postgres ...

Install a Postgres server locally\
https://www.postgresql.org/download/

On installation ensure sure that the server has the following credentials to allow the .NET to connect\
Username=postgres\
Password=postgres\
Port=5432

## Bank end
Make sure the supporting database is online from the previous step

In Visual Studio open the backend Solution file from\
\<Project Root>\Back\OI.Web.sln

Ensure that the solution configuration is set to **Debug**\
Ensure that the startup project is set to **OI.Web.Services**\
Ensure that the launch configuration is set to **https**

Run the project OI.Web.Services project; this will launch the default swagger UI at:

https://localhost:8001/swagger/index.html

## Front end
Make sure the supporting database and the back end are both online.

In VS Code open the directory where the Angular app is located\
\<Project Root>\Front\OI.Web.Static

Make sure the NPM packages are up to date\
$<Project Root>\Front\OI.Web.Static>npm i
  
Run the project\
$<Project Root>\Front\OI.Web.Static>ng serve -o

This should bring up the front end UI on

http://localhost:4200/

Ensure that the connection status icon is green (connected). This means that a socket between the client
and server has been created
  
  

## Back end tests

In Visual Studio open the backend Solution file from
\<Project Root>\Back\OI.Web.sln

**Unit**\
Run the NUnit tests from Visual Studio Test Explorer

**Integration**\
Run the xUnit tests from Visual Studio Test Explorer - will need docker to run these tests

  
  

## Remarks
For simplicity this project uses username/password coded into the configuration as opposed to consuming secrets.

For demo purposes the migration runs every time the back end is brought up. This can lead to 'already exists' migration warnings in the console if the database has already been populated, but can be safely ignored for demo purposes.

The default log level is "Information" for release also, for demo purposes.

Hangfire spits out a lot of randomness to the console

## Todo

Add authentication\
Clear down SignalR Connection mapping 

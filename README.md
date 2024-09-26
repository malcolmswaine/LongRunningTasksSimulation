  

# Overview

This project simulates long running server tasks in a microservice environment.


##Basic Happy Days Workflow
The Front End sends a string to be decoded to the Web App REST/Signalling server (OI.Web.Service)

The REST API receives the request and adds a JobCreationRequest message to the message bus (RabiitMQ)

The Job Processing microservice (OI.JobProcessing) consumes the JobCreationRequest message.

The Job Processing microservice starts the simulation of a long running job using timer delays.

The Job Processing microservice then adds a JobCreationResponse message to the message bus

The Web App consumes the message and using SignalR informs the Front End using sockets that the job has been created.

The Job Processing microservice continues to process the task in time increments and every time it completes an increment, puts a JobProcessingStepResponse on the service bus. It also logs a checkpoint to the database in case the task fails, and needs to be analyzed 

The Web App consumes the JobProcessingStepResponse message and signals the Front End that a task increment has been completed, along with the current task results.

The Job Processing microservice finished the task.

The Job Processing microservice puts a JobComplete message on the bus.

The Web App consumes the JobComplete message and signals the Front End that the task is done.

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

==================

The client is expected to stay updated of the task real-time, and while the task is active is allow to cancel, which cancels the task on the server gracefully.

If the client is disconnected, then they should be informed, and the app should attempt to reconnect when the server is available.

Jobs on the server should provide resilient to server failure and persistence for recovery and audit.

Jobs can be monitored using the Hangfire UI http://containerurl/hangfire

The queue can be monitored using the RabbitMQ dashboard by accessing the exposed container port using a browser




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

**IMPORTANT!!**
On windows docker manages a DNS lookup **host.docker.internal** which **needs to present
in your hosts file**, or have some name resolution in your network. This is to
facilitate container to container communication where localhost maps to the 
resolution of the local container instance

There is an auto update option in docker desktop top under settings -> General -> Use the WSL 2 based engine -> Add the *.docker.internal names to the host's /etc/hosts file\
**Make sure it's switched on!!**

If you're not using container and just don't want to change your hosts file, edit the connection string in the Back/Oi.Web.Service project and change the Host=host.docker.internal to Host=localhost

Do these steps in order!

## Database

With a command prompt drop into the project DB folder\
  $\<Project Root\>cd DB

Run the docker compose up command\
$\<Project Root\DB\>docker-compose up

This should bring up a Postgres instance in a container with port 5432 exposed.


## Message Bus (RabbitMQ)

With a command prompt drop into the project Bus folder\
  $\<Project Root\>cd Bus
  
Run the docker compose up command\
$\<Project Root\Back\>docker-compose up

This should bring up a rabbitmq container with default ports 15672 and 5672

Open the Rabbit Dashboard on\
http://localhost:15672/


## Bank end .NET x2

With a command prompt drop into the project Back folder\
  $\<Project Root\>cd Back
  
Run the docker compose up command\
$\<Project Root\Back\>docker-compose up

This will run an initial data migration, so make sure the database is online and accepting connections.

The new container instance should create a web server exposing http on port 8000, and https on 8001

Open the swagger gen to check everything is working 

http://localhost:8000/swagger/index.html
http://localhost:8005/swagger/index.html

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

## Message Bus

You can download and install RabbitMQ from\
https://www.rabbitmq.com/docs/download


Run it locally with default ports 15672 and 5672


## Bank end
Make sure the supporting database is online from the previous step

In Visual Studio open the backend Solution file from\
\<Project Root>\Back\OI.Web.sln

Ensure that the solution configuration is set to **Debug**\
Ensure that the startup project is set to **OI.Web.Services**\
Ensure that the launch configuration is set to **http**

Run the project OI.Web.Services project; this will launch the default swagger UI at:

http://localhost:8000/swagger/index.html



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

The default log level is set to "Information" for release - change as required.

## Future Possible Enhancements

Add authentication\
Clear down SignalR Connection mapping\
Add a message bus to the solution\
Move the job service to a standalone microservice\
Wire up the message bus and the job service

To run this project
====================

!!Important - make sure the database is online from the DB project!!

Docker Compose
====================
open a command line at one level above this project
cd ..

run the command (requires docker or docker desktop to be installed)
docker-compose up

This will run an initial data migration, so make sure the database is online and accepting connections.

The new container instance should create a web server exposing http on port 8000, and https on 8001

Open the swagger gen to check everything is working 

https://localhost:8001/swagger/index.html



VS Local
======================
In Visual Studio open the backend Solution file from\
\<Project Root>\Back\OI.Web.sln

Ensure that the solution configuration is set to **Debug**\
Ensure that the startup project is set to **OI.Web.Services**\
Ensure that the launch configuration is set to **https**

Run the project OI.Web.Services project; this will launch the default swagger UI at:

https://localhost:8001/swagger/index.html



Tests
======================

Run the NUnit tests from Visual Studio Test Explorer
To run this project
====================


Docker Compose
====================
open a command line at one level above this project
cd ..

run the command (requires docker or docker desktop to be installed)
docker-compose up

this will start containers for both the back and it's supporting database

Check the wep api us launched at (sometimes ports aren't displayed in docker desktop)
https://localhost:8001/swagger/index.html

Be aware that if you have a local PostGres server running on port 5432 this will conflict 
with the port mapping of the PostGres exposed on the container port mapping, so 
the local server instance should be stopped first.


VS Local
======================
Use Debug configuration

Uncomment the ConnectionStrings in appsettings.development.json (issue with docker compose picking up incorrect config)

Taget the OI.Web.Services Project as startup

Use the https launch settings

this will launch the default swagger UI at
https://localhost:8001/swagger/index.html

The app is expecting a PostGres SQL instance to be listening on 5432, so either
use the instance provided by running the 'docker-compose up' command to bring up
a PostGres instance, or provide your own PostGres and modify the connection string in
appsettings.json


Tests
======================
Run NUnit tests from VS Test Explorer
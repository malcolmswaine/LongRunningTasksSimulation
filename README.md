Run this project in containers

========================

Ensure you have docker running 


Bank end
========================
Bring the back end containers online


<Project Root>/>cd Back
<Project Root>/Back/>docker-compose up

This should bring up the web service on 
https://localhost:8001/swagger/index.html

And a postgres SQL database on
port 5432


Ensure both containers are running


Front end
========================
Bring the front end container online


<Project Root>/>cd Front
<Project Root>/Front/>docker-compose up

This should bring up the web service on 
http://localhost:4200/

Ensure that the Connection status is green (connected)




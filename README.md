# Movie Voting App (on .NET Core framework, and MySQL)
This project is build with `ASP.NET Core` framework, conecting to a database in `MySQL on Docker`.

To run this project on your local machine, [Setup MySQL Server on Docker and connect to it on local dev environment](#setup-mysql-server-on-docker-and-connect-to-it-on-local-dev-environment).

To deploy this project to Docker, [Setup project to deploy to Docker](#setup-project-to-deploy-to-docker).

To deploy to VIC, follows [Deploy project to vSphere Integrated Containers (VIC)](#deploy-project-to-vsphere-integrated-containers-vic).

## Setup MySQL Server on Docker and connect to it on local dev environment
1. Download SQL Server container
```shell
docker pull mysql:latest
```
2. Install MySQL container (replace all `<YourStrong!Passw0rd>` after this to your own password)
```shell
docker run --detach -p 3306:3306 --name=mysql1 --env="MYSQL_ROOT_PASSWORD=<YourStrong!Passw0rd>" mysql
```
3. View docker containers to check if mysql is running
```shell
docker ps
```
4. Login to the MySQL server, enter your password when prompted
```shell
docker exec -it mysql1 mysql -u root -p
```
5. Change authorization plugins for root users
```shell
ALTER USER 'root'@'%' IDENTIFIED WITH mysql_native_password BY '<YourStrong!Passw0rd>';
```
, and then
```shell
ALTER USER 'root'@'localhost' IDENTIFIED WITH mysql_native_password BY '<YourStrong!Passw0rd>';
```
6. List all Users to check root users are using `mysql_native_password` plugin
```shell
SELECT Host, User, plugin FROM mysql.user;
```
7. Exit from MySQL server
```shell
exit
```
8. Access your MySQL server from Database IDE with this configuration:
```shell
    Host: localhost
    Port: 3306
    User: root
    Password: <YourStrong!Passw0rd>
```
9. On your Database IDE, create or restore your database. For this project, you can use [dbbackup.sql](dbbackup.sql)

10. In `appsettings.json` file of your project, add a default connection string
```shell
ConnectionStrings": {
  "DefaultConnection": "server=localhost;userid=root;password=<YourStrong!Passw0rd>;database=Movie;"
}
```
11. In `Startup.cs` file of your project, add a SQLServer connection (or change from the original SQLite connection)
```shell
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{
    .
    .
    .
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(Configuration.GetConnectionString("DefaultConnection")));
    .
    .
    .
}
```
12. In the `.csproj` file of your project, include the following packages
```shell
  <ItemGroup>
    .
    .
    .
    <PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="2.0.1" />
  </ItemGroup>
  <ItemGroup>
    .
    .
    .
    <DotNetCliToolReference Include="Microsoft.EntityFrameworkCore.Tools.DotNet" Version="2.0.1" />
  </ItemGroup>
```


## Setup project to deploy to Docker
1. Add `Dockerfile` to project's root directory with this content
```shell
FROM microsoft/aspnetcore-build:2.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore MovieVotingCoreMySQL.csproj

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out MovieVotingCoreMySQL.csproj

# Build runtime image
FROM microsoft/aspnetcore:2.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "MovieVotingCoreMySQL.dll"]
```
(replace `MovieVotingCoreMySQL` to your own project name)

2. Add `docker-compose.yml` to project's root directory with this content
```shell
version: '2'

services:
  movievotingcoremysql:
    image: movievotingcoremysql
    build:
      context: .
      dockerfile: Dockerfile
    restart: always
    ports:
      - "8080:80"
    depends_on:
      - db
  db:
    image: mysql:latest
    container_name: db
    restart: always
    environment:
      MYSQL_ROOT_PASSWORD: <YourStrong!Passw0rd>
      MYSQL_DATABASE: Movie
    ports:
      - 3306:3306
    volumes:
      - db-data:/var/opt/mysql/data
      - ./dbbackup.sql:/docker-entrypoint-initdb.d/1-dbbackup.sql

volumes:
  db-data:
```
Here,

a. `movievotingcoremysql` must be your image's name.

b. `./dbbackup.sql:/docker-entrypoint-initdb.d/1-dbbackup.sql` is to initialize the `mysql` database with `./dbbackup.sql`, `./dbbackup.sql` must be the filename of your DB's backup sql script.

3. In `appsettings.json` file of your project, change your default connection string
```shell
ConnectionStrings": {
  "DefaultConnection": "server=db;port=3306;userid=root;password=<YourStrong!Passw0rd>;database=Movie;"
}
```
4. On command prompt, navigate to project's directory
5. On command prompt, run 
```shell
docker build -t movievotingcoremysql .
```
6. On command prompt, run 
```shell
docker-compose up --no-build -d
```
7. Check if your containers are running
```shell
docker ps
``` 
8. Visit http://localhost:8080
```shell
Note: If your application is up, but have connection issue to the database, try `docker restart <container_name>`
```
9. Once you've done, stop and remove the containers with this command
```shell
docker-compose down
```

## Deploy project to vSphere Integrated Containers (VIC)
#### Upload app's image to Docker Hub
Because VIC doesn't support `docker build`, you have to build your images with `Docker` on your local pc, and then upload your images to `Docker Hub` for VIC to perform a `docker pull` on.

1. Create an account on https://hub.docker.com
2. On command prompt, login to your docker account
```shell
docker login
```
3. Build navigate to your project's root directory, and build your app image
```shell
docker build -t <image-name> .
```
for this project's case
```shell
docker build -t movievotingcoremysql .
```
4. List all docker images to get your `IMAGE ID`
```shell
docker images
```
5. Tag your image
```shell
docker tag <IMAGE ID> yourdockerhubusername/image-name:1.0
```
for this project's case
```shell
docker tag bb38976d03cf vsgdev/movievotingcoremysql:1.0
```
6. Push image to Docker Hub, and your image is ready for everyone to use
```shell
docker push vsgdev/movievotingcoremysql
```

#### Create your own mysql image on Docker Hub
Because VIC does not support mounting directories as a data volume, you cannot use `/docker-entrypoint-initdb.d` on `docker-compose.yml` to initialize your `mysql` databases.

1. Create a new folder, name it `DBBackup`.
2. Copy your sql script into the folder.
3. Create a `Dockerfile` with this content
```shell
FROM mysql:latest
COPY dbbackup.sql /mysql/dbbackup.sql
```
4. Build your own `mysql` image
```shell
docker build -t mysql-mv .
```
5. Get your `IMAGE ID` from running `docker images` and tag you image
```shell
docker tag bb38976d03cf vsgdev/mysql-mv:1.0
```
6. Push image to `Docker Hub`
```shell
docker push vsgdev/mysql-mv
```

#### Deploy and run on VIC
1. On command prompt, connect to VIC
```shell
export DOCKER_HOST=<docker-ip-address>:<port>
```
for example,
```shell
export DOCKER_HOST=192.168.1.200:1234
```
2. Verify if your `docker info` is now showing info of VIC
```shell
docker info
```
3. Pull images from your `Docker Hub`, for this project's case
```shell
docker pull vsgdev/movievotingcoremysql:1.0
docker pull vsgdev/mysql-my:1.0
```
4. Update for `docker-compose.yml` file
```shell
version: '2'

services:
  movievotingcoremysql:
    image: vsgdev/movievotingcoremysql:1.0
    build:
      context: .
      dockerfile: Dockerfile
    restart: always
    ports:
      - "8080:80"
    depends_on:
      - db
  db:
    image: vsgdev/mysql-mv:1.0
    container_name: db
    restart: always
    environment:
      MYSQL_ROOT_PASSWORD: <YourStrong!Passw0rd>
      MYSQL_DATABASE: Movie
    ports:
      - 3306:3306
    volumes:
      - db-data:/var/opt/mysql/data

volumes:
  db-data:
    driver: "vsphere"
    driver_opts:
      Capacity: "2G"
      VolumeStore: "default"

networks:
  default:
    ipam:
      config:
        - subnet: 192.168.0.0/16
```
Note: 
```
a. `version:` needs to be `'2'` because thats the version supported on VIC.
b. In `movievotingcoremysql:` and `db:`, `image:` is changed to the new image name pulled from Docker Hub.
c. `- ./dbbackup.sql:/docker-entrypoint-initdb.d/1-dbbackup.sql` on `db: volumes:` is removed because its not supported
d. `volumes: db-data:` is updated with more settings.
e. `networks` settings are added.
f. Change `<YourStrong!Passw0rd>`
```
5. Create and run your containers with `docker-compose up`

```shell
docker-compose up --no-build -d
```
6. List all running containers
```shell
docker ps
```
Here, you will need to use
    a. `db`'s `CONTAINER_ID`, we will call it `<DB_CONTAINER_ID>`.  
    b. `movievotingcoremysql_movievotingcoremysql_1` or see `NAMES` column if you defined it will different name, we will call it `<APP_CONTAINER_NAME>`.
    c. `movievotingcoremysql_movievotingcoremysql_1`'s `PORTS` which you can use to access your web app in a browser later, we will call it `<APP_IP_ADDRESS_AND_PORT>`.

7. Initialize your MySQL database
```shell
docker exec <DB_CONTAINER_ID> /bin/sh -c 'mysql -u root -p<YourStrong!Passw0rd> </mysql/dbbackup.sql'
```
8. Restart your app
```shell
docker restart <APP_CONTAINER_NAME>
```
9. On a web browser, visits `<APP_IP_ADDRESS_AND_PORT>`.

10. When finish using, stop and remove your containers with `docker-compose down`.
```shell
docker-compose down
```

## References
1. Run MySQL container image on Docker - https://severalnines.com/blog/mysql-docker-containers-understanding-basics
2. Connecting web app container to MySQL container on Docker - https://medium.com/@Likhitd/asp-net-core-and-mysql-with-docker-part-3-e3827e006e3
3. Push images to Docker Hub - https://ropenscilabs.github.io/r-docker-tutorial/04-Dockerhub.html
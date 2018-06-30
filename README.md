# Movie Voting App (on .NET Core framework, and MySQL)
This project is build with `ASP.NET Core` framwork, conecting to a database in `MySQL on Docker`.

To run this project on your local machine, [Setup MySQL Server on Docker and connect to it on local dev environment](#setup-mysql-server-on-docker-and-connect-to-it-on-local-dev-environment).

To deploy this project to Docker, [Setup project to deploy to Docker](#setup-project-to-deploy-to-docker).


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
a. `vsgdev/movievotingcoremysql:1.0` must be your image's name.
b. `./dbbackup.sql` on `volumes` must be the filename of your DB's backup script.

3. In `appsettings.json` file of your project, change your default connection string
```shell
ConnectionStrings": {
  "DefaultConnection": "server=db;port=3306;userid=root;password=<YourStrong!Passw0rd>;database=Movie;"
}
```
4. On command prompt, navigate to project's directory
5. On command prompt, run 
```shell
docker-compose up --no-build -d
```
6. Check if your containers are running
```shell
docker ps
``` 
7. Visit http://localhost:8080
```shell
Note: If your application is up, but have connection issue to the database, try `docker restart <container_name>`
```
8. Once you've done, stop and remove the containers with this command
```shell
docker-compose down
```

## Deploy project to VIC


## References
1. Run MySQL container image on Docker - https://severalnines.com/blog/mysql-docker-containers-understanding-basics
2. Connecting web app container to MySQL container on Docker - https://medium.com/@Likhitd/asp-net-core-and-mysql-with-docker-part-3-e3827e006e3
# Movie Voting App (on .NET Core framework, and MySQL)
This project is build with `ASP.NET Core` framwork, conecting to a database in `MySQL on Docker`.

To run this project on your local machine, [Setup MySQL on Docker](#setup-mysql-on-docker) and follows the settings mentioned on the [Setup database connection on local dev environment](#setup-database-connection-on-local-dev-environment) section.

To deploy this project to Docker, [Setup MySQL on Docker](#setup-mysql-on-docker) and follow steps on the [Setup project to deploy to Docker](#setup-project-to-deploy-to-docker) section.


## Setup MySQL on Docker
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
9. On your Database IDE, create or restore your database. For this project, you can use [DBBackup/DBBackup.sql](DBBackup/DBBackup.sql)


## Setup database connection on local dev environment
1. In `appsettings.json` file of your project, add a default connection string
```shell
ConnectionStrings": {
  "DefaultConnection": "server=localhost;userid=root;password=<YourStrong!Passw0rd>;database=Movie;"
}
```
2. In `Startup.cs` file of your project, add a SQLServer connection (or change from the original SQLite connection)
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
3. In the `.csproj` file of your project, include the following packages
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
1. Right click project on Solution Explorer > Add > Add Docker Support
2. Change `Dockerfile` to 
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
3. In `appsettings.json` file of your project, change your default connection string
```shell
ConnectionStrings": {
  "DefaultConnection": "server=mysql1;userid=root;password=<YourStrong!Passw0rd>;database=Movie;"
}
```
Note: `mysql1` needs to be the same as you mysql image name set on Step 2 of [Setup MySQL on Docker](#setup-mysql-on-docker)
4. On command prompt, navigate to project's directory
5. On command prompt, run 
```shell
docker build -t image-name .
``` 
(replace `image-name` to your own preferred name)
6. On command prompt, run 
```shell
docker run -d -p 8080:80 --name container-name --link mysql1 -it image-name
``` 
(replace `container-name` and `image-name` to your own preferred name)
7. Visit http://localhost:8080


## References
1. Run MySQL container image on Docker - https://severalnines.com/blog/mysql-docker-containers-understanding-basics
2. Connecting web app container to MySQL container on Docker - https://medium.com/@Likhitd/asp-net-core-and-mysql-with-docker-part-3-e3827e006e3
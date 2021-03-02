# PermissionAccessControl2

Welcome to version 2 of the example application that contains my approach to feature and data authorization code. This web site pretends to be a SaaS application which provides stock and sales management to companies with multiple retail outlets. 

**NOTE: This version has been updated to NET Core 3.1**  
*I didn't update to NET 5 because NET Core 3.1 is the long-term support version. The update to NET 5 should be fairly easy (converting from 2.2 to 3.1 was had work!)*

This is open-source application (MIT license).

## See the articles

* Part 3: [A better way to handle authorization - six months on](https://www.thereformedprogrammer.net/a-better-way-to-handle-asp-net-core-authorization-six-months-on/).
* Part 4: [Building a robust and secure data authorization with EF Core](https://www.thereformedprogrammer.net/part-4-building-a-robust-and-secure-data-authorization-with-ef-core/).
* Part 5: [A better way to handle authorization - refreshing users claims](https://www.thereformedprogrammer.net/part-5-a-better-way-to-handle-authorization-refreshing-users-claims/).
* Part 6: [Adding user impersonation to an ASP.NET Core web application](https://www.thereformedprogrammer.net/adding-user-impersonation-to-an-asp-net-core-web-application/).
* Part 7: [Adding the �better ASP.NET Core authorization� code into your app](https://www.thereformedprogrammer.net/part-7-adding-the-better-asp-net-core-authorization-code-into-your-app/).

**NOTE: If you like what these articles describe and want to add one or more of these features to your application I STRONGLY suggest you read the [Part 7 article](https://www.thereformedprogrammer.net/part-7-adding-the-better-asp-net-core-authorization-code-into-your-app/), which gives a step-by-step guide to how to pick/copy the right code from the `PermissionAccessControl2` into your app.**

## How to play with the application

You start the PermissionAccessControl2 project to run the ASP.NET Core application. The home screen shows you what you can do. It also tells you what setup was used the features/database - section [Controlling how the demo works](https://github.com/JonPSmith/PermissionAccessControl2#controlling-how-the-demo-works).

The default setting (see Configuration section below) will use in-memory databases which it will preload with demo users and data at startup (NOTE: Its a bit slow to start as it is setting up all the demo users and data). The demo users have:

1. Different **Permissions**, which controls what they can do, e.g. only a StoreManager can provide a refund.
2. Different **DataKey**, which controls what part of the shop data they can see, e.g. a SalesAssistant and StoreManager can only see the data in their shop, but a Director can see all shop data in the company.
3. There is **Refresh Claims** menu dropdown which allows you to try the "refreshing claims" feature described in the [Part 5 article](https://www.thereformedprogrammer.net/part-5-a-better-way-to-handle-authorization-refreshing-users-claims/).
4. There is a **Impersonation**  menu dropdown which allows you to try the "user impersonation" feature described in the [Part 6 article](#).

There is a link on the home page to a list of users that you can log in via (the email address is also the password). There are two different companies, 4U Inc. and Pets2 Ltd., which have a number of shops in different divisions, represented by hierarchical data. Logging in as a user will give you access to some features and data (if linked to data).

The home page gives you more information on what you can do.

## Configuration

The [appsetting.json file](https://github.com/JonPSmith/PermissionAccessControl2/blob/master/PermissionAccessControl2/appsettings.json) contains settings that configure how the system runs.

### Controlling how the demo works

This application is written to work with both in-memory or normal (e.g. SQL Server) databases (version 1 only worked with in-memory, but that made it difficult to convert to normal databases). The "DemoSetup" section is shown below: 

```javascript
  "DemoSetup": {
    "DatabaseSetup": "InMemory", //This can be "InMemory" or "Permanent" (a real database) database.
    "CreateAndSeed": true, //If this is true then it will create the dbs and ensure the data is seeded
    "AuthVersion": "Everything" //The options are Off, LoginPermissions, LoginPermissionsDataKey, PermissionsOnly, PermissionsDataKey, Impersonation, RefreshClaims, Everything
  
```

They are descibed in the next three subsections.

#### 1. DatabaseSetup property

This swiches between:
* **"InMemory"**: which selects an in-memory Sqlite database - very easy to try out things or changing the database.
* **"Permanent"**: which selects a SQL Server database. 

*NOTE that I use `context.Database.EnsureCreated()` on startup to create the database because its easy. BUT it does preclude the use of EF Core Migrations. See [PermissionsOnlyApp](https://github.com/JonPSmith/PermissionsOnlyApp), which I create as part of the [Part 7 acticle](https://www.thereformedprogrammer.net/part-7-adding-the-better-asp-net-core-authorization-code-into-your-app/). It usees EF Core Migrations to handle database changes.*

If you use "Permanent" for the "DatabaseSetup" then you need to provide two connection strings: one for the ASP.NET Core Identity database and the other for the database which holds both the multi-tenant data and the extra authorization data.

```javascript
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PermissionAccessControl2-AspNetCoreIdentity;Trusted_Connection=True;MultipleActiveResultSets=true",
    "DemoDatabaseConnection": "Server=(localdb)\\mssqllocaldb;Database=PermissionAccessControl2-DemoDatabase;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
```

#### 2. CreateAndSeed property

This is there for people who want to mess about with a SQL Server database. If its `false` then all the database create and seed parts are turned off. 

*NOTE: The check/add of the SuperAdmin user isn't turned off by this property.*

#### 3. AuthVersion property

This allows you to try the different authorization features covered in the articles. I'm not going to describe all the features here beacause they can be seen in the [AddClaimsToCookie](https://github.com/JonPSmith/PermissionAccessControl2/blob/master/AuthorizeSetup/AddClaimsToCookie.cs) class.

### Setting up SuperAdmin user

The appsetting.json file should have a "SuperAdmin" section as shown below. on startup the extension method `CheckAddSuperAdminAsync` checks to see if there is a user with the role "SuperAdmin". If there isn't it tries to add a user with the given email (which will fail if that is already used).

```javascript
  "SuperAdmin": //This holds the information on the superuser. You must have one SuperUser setup otherwise you can't manage users
  {
    "Email": "... email of super admin user ...",
    "Password": "... password ..."
  },
  ```

NOTES:

1. I recommend you override the email/password values when deploying, using something like Azure's override of appsettings.json.
2. Because the role "SuperAdmin" is so powerful I recommend you only have one user with that role. You use the "SuperAdmin" user to set up the other admin users and use them for your normal admin jobs.

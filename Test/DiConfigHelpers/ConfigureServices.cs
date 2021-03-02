﻿// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Reflection;
using DataKeyParts;
using DataLayer.EfCode;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PermissionAccessControl2.Data;
using RefreshClaimsParts;
using Test.FakesAndMocks;
using TestSupport.Helpers;

namespace Test.DiConfigHelpers
{
    public static class ConfigureServices
    {
        public static ServiceProvider SetupServicesForTest(this object callingClass, bool useSqlDbs = false)
        {
            var services = new ServiceCollection();
            services.RegisterDatabases(callingClass, useSqlDbs);

            //Wanted to use the line below but just couldn't get the right package for it
            //services.AddDefaultIdentity<IdentityUser>()
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            var startupConfig = AppSettings.GetConfiguration((Assembly)null, "demosettings.json");
            services.AddLogging();
            services.AddSingleton<IWebHostEnvironment>(new MockHostingEnvironment { WebRootPath = TestData.GetTestDataDir()});
            services.AddSingleton<IConfiguration>(startupConfig);
            services.AddSingleton<IGetClaimsProvider>(new FakeGetClaimsProvider(null));
            services.AddSingleton<IAuthChanges>(x => null);

            var serviceProvider = services.BuildServiceProvider();

            //make sure the  databases are created
            serviceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();
            serviceProvider.GetRequiredService<CombinedDbContext>().Database.EnsureCreated();

            return serviceProvider;
        }

        private static void RegisterDatabases(this ServiceCollection services, object callingClass, bool useSqlDbs)
        {
            if (useSqlDbs)
            {
                var aspNetConnectionString = callingClass.GetUniqueDatabaseConnectionString("AspNet");
                services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(aspNetConnectionString));

                var appConnectionString = callingClass.GetUniqueDatabaseConnectionString("AppData");
                services.AddDbContext<CompanyDbContext>(options => options.UseSqlServer(appConnectionString));
                services.AddDbContext<ExtraAuthorizeDbContext>(options => options.UseSqlServer(appConnectionString));
                services.AddDbContext<CombinedDbContext>(options => options.UseSqlServer(appConnectionString));
            }
            else
            {

                var aspNetAuthConnection = SetupSqliteInMemoryConnection();
                services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(aspNetAuthConnection));
                var appExtraConnection = SetupSqliteInMemoryConnection();
                services.AddDbContext<CompanyDbContext>(options => options.UseSqlite(appExtraConnection));
                services.AddDbContext<ExtraAuthorizeDbContext>(options => options.UseSqlite(appExtraConnection));
                services.AddDbContext<CombinedDbContext>(options => options.UseSqlite(appExtraConnection));
            }
        }

        private static SqliteConnection SetupSqliteInMemoryConnection()
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);
            connection.Open();  //see https://github.com/aspnet/EntityFramework/issues/6968
            return connection;
        }
    }
}
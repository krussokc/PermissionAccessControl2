﻿// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Test.FakesAndMocks;
using UserImpersonation.Concrete;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.ImpersonateTests
{
    public class TestImpersonationHandler
    {
        private readonly string _cookieEncryptPurpose;

        public TestImpersonationHandler()
        {
            var httpContext = new DefaultHttpContext();
            _cookieEncryptPurpose = new ImpersonationCookie(httpContext, null).EncryptPurpose;
        }

        private void AddCookieToHttpContext(HttpContext httpContext, IDataProtectionProvider eProvider, bool keepOwnPermissions = false)
        {
            var data = new ImpersonationData("differentUserId", "name@gmail.com", keepOwnPermissions);
            httpContext.AddRequestCookie("UserImpersonation", 
                eProvider.CreateProtector(_cookieEncryptPurpose).Protect(data.GetPackImpersonationData()));
        }


        [Fact]
        public void TestHandlerNoCookieNoClaim()
        {
            //SETUP
            var httpContext = new DefaultHttpContext();
            var eProvider = new EphemeralDataProtectionProvider();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "userid")
            };

            //ATTEMPT
            var handler = new ImpersonationHandler(httpContext, eProvider, claims);

            //VERIFY
            handler.ImpersonationChange.ShouldBeFalse();
            handler.GetUserIdForWorkingOutPermissions().ShouldEqual("userid");
            handler.GetUserIdForWorkingDataKey().ShouldEqual("userid");
            handler.AddOrRemoveImpersonationClaim(claims);
            claims.Count.ShouldEqual(1);
        }

        [Fact]
        public void TestHandlerHasCookieAndClaim()
        {
            //SETUP
            var httpContext = new DefaultHttpContext();
            var eProvider = new EphemeralDataProtectionProvider();
            AddCookieToHttpContext(httpContext, eProvider);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "userid"),
                new Claim(ImpersonationHandler.ImpersonationClaimType, "name@gmail.com")
            };

            //ATTEMPT
            var handler = new ImpersonationHandler(httpContext, eProvider, claims);

            //VERIFY
            handler.ImpersonationChange.ShouldBeFalse();
            handler.GetUserIdForWorkingOutPermissions().ShouldEqual("differentUserId");
            handler.GetUserIdForWorkingDataKey().ShouldEqual("differentUserId");
            handler.AddOrRemoveImpersonationClaim(claims);
            claims.Count.ShouldEqual(2);
        }

        [Fact]
        public void TestHandlerStartingKeepOwnPermissions()
        {
            //SETUP
            var httpContext = new DefaultHttpContext();
            var eProvider = new EphemeralDataProtectionProvider();
            AddCookieToHttpContext(httpContext, eProvider, true);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "userid")
            };

            //ATTEMPT
            var handler = new ImpersonationHandler(httpContext, eProvider, claims);

            //VERIFY
            handler.ImpersonationChange.ShouldBeTrue();
            handler.GetUserIdForWorkingOutPermissions().ShouldEqual("userid");
            handler.GetUserIdForWorkingDataKey().ShouldEqual("differentUserId");
            handler.AddOrRemoveImpersonationClaim(claims);
            claims.SingleOrDefault(x => x.Type == ImpersonationHandler.ImpersonationClaimType)?.Value.ShouldEqual("name@gmail.com");
        }

        [Fact]
        public void TestHandlerStartingUseImpersonationUsersPermissions()
        {
            //SETUP
            var httpContext = new DefaultHttpContext();
            var eProvider = new EphemeralDataProtectionProvider();
            AddCookieToHttpContext(httpContext, eProvider);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "userid")
            };

            //ATTEMPT
            var handler = new ImpersonationHandler(httpContext, eProvider, claims);

            //VERIFY
            handler.ImpersonationChange.ShouldBeTrue();
            handler.GetUserIdForWorkingOutPermissions().ShouldEqual("differentUserId");
            handler.GetUserIdForWorkingDataKey().ShouldEqual("differentUserId");
            handler.AddOrRemoveImpersonationClaim(claims);
            claims.SingleOrDefault(x => x.Type == ImpersonationHandler.ImpersonationClaimType)?.Value.ShouldEqual("name@gmail.com");
        }

        [Fact]
        public void TestHandlerStopping()
        {
            //SETUP
            var httpContext = new DefaultHttpContext();
            var eProvider = new EphemeralDataProtectionProvider();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "userid"),
                new Claim(ImpersonationHandler.ImpersonationClaimType, "name@gmail.com")
            };

            //ATTEMPT
            var handler = new ImpersonationHandler(httpContext, eProvider, claims);

            //VERIFY
            handler.ImpersonationChange.ShouldBeTrue();
            handler.GetUserIdForWorkingOutPermissions().ShouldEqual("userid");
            handler.GetUserIdForWorkingDataKey().ShouldEqual("userid");
            handler.AddOrRemoveImpersonationClaim(claims);
            claims.Count.ShouldEqual(1);
        }


    }
}
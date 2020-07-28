using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.ServiceApi.Owin;
using Owin;
using Microsoft.Owin;
using CommerceTraining;

[assembly:OwinStartup(typeof(Startup))]
namespace CommerceTraining
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Enable bearer token authentication using Membership for Service Api
            app.UseServiceApiMembershipTokenAuthorization();
        }

    }
}
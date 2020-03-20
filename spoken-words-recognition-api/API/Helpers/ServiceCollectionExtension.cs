using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace API.Helpers
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, JwtToken jwtToken)
        {
            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(jwtToken.StsDiscoveryEndpoint, new OpenIdConnectConfigurationRetriever());
            var config = configManager.GetConfigurationAsync().Result;

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtToken.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtToken.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKeys = config.SigningKeys,
                        ValidateLifetime = false
                    };
                });

            return services;
        }

        public static IServiceCollection AddClaimsAuthorizationPolicy(this IServiceCollection services, JwtToken jwtToken)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ClaimsAuthorizationPolicy", policy =>
                {
                    policy.RequireAssertion(context =>
                    {
                        foreach (var tokensRequiredClaim in jwtToken.Claims)
                        {
                            var value = context.User.Claims.FirstOrDefault(x => x.Type == tokensRequiredClaim.Type)?.Value;

                            if (value == null)
                            {
                                Console.WriteLine($"Missing claim {tokensRequiredClaim.Type}");
                                return false;
                            }

                            if (value != tokensRequiredClaim.Value)
                            {
                                Console.WriteLine($"Claim {tokensRequiredClaim.Type} value doesn't match the expected value {tokensRequiredClaim.Value}");
                                return false;
                            }
                        }

                        return true;
                    });
                });
            });

            return services;
        }
    }
}
using FastEndpoints;
using FastEndpoints.Swagger;
using Framework.BuildingBlock.HttpApi;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RequestLocalization;
using Service.Template.Application;
using Service.Template.Application.Contracts;
using Service.Template.Domain;
using Service.Template.Domain.Shared;
using Service.Template.EntityFrameworkCore;
using Service.Template.HttpApi;
using Template.Service.Domain.Shared;
using Volo.Abp;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;
using Volo.Abp.AspNetCore.Mvc.Libs;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.AspNetCore.Uow;
using Volo.Abp.Auditing;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Caching;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;

namespace ObjectStorageService.Host;


[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMultiTenancyModule),

    typeof(ObjectStorageServiceApplicationContractsModule),
    typeof(ObjectStorageServiceApplicationModule),
    typeof(ObjectStorageServiceEntityFrameworkCoreModule),
    typeof(ObjectStorageServiceHttpApiModule),

    typeof(AbpAspNetCoreSerilogModule)
)]

public class ObjectStorageServiceHostModule : AbpModule
{

    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();


        PreConfigure<AuthenticationOptions>(c =>
        {
            c.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            c.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        });

        //context.Services.AddAuthentication(options =>
        //{
        //    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

        //})
        //.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        //{
        //    options.TokenValidationParameters = new TokenValidationParameters
        //    {
        //        ValidateIssuer = true,
        //        ValidIssuer = configuration["AuthServer:Issuer"],
        //        ValidateAudience = true,
        //        ValidAudience = configuration["AuthServer:Audience"],
        //        ValidateLifetime = true,
        //        ValidateIssuerSigningKey = true,
        //        ClockSkew = TimeSpan.Zero,
        //        IssuerSigningKey = new SymmetricSecurityKey(
        //            Encoding.UTF8.GetBytes(configuration["AuthServer:SecurityKey"])
        //        )
        //    };

        //    options.Events = new JwtBearerEvents
        //    {
        //        OnMessageReceived = context =>
        //        {
        //            var path = context.HttpContext.Request.Path;
        //            if (path.StartsWithSegments("/signalr-hubs/notification"))
        //            {
        //                var accessToken = context.Request.Query["access_token"];
        //                if (!string.IsNullOrEmpty(accessToken))
        //                    context.Token = accessToken;
        //            }
        //            return Task.CompletedTask;
        //        }
        //    };
        //});

        if (!hostingEnvironment.IsDevelopment())
        {
        }
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        //context.Services.AddTransient<IClaimsTransformation, TenantClaimTransformer>();

        context.Services.AddMediator(c =>
        {
            c.AddConsumers(
            typeof(ObjectStorageServiceApplicationModule).Assembly,
            typeof(ObjectStorageServiceApplicationContractsModule).Assembly
            );
        });

        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();


        if (!configuration.GetValue<bool>("App:DisablePII"))
        {
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.LogCompleteSecurityArtifact = true;
        }

        if (!configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata"))
        {
            Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
            });
        }

        Configure<AbpAuditingOptions>(options =>
        {
            options.EntityHistorySelectors.AddAllEntities();
        });

        ConfigureAuthentication(context, configuration);
        ConfigureUrls(configuration);
        //ConfigureBundles();
        ConfigureConventionalControllers();
        //ConfigureSwagger(context, configuration);
        //ConfigureVirtualFileSystem(context);
        ConfiureCultures();

        ConfigureCors(context, configuration);
        ConfigureRedis(context, configuration);

        Configure<AbpMvcLibsOptions>(options =>
        {
            options.CheckLibs = false;
        });
        Configure<AbpAntiForgeryOptions>(c =>
        {
            c.AutoValidate = false;
        });
        Configure<AbpMultiTenancyOptions>(options =>
        {
            options.IsEnabled = true;
            options.DatabaseStyle = MultiTenancyDatabaseStyle.PerTenant;
        });
        Configure<AbpTenantResolveOptions>(options =>
        {
            options.TenantResolvers.Clear(); // optional: start clean

            // ✅ Only use these:
            options.TenantResolvers.Add(new HeaderTenantResolveContributor());
            options.TenantResolvers.Add(new QueryStringTenantResolveContributor());
            options.TenantResolvers.Add(new RouteTenantResolveContributor());

            // ❌ Don't include CurrentUserTenantResolveContributor
        });

        Configure<AbpBackgroundJobOptions>(options =>
        {
            options.IsJobExecutionEnabled = false;
        });

        Configure<AbpExceptionHandlingOptions>(options =>
        {
            options.SendStackTraceToClients = false;
            options.SendExceptionsDetailsToClients = false;
        });

        context.Services.AddTransient<IValidationFailureHandler, ValidationFailureHandler>();

        context.Services.AddSingleton<FrameworkExceptionMiddlware>();
        //context.Services.AddSingleton<HashtCookieAuthenticationEvents>();
        context.Services.AddSingleton<WrapAbpRoutesResponseMiddleware>();

        context.Services.AddFastEndpoints(c =>
        {
            c.IncludeAbstractValidators = true;
        })
            .FrameworkNSwagDocsPerModule(SwaggerDefinitions.GetSwaggerModules());

        //context.Services.Replace(ServiceDescriptor.Singleton<IAuthorizationPolicyProvider, BearerAwarePolicyProvider>());

    }

    private void ConfigureAuthentication(ServiceConfigurationContext context, IConfiguration configuration)
    {

        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            options.Applications["Angular"].RootUrl = configuration["App:AngularUrl"];
            // options.Applications["Angular"].Urls[AccountUrlNames.PasswordReset] = "account/reset-password";
            options.RedirectAllowedUrls.AddRange(
                configuration["App:RedirectAllowedUrls"]?.Split(',') ?? Array.Empty<string>()
            );
        });
    }

    //private void ConfigureBundles()
    //{
    //    Configure<AbpBundlingOptions>(options =>
    //    {
    //        options.StyleBundles.Configure(
    //            LeptonXLiteThemeBundles.Styles.Global,
    //            bundle =>
    //            {
    //                bundle.AddFiles("/global-scripts.js");
    //                bundle.AddFiles("/global-styles.css");
    //            }
    //        );
    //    });
    //}

    private void ConfigureVirtualFileSystem(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<ObjectStorageServiceDomainSharedModule>(
                    Path.Combine(
                        hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}ObjectStorageService.Domain.Shared"
                    )
                );
                options.FileSets.ReplaceEmbeddedByPhysical<ObjectStorageServiceDomainModule>(
                    Path.Combine(
                        hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}ObjectStorageService.Domain"
                    )
                );
                options.FileSets.ReplaceEmbeddedByPhysical<ObjectStorageServiceApplicationContractsModule>(
                    Path.Combine(
                        hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}ObjectStorageService.Application.Contracts"
                    )
                );
                options.FileSets.ReplaceEmbeddedByPhysical<ObjectStorageServiceApplicationModule>(
                    Path.Combine(
                        hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}ObjectStorageService.Application"
                    )
                );
            });
        }
    }

    private void ConfigureConventionalControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            // options.ConventionalControllers.Create(typeof(TaxApplicationModule).Assembly);
        });
    }

    private static void ConfigureSwagger(
        ServiceConfigurationContext context,
        IConfiguration configuration
    )
    {
        // context.Services.AddAbpSwaggerGenWithOidc(
        //     configuration["AuthServer:Authority"]!,
        //     ["Tax"],
        //     [AbpSwaggerOidcFlows.AuthorizationCode],
        //     null,
        //     options =>
        //     {
        //         options.SwaggerDoc("v1", new OpenApiInfo { Title = "Tax API", Version = "v1" });
        //         options.DocInclusionPredicate((docName, description) => true);
        //         options.CustomSchemaIds(type => type.FullName);
        //     });
    }

    private void ConfiureCultures()
    {
        Configure<AbpRequestLocalizationOptions>(options =>
        {
            options.RequestLocalizationOptionConfigurators.Add(async (serviceProvider, requestOptions) =>
            {
                requestOptions.DefaultRequestCulture = new RequestCulture("en");
                var supportedCultures = new[] { "en-US", "fa-IR", "fr" };

                requestOptions.AddSupportedCultures(supportedCultures);
                requestOptions.AddSupportedUICultures(supportedCultures);

                requestOptions.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider());
            });
        });

    }

    private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(
                        configuration["App:CorsOrigins"]
                            ?.Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(o => o.Trim().RemovePostFix("/"))
                            .ToArray() ?? Array.Empty<string>()
                    )
                    .WithAbpExposedHeaders()
                    .WithExposedHeaders("Content-Disposition", "Content-Type", "Content-Length")
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }
    private void ConfigureRedis(ServiceConfigurationContext context, IConfiguration configuration)
    {
        Configure<AbpDistributedCacheOptions>(options =>
        {
            options.KeyPrefix = "ObjectStorageService:";
        });

        if (configuration["Redis:IsEnabled"] == "true")
        {

            var redisHost = configuration["Redis:Host"];       // e.g., "192.168.1.53"
            var redisPort = configuration["Redis:Port"];       // e.g., "6379"
            var redisUser = configuration["Redis:Username"];   // optional
            var redisPassword = configuration["Redis:Password"];

            var configOptions = new StackExchange.Redis.ConfigurationOptions
            {
                EndPoints = { $"{redisHost}:{redisPort}" },
                Password = redisPassword,
                User = redisUser
            };

            context.Services.AddStackExchangeRedisCache(options =>
            {
                options.ConfigurationOptions = configOptions;
            });
        }
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        app.UseForwardedHeaders();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            //app.UseErrorPage();
        }

        app.UseMiddleware<FrameworkExceptionMiddlware>();
        app.MapAbpStaticAssets();
        //app.UseAbpStudioLink();
        app.UseRouting();
        app.UseMiddleware<WrapAbpRoutesResponseMiddleware>();
        app.UseAbpSecurityHeaders();
        app.UseCors();
        // app.UseAbpOpenIddictValidation();
        // app.UseMiddleware<SampleAuhtorizeMiddleware>();

        if (MultiTenancyConsts.IsEnabled)
        {
            //app.UseMiddleware<MyMultiTenancyMiddleware>();
            app.UseMultiTenancy();
        }
        app.UseAuthentication();

        //app.UseUnitOfWork();
        app.UseMiddleware<AbpUnitOfWorkMiddleware>();
        app.UseDynamicClaims();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            // Map root URL to Swagger redirect
            endpoints.MapGet("/", context =>
            {
                context.Response.Redirect("/swagger", permanent: false);
                return Task.CompletedTask;
            });

            // You can keep the default controller route if needed for other controllers
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        //app.UseConfiguredEndpoints(endpoints =>
        //{
        //    endpoints.MapHub<NotificationHub>("/notification", options =>
        //    {
        //        options.WebSockets.CloseTimeout = TimeSpan.FromSeconds(30);
        //    });
        //});
        app.UseFastEndpoints(cfg =>
        {
            cfg.Errors.ResponseBuilder = (failures, ctx, statusCode) =>
            {
                var validationFailureHandler =
                    ctx.RequestServices.GetRequiredService<IValidationFailureHandler>();
                return validationFailureHandler
                    .BuildValidationResponseAsync(failures, ctx, statusCode);
            };

        })
            .UseSwaggerGen();
    }
}

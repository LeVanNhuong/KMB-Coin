
using KMB_Coin.Services.Classes;
using KMB_Coin.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace KMB_Coin
{
    public class Startup
    {
        readonly string AllowAllOrigins = "AllowAllOrigins";
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IClock, Clock>();
            services.AddSingleton<IRedis, Redis>();
            services.AddSingleton<IBlockChain, BlockChain>();
            services.AddSingleton<ITransactionPool, TransactionPool>();
            services.AddSingleton<IWallet, Wallet>();
            services.AddSingleton<ITransactionMiner, TransactionMiner>();
            
            services.AddCors(options =>
            {
                options.AddPolicy(AllowAllOrigins,
                builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                    
                });
            });

            services.AddMvc().AddControllersAsServices()
                 .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                 .AddNewtonsoftJson(options =>
                 {
                     options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                     options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;

                     options.SerializerSettings.FloatParseHandling = Newtonsoft.Json.FloatParseHandling.Decimal;
                     options.SerializerSettings.FloatFormatHandling = Newtonsoft.Json.FloatFormatHandling.DefaultValue;
                     options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.None;
                     options.SerializerSettings.StringEscapeHandling = Newtonsoft.Json.StringEscapeHandling.Default;
                 });
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,IRedis redis)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors(AllowAllOrigins);


            app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto });


            app.UseStaticFiles();


            #region Websockets
           
            #endregion

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=api}/{action=ping}/{id?}");
            });

            
        }
    }
}

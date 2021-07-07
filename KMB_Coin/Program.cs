using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KMB_Coin.Models;
using KMB_Coin.Services.Classes;
using KMB_Coin.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace KMB_Coin
{
    public class Program
    {

        public static void Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Info("init main function"); 
                var host = CreateHostBuilder(args).Build(); 
                
                host.Run();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception => {ex.Message}");
                logger.Error(ex, "error in init");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            } 
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
             .ConfigureWebHostDefaults(webBuilder =>
             {
                 webBuilder.ConfigureKestrel(serverOptions =>
                 {
                 }).UseStartup<Startup>().ConfigureLogging(logging =>
                 {
                     logging.ClearProviders();
                     logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);

                 }).UseNLog().UseKestrel().ConfigureKestrel((context, options) =>
                 {
                  
                     options.Limits.MaxRequestBodySize = 10 * 10 * 1024;
                     
                     var port = args.Where(x => x.ToLower().StartsWith("port:")).Select(x => int.Parse(x.Replace("port:", string.Empty).Trim())).FirstOrDefault();


                     if (Debugger.IsAttached)
                     { 
                         options.Listen(IPAddress.Loopback, Constants.HTTP_PORT+1);
                     }
                     else
                     {
                         options.Listen(IPAddress.Loopback, port);
                     }
                 });
             });

    }
}

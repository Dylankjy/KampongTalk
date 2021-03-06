using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KampongTalk.Hubs;
using KampongTalk.Tools;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KampongTalk
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddSignalR();
            services.AddSession(options =>
            {
                // options.IdleTimeout = TimeSpan.FromMinutes(60);
            });
            services.AddSingleton<ITranslator, Translator>();
            services.AddSingleton<ISpeech, SpeechSythesizer>();
            services.AddTransient<IUserAttributes, UserAttributes>();
            services.AddTransient<IEventRecommender, EventRecommender>();

            services.AddMvc().AddRazorOptions(options =>
            {
                options.PageViewLocationFormats
                 .Add("/Pages/Partials/{0}.cshtml");
            });
            
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddControllers().AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );
            services.AddHttpContextAccessor();
            services.AddControllers();

            services.AddMvc();
            services.AddAntiforgery(options => options.HeaderName = "XSRF-TOKEN");
            services.Configure<IISServerOptions>(options => { options.AllowSynchronousIO = true; });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapHub<ChatHub>("/chatHub");
            });
        }
    }
}

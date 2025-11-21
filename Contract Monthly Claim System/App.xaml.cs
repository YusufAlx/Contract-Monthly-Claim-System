using System;
using System.IO;
using System.Windows;
using Quartz;
using Quartz.Impl;
using Contract_Monthly_Claim_System.Jobs;
using OfficeOpenXml;


namespace Contract_Monthly_Claim_System
{
    public partial class App : Application
    {
        private IScheduler _scheduler;

        protected override void OnStartup(StartupEventArgs e)
        {
            // EPPlus license setup (required in EPPlus 8+)
            try
            {
                ExcelPackage.License.SetNonCommercialPersonal("Your Name");
            }
            catch
            {
                // Fallback for older EPPlus versions
                try { ExcelPackage.LicenseContext = LicenseContext.NonCommercial; } catch { }
            }

            base.OnStartup(e);

            // Ensure folders exist
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uploads"));
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "invoices"));
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reports"));
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data"));

            // Start Quartz scheduler (optional)
            try
            {
                var factory = new StdSchedulerFactory();
                _scheduler = factory.GetScheduler().Result;
                _scheduler.Start().Wait();

                var job = JobBuilder.Create<AutoInvoiceJob>().WithIdentity("AutoInvoiceJob").Build();
                var trigger = TriggerBuilder.Create()
                    .WithIdentity("AutoInvoiceTrigger")
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever())
                    .Build();

                _scheduler.ScheduleJob(job, trigger).Wait();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Scheduler init error: {ex.Message}");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _scheduler?.Shutdown().Wait();
            base.OnExit(e);
        }
    }
}

/*
Customer_Success_for_C_Sharp_Developers_Succinctly.
Eduardo Freitas - 2016 - http://edfreitas.me
*/

#region "using"
using CrmCore;
using System;
using System.Threading.Tasks;
#endregion

#region "namespace"
namespace CustomerSuccess
{
    #region "program"
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();

            OpenFindCase();

            OpenMultipleCases();

            CrmExample.FindByDescription("Exception");

            CrmExample.FindByDateOpenedAfter(new DateTime(2016, 3, 4, 16, 03, 58, DateTimeKind.Utc));

            CrmExample.FindByDateOpenedBefore(new DateTime(2016, 3, 4, 16, 03, 57, DateTimeKind.Utc));

            CrmExample.FindByDatesBetween(new DateTime(2016, 3, 4, 16, 03, 57, DateTimeKind.Utc),
              new DateTime(2016, 3, 4, 16, 03, 58, DateTimeKind.Utc));

            CrmExample.FindByStatus(IncidentStatus.Reported);

            CrmExample.FindBySeverity(IncidentSeverity.Urgent);

            CrmExample.FindByFeedbackFrequency(IncidentFeedbackFrequency.Every4Hours);

            CrmExample.FindByCommunicationType(IncidentCommunicationType.Bidirectional);

            CrmExample.ChangePropertyValue("4b992d62-4750-47d2-ac4a-dbce2ce85c12", EnumUtils.stringValueOf(IncidentStatus.FeedbackRequested), "Status");
            CrmExample.AddComment("4b992d62-4750-47d2-ac4a-dbce2ce85c12", "jrobinson", "Request for feedback", "https://eu3.salesforce.com/500w0000015tJgV?srPos=0&srKp=500");
            CrmExample.AddResource("4b992d62-4750-47d2-ac4a-dbce2ce85c12", IncidentStatus.IssueFound, "ysagi", DateTime.Now, DateTime.Now);

            OpenWithComment();

            Console.ReadLine();
        }

        public static async void OpenFindCase()
        {
            await Task.Run(
            async () =>
            {
                string id = await CrmExample.OpenCase("Azure runtime issue, system critical error");

                Console.WriteLine(Environment.NewLine);

                CrmExample.CheckCase(id);
            });
        }

        public static async void OpenWithComment()
        {
            await Task.Run(
            async () =>
            {
                string id = await CrmExample.OpenCase("Business Objects is down");

                CrmExample.AddComment(id, "jrobinson", "Request for feedback", "https://eu3.salesforce.com/500w0000015tJgV?srPos=0&srKp=500");
            });
        }

        public static async void OpenMultipleCases()
        {
            await Task.Run(
            async () =>
            {
                await CrmExample.OpenCase("Export failing", IncidentStatus.Reported, IncidentSeverity.High, 
                    IncidentFeedbackFrequency.Every8Hours, IncidentCommunicationType.Bidirectional);

                await CrmExample.OpenCase("Change user login", IncidentStatus.Reported, IncidentSeverity.Low,
                    IncidentFeedbackFrequency.Weekly, IncidentCommunicationType.ReceiveUpdatesOnly);

                await CrmExample.OpenCase("SAP connection failed", IncidentStatus.Reopen, IncidentSeverity.Urgent,
                    IncidentFeedbackFrequency.Every4Hours, IncidentCommunicationType.Bidirectional);

                await CrmExample.OpenCase("PDF conversion error", IncidentStatus.WorkingOnFix, IncidentSeverity.Normal,
                    IncidentFeedbackFrequency.Daily, IncidentCommunicationType.ReceiveUpdatesOnly);

                await CrmExample.OpenCase("Critical Exception", IncidentStatus.UnderAnalysis, IncidentSeverity.Urgent,
                    IncidentFeedbackFrequency.Hourly, IncidentCommunicationType.Bidirectional);

                await CrmExample.OpenCase("VS 2015 Install", IncidentStatus.FixDelivered, IncidentSeverity.Low,
                    IncidentFeedbackFrequency.Monthly, IncidentCommunicationType.ReceiveUpdatesOnly);
            });
        }
    }
    #endregion
}
#endregion
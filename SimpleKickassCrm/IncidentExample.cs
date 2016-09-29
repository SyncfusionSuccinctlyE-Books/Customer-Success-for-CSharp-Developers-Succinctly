/*
Customer_Success_for_C_Sharp_Developers_Succinctly.
Eduardo Freitas - 2016 - http://edfreitas.me
*/

#region "using"
using System;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using System.Linq;
using System.Collections.Generic;
#endregion

#region "namespace"
namespace CrmCore
{
    #region "IncidentExample"
    public class CrmExample
    {
        private const string cStrCaseCreated = "Case created (as JSON) ->";
        private const string cStrTotalResults = "Total results: ";

        private const string cStrDescription = "Description: ";
        private const string cStrStatus = "Status: ";
        private const string cStrSeverity = "Severity: ";
        private const string cStrCommunication = "Communication: ";
        private const string cStrFrequency = "Frequency: ";
        private const string cStrOpened = "Opened: ";
        private const string cStrClosed = "Closed: ";
        
        private const string cStrDateTimeFormat = "dd-MMM-yyyy hh:mm:ss UTC";

        public static async Task<string> OpenCase(string description)
        {
            string id = string.Empty;

            using (Incident inc = new Incident(IncidentStatus.Reported,
                IncidentSeverity.High,
                IncidentFeedbackFrequency.Every4Hours,
                IncidentCommunicationType.Bidirectional))
            {
                var issue = await inc.Open(description);

                if (issue != null)
                {
                    var i = JsonConvert.DeserializeObject<IncidentInfo>(issue.ToString());

                    Console.WriteLine(cStrCaseCreated);
                    Console.WriteLine(issue.ToString());

                    id = issue.Id;
                }
            }

            return id;
        }

        public static async Task<string> OpenCase(string description, IncidentStatus st, IncidentSeverity sv, 
            IncidentFeedbackFrequency ff, IncidentCommunicationType ct)
        {
            string id = string.Empty;

            using (Incident inc = new Incident(st, sv, ff, ct))
            {
                var issue = await inc.Open(description);

                if (issue != null)
                {
                    var i = JsonConvert.DeserializeObject<IncidentInfo>(issue.ToString());

                    Console.WriteLine(cStrCaseCreated);
                    Console.WriteLine(issue.ToString());

                    id = issue.Id;
                }
            }

            return id;
        }

        public static void CheckCase(string id)
        {
            using (Incident inc = new Incident())
            {
                IEnumerable<Document> issues = inc.FindById(id);

                foreach (var issue in issues)
                    OutputCaseDetails(issue);
            }
        }

        public static void FindByDescription(string description)
        {
            using (Incident inc = new Incident())
            {
                IEnumerable<IncidentInfo> issues = inc.FindByDescription(description);

                Console.WriteLine("FindByDescription: " + description);

                if (issues.Count() > 0)
                {
                    Console.Write(Environment.NewLine);

                    foreach (var issue in issues)
                    {
                        OutputCaseDetails(issue);
                        Console.Write(Environment.NewLine);
                    }
                }

                Console.WriteLine(cStrTotalResults + issues.Count().ToString());

            }
        }

        public static void FindByDateOpenedAfter(DateTime opened)
        {
            using (Incident inc = new Incident())
            {
                IEnumerable<IncidentInfo> issues = inc.FindByDateOpenedAfter(opened);

                Console.WriteLine("FindByDateOpenedAfter: " + opened.ToString(cStrDateTimeFormat));

                if (issues.Count() > 0)
                {
                    Console.Write(Environment.NewLine);

                    foreach (var issue in issues)
                    {
                        OutputCaseDetails(issue);
                        Console.Write(Environment.NewLine);
                    }
                }

                Console.WriteLine(cStrTotalResults + issues.Count().ToString());
            }
        }

        public static void FindByDateOpenedBefore(DateTime opened)
        {
            using (Incident inc = new Incident())
            {
                IEnumerable<IncidentInfo> issues = inc.FindByDateOpenedBefore(opened);

                Console.WriteLine("FindByDateOpenedBefore: " + opened.ToString(cStrDateTimeFormat));

                if (issues.Count() > 0)
                {
                    Console.Write(Environment.NewLine);

                    foreach (var issue in issues)
                    {
                        OutputCaseDetails(issue);
                        Console.Write(Environment.NewLine);
                    }
                }

                Console.WriteLine(cStrTotalResults + issues.Count().ToString());
            }
        }

        public static void FindByDatesBetween(DateTime start, DateTime end)
        {
            using (Incident inc = new Incident())
            {
                IEnumerable<IncidentInfo> issues = inc.FindByDateOpenedBetween(start, end);

                Console.WriteLine("FindByDatesBetween: " + start.ToString(cStrDateTimeFormat)
                    + " and " + end.ToString(cStrDateTimeFormat));

                if (issues.Count() > 0)
                {
                    Console.Write(Environment.NewLine);

                    foreach (var issue in issues)
                    {
                        OutputCaseDetails(issue);
                        Console.Write(Environment.NewLine);
                    }
                }

                Console.WriteLine(cStrTotalResults + issues.Count().ToString());
            }
        }

        public static void FindByStatus(IncidentStatus status)
        {
            using (Incident inc = new Incident())
            {
                IEnumerable<IncidentInfo> issues = inc.FindByStatus(status);

                Console.WriteLine("FindByStatus: " + EnumUtils.stringValueOf(status));

                if (issues.Count() > 0)
                {
                    Console.Write(Environment.NewLine);

                    foreach (var issue in issues)
                    {
                        OutputCaseDetails(issue);
                        Console.Write(Environment.NewLine);
                    }
                }

                Console.WriteLine(cStrTotalResults + issues.Count().ToString());
            }
        }

        public static void FindBySeverity(IncidentSeverity severity)
        {
            using (Incident inc = new Incident())
            {
                IEnumerable<IncidentInfo> issues = inc.FindBySeverity(severity);

                Console.WriteLine("FindBySeverity: " + EnumUtils.stringValueOf(severity));

                if (issues.Count() > 0)
                {
                    Console.Write(Environment.NewLine);

                    foreach (var issue in issues)
                    {
                        OutputCaseDetails(issue);
                        Console.Write(Environment.NewLine);
                    }
                }

                Console.WriteLine(cStrTotalResults + issues.Count().ToString());
            }
        }

        public static void FindByFeedbackFrequency(IncidentFeedbackFrequency ff)
        {
            using (Incident inc = new Incident())
            {
                IEnumerable<IncidentInfo> issues = inc.FindByFeedbackFrequency(ff);

                Console.WriteLine("FindByFeedbackFrequency: " + EnumUtils.stringValueOf(ff));

                if (issues.Count() > 0)
                {
                    Console.Write(Environment.NewLine);

                    foreach (var issue in issues)
                    {
                        OutputCaseDetails(issue);
                        Console.Write(Environment.NewLine);
                    }
                }

                Console.WriteLine(cStrTotalResults + issues.Count().ToString());
            }
        }

        public static void FindByCommunicationType(IncidentCommunicationType ct)
        {
            using (Incident inc = new Incident())
            {
                IEnumerable<IncidentInfo> issues = inc.FindByCommunicationType(ct);

                Console.WriteLine("FindByCommunicationType: " + EnumUtils.stringValueOf(ct));

                if (issues.Count() > 0)
                {
                    Console.Write(Environment.NewLine);

                    foreach (var issue in issues)
                    {
                        OutputCaseDetails(issue);
                        Console.Write(Environment.NewLine);
                    }
                }

                Console.WriteLine(cStrTotalResults + issues.Count().ToString());
            }
        }

        public static async void ChangePropertyValue(string id, string propValue, string propName)
        {
            using (Incident inc = new Incident())
            {
                await Task.Run(
                async () =>
                {
                    IncidentInfo ic = await inc.ChangePropertyValue(id, propValue, propName);

                    OutputCaseDetails(ic);
                });
            }
        }

        public static async void AddResource(string id, IncidentStatus stage, string engineer, DateTime st, DateTime end)
        {
            using (Incident inc = new Incident())
            {
                await Task.Run(
                async () =>
                {
                    IncidentInfo ic = await inc.AddResource(id, stage, engineer, st, end);

                    OutputCaseDetails(ic);
                });
            }
        }

        public static async void AddComment(string id, string userId, string comment, string attachUrl)
        {
            using (Incident inc = new Incident())
            {
                await Task.Run(
                async () =>
                {
                    IncidentInfo ic = await inc.AddComment(id, userId, comment, attachUrl);

                    OutputCaseDetails(ic);
                });
            }
        }

        private static void OutputCaseDetails(object issue)
        {
            IncidentInfo i = (issue is IncidentInfo) ? (IncidentInfo)issue :
                JsonConvert.DeserializeObject<IncidentInfo>(issue.ToString());

            Console.WriteLine(cStrDescription + i?.Description);
            Console.WriteLine(cStrStatus + i?.Status);
            Console.WriteLine(cStrSeverity + i?.Severity);
            Console.WriteLine(cStrCommunication + i?.CommunicationType);
            Console.WriteLine(cStrFrequency + i?.FeedbackFrequency);
            Console.WriteLine(cStrOpened + i?.Opened?.Date.ToString(cStrDateTimeFormat));
            Console.WriteLine(cStrClosed + i?.Closed?.Date.ToString(cStrDateTimeFormat));
        }
    }
    #endregion
}
#endregion

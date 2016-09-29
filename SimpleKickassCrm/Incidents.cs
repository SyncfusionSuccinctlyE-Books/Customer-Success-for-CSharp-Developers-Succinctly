/*
Customer_Success_for_C_Sharp_Developers_Succinctly.
Eduardo Freitas - 2016 - http://edfreitas.me
*/

#region "using"
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
#endregion

#region "namespace"
namespace CrmCore
{
    #region "DateEpoch"
    public static class Extensions
    {
        public static int ToEpoch(this DateTime date)
        {
            if (date == null) return int.MinValue;

            DateTime epoch = new DateTime(1970, 1, 1);
            TimeSpan epochTimeSpan = date - epoch;

            return (int)epochTimeSpan.TotalSeconds;
        }
    }

    public class DateEpoch
    {
        public DateTime Date { get; set; }
        public int Epoch
        {
            get
            {
                return (Date.Equals(null) || Date.Equals(DateTime.MinValue))
                    ? DateTime.UtcNow.ToEpoch()
                    : Date.ToEpoch();
            }
        }

        public DateEpoch(DateTime dt)
        {
            Date = dt;
        }
    }
    #endregion

    #region "CrmCore"
    #region "EnumUtils"
    public class EnumUtils
    {
        protected const string cStrExcep = "The string is not a description or value of the specified enum.";

        public static string stringValueOf(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])
                fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
        }

        public static object enumValueOf(string value, Type enumType)
        {
            string[] names = Enum.GetNames(enumType);
            foreach (string name in names)
            {
                if (stringValueOf((Enum)Enum.Parse(enumType, name)).Equals(value))
                {
                    return Enum.Parse(enumType, name);
                }
            }

            throw new ArgumentException(cStrExcep);
        }
    }

    public enum IncidentSeverity {
        [Description("Urgent")]
        Urgent,
        [Description("High")]
        High,
        [Description("Normal")]
        Normal,
        [Description("Low")]
        Low
    };

    public enum IncidentFeedbackFrequency {
        [Description("Hourly")]
        Hourly,
        [Description("Every4Hours")]
        Every4Hours,
        [Description("Every8Hours")]
        Every8Hours,
        [Description("Daily")]
        Daily,
        [Description("Every2Days")]
        Every2Days,
        [Description("Weekly")]
        Weekly,
        [Description("Every2Weeks")]
        Every2Weeks,
        [Description("Monthly")]
        Monthly
    };

    public enum IncidentCommunicationType {
        [Description("ReceiveUpdatesOnly")]
        ReceiveUpdatesOnly,
        [Description("Bidirectional")]
        Bidirectional
    };

    public enum IncidentStatus {
        [Description("NotReported")]
        NotReported,
        [Description("Reported")]
        Reported,
        [Description("FeedbackRequested")]
        FeedbackRequested,
        [Description("UnderAnalysis")]
        UnderAnalysis,
        [Description("IssueFound")]
        IssueFound,
        [Description("WorkingOnFix")]
        WorkingOnFix,
        [Description("FixDelivered")]
        FixDelivered,
        [Description("FixAccepted")]
        FixAccepted,
        [Description("Solved")]
        Solved,
        [Description("Closed")]
        Closed,
        [Description("Reopen")]
        Reopen
    };
    #endregion

    #region "CaseInfo"
    public sealed class AllocatedResource
    {
        private IncidentStatus stage;

        public string Engineer { get; set; }

        public string Stage {
            get
            {
                return EnumUtils.stringValueOf(stage);
            }
            set
            {
                stage = (IncidentStatus)EnumUtils.
                    enumValueOf(value, typeof(IncidentStatus));
            }
        }

        public DateEpoch Start { get; set; }
        public DateEpoch End { get; set; }
    }

    public sealed class Comment
    {
        public string Description { get; set; }
        public string UserId { get; set; }
        public string AttachmentUrl { get; set; }
        public DateEpoch When { get; set; }
    }

    public sealed class IncidentInfo
    {
        private IncidentSeverity severity;
        private IncidentStatus status;
        private IncidentFeedbackFrequency feedbackFrequency;
        private IncidentCommunicationType communicationType;

        public string Description { get; set; }

        public string Severity
        {
            get
            {
                return EnumUtils.stringValueOf(severity);
            }
            set
            {
                severity = (IncidentSeverity)EnumUtils.
                    enumValueOf(value, typeof(IncidentSeverity));
            }
        }

        public string Status
        {
            get
            {
                return EnumUtils.stringValueOf(status);
            }
            set
            {
                status = (IncidentStatus)EnumUtils.
                    enumValueOf(value, typeof(IncidentStatus));
            }
        }

        public string FeedbackFrequency
        {
            get
            {
                return EnumUtils.stringValueOf(feedbackFrequency);
            }
            set
            {
                feedbackFrequency = (IncidentFeedbackFrequency)EnumUtils.
                    enumValueOf(value, typeof(IncidentFeedbackFrequency));
            }
        }

        public string CommunicationType
        {
            get
            {
                return EnumUtils.stringValueOf(communicationType);
            }
            set
            {
                communicationType = (IncidentCommunicationType)EnumUtils.
                    enumValueOf(value, typeof(IncidentCommunicationType));
            }
        }

        public AllocatedResource[] Resources { get; set; }
        public Comment[] Comments { get; set; }
        public DateEpoch Opened { get; set; }
        public DateEpoch Closed { get; set; }
    }
    #endregion

    public class Incident : IDisposable
    {
        #region "vars"
        private bool disposed = false;

        private const string docDbUrl = "dbs/SimpleAwesomeCrm/colls/CrmObjects";
        private const string docDbEndpointUrl = "https://fastapps.documents.azure.com:443/"; // change to your DocDb endpoint
        private const string docDbAuthorizationKey = "<<Your DocDB authorization key>>";

        private const string cStrSeverityProp = "Severity";
        private const string cStrStatusProp = "Status";
        private const string cStrFrequencyProp = "FeedbackFrequency";
        private const string cStrCTProp = "CommunicationType";
        private const string cStrClosedProp = "Closed";
        private const string cStrComments = "Comments";
        private const string cStrResources = "Resources";
        private const string cStrOpened = "Opened";

        public IncidentInfo info = null;

        protected DocumentClient client = null;
        #endregion

        #region "properties"
        #endregion

        #region "destructor"
        ~Incident()
        {
            Dispose(false);
        }
        #endregion

        #region "constructor"
        public Incident()
        {
            info = new IncidentInfo();

            info.Status = EnumUtils.stringValueOf(IncidentStatus.Reported);
            info.Severity = EnumUtils.stringValueOf(IncidentSeverity.Normal);
            info.FeedbackFrequency = EnumUtils.stringValueOf(IncidentFeedbackFrequency.Daily);
            info.CommunicationType = EnumUtils.stringValueOf(IncidentCommunicationType.ReceiveUpdatesOnly);
            info.Opened = new DateEpoch(DateTime.UtcNow);
            info.Resources = null;
            info.Comments = null;
        }

        public Incident(IncidentStatus status, IncidentSeverity severity, IncidentFeedbackFrequency freq, IncidentCommunicationType comType)
        {
            info = new IncidentInfo();

            info.Status = EnumUtils.stringValueOf(status);
            info.Severity = EnumUtils.stringValueOf(severity);
            info.FeedbackFrequency = EnumUtils.stringValueOf(freq);
            info.CommunicationType = EnumUtils.stringValueOf(comType);
            info.Opened = new DateEpoch(DateTime.UtcNow);
            info.Resources = null;
            info.Comments = null;
        }
        #endregion

        #region "methods"
        private void Connect2DocDb()
        {
            client = new DocumentClient(new Uri(docDbEndpointUrl), docDbAuthorizationKey);
        }

        public IEnumerable<Document> FindById(string id)
        {
            if (client == null)
                Connect2DocDb();

            if (info != null && client != null)
            {
                var cases =
                    from c in client.CreateDocumentQuery(docDbUrl)
                    where c.Id == id
                    select c;

                return cases;
            }
            else
                return null;
        }

        public IEnumerable<IncidentInfo> FindByDescription(string description)
        {
            if (client == null)
                Connect2DocDb();

            if (info != null && client != null)
            {
                var cases =
                    from c in client.CreateDocumentQuery<IncidentInfo>(docDbUrl)
                    where c.Description.ToUpper().
                        Contains(description.ToUpper())
                    select c;

                return cases;
            }
            else
                return null;
        }

        public IEnumerable<IncidentInfo> FindByDateOpenedAfter(DateTime opened)
        {
            if (client == null)
                Connect2DocDb();

            if (info != null && client != null)
            {
                var cases =
                    from c in client.CreateDocumentQuery<IncidentInfo>(docDbUrl)
                    where c.Opened.Epoch >= opened.ToEpoch()
                    select c;

                return cases;
            }
            else
                return null;
        }

        public IEnumerable<IncidentInfo> FindByDateOpenedBefore(DateTime opened)
        {
            if (client == null)
                Connect2DocDb();

            if (info != null && client != null)
            {
                var cases =
                    from c in client.CreateDocumentQuery<IncidentInfo>(docDbUrl)
                    where c.Opened.Epoch < opened.ToEpoch()
                    select c;

                return cases;
            }
            else
                return null;
        }

        public IEnumerable<IncidentInfo> FindByDateOpenedBetween(DateTime start, DateTime end)
        {
            if (client == null)
                Connect2DocDb();

            if (info != null && client != null)
            {
                var cases =
                    from c in client.CreateDocumentQuery<IncidentInfo>(docDbUrl)
                    where c.Opened.Epoch >= start.ToEpoch()
                    where c.Opened.Epoch < end.ToEpoch()
                    select c;

                return cases;
            }
            else
                return null;
        }

        public IEnumerable<IncidentInfo> FindByStatus(IncidentStatus status)
        {
            if (client == null)
                Connect2DocDb();

            if (info != null && client != null)
            {
                var cases =
                    from c in client.CreateDocumentQuery<IncidentInfo>(docDbUrl)
                    where c.Status == status.ToString()
                    select c;

                return cases;
            }
            else
                return null;
        }

        public IEnumerable<IncidentInfo> FindBySeverity(IncidentSeverity severity)
        {
            if (client == null)
                Connect2DocDb();

            if (info != null && client != null)
            {
                var cases =
                    from c in client.CreateDocumentQuery<IncidentInfo>(docDbUrl)
                    where c.Severity == severity.ToString()
                    select c;

                return cases;
            }
            else
                return null;
        }

        public IEnumerable<IncidentInfo> FindByFeedbackFrequency(IncidentFeedbackFrequency ff)
        {
            if (client == null)
                Connect2DocDb();

            if (info != null && client != null)
            {
                var cases =
                    from c in client.CreateDocumentQuery<IncidentInfo>(docDbUrl)
                    where c.FeedbackFrequency == ff.ToString()
                    select c;

                return cases;
            }
            else
                return null;
        }

        public IEnumerable<IncidentInfo> FindByCommunicationType(IncidentCommunicationType ct)
        {
            if (client == null)
                Connect2DocDb();

            if (info != null && client != null)
            {
                var cases =
                    from c in client.CreateDocumentQuery<IncidentInfo>(docDbUrl)
                    where c.CommunicationType == ct.ToString()
                    select c;

                return cases;
            }
            else
                return null;
        }

        public async Task<Document> Open(string description, bool check = false)
        {
            if (client == null)
                Connect2DocDb();

            if (info != null && client != null)
            {
                info.Description = description;

                Document id = await client.CreateDocumentAsync(docDbUrl, info);

                if (check)
                    return (id != null) ? client.CreateDocumentQuery(docDbUrl).
                        Where(d => d.Id == id.Id).AsEnumerable().FirstOrDefault() :
                        null;
                else
                    return id;
            }
            else
                return null;
        }

        public async Task<IncidentInfo> AddResource(string id, IncidentStatus stage, string engineer, DateTime st, DateTime end)
        {
            if (client == null)
                Connect2DocDb();

            if (info != null && client != null)
            {
                var cases =
                    from c in client.CreateDocumentQuery(docDbUrl)
                    where c.Id.ToUpper().Contains(id.ToUpper())
                    select c;

                IncidentInfo issue = null;
                Document oDoc = null;

                foreach (var cs in cases)
                {
                    var it = await client.ReadDocumentAsync(cs.AltLink);

                    oDoc = it;
                    issue = (IncidentInfo)(dynamic)it.Resource;

                    break;
                }

                if (oDoc != null)
                {
                    AllocatedResource rc = new AllocatedResource();

                    rc.End = new DateEpoch((end != null) ? end.ToUniversalTime() : DateTime.UtcNow);
                    rc.Engineer = engineer;
                    rc.Stage = EnumUtils.stringValueOf(stage);
                    rc.Start = new DateEpoch((st != null) ? st.ToUniversalTime() : DateTime.UtcNow);

                    List<AllocatedResource> rRsc = new List<AllocatedResource>();

                    if (issue?.Resources?.Length > 0)
                    {
                        rRsc.AddRange(issue.Resources);
                        rRsc.Add(rc);

                        oDoc.SetPropertyValue(cStrResources, rRsc.ToArray());
                    }
                    else
                    {
                        rRsc.Add(rc);
                        oDoc.SetPropertyValue(cStrResources, rRsc.ToArray());
                    }

                    var updated = await client.ReplaceDocumentAsync(oDoc);
                    issue = (IncidentInfo)(dynamic)updated.Resource;
                }

                return issue;
            }
            else
                return null;
        }

        public async Task<IncidentInfo> AddComment(string id, string userId, string comment, string attachUrl)
        {
            if (client == null)
                Connect2DocDb();

            if (info != null && client != null)
            {
                var cases =
                    from c in client.CreateDocumentQuery(docDbUrl)
                    where c.Id.ToUpper().Contains(id.ToUpper())
                    select c;

                IncidentInfo issue = null;
                Document oDoc = null;

                foreach (var cs in cases)
                {
                    var it = await client.ReadDocumentAsync(cs.AltLink);

                    oDoc = it;
                    issue = (IncidentInfo)(dynamic)it.Resource;

                    break;
                }

                if (oDoc != null)
                {
                    Comment c = new Comment();

                    c.AttachmentUrl = attachUrl;
                    c.Description = comment;
                    c.UserId = userId;
                    c.When = new DateEpoch(DateTime.UtcNow);

                    List<Comment> cMts = new List<Comment>();

                    if (issue?.Comments?.Length > 0) {
                        cMts.AddRange(issue.Comments);
                        cMts.Add(c);

                        oDoc.SetPropertyValue(cStrComments, cMts.ToArray());
                    }
                    else {
                        cMts.Add(c);
                        oDoc.SetPropertyValue(cStrComments, cMts.ToArray());
                    }

                    var updated = await client.ReplaceDocumentAsync(oDoc);
                    issue = (IncidentInfo)(dynamic)updated.Resource;
                }

                return issue;
            }
            else
                return null;
        }

        public async Task<IncidentInfo> ChangePropertyValue(string id, string propValue, string propName)
        {
            if (client == null)
                Connect2DocDb();

            if (info != null && client != null)
            {
                var cases =
                    from c in client.CreateDocumentQuery(docDbUrl)
                    where c.Id.ToUpper().Contains(id.ToUpper())
                    select c;

                IncidentInfo issue = null;
                Document oDoc = null;

                foreach (var cs in cases)
                {
                    var it = await client.ReadDocumentAsync(cs.AltLink);

                    oDoc = it;
                    issue = (IncidentInfo)(dynamic)it.Resource;
                    
                    break;
                }

                if (oDoc != null)
                {
                    switch (propName)
                    {
                        case cStrSeverityProp:
                            oDoc.SetPropertyValue(cStrSeverityProp, propValue);
                            break;

                        case cStrStatusProp:
                            oDoc.SetPropertyValue(cStrStatusProp, propValue);
                            break;

                        case cStrFrequencyProp:
                            oDoc.SetPropertyValue(cStrFrequencyProp, propValue);
                            break;

                        case cStrCTProp:
                            oDoc.SetPropertyValue(cStrCTProp, propValue);
                            break;

                        case cStrClosedProp:
                            oDoc.SetPropertyValue(cStrClosedProp, issue.Closed);
                            break;
                    }

                    var updated = await client.ReplaceDocumentAsync(oDoc);
                    issue = (IncidentInfo)(dynamic)updated.Resource;
                }

                return issue;
            }
            else
                return null;
        }
        #endregion

        #region "Protected Dispose"
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        /// <remarks>
        /// If the main class was marked as sealed, we could just make this a private void Dispose(bool).  Alternatively, we could (in this case) put
        /// all of our logic directly in Dispose().
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    client.Dispose();
                }
            }

            disposed = true;
        }
        #endregion

        #region "Dispose"
        /// <summary>
        /// Dispose() --> Performs Internals defined tasks associated with freeing, releasing, or resetting managed and unmanaged resources.
        /// </summary>
        /// <example><code>s.Dispose();</code></example>
        public void Dispose()
        {
            // We start by calling Dispose(bool) with true
            Dispose(true);

            // Now suppress finalization for this object, since we've already handled our resource cleanup tasks
            GC.SuppressFinalize(this);
        }
        #endregion "Dispose"
    }
    #endregion
}
#endregion
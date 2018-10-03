using System;
using Atlassian.Jira;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace jiratesting
{
    class MainClass
    {
        struct Issue
        {
            public string ProjectKey;
            public string IssueKey;
            public string Assignee;
            public string Created;
			public string WeekBeginning;
            public string DueDate;
            public string FixVersions;
            public string Priority;
            public string Status;
            public string Summary;
            public string Reporter;
			public string AffectsVersions;
        }

        public static void Main(string[] args)
        {         
            // create a connection to JIRA using the Rest client
			Jira jira = Jira.CreateRestClient("<url>", "<user>", "<password>");

            // use LINQ syntax to retrieve issues
            jira.Issues.MaxIssuesPerRequest = int.MaxValue;
            var issues = from i in jira.Issues.Queryable
                         where i.Project == "<key>" //the Jira project key
                         orderby i.Created
                         select i;

            List<Issue> issueList = new List<Issue>();
			List<string> rowsInCsv = new List<string>();
			rowsInCsv.Add("Project,Issue Key,Assignee,Date Created,Week Beginning,Due Date,Affects Versions,Fix Versions,Priority,Status,Summary,Reporter");

            foreach (var issue in issues)
            {
				List<string> versions = new List<string>();
				foreach (var version in issue.FixVersions)
				{
					versions.Add(version.ToString());
				}
				string versionsString = String.Join(" ", versions);

				List<string> affectedVersions = new List<string>();
				foreach (var version in issue.AffectsVersions)
                {
					affectedVersions.Add(version.ToString());
                }
				string affectedVersionsString = String.Join(" ", affectedVersions);
                
				DateTime weekBeginning = issue.Created.Value.StartOfWeek(DayOfWeek.Monday);
                

                Issue newIssue;
                newIssue.ProjectKey = issue.Project;
                newIssue.IssueKey = issue.Key.ToString();
                newIssue.Assignee = issue.Assignee;
				newIssue.WeekBeginning = weekBeginning.ToString();
                newIssue.Created = issue.Created.ToString();
                newIssue.DueDate = issue.DueDate.ToString();
				newIssue.AffectsVersions = affectedVersionsString;
				newIssue.FixVersions = versionsString;
                newIssue.Priority = issue.Priority.ToString();
                newIssue.Status = issue.Status.ToString();
                newIssue.Summary = issue.Summary.ToString();
                newIssue.Reporter = issue.Reporter;


				if (newIssue.Summary.Contains('"'))
				{
					newIssue.Summary = newIssue.Summary.Replace('"', ' ');
				}

				if (newIssue.FixVersions.Contains('"'))
                {
					newIssue.FixVersions = newIssue.FixVersions.Replace('"', ' ');
                }
                
				issueList.Add(newIssue);
				rowsInCsv.Add(newIssue.ProjectKey + "," +
							  newIssue.IssueKey + "," +
							  newIssue.Assignee + "," +
							  newIssue.Created + "," +
				              newIssue.WeekBeginning + "," +
							  newIssue.DueDate + "," +
				              newIssue.AffectsVersions + "," +
							  newIssue.FixVersions + "," +
							  newIssue.Priority + "," +
							  newIssue.Status + ",\"" +
							  newIssue.Summary + "\"," +
							  newIssue.Reporter);
            }

			string csv = String.Join(",\n", rowsInCsv);

			File.WriteAllText("JiraIssues.csv", csv);
        }
    }

	public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}

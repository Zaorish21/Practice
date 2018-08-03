using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TFS_help
{
    class TFSmodule
    {
        private TfsTeamProjectCollection ProjectCollection;
        private WorkItemStore WorkItemStore;
        private Project teamProject;
        private WorkItemLinkType LinkType;

        public enum NodeCollectionType
        {
            AreaPaths,
            Iterations
        }

        public bool ConnectionState
        {
            get;
            set;
        }

        public void Connect()
        {
            TeamProjectPicker projectPicker = new TeamProjectPicker();
            projectPicker.ShowDialog();
            if (projectPicker.SelectedTeamProjectCollection != null)
            {
                ProjectCollection = projectPicker.SelectedTeamProjectCollection;
                ProjectCollection.EnsureAuthenticated();
                ConnectionState = ProjectCollection.HasAuthenticated;
                WorkItemStore = (WorkItemStore)ProjectCollection.GetService(typeof(WorkItemStore));
                WorkItemStore.WorkItemLinkTypes.TryGetByName("System.LinkTypes.Hierarchy", out LinkType);
                teamProject = WorkItemStore.Projects[projectPicker.SelectedProjects[0].Name];
            }
        }

        public void RecCollection(NodeCollection collection, List<string> list)
        {

            foreach (Node item in collection)
            {
                list.Add(item.Path);
                if (item.HasChildNodes)
                {
                    RecCollection(item.ChildNodes, list);
                }
            }
        }

        public List<string> GetNodeCollection(NodeCollectionType type)
        {
            NodeCollection collection;
            switch (type)
            {
                case NodeCollectionType.AreaPaths:
                    collection = teamProject.AreaRootNodes;
                    break;
                case NodeCollectionType.Iterations:
                    collection = teamProject.IterationRootNodes;
                    break;
                default:
                    return null;
            }
            List<string> listOfNodes = new List<string>() { teamProject.Name };
            RecCollection(collection, listOfNodes);
            return listOfNodes;
        }

        public WorkItem FindWorkItem(int id)
        {
            var temp = this.WorkItemStore.GetWorkItem(id);
            if (temp != null)
            {
                return temp;
            }
            return null;
        }

        public bool AddWorkItems(out string outMessage, WorkItem ParentWorkItem, List<WorkItemDefination> workItemsList, string WorkItemType )
        {
            try
            {
                WorkItemType workItemType = teamProject.WorkItemTypes[WorkItemType];
                foreach (WorkItemDefination item in workItemsList)
                {
                    WorkItem tempWorkItem = new WorkItem(workItemType);
                    tempWorkItem.Title = item.Title;
                    tempWorkItem.AreaPath = item.Area;
                    tempWorkItem.IterationPath = item.Iteration;
                    tempWorkItem.Fields["System.AssignedTo"].Value = item.AssignedTo;
                    tempWorkItem.Description = item.Description;
                    WorkItemLinkTypeEnd linkTypeEnd = WorkItemStore.WorkItemLinkTypes.LinkTypeEnds[LinkType.ReverseEnd.Name];
                    tempWorkItem.Links.Add(new RelatedLink(linkTypeEnd, ParentWorkItem.Id));
                    tempWorkItem.Save();
                }
                ParentWorkItem.Save();
                outMessage = "WorkItems successfully added to the server.";
                return true;
            }
            catch (Exception e)
            {
                outMessage = e.Message;
                return false;
            }
        }

        public List<string> GetUsersList()
        {
            IIdentityManagementService ims = ProjectCollection.GetService<IIdentityManagementService>();

            List<string> listUsers = new List<string>();

            TeamFoundationIdentity group = ims.ReadIdentity(
                IdentitySearchFactor.DisplayName,
                $"[{teamProject.Name}]\\Project Valid Users",
                MembershipQuery.Expanded,
                ReadIdentityOptions.ExtendedProperties);

            List<string> memebersIds = new List<string>();
            foreach (var member in group.Members) memebersIds.Add(member.Identifier);

            var _members = ims.ReadIdentities(
                IdentitySearchFactor.Identifier,
                memebersIds.ToArray(),
                MembershipQuery.Expanded,
                ReadIdentityOptions.ExtendedProperties);

            foreach (TeamFoundationIdentity[] member in _members)
                if (!member[0].IsContainer) listUsers.Add(member[0].DisplayName);

            return listUsers;
        }
    }
}

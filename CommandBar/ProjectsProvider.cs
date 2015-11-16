using EnvDTE;
using EnvDTE80;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandBar
{
    public class ProjectsProvider : ISuggestionsProvider
    {
        public ProjectsProvider(DTE2 dte)
        {
            this.Dte = dte;
        }

        public DTE2 Dte { get; private set; }

        public IEnumerable GetSuggestions(string filter)
        {
            var solution = this.Dte.Solution;
            var projects = solution.Projects;

            List<Project> projectsList = new List<Project>();
            var item = projects.GetEnumerator();
            while (item.MoveNext())
            {
                var project = item.Current as Project;
                if (project == null)
                {
                    continue;
                }

                if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    projectsList.AddRange(GetSolutionFolderProjects(project));
                }
                else
                {
                    projectsList.Add(project);
                }
            }

            return projectsList.Select(p => p.FullName);
        }

        private static IEnumerable<Project> GetSolutionFolderProjects(Project solutionFolder)
        {
            List<Project> projectsList = new List<Project>();
            for (var i = 1; i <= solutionFolder.ProjectItems.Count; i++)
            {
                var subProject = solutionFolder.ProjectItems.Item(i).SubProject;
                if (subProject == null)
                {
                    continue;
                }

                if (subProject.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    projectsList.AddRange(GetSolutionFolderProjects(subProject));
                }
                else
                {
                    projectsList.Add(subProject);
                }
            }

            return projectsList;
        }
    }
}

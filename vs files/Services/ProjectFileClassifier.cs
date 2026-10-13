using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Snapcode.Services
{
    public enum ProjectFileType
    {
        VisualPage,      // Managed by UI Designer + Blockly (has .snapcode/pages/{name}_meta.json or is main)
        CustomView,      // Reusable visual component template
        PureDartFile     // Plain Dart file (service, model, helper, etc.) - edited in Code Editor only
    }

    /// <summary>
    /// Distinguishes between visual designer pages and custom Dart files.
    /// Ensures pure Dart files created in the Code Editor NEVER appear in the UI designer or View Manager.
    /// </summary>
    public static class ProjectFileClassifier
    {
        /// <summary>
        /// Checks if a page name corresponds to a visual designer page.
        /// </summary>
        public static bool IsVisualPage(string projectRoot, string pageName)
        {
            if (string.Equals(pageName, "main", StringComparison.OrdinalIgnoreCase))
                return true;

            string metaPath = Path.Combine(ProjectService.GetPagesDirectory(projectRoot), $"{pageName}_meta.json");
            return File.Exists(metaPath);
        }

        /// <summary>
        /// Returns all visual pages defined in .snapcode/pages.
        /// </summary>
        public static List<string> GetVisualPages(string projectRoot)
        {
            var pages = new List<string> { "main" };
            string pagesDir = ProjectService.GetPagesDirectory(projectRoot);

            if (Directory.Exists(pagesDir))
            {
                foreach (var metaFile in Directory.GetFiles(pagesDir, "*_meta.json"))
                {
                    string fileName = Path.GetFileName(metaFile);
                    string pageName = fileName.Substring(0, fileName.Length - "_meta.json".Length);
                    if (!pages.Any(p => string.Equals(p, pageName, StringComparison.OrdinalIgnoreCase)))
                    {
                        pages.Add(pageName);
                    }
                }
            }
            return pages;
        }

        /// <summary>
        /// Determines the ProjectFileType for a given file path.
        /// </summary>
        public static ProjectFileType ClassifyFile(string projectRoot, string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            if (IsVisualPage(projectRoot, fileName))
            {
                return ProjectFileType.VisualPage;
            }

            return ProjectFileType.PureDartFile;
        }
    }
}

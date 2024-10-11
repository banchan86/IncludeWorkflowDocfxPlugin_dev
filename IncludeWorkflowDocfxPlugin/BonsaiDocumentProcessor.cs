namespace BonsaiDocumentProcessors
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.IO;
    using System.Xml.Linq; //read .bonsai XML files

    using Docfx.Common;
    using Docfx.Plugins;

    [Export(typeof(IDocumentProcessor))]
    public class BonsaiDocumentProcessor : IDocumentProcessor
    {
        #region BuildSteps
        [ImportMany(nameof(BonsaiDocumentProcessor))]
        public IEnumerable<IDocumentBuildStep> BuildSteps { get; set; }
        #endregion

        #region Name
        public string Name => nameof(BonsaiDocumentProcessor);
        #endregion

        #region GetProcessingPriority
        public ProcessingPriority GetProcessingPriority(FileAndType file)
        {
            if (file.Type == DocumentType.Article &&
                ".bonsai".Equals(Path.GetExtension(file.File), StringComparison.OrdinalIgnoreCase))
            {
                return ProcessingPriority.Normal;
            }
            return ProcessingPriority.NotSupported;
        }
        #endregion

        #region Load
        public FileModel Load(FileAndType file, ImmutableDictionary<string, object> metadata)
        {
            var content = new Dictionary<string, object>
            {
                ["conceptual"] = XDocument.Load(Path.Combine(file.BaseDir, file.File)),
                ["type"] = "Conceptual",
                ["path"] = file.File,
            };
            var localPathFromRoot = PathUtility.MakeRelativePath(EnvironmentContext.BaseDirectory, EnvironmentContext.FileAbstractLayer.GetPhysicalPath(file.File));

            return new FileModel(file, content)
            {
                LocalPathFromRoot = localPathFromRoot,
            };
        }
        #endregion

        #region Save
        public SaveResult Save(FileModel model)
        {
            // The docfx plugin directly converts .bonsai files to .html files and saves it in _site/api
            // The next few lines instead saves it to the api/ folder
            // Hopefully we can generate mref YAML files instead (so they can share the same template)

            // Cast model.Content to a Dictionary<string, object>
            var contentDict = (Dictionary<string, object>)model.Content;

            // Strip the "api/" prefix from model.File if it starts with it
            string apiFolder = "api";
            string relativePath = model.File.StartsWith(apiFolder)
                ? model.File.Substring(apiFolder.Length + 1)  // Remove 'api/' from the path
                : model.File;

            // Replace directory separators (like / or \) with dots and remove the .bonsai extension
            string fileNameWithDots = Path.ChangeExtension(relativePath, null)
                .Replace(Path.DirectorySeparatorChar, '.')
                .Replace('/', '.')   // Also handle the case for '/'
                .Replace('\\', '.'); // Handle Windows-style backslashes explicitly

            // Set the output path for the HTML file in the api/ folder (all flattened into api/)
            string outputPath = Path.Combine("api", fileNameWithDots + ".yml");

            // Write the transformed HTML content to the output path
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)); // Ensure the api/ directory exists
            File.WriteAllText(outputPath, contentDict["conceptual"].ToString()); // Save the content as an HTML file

            return new SaveResult
            {
                DocumentType = "ManagedReference",
                FileWithoutExtension = Path.ChangeExtension(model.File, null),
            };
        }
        #endregion

        #region UpdateHref
        public void UpdateHref(FileModel model, IDocumentBuildContext context)
        {
        }
        #endregion
    }
}

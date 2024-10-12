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

            #region ExtractUID
            // Strip src prefix from file.File
            string relativePath = file.File.Replace("../src/", "");

            // Create UID from folder structure
            string fileNameWithDots = Path.ChangeExtension(relativePath, null).Replace(Path.DirectorySeparatorChar, '.');

            // Store the computed UID for later use
            content["UID"] = fileNameWithDots; 
            #endregion

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
            
            #region SaveYaml
            // Cast model.Content to a Dictionary<string, object>
            var contentDict = (Dictionary<string, object>)model.Content;
            
            // make filename for yml file
            string outputPath = Path.Combine("api", (string)contentDict["UID"] + ".yml");

            // Write the transformed YAML content to the output path
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)); // Ensure the api/ directory exists
            File.WriteAllText(outputPath, contentDict["conceptual"].ToString()); // Save the content as an yaml file
            #endregion

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

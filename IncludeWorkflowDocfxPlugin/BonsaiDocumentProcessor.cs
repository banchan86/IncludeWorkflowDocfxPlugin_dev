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
            return new SaveResult
            {
                DocumentType = "Conceptual",
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

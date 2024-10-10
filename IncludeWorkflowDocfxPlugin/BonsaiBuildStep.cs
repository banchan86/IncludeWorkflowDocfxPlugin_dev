namespace BonsaiDocumentProcessors
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Xml.Linq; //read .bonsai XML files

    using Docfx.Plugins;

    [Export(nameof(BonsaiDocumentProcessor), typeof(IDocumentBuildStep))]
    public class BonsaiBuildStep : IDocumentBuildStep
    {
        #region Build

        public void Build(FileModel model, IHostService host)
        {
            XDocument xmlDoc = (XDocument)((Dictionary<string, object>)model.Content)["conceptual"];
            string content = BonsaiToHtmlConverter.ConvertBonsaiToHtml(xmlDoc.ToString());
            ((Dictionary<string, object>)model.Content)["conceptual"] = content;
        }
        #endregion

        #region Others
        public int BuildOrder => 0;

        public string Name => nameof(BonsaiBuildStep);

        public void Postbuild(ImmutableList<FileModel> models, IHostService host)
        {
        }

        public IEnumerable<FileModel> Prebuild(ImmutableList<FileModel> models, IHostService host)
        {
            return models;
        }
        #endregion
    }
}
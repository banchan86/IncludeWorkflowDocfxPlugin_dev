namespace BonsaiDocumentProcessors
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Schedulers;

    using MarkupConverter;
    using Docfx.Plugins;

    [Export(nameof(BonsaiDocumentProcessor), typeof(IDocumentBuildStep))]
    public class BonsaiBuildStep : IDocumentBuildStep
    {
        #region Build
        private readonly TaskFactory _taskFactory = new TaskFactory(new StaTaskScheduler(1));

        public void Build(FileModel model, IHostService host)
        {
            string content = (string)((Dictionary<string, object>)model.Content)["conceptual"];
            content = _taskFactory.StartNew(() => BonsaiToHtmlConverter.ConvertBonsaiToHtml(content)).Result;
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
[Export(typeof(IDocumentProcessor))]
public class BonsaiDocumentProcessor : IDocumentProcessor
{
    // todo : implements IDocumentProcessor.
}

public ProcessingPriority GetProcessingPriority(FileAndType file)
{
    if (file.Type == DocumentType.Article &&
        ".bonsai".Equals(Path.GetExtension(file.File), StringComparison.OrdinalIgnoreCase))
    {
        return ProcessingPriority.Normal;
    }
    return ProcessingPriority.NotSupported;
}



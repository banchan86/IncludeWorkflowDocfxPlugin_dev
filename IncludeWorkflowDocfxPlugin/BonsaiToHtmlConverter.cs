namespace BonsaiDocumentProcessors
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml.Linq; //read .bonsai XML files

    public static class BonsaiToHtmlConverter
    {
        public static string ConvertBonsaiToHtml(string xmlContent)
        {
            // Load XML content using XDocument
            var xmlDoc = XDocument.Parse(xmlContent);

            // Start building the HTML content
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.Append("<html><head><title>Bonsai Workflow</title></head><body>");
            htmlBuilder.Append("<h1>Bonsai Workflow Documentation</h1>");

            // Extract the description if available
            var description = xmlDoc.Root.Element("Description")?.Value;
            if (!string.IsNullOrEmpty(description))
            {
                htmlBuilder.Append($"<h2>Description</h2><p>{description}</p>");
            }

            // Extract nodes and append them as HTML
            var nodes = xmlDoc.Root.Descendants("Nodes");
            htmlBuilder.Append("<h2>Nodes</h2><ul>");
            foreach (var node in nodes.Descendants())
            {
                var nodeType = node.Name.LocalName;
                htmlBuilder.Append($"<li>Node Type: {nodeType}");

                // Extract and add attributes if available
                foreach (var attribute in node.Attributes())
                {
                    htmlBuilder.Append($"<br>{attribute.Name}: {attribute.Value}");
                }
                htmlBuilder.Append("</li>");
            }
            htmlBuilder.Append("</ul>");

            htmlBuilder.Append("</body></html>");
            return htmlBuilder.ToString();
        }
    }
}

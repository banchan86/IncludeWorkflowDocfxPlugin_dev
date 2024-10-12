namespace BonsaiDocumentProcessors
{
    using System.Xml.Linq; //read .bonsai XML files
    using YamlDotNet.Serialization; // To serialize to YAML
    using System.Collections.Generic; // To handle node attributes

    public static class BonsaiToYamlConverter
    {
        public static string ConvertBonsaiToYaml(string xmlContent, string fileNameWithoutExtension)
        {
            // Load XML content using XDocument
            var xmlDoc = XDocument.Parse(xmlContent);

            // Get the XML namespace (xmlns="https://bonsai-rx.org/2018/workflow")
            XNamespace ns = xmlDoc.Root.GetDefaultNamespace();

            // Use file name as the name
            var name = fileNameWithoutExtension;

            // Extract the description from the .bonsai XML
            var description = xmlDoc.Root.Element(ns + "Description")?.Value ?? "No description available.";

            // Create a YAML-compatible object with the extracted information
            var yamlData = new
            {
                uid = name,  // Unique identifier
                name = name,
                summary = description,
                // Additional fields...
            };

            // Serialize to YAML using YamlDotNet
            var serializer = new SerializerBuilder().Build();
            string yamlContent = serializer.Serialize(yamlData);

            // Add "### YamlMime:ManagedReference" to the top of the YAML content
            return $"### YamlMime:ManagedReference\n{yamlContent}";
        }
    }
}

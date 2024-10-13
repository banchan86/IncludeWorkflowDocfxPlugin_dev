namespace BonsaiDocumentProcessors
{
    using System.Xml.Linq; //read .bonsai XML files
    using System.Linq; 
    using YamlDotNet.Serialization; // To serialize to YAML
    using System.Collections.Generic; // To handle node attributes

    public static class BonsaiToYamlConverter
    {
        public static string ConvertBonsaiToYaml(string xmlContent, string fileNameWithoutExtension, string UID)
        {
            // Load XML content using XDocument
            var xmlDoc = XDocument.Parse(xmlContent);

            // Extract the XML namespace (xmlns)
            XNamespace ns = xmlDoc.Root.GetDefaultNamespace();

            // Extract the XML prefix namespace for properties (xmlns:xsi)
            XNamespace xsiNamespace = xmlDoc.Root.GetNamespaceOfPrefix("xsi");

            // Use UID as uid
            var uid = UID;

            // Use file name as the name
            var name = fileNameWithoutExtension;

            // extract namespace
            string @namespace = uid.Replace("." + name, "");

            // Extract the description from the .bonsai XML
            var description = xmlDoc.Root.Element(ns + "Description")?.Value ?? "No description available.";



            // Create YAML-compatible items for the properties
            var propertyItems = xmlDoc.Descendants(ns + "Expression")
                .Where(x => (string)x.Attribute(XName.Get("type", xsiNamespace.NamespaceName)) == "ExternalizedMapping")
                .SelectMany(x => x.Elements(ns + "Property"))
                .Select(p => 
                {
                    // Use DisplayName if it exists, otherwise fall back to Name
                    string propertyName = p.Attribute("DisplayName")?.Value 
                              ?? p.Attribute("Name")?.Value 
                              ?? "Unnamed";

                    return new
                    {
                        uid = $"{uid}.{propertyName}",
                        id = propertyName,
                        parent = uid,
                        name = propertyName,
                        summary = (string)p.Attribute("Description") ?? "No description available.",
                        type = "Property",
                    };
                })
                // Group by property name and select the one with the best description
                // This avoids duplicate properties appearing in the final yaml
                .GroupBy(p => p.name)
                .Select(g => g.OrderByDescending(p => p.summary != "No description available.").First())
                .ToList();

            // Create the list of child UIDs to be included in the parent item
            var childUids = propertyItems.Select(p => p.uid).ToList();

            // Create a YAML-compatible item for the operator with the extracted information
            var operatorItem = new
            {
                uid = uid,  
                id = name,
                parent = @namespace,
                children = childUids,
                name = name,
                nameWithType = name,
                fullName = uid,
                type = "Class",
                @namespace = @namespace,
                summary = description,
                // Additional fields...
            };

            // Combine the operator item and property items into a single list
            var allItems = new List<object> { operatorItem }.Concat(propertyItems).ToList();

            // Create the final YAML structure
            var yamlData = new { items = allItems };

            // // Wrap the items in an `items` list (like in DocFX)
            // var yamlData = new
            // {
            //     items = new[] { yamlItem }
            // };

            // Serialize to YAML using YamlDotNet
            var serializer = new SerializerBuilder().Build();
            string yamlContent = serializer.Serialize(yamlData);

            // Add "### YamlMime:ManagedReference" to the top of the YAML content
            return $"### YamlMime:ManagedReference\n{yamlContent}";
        }
    }
}

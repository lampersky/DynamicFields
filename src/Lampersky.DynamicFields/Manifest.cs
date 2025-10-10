using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Dynamic Fields",
    Author = "Lampersky",
    Website = "https://github.com/lampersky/DynamicFields",
    Version = "1.0.0",
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.DynamicFields",
    Name = "Dynamic Fields",
    Category = "Content Management",
    Description = "Dynamic Fields module adds dynamic fields to be used with your custom types.",
    Dependencies = ["OrchardCore.ContentTypes", "OrchardCore.Shortcodes"]
)]

[assembly: Feature(
    Id = "OrchardCore.DynamicFields.Indexing.SQL",
    Name = "Dynamic Fields Indexing (SQL)",
    Category = "Content Management",
    Description = "Dynamic Fields Indexing module adds database indexing for user dynamic fields.",
    Dependencies =
    [
        "OrchardCore.DynamicFields",
        "OrchardCore.ContentFields",
        "OrchardCore.ContentFields.Indexing.SQL"
    ]
)]

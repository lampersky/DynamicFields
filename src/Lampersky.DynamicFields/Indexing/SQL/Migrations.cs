using OrchardCore.ContentManagement.Records;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace OrchardCore.DynamicFields.Indexing.SQL;

public sealed class Migrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        // NOTE: The Text Length has been decreased from 4000 characters to 768.
        // For existing SQL databases update the DynamicFieldIndex tables Text column length manually.
        // INFO: The Text Length is now of 766 chars, but this is only used on a new installation.
        await SchemaBuilder.CreateMapIndexTableAsync<DynamicFieldIndex>(table => table
            .Column<string>("ContentItemId", column => column.WithLength(26))
            .Column<string>("ContentItemVersionId", column => column.WithLength(26))
            .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
            .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
            .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
            .Column<bool>("Published", column => column.Nullable())
            .Column<bool>("Latest", column => column.Nullable())
            .Column<string>("Path", column => column.Nullable().WithLength(DynamicFieldIndex.MaxTextSize))
            .Column<string>("Type", column => column.Nullable().WithLength(DynamicFieldIndex.MaxTextSize))
            .Column<string>("Text", column => column.Nullable().WithLength(DynamicFieldIndex.MaxTextSize))
            .Column<string>("BigText", column => column.Nullable().Unlimited())
        );

        await SchemaBuilder.AlterIndexTableAsync<DynamicFieldIndex>(table => table
            .CreateIndex("IDX_DynamicFieldIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "ContentItemVersionId",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<DynamicFieldIndex>(table => table
            .CreateIndex("IDX_DynamicFieldIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + Text (764) + Published and Latest (1) = 767 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<DynamicFieldIndex>(table => table
            .CreateIndex("IDX_DynamicFieldIndex_DocumentId_Text",
                "DocumentId",
                "Text(764)",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + Path (764) + Published and Latest (1) = 767 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<DynamicFieldIndex>(table => table
            .CreateIndex("IDX_DynamicFieldIndex_DocumentId_Path",
                "DocumentId",
                "Path(764)",
                "Published",
                "Latest")
        );

        return 1;
    }
}

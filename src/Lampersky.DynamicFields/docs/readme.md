# DynamicField Module for Orchard Core  

**DynamicField** is a custom Orchard Core module that introduces a flexible new type of field.  
Unlike standard fields, it allows you to define rich, fully interactive editors using vanilla JavaScript - without writing or redeploying backend code.  
You can easily wrap existing JavaScript components into a **DynamicField** and use them directly within your content types.  

## Key Features  
- **Build Fields on the Fly**  
  Create and configure new fields dynamically with zero changes in OrchardCore code and no deployment.  

- **Adapt Existing JS Components**  
  Bring any JavaScript-based UI component into Orchard Core as a reusable field.  

- **GraphQL Support**  
  Query DynamicField values seamlessly via Orchard Core’s GraphQL endpoints.  

- **SQL Indexing**  
  Field values are fully indexable in SQL, enabling efficient queries and filtering.  

- **Recipe with Examples**  
  Shipped with a ready-to-use Orchard Core recipe that installs various DynamicField examples to explore out-of-the-box.  

## Included Examples  
To showcase the flexibility of DynamicField, several ready-to-use integrations are included:  

- **[GrapesJS editor](https://grapesjs.com)** - drag & drop web page builder,
- **[LeafletJS map](https://leafletjs.com)** - interactive maps,
- **[Coloris](http://coloris.js.org)** - lightweight color picker,
- **[Vanilla Colorful](https://github.com/web-padawan/vanilla-colorful)** - modern color picker,
- **Simple paint app** - vanilla JS canvas drawing tool,
- **Image gallery** - powered by [Picsum Photos](http://picsum.photos) free image service,
- **[Image crop & upload](https://github.com/github/image-crop-element)** - simple image editing,
- **Geo-coordinates field** - lightweight alternative to OrchardCore Spatial.

## Use Cases  
- Rapid prototyping of custom field types,
- Integrating third-party JavaScript components into Orchard Core,
- Enhancing the content editing experience without backend development,
- Lowering the barrier for non-OrchardCore experts to create new editors.
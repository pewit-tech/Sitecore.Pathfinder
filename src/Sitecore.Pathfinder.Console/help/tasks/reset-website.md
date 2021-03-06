reset-website
=============
Resets the website.

Remarks
-------
Sometimes it is nice to revert back to a known state on the website or simply clean up. The `scc reset-website` command
can delete items and files on a website based on the project/website settings. Any items or files that are mapped back 
to the project in the 'project-website-mappings' settings will be deleted.

If you have additional items to be deleted using reset-website, you can configure them in the 'reset-website' settings.

```js
   "reset-website": {
        "clean-blog-renderings": {
            "delete-item-path": "/sitecore/layout/Renderings/CleanBlog" 
        }
    }
}
```

This will delete the /sitecore/layout/Renderings/CleanBlog item.

Settings
--------
| Setting name    | Description             | 
|-----------------|-------------------------|
| reset-website:* | The items to delete.    |

Example
-------
```cmd
> scc reset-website
```
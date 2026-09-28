# Bicep

Use `resource-group.bicep` first, `foundation.bicep` second, and `app.bicep` after pushing an image and creating the search index. Follow the shared deployment steps in [../README.md](../README.md). Pass SQL secrets as secure parameters from a protected source; keep local parameter files with secrets out of source control.

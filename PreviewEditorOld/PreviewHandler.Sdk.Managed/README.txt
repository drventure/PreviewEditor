This is the source folder for the PreviewHandler.Sdk.Managed project.

The source is included directly because, since this project exposes classes
that the PreviewEditor inherits from, it can't be embedded as a resource
like the actual editors can.

Literally, this folder is a copy of the original project, minus the 
strong signing key file and the CSPROJ file, as they aren't needed.

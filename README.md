# AndroidManifestLocator
A tool to (recursively) locate the path of all AndroidManifest.xml files in a directory. The tool creates a CSV with the following columns:

```
FilePath,PackageName,MaxSdkVersion,MinSdkVersion,TargetSdkVersion,PermissionCount,Permissions
```
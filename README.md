This repository contains the package that we had created to enable Source Link on projects hosted in Subversion repositories. We open it up to benefit the open source community.

# SourceLink.VisualSVN
Package SourceLink.VisualSVN supports VisualSVN Server. To embed Source Link metadata in the PDB file, all that is needed is to add a reference to package SourceLink.VisualSVN in the project file.

```xml
  <ItemGroup>
    <PackageReference Include="SourceLink.VisualSVN" Version="1.0.2" PrivateAssets="all" />
  </ItemGroup>
```

# Other Subversion servers
Other Subversion servers may use a different format of Web URL to retrieve documents of a specific revision. To support a new Subversion server, just clone project SourceLink.VisualSVN and provide an implementation of SourceLink.SVN.GetSourceLinkUrl() method that generates the a URL for that server.
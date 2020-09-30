using Microsoft.Build.Utilities;
using SourceLink.Common;

namespace SourceLink.SVN
{
    public sealed partial class GetSourceLinkUrlTask : Task
    {
        private static string GetSourceLineUrl(SvnInfo svnInfo)
        {
            return $"{svnInfo.SvnRootUrl}/*?p={svnInfo.SvnRevision}";
        }
    }
}
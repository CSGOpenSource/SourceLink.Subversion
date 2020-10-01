using Microsoft.Build.Utilities;
using SourceLink.Common;

namespace SourceLink.SVN
{
    public sealed partial class GetSourceLinkUrlTask : Task
    {
        private static string GetSourceLinkUrl(SvnInfo svnInfo)
        {
            return $"{svnInfo.SvnRootUrl}/*?p={svnInfo.SvnRevision}";
        }
    }
}
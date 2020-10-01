using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SourceLink.Common;

namespace SourceLink.SVN
{
    public sealed partial class GetSourceLinkUrlTask : Task
    {
        /// <summary>
        /// The project's local directory. (Used for SVN working directory)
        /// </summary>
        [Required]
        public string ProjectPath { get; set; }

        /// <summary>
        /// SourceRoot items.
        /// </summary>
        public ITaskItem SourceRoot { get; set; }

        /// <summary>
        /// Returns items describing repository source roots:
        /// 
        /// Metadata
        ///   SourceControl: "svn"
        ///   MappedPath: The module's local directory. e.g.: D:\work\Modules\Configuration\trunk\
        ///   SourceLinkUrl: SourceLink URL to be embedded in PDB. e.g.: https://svn.xxxcorp.com/svn/repos-acpx2/Modules/Configuration/trunk/*?p=611901
        /// </summary>
        [Output]
#pragma warning disable CA1819 // Properties should not return arrays
        public ITaskItem[] Roots { get; private set; }
#pragma warning restore CA1819 // Properties should not return arrays

        public override bool Execute()
        {
            var svnInfo = new SvnInfo(ProjectPath, Log);

            if (string.IsNullOrEmpty(svnInfo.WorkingCopyPath) || string.IsNullOrEmpty(svnInfo.SvnRootUrl) || string.IsNullOrEmpty(svnInfo.SvnRevision))
                Roots = null;
            else
            {
                ITaskItem item = new TaskItem($@"{svnInfo.WorkingCopyPath}\");
                item.SetMetadata("SourceControl", "svn");
                item.SetMetadata("MappedPath", $@"{svnInfo.WorkingCopyPath}\");
                item.SetMetadata("SourceLinkUrl", $"{GetSourceLinkUrl(svnInfo)}");

                Roots = new[] {item};
            }

            return !Log.HasLoggedErrors;
        }
    }
}
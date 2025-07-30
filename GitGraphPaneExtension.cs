using Mendix.StudioPro.ExtensionsAPI.UI.DockablePane;
using System.ComponentModel.Composition;

namespace CommitGraph
{
    [Export(typeof(DockablePaneExtension))]
    public class GitGraphPaneExtension : DockablePaneExtension
    {
        public const string ID = "git-graph-pane";
        public override string Id => ID;
        
        public override DockablePaneViewModelBase Open() => new GitGraphViewModel();
    }
}
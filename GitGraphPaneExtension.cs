// Required namespace to define and manage custom dockable panes (tool windows) in Mendix Studio Pro
using Mendix.StudioPro.ExtensionsAPI.UI.DockablePane;
using System.ComponentModel.Composition;  // For exporting the class as an extension using MEF

namespace CommitGraph
{
    // Export this class so that Mendix Studio Pro can recognize it as a DockablePaneExtension
    [Export(typeof(DockablePaneExtension))]
    public class GitGraphPaneExtension : DockablePaneExtension
    {
        // Define a constant ID used to uniquely identify this pane within Studio Pro
        public const string ID = "git-graph-pane";

        // Override the Id property to return the unique pane ID
        public override string Id => ID;

        // This method is called when the pane is opened — return the ViewModel for the pane's content
        public override DockablePaneViewModelBase Open() => new GitGraphViewModel();
    }
}
